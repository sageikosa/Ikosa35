using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.IO;
using Uzi.Visualize.Contracts;
using Uzi.Visualize.Packaging;
using System;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class MetaModel3DViewModel : Model3DViewModel, IResolveMaterial, IResolveFragment
    {
        public MetaModel3DViewModel(MetaModel3DInfo info, LocalMapInfo map)
            : base(info, map)
        {
        }

        private Dictionary<string, MetaModelFragmentInfo> _FragInfos = new Dictionary<string, MetaModelFragmentInfo>();

        public MetaModel3DInfo MetaModel3DInfo { get { return Info as MetaModel3DInfo; } }

        #region private MetaModelFragmentInfo GetFragInfo(string name)
        private MetaModelFragmentInfo GetFragInfo(string name)
        {
            if (_FragInfos == null)
                _FragInfos = new Dictionary<string, MetaModelFragmentInfo>();

            if (!_FragInfos.ContainsKey(name))
            {
                var _frag = LocalMap.MyProxy.Service.GetModel3DFragmentForModel(MetaModel3DInfo.Name, name);
                if (_frag != null)
                {
                    _FragInfos.Add(name, _frag);
                    return _frag;
                }
            }
            else
                return _FragInfos[name];

            return null;
        }
        #endregion

        #region public override Model3D ResolveModel()
        public override Model3D ResolveModel()
        {
            try
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
                    var _entry = _ModelCache
                        .FirstOrDefault(_ce => _ce.Item1.Equals(_selector));
                    if (_entry != null)
                        return _entry.Item2;
                }
                else
                {
                    _selector = new ModelCacheSelector(null, ExternalVal.Values);
                }

                MetaModelResolutionStack.Clear(MetaModel3DInfo.State);
                MetaModelResolutionStack.Push(MetaModel3DInfo.State.RootFragment);

                // NOTE: different and additional resolution of references
                VisualEffectMaterial.PushResolver(this);
                IkosaImageSource.PushResolver(this);
                FragmentReference.PushResolver(this);
                if (_SETypes == null)
                {
                    // referenced sense extensions
                    _SETypes = new List<Type>();
                    SenseEffectExtension.ReferencedEffect = (type) => { if (!_SETypes.Contains(type)) _SETypes.Add(type); };
                }

                using (var _memStream = new MemoryStream(MetaModel3DInfo.Bytes))
                {
                    var _mdl =  XamlReader.Load(_memStream) as Model3D;
                    if (_ModelCache
                        .FirstOrDefault(_ce => _ce.Item1.Equals(_selector)) == null)
                    {
                        _mdl.Freeze();
                        _ModelCache.Add(new Tuple<ModelCacheSelector, Model3D>(_selector.GetCacheKey(), _mdl));
                    }
                    return _mdl;
                }
            }
            finally
            {
                // cleanup thread tracking
                SenseEffectExtension.ReferencedEffect = null;
                VisualEffectMaterial.PullResolver(this);
                IkosaImageSource.PullResolver(this);
                FragmentReference.PullResolver(this);
                MetaModelResolutionStack.Clear(null);
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
                    // try to resolve the material from out private set
                    var _material = this.Brushes.GetMaterial(_brushNode.BrushKey, effect);
                    if (_material != null)
                        return _material;

                    // use the resolver, only if the brush key wouldn't circle back to reference key resolution
                    if (!_current.Brushes.Any(_b => _b.ReferenceKey == _brushNode.BrushKey)
                        && !MetaModel3DInfo.State.DefaultBrushes.Any(_b => _b.ReferenceKey == _brushNode.BrushKey))
                        return VisualEffectMaterial.ResolveMaterial(_brushNode.BrushKey, effect);
                }
            }

            // get default mapped brush (if any)
            var _defBrush = MetaModel3DInfo.State.DefaultBrushes
                .FirstOrDefault(_bn => (_bn.ReferenceKey == key.ToString()) && !string.IsNullOrEmpty(_bn.BrushKey));
            if (_defBrush != null)
            {
                // try to resolve the material from out private set
                var _material = this.Brushes.GetMaterial(_defBrush.BrushKey, effect);
                if (_material != null)
                    return _material;

                // use the resolver, only if the brush key wouldn't circle back to reference key resolution
                if (!MetaModel3DInfo.State.DefaultBrushes.Any(_b => _b.ReferenceKey == _defBrush.BrushKey))
                    return VisualEffectMaterial.ResolveMaterial(_defBrush.BrushKey, effect);
            }

            // no mapping defined
            return null;
        }

        public IResolveMaterial IResolveMaterialParent { get { return LocalMap; } }

        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes { get { return Brushes.ResolvableBrushes; } }

        #endregion

        #region IResolveFragment Members

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
        {
            var _group = new Model3DGroup();

            if (MetaModel3DInfo.Fragments.Any(_f => _f.Equals(node.FragmentKey)))
            {
                var _fragPart = GetFragInfo(node.FragmentKey);
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

        public IResolveFragment IResolveFragmentParent { get { return LocalMap; } }

        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments { get { yield break; } }

        #endregion
    }
}