using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Packaging;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.IO;
using System.Windows.Markup;
using System.Threading;

namespace Uzi.Visualize.Packaging
{
    public class MetaModel : Model3DPart, IResolveMaterial, IResolveFragment
    {
        /// <summary>Relationship to map meta-models (http://pack.guildsmanship.com/visualize/metamodel)</summary>
        public const string MetaModelRelation = @"http://pack.guildsmanship.com/visualize/metamodel";

        /// <summary>Relationship to meta model nodes (http://pack.guildsmanship.com/visualize/metamodelstate)</summary>
        public const string MetaModelStateRelation = @"http://pack.guildsmanship.com/visualize/metamodelstate";

        /// <summary>Expected XMLNamespace for instance of FragmentReference and VisualEffectMaterial</summary>
        public const string CLRNamespace = @"clr-namespace:Uzi.Visualize;assembly=Uzi.Visualize";

        #region ctor()
        public MetaModel(ICorePartNameManager manager, FileInfo modelFile)
            : base(manager, modelFile)
        {
            _Fragments = new Dictionary<string, MetaModelFragment>();
            _State = new MetaModelState();
            _FragParent = this.GetIResolveFragment();
            _MatParent = this.GetIResolveMaterial();
        }

        public MetaModel(ICorePartNameManager manager, MetaModel source, string name)
            : base(manager, source, name)
        {
            _Fragments = new Dictionary<string, MetaModelFragment>();
            foreach (var _frag in _Fragments)
                AddFragment(new MetaModelFragment(this, _frag.Value, _frag.Key));

            _FragParent = this.GetIResolveFragment();
            _MatParent = this.GetIResolveMaterial();

            _State = new MetaModelState(source.State);
        }

        public MetaModel(ICorePartNameManager manager, PackagePart part, string id)
            : base(manager, part, id)
        {
            _Fragments = new Dictionary<string, MetaModelFragment>();

            foreach (var _fRef in MetaModelFragment.GetMetaModelFragmentResources(this, Part))
            {
                if (!_Fragments.ContainsKey(_fRef.Name))
                {
                    _Fragments.Add(_fRef.Name, _fRef);
                }
            }

            _FragParent = this.GetIResolveFragment();
            _MatParent = this.GetIResolveMaterial();

            var _nodesRel = part.GetRelationshipsByType(MetaModelStateRelation).FirstOrDefault();
            if (_nodesRel != null)
            {
                var _nodesPart = part.Package.GetPart(_nodesRel.TargetUri);
                if (_nodesPart != null)
                {
                    try
                    {
                        using var _nodeStream = _nodesPart.GetStream(FileMode.Open, FileAccess.Read);
                        _State = XamlReader.Load(_nodeStream) as MetaModelState;
                    }
                    catch
                    {
                    }
                }
            }
            if (_State == null)
                _State = new MetaModelState();
        }
        #endregion

        #region data
        private Dictionary<string, MetaModelFragment> _Fragments;
        private readonly MetaModelState _State;
        private readonly IResolveFragment _FragParent = null;
        private readonly IResolveMaterial _MatParent = null;
        #endregion

        /// <summary>Lists all instances of MetaModelFragment related to this MetaModel</summary>
        public IEnumerable<KeyValuePair<string, MetaModelFragment>> Fragments { get { return _Fragments.OrderBy(_f => _f.Key); } }

        #region public void AddFragment(MetaModelFragment fragment)
        public void AddFragment(MetaModelFragment fragment)
        {
            if (CanUseName(fragment.Name, typeof(MetaModelFragment)))
            {
                if (_Fragments == null)
                    _Fragments = new Dictionary<string, MetaModelFragment>();
                _Fragments.Add(fragment.Name, fragment);
                fragment.NameManager = this;
                // TODO: add fragment parameter expectations
                DoPropertyChanged(@"Relationships");
            }
        }
        #endregion

        #region public void RemoveFragment(MetaModelFragment fragment)
        public void RemoveFragment(MetaModelFragment fragment)
        {
            if ((_Fragments != null) && _Fragments.ContainsKey(fragment.Name))
            {
                _Fragments.Remove(fragment.Name);
                // TODO: remove fragment parameter expectations
                DoPropertyChanged(@"Relationships");
            }
        }
        #endregion

        public void FlushCache()
        {
            _SETypes?.Clear();
            _ModelCache?.Clear();
        }

        #region public override Model3D ResolveModel()
        public override Model3D ResolveModel()
        {
            ModelCacheSelector _selector = null;
            if (_SETypes != null)
            {
                // try the sense effect cache...
                _selector = new ModelCacheSelector((
                    from _t in _SETypes
                    let _i = (MarkupExtension)Activator.CreateInstance(_t)
                    select (VisualEffect)_i.ProvideValue(null)).ToList(),
                    ExternalVal.Values);
                var (_, _model) = _ModelCache
                    .FirstOrDefault(_ce => _ce.selector.Equals(_selector));
                if (_model != null)
                    return _model;
            }
            else
            {
                _selector = new ModelCacheSelector(null, ExternalVal.Values);
            }

            try
            {
                MetaModelResolutionStack.Clear(State);
                MetaModelResolutionStack.Push(State.RootFragment);

                // start tracking visualEffectMaterial and FragmentReference nodes
                _VEMKeys = new SortedSet<string>();

                State.RootFragment.AttachReferenceTrackers();
                if (_SETypes == null)
                {
                    // referenced sense extensions
                    _SETypes = new List<Type>();
                    SenseEffectExtension.ReferencedEffect = (type) => { if (!_SETypes.Contains(type)) _SETypes.Add(type); };
                }

                // NOTE: different and additional resolution of references
                VisualEffectMaterial.PushResolver(this);
                IkosaImageSource.PushResolver(this);
                FragmentReference.PushResolver(this);
                State.RootFragment.PrepareGather();

                var _resolve = ResolveFromStream();
                if (!_ModelCache.Any(_ce => _ce.selector.Equals(_selector)))
                {
                    _resolve.Freeze();
                    _ModelCache.Add((_selector.GetCacheKey(), _resolve));
                }

                return _resolve;
            }
            finally
            {
                State.RootFragment.PruneAfterGather();

                // default brush sync
                var _defBrushes = State.RootFragment.GatherDefaultBrushNames().Distinct().OrderBy(_s => _s);
                foreach (var _brush in State.DefaultBrushes.Where(_b => !_defBrushes.Contains(_b.ReferenceKey)).ToList())
                    State.DefaultBrushes.Remove(_brush);
                foreach (var _newBrush in _defBrushes.Where(_b => !State.DefaultBrushes.Any(_db => _db.ReferenceKey == _b)))
                    State.DefaultBrushes.Add(new BrushCrossRefNode { ReferenceKey = _newBrush });

                // default IntRefs sync
                var _intRefs = State.RootFragment.GatherMasterIntReferences();
                foreach (var _iRef in State.MasterIntReferences
                    .Where(_mir => !_intRefs.Any(_ir => _mir.Key == _ir.Key)).ToList())
                    State.MasterIntReferences.Remove(_iRef);
                foreach (var _newIRef in _intRefs
                    .Where(_ir => !State.MasterIntReferences.Any(_mir => _mir.Key == _ir.Key)).ToList())
                    State.MasterIntReferences.Add(new IntReference
                    {
                        Key = _newIRef.Key,
                        IsActive = true,
                        Value = _newIRef.Value,
                        UseMaster = false,
                        MinValue = _newIRef.MinValue,
                        MaxValue = _newIRef.MaxValue
                    });

                // default DoubleRefs sync
                var _dblRefs = State.RootFragment.GatherMasterDoubleReferences();
                foreach (var _dRef in State.MasterDoubleReferences
                    .Where(_mdr => !_dblRefs.Any(_dr => _mdr.Key == _dr.Key)).ToList())
                    State.MasterDoubleReferences.Remove(_dRef);
                foreach (var _newDRef in _dblRefs
                    .Where(_dr => !State.MasterDoubleReferences.Any(_mdr => _mdr.Key == _dr.Key)).ToList())
                    State.MasterDoubleReferences.Add(new DoubleReference
                    {
                        Key = _newDRef.Key,
                        IsActive = true,
                        Value = _newDRef.Value,
                        UseMaster = false,
                        MinValue = _newDRef.MinValue,
                        MaxValue = _newDRef.MaxValue
                    });

                // cleanup thread tracking
                VisualEffectMaterial.ReferencedKey = null;
                FragmentReference.ReferencedKey = null;
                IntVal.ReferencedKey = null;
                DoubleVal.ReferencedKey = null;
                VisualEffectMaterial.PullResolver(this);
                IkosaImageSource.PullResolver(this);
                FragmentReference.PullResolver(this);
                MetaModelResolutionStack.Clear(null);
            }
        }
        #endregion

        // Relationships
        public override IEnumerable<ICorePart> Relationships
            => (_Fragments != null)
            ? base.Relationships.Union(Fragments.Select(_f => _f.Value))
            : base.Relationships;

        public MetaModelState State => _State;

        public override string TypeName => GetType().FullName;

        protected override string SaveTarget => @"metamodel.xaml";
        protected override string SaveRelation => MetaModelRelation;
        protected const string NodeTarget = @"metastate.xaml";

        #region protected override void DoSave(Stream modelStream, Uri baseUri, bool newPart )
        protected override void DoSave(Stream modelStream, Uri baseUri, bool newPart)
        {
            base.DoSave(modelStream, baseUri, newPart);

            // node references
            var _nodeUri = UriHelper.ConcatRelative(baseUri, NodeTarget);
            if (newPart)
            {
                // must make part and relation 
                var _nodePart = _Part.Package.CreatePart(_nodeUri, @"text/xaml+xml", CompressionOption.Normal);
                _Part.CreateRelationship(_nodeUri, TargetMode.Internal, MetaModelStateRelation, NodeTarget);
                using var _nodeStream = _nodePart.GetStream(FileMode.Create, FileAccess.ReadWrite);
                using var _writer = new StreamWriter(_nodeStream);
                XamlWriter.Save(_State, _writer);
            }
            else
            {
                // find existing part and replace stream
                var _nodesRel = _Part.GetRelationshipsByType(MetaModelStateRelation).FirstOrDefault();
                if (_nodesRel != null)
                {
                    var _nodesPart = _Part.Package.GetPart(_nodesRel.TargetUri);
                    if (_nodesPart != null)
                    {
                        try
                        {
                            using var _nodeStream = _nodesPart.GetStream(FileMode.Create, FileAccess.ReadWrite);
                            using var _writer = new StreamWriter(_nodeStream);
                            XamlWriter.Save(_State, _writer);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            // save related fragments
            var _base = UriHelper.ConcatRelative(baseUri, @"fragments");
            foreach (var _item in Fragments)
            {
                _item.Value.Save(_Part, _base);
            }
        }
        #endregion

        #region protected override void OnRefreshPart()
        protected override void OnRefreshPart()
        {
            base.OnRefreshPart();
            if (Part != null)
            {
                foreach (var _fRef in MetaModelFragment.GetMetaModelFragmentResources(this, Part))
                {
                    if (_Fragments.ContainsKey(_fRef.Name))
                        _Fragments[_fRef.Name].RefreshPart(_fRef.Part);
                    else
                        _Fragments.Add(_fRef.Name, _fRef);
                }
            }
        }
        #endregion

        #region ICorePartNameManager Members

        public override bool CanUseName(string name, Type partType)
        {
            var _name = name.ToSafeString();
            if (partType == typeof(MetaModelFragment))
                return ((_Fragments != null) && !_Fragments.ContainsKey(_name));
            return base.CanUseName(name, partType);
        }

        public override void Rename(string oldName, string newName, Type partType)
        {
            if ((_Fragments != null) && (partType == typeof(MetaModelFragment)))
            {
                var _frag = _Fragments[oldName];
                if (_frag != null)
                {
                    _Fragments.Remove(oldName);
                    _Fragments.Add(newName, _frag);
                }
            }
            base.Rename(oldName, newName, partType);
        }

        #endregion

        #region public static IEnumerable<MetaModel> GetMetaModelResources(ICorePartNameManager manager, PackagePart part, IResolveBitmapImage bParent)
        /// <summary>Pre-loads meta-models from related package parts for IResolveModel3D</summary>
        /// <param name="part">part with (possible) MetaModel relations</param>
        public static IEnumerable<MetaModel> GetMetaModelResources(ICorePartNameManager manager, PackagePart part, IResolveBitmapImage bParent, IResolveFragment fParent)
        {
            foreach (var _imgRel in part.GetRelationshipsByType(MetaModel.MetaModelRelation))
            {
                var _imgPart = part.Package.GetPart(_imgRel.TargetUri);
                yield return new MetaModel(manager, _imgPart, _imgRel.Id);
            }
        }
        #endregion

        #region IResolveMaterial Members

        public Material GetMaterial(object key, VisualEffect effect)
        {
            if (MetaModelResolutionStack.Any())
            {
                // get specific mapped brush (if any)
                var _current = MetaModelResolutionStack.Peek();
                var _brushNode = _current.Brushes
                    .FirstOrDefault(_bn => (_bn.ReferenceKey == key.ToString()) && !string.IsNullOrEmpty(_bn.BrushKey));
                if (_brushNode != null)
                {
                    // try to resolve the material from private set
                    var _material = Brushes.GetMaterial(_brushNode.BrushKey, effect);
                    if (_material != null)
                        return _material;

                    // use the resolver, only if the brush key wouldn't circle back to reference key resolution
                    if (!_current.Brushes.Any(_b => _b.ReferenceKey == _brushNode.BrushKey)
                        && !State.DefaultBrushes.Any(_b => _b.ReferenceKey == _brushNode.BrushKey))
                        return VisualEffectMaterial.ResolveMaterial(_brushNode.BrushKey, effect);
                }
            }

            // get default mapped brush (if any)
            var _defBrush = State.DefaultBrushes
                .FirstOrDefault(_bn => (_bn.ReferenceKey == key.ToString()) && !string.IsNullOrEmpty(_bn.BrushKey));
            if (_defBrush != null)
            {
                // try to resolve the material from private set
                var _material = Brushes.GetMaterial(_defBrush.BrushKey, effect);
                if (_material != null)
                    return _material;

                // use the resolver, only if the brush key wouldn't circle back to reference key resolution
                if (!State.DefaultBrushes.Any(_b => _b.ReferenceKey == _defBrush.BrushKey))
                    return VisualEffectMaterial.ResolveMaterial(_defBrush.BrushKey, effect);
            }

            // no mapping defined
            return null;
        }

        public IResolveMaterial IResolveMaterialParent { get { return _MatParent; } }

        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes
        {
            get
            {
                // self
                var _brsh = Brushes.BrushDefinitions.Select(_bd => new BrushDefinitionListItem
                {
                    IsLocal = true,
                    BrushDefinition = _bd
                });

                if (_MatParent != null)
                    _brsh = _brsh.Union(_MatParent.ResolvableBrushes.Select(_pb => new BrushDefinitionListItem
                    {
                        BrushDefinition = _pb.BrushDefinition,
                        IsLocal = false
                    }));

                // done
                return _brsh;
            }
        }

        #endregion

        #region IResolveFragment Members

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
        {
            var _group = new Model3DGroup();

            if (_Fragments.ContainsKey(node.FragmentKey))
            {
                var _fragPart = _Fragments[node.FragmentKey];
                var _mdl = _fragPart.ResolveModel(node);

                // collect and end resolve of fragment
                if (_mdl != null)
                    _group.Children.Add(fragRef.ApplyTransforms(node, _mdl));
            }

            // return smallest possible grouping
            if (_group.Children.Count == 1)
                return _group.Children.First();
            else if (_group.Children.Any())
                return _group;
            return null;
        }

        public IResolveFragment IResolveFragmentParent { get { return _FragParent; } }

        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments
        {
            get
            {
                // our own (and anything available through parent's)
                var _resolvable = _Fragments.Select(_kvp => new MetaModelFragmentListItem
                {
                    MetaModelFragment = _kvp.Value,
                    IsLocal = true
                });
                if (IResolveFragmentParent != null)
                    _resolvable = _resolvable.Union(IResolveFragmentParent.ResolvableFragments
                        .Select(_f => new MetaModelFragmentListItem
                        {
                            IsLocal = false,
                            MetaModelFragment = _f.MetaModelFragment
                        }));
                return _resolvable;
            }
        }

        #endregion
    }
}
