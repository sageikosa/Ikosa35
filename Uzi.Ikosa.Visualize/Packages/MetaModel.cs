using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.IO;
using System.Windows.Markup;
using System.Threading;
using Ikosa.Packaging;
using System.IO.Compression;

namespace Uzi.Visualize.Packages
{
    public class MetaModel : Model3DPart, IPartResolveMaterial, IPartResolveFragment
    {
        /// <summary>Expected XMLNamespace for instance of FragmentReference and VisualEffectMaterial</summary>
        public const string CLRNamespace = @"clr-namespace:Uzi.Visualize;assembly=Uzi.Visualize";

        #region state
        private Dictionary<string, MetaModelFragment> _Fragments;
        private MetaModelState _State;
        private readonly IPartResolveFragment _FragParent = null;
        private readonly IPartResolveMaterial _MatParent = null;
        #endregion

        #region ctor()
        public MetaModel(IRetrievablePartNameManager manager, string id)
            : base(manager, id)
        {
            _Fragments = new Dictionary<string, MetaModelFragment>();
            _FragParent = this.GetIPartResolveFragment();
            _MatParent = this.GetIPartResolveMaterial();
            _State = new MetaModelState();
        }

        public MetaModel(IRetrievablePartNameManager manager, FileInfo modelFile)
            : base(manager, modelFile)
        {
            _Fragments = new Dictionary<string, MetaModelFragment>();
            _FragParent = this.GetIPartResolveFragment();
            _MatParent = this.GetIPartResolveMaterial();
            _State = new MetaModelState();
        }

        public MetaModel(IRetrievablePartNameManager manager, MetaModel source, string name)
            : base(manager, source, name)
        {
            _Fragments = new Dictionary<string, MetaModelFragment>();
            _FragParent = this.GetIPartResolveFragment();
            _MatParent = this.GetIPartResolveMaterial();
            _State = new MetaModelState(source.State);

            foreach (var _frag in _Fragments)
                AddFragment(new MetaModelFragment(this, _frag.Value, _frag.Key));
        }
        #endregion

        /// <summary>Lists all instances of MetaModelFragment related to this MetaModel</summary>
        public IEnumerable<KeyValuePair<string, MetaModelFragment>> Fragments => _Fragments.OrderBy(_f => _f.Key);

        #region public void AddFragment(MetaModelFragment fragment)
        public void AddFragment(MetaModelFragment fragment)
        {
            if (CanUseName(fragment.PartName))
            {
                if (_Fragments == null)
                    _Fragments = new Dictionary<string, MetaModelFragment>();
                _Fragments.Add(fragment.PartName, fragment);
                fragment.PartNameManager = this;
                // TODO: add fragment parameter expectations
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        #region public void RemoveFragment(MetaModelFragment fragment)
        public void RemoveFragment(MetaModelFragment fragment)
        {
            if ((_Fragments != null) && _Fragments.ContainsKey(fragment.PartName))
            {
                _Fragments.Remove(fragment.PartName);
                // TODO: remove fragment parameter expectations
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        public void FlushCache()
        {
            _SETypes?.Clear();
            _ModelCache?.Clear();
        }

        #region IPartResolveMaterial Members

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

        public IPartResolveMaterial IPartResolveMaterialParent => _MatParent;

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

        #region IPartResolveFragment

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

        public IPartResolveFragment IPartResolveFragmentParent => _FragParent;

        public IEnumerable<MetaModelFragmentPartListItem> ResolvableFragments
        {
            get
            {
                // our own (and anything available through parent's)
                var _resolvable = _Fragments.Select(_kvp => new MetaModelFragmentPartListItem
                {
                    MetaModelFragment = _kvp.Value,
                    IsLocal = true
                });
                if (IPartResolveFragmentParent != null)
                    _resolvable = _resolvable.Union(IPartResolveFragmentParent.ResolvableFragments
                        .Select(_f => new MetaModelFragmentPartListItem
                        {
                            IsLocal = false,
                            MetaModelFragment = _f.MetaModelFragment
                        }));
                return _resolvable;
            }
        }

        #endregion

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
        public override IEnumerable<IRetrievablePart> Parts
            => (_Fragments != null)
            ? base.Parts.Union(Fragments.Select(_f => _f.Value))
            : base.Parts;

        public MetaModelState State => _State;

        public override string PartType => GetType().FullName;

        protected override string SaveTarget => @"metamodel.xaml";
        protected const string NodeTarget = @"metastate.xaml";

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            base.StorePart(archive, parentPath);
            var _partPath = $@"{parentPath}/{PartName}";

            // metastate
            var _statePath = $@"{_partPath}/{NodeTarget}";
            var _stateEntry = archive.CreateEntry(_statePath);
            using var _stateStream = _stateEntry.Open();
            using var _stateWriter = new StreamWriter(_stateStream);
            XamlWriter.Save(_State, _stateWriter);

            // fragments
            var _fragPath = $@"{_partPath}/Fragments";
            foreach (var _frag in ResolvableFragments.Where(_f => _f.IsLocal))
            {
                _frag.MetaModelFragment.StorePart(archive, _fragPath);
            }
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            base.ReloadPart(archive, parentPath);
            var _partPath = $@"{parentPath}/{PartName}";

            // metastate
            var _statePath = $@"{_partPath}/{NodeTarget}";
            var _stateEntry = archive.GetEntry(_statePath);
            using var _stateStream = _stateEntry.Open();
            _State = XamlReader.Load(_stateStream) as MetaModelState;

            // fragments
            var _fragPath = $@"{_partPath}/Fragments";
            foreach (var _fRef in archive.Entries
               .Where(_e
               // starts with frag path
               => _e.FullName.StartsWith(_fragPath)
               // only has fragment path and final name with a separator
               && string.Equals(string.Concat(_fragPath, @"/", _e.Name), _e.FullName)))
            {
                if (!_Fragments.TryGetValue(_fRef.Name, out var _frag))
                {
                    _frag = new MetaModelFragment(this, _fRef.Name);
                    _Fragments.Add(_fRef.Name, _frag);
                }
                _frag.ReloadPart(archive, _fragPath);
            }
        }

        #region ICorePartNameManager Members

        public override bool CanUseName(string name)
        {
            var _name = name.ToSafeString();
            return (_Fragments != null) && !_Fragments.ContainsKey(_name);

            // TODO: ???
            //return base.CanUseName(name);
        }

        public override void Rename(string oldName, string newName)
        {
            if (_Fragments != null)
            {
                var _frag = _Fragments[oldName];
                if (_frag != null)
                {
                    _Fragments.Remove(oldName);
                    _Fragments.Add(newName, _frag);
                }
            }

            // TODO: ???
            // base.Rename(oldName, newName);
        }

        #endregion

        //#region public static IEnumerable<MetaModel> GetMetaModelResources(ICorePartNameManager manager, PackagePart part, IResolveBitmapImage bParent)
        ///// <summary>Pre-loads meta-models from related package parts for IResolveModel3D</summary>
        ///// <param name="part">part with (possible) MetaModel relations</param>
        //public static IEnumerable<MetaModel> GetMetaModelResources(IRetrievablePartNameManager manager, PackagePart part, IResolveBitmapImage bParent, IResolveFragment fParent)
        //{
        //    foreach (var _imgRel in part.GetRelationshipsByType(MetaModel.MetaModelRelation))
        //    {
        //        var _imgPart = part.Package.GetPart(_imgRel.TargetUri);
        //        yield return new MetaModel(manager, _imgPart, _imgRel.Id);
        //    }
        //}
        //#endregion
    }
}
