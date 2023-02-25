using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.IO;
using System.IO.Packaging;
using System.Runtime.Serialization;
using Ikosa.Packaging;
using System.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize.IkosaPackaging
{
    [Serializable]
    public class ResourceReferenceManager :
        IResolveBitmapImage, IResolveModel3D, IResolveFragment, IResolveMaterial, IResolveBrushCollection,
        IDeserializationCallback, ICorePart, INotifyPropertyChanged, IResolveIcon
    {
        public ResourceReferenceManager()
        {
        }

        #region non serialized processing state
        [NonSerialized, JsonIgnore]
        private Dictionary<string, Tuple<CorePackage, Dictionary<string, IBasePart>>> _Cache = new Dictionary<string, Tuple<CorePackage, Dictionary<string, IBasePart>>>();
        [NonSerialized, JsonIgnore]
        private ResourceModel3DResolver _ResolveModels = new ResourceModel3DResolver(null);
        [NonSerialized, JsonIgnore]
        private ResourceBitmapImageResolver _ResolveImages = new ResourceBitmapImageResolver(null);
        [NonSerialized, JsonIgnore]
        private ResourceIconResolver _ResolveIcons = new ResourceIconResolver(null);
        [NonSerialized, JsonIgnore]
        private ResourceMetaModelFragmentResolver _ResolveFragments = new ResourceMetaModelFragmentResolver(null);
        [NonSerialized, JsonIgnore]
        private ResourceBrushSetResolver _ResolveBrushSets = new ResourceBrushSetResolver(null);
        [NonSerialized, JsonIgnore]
        private ResourceBrushesResolver _ResolveBrushes = new ResourceBrushesResolver(null);
        #endregion

        #region state
        private readonly List<ImagesResourceReference> _ImageResRefs = new List<ImagesResourceReference>();
        private readonly List<ModelsResourceReference> _ModelResRefs = new List<ModelsResourceReference>();
        private readonly List<FragmentsResourceReference> _FragResRefs = new List<FragmentsResourceReference>();
        private readonly List<BrushSetsResourceReference> _BrushSetResRefs = new List<BrushSetsResourceReference>();
        private readonly List<BrushesResourceReference> _BrushResRefs = new List<BrushesResourceReference>();
        private List<IconsResourceReference> _IconResRefs = new List<IconsResourceReference>();
        #endregion

        #region internal Tuple<CorePackage, Dictionary<string, IBasePart>> GetPackage(ResourceReference reference)
        internal Tuple<CorePackage, Dictionary<string, IBasePart>> GetPackage(ResourceReference reference)
        {
            var _key = reference.FileName;
            if (_Cache.ContainsKey(_key))
            {
                // package already tracked
                return _Cache[_key];
            }
            else if (!string.IsNullOrEmpty(_key))
            {
                try
                {
                    var _fInfo = new FileInfo(_key);
                    var _pck = new CorePackage(_fInfo, Package.Open(_key, FileMode.Open, FileAccess.Read, FileShare.Read));
                    var _tuple = new Tuple<CorePackage, Dictionary<string, IBasePart>>(_pck, new Dictionary<string, IBasePart>());
                    _Cache.Add(_key, _tuple);
                    return _tuple;
                }
                catch
                {
                    // unable to get package
                    return new Tuple<CorePackage, Dictionary<string, IBasePart>>(null, null);
                }
            }

            // nothing to work with
            return new Tuple<CorePackage, Dictionary<string, IBasePart>>(null, null);
        }
        #endregion

        #region private void CleanupCache()
        private void CleanupCache()
        {
            // all parts in cache that are not referenced anymore
            var _clean = (from _c in _Cache
                          from _kvp in _c.Value.Item2
                          let _part = _kvp.Value
                          where !_ImageResRefs.Any(_ref => _ref.Part == _part)
                          && !_ModelResRefs.Any(_ref => _ref.Part == _part)
                          && !_FragResRefs.Any(_ref => _ref.Part == _part)
                          && !_BrushSetResRefs.Any(_ref => _ref.Part == _part)
                          && !_BrushResRefs.Any(_ref => _ref.Part == _part)
                          && !_IconResRefs.Any(_ref => _ref.Part == _part)
                          select new { _kvp.Key, Dictionary = _c.Value.Item2 }).ToList();
            foreach (var _rmv in _clean)
                _rmv.Dictionary.Remove(_rmv.Key);

            // all cache entries that no longer have parts
            var _recover = (from _c in _Cache
                            where !_c.Value.Item2.Any()
                            select _c).ToList();
            foreach (var _del in _recover)
            {
                _del.Value.Item1.Close();
                _Cache.Remove(_del.Key);
            }
        }
        #endregion

        // bitmap Images

        #region private IResolveBitmapImage GetIResolveBitmapImage(ImagesResourceReference reference)
        private IResolveBitmapImage GetIResolveBitmapImage(ImagesResourceReference reference)
        {
            var _tuple = GetPackage(reference);
            if (_tuple.Item2 != null)
            {
                if (_tuple.Item2.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _tuple.Item2[reference.InternalPath] as ResourceManager;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_tuple.Item1.FindBasePart(reference.InternalPath) is ResourceManager _part)
                    {
                        // found, so add ...
                        _tuple.Item2.Add(reference.InternalPath, _part);
                        reference.Resolver = _part;

                        // ... and supply
                        return reference.Resolver;
                    }
                }
            }

            // no good
            reference.Resolver = null;
            return null;
        }
        #endregion

        #region internal void RefreshImageResolver()
        internal void RefreshImageResolver()
        {
            var _resolvers = from _rsrc in _ImageResRefs
                             let _part = GetIResolveBitmapImage(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveImages = new ResourceBitmapImageResolver(_resolvers);
            CleanupCache();
            DoPropertyChanged(@"ImageResourceReferences");
            DoPropertyChanged(@"Relationships");
        }
        #endregion

        #region public void AddImageResourceReference(ImagesResourceReference imageResource)
        public void AddImageResourceReference(ImagesResourceReference imageResource)
        {
            if (!_ImageResRefs.Any(_ir => _ir.Name.Equals(imageResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _ImageResRefs.Add(imageResource);
                RefreshImageResolver();
            }
        }
        #endregion

        #region public void InsertImageResourceReference(int index, ImagesResourceReference imageResource)
        public void InsertImageResourceReference(int index, ImagesResourceReference imageResource)
        {
            if (!_ImageResRefs.Any(_ir => _ir.Name.Equals(imageResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _ImageResRefs.Insert(index, imageResource);
                RefreshImageResolver();
            }
        }
        #endregion

        #region public void RemoveImageResourceReference(ImagesResourceReference imageResource)
        public void RemoveImageResourceReference(ImagesResourceReference imageResource)
        {
            var _imgRsrc = _ImageResRefs.FirstOrDefault(_ir => _ir.Name.Equals(imageResource.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_imgRsrc != null)
            {
                // remove referebce
                _ImageResRefs.Remove(_imgRsrc);
                RefreshImageResolver();
            }
        }
        #endregion

        public int GetIndex(ImagesResourceReference resourceReference)
            => _ImageResRefs.IndexOf(resourceReference);

        public int ImageResourceReferenceCount => _ImageResRefs.Count;

        public IEnumerable<ImagesResourceReference> ImageResourceReferences
            => _ImageResRefs.Select(_i => _i);

        // models

        #region private IResolveModel3D GetIResolveModel3D(ModelsResourceReference reference)
        private IResolveModel3D GetIResolveModel3D(ModelsResourceReference reference)
        {
            var _tuple = GetPackage(reference);
            if (_tuple.Item2 != null)
            {
                if (_tuple.Item2.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _tuple.Item2[reference.InternalPath] as ResourceManager;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_tuple.Item1.FindBasePart(reference.InternalPath) is ResourceManager _part)
                    {
                        // found, so add ...
                        _tuple.Item2.Add(reference.InternalPath, _part);
                        reference.Resolver = _part;

                        // ... and supply
                        return reference.Resolver;
                    }
                }
            }

            // no good
            reference.Resolver = null;
            return null;
        }
        #endregion

        #region internal void RefreshModelResolver()
        internal void RefreshModelResolver()
        {
            var _resolvers = from _rsrc in _ModelResRefs
                             let _part = GetIResolveModel3D(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveModels = new ResourceModel3DResolver(_resolvers);
            CleanupCache();
            DoPropertyChanged(@"ModelResourceReferences");
            DoPropertyChanged(@"Relationships");
        }
        #endregion

        #region public void AddModelResourceReference(ModelsResourceReference modelResource)
        public void AddModelResourceReference(ModelsResourceReference modelResource)
        {
            if (!_ModelResRefs.Any(_ir => _ir.Name.Equals(modelResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _ModelResRefs.Add(modelResource);
                RefreshModelResolver();
            }
        }
        #endregion

        #region public void InsertModelResourceReference(int index, ModelsResourceReference modelResource)
        public void InsertModelResourceReference(int index, ModelsResourceReference modelResource)
        {
            if (!_ModelResRefs.Any(_ir => _ir.Name.Equals(modelResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _ModelResRefs.Insert(index, modelResource);
                RefreshModelResolver();
            }
        }
        #endregion

        #region public void RemoveModelResourceReference(ModelsResourceReference modelResource)
        public void RemoveModelResourceReference(ModelsResourceReference modelResource)
        {
            var _mdlRsrc = _ModelResRefs.FirstOrDefault(_ir => _ir.Name.Equals(modelResource.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_mdlRsrc != null)
            {
                // remove referebce
                _ModelResRefs.Remove(_mdlRsrc);
                RefreshModelResolver();
            }
        }
        #endregion

        public int GetIndex(ModelsResourceReference resourceReference)
            => _ModelResRefs.IndexOf(resourceReference);

        public int ModelResourceReferenceCount => _ModelResRefs.Count;

        public IEnumerable<ModelsResourceReference> ModelResourceReferences
            => _ModelResRefs.Select(_m => _m);

        // fragments

        #region private IResolveFragment GetIResolveFragment(FragmentsResourceReference reference)
        private IResolveFragment GetIResolveFragment(FragmentsResourceReference reference)
        {
            var _tuple = GetPackage(reference);
            if (_tuple.Item2 != null)
            {
                if (_tuple.Item2.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _tuple.Item2[reference.InternalPath] as ResourceManager;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_tuple.Item1.FindBasePart(reference.InternalPath) is ResourceManager _part)
                    {
                        // found, so add ...
                        _tuple.Item2.Add(reference.InternalPath, _part);
                        reference.Resolver = _part;

                        // ... and supply
                        return reference.Resolver;
                    }
                }
            }

            // no good
            reference.Resolver = null;
            return null;
        }
        #endregion

        #region internal void RefreshFragmentResolver()
        internal void RefreshFragmentResolver()
        {
            var _resolvers = from _rsrc in _FragResRefs
                             let _part = GetIResolveFragment(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveFragments = new ResourceMetaModelFragmentResolver(_resolvers);
            CleanupCache();
            DoPropertyChanged(@"FragmentResourceReferences");
            DoPropertyChanged(@"Relationships");
        }
        #endregion

        #region public void AddFragmentResourceReference(FragmentsResourceReference fragmentResource)
        public void AddFragmentResourceReference(FragmentsResourceReference fragmentResource)
        {
            if (!_FragResRefs.Any(_fr => _fr.Name.Equals(fragmentResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _FragResRefs.Add(fragmentResource);
                RefreshFragmentResolver();
            }
        }
        #endregion

        #region public void InsertFragmentResourceReference(int index, FragmentsResourceReference fragmentResource)
        public void InsertFragmentResourceReference(int index, FragmentsResourceReference fragmentResource)
        {
            if (!_FragResRefs.Any(_fr => _fr.Name.Equals(fragmentResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _FragResRefs.Insert(index, fragmentResource);
                RefreshFragmentResolver();
            }
        }
        #endregion

        #region public void RemoveFragmentResourceReference(FragmentsResourceReference fragmentResource)
        public void RemoveFragmentResourceReference(FragmentsResourceReference fragmentResource)
        {
            var _fragRsrc = _FragResRefs.FirstOrDefault(_fr => _fr.Name.Equals(fragmentResource.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_fragRsrc != null)
            {
                // remove reference
                _FragResRefs.Remove(_fragRsrc);
                RefreshFragmentResolver();
            }
        }
        #endregion

        public int GetIndex(FragmentsResourceReference resourceReference)
            => _FragResRefs.IndexOf(resourceReference);

        public int FragmentResourceReferenceCount => _FragResRefs.Count;

        public IEnumerable<FragmentsResourceReference> FragmentResourceReferences
            => _FragResRefs.Select(_f => _f);

        // brush sets

        #region private IResolveBrushCollection GetIResolveBrushCollection(BrushSetsResourceReference reference)
        private IResolveBrushCollection GetIResolveBrushCollection(BrushSetsResourceReference reference)
        {
            var _tuple = GetPackage(reference);
            if (_tuple.Item2 != null)
            {
                if (_tuple.Item2.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _tuple.Item2[reference.InternalPath] as ResourceManager;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_tuple.Item1.FindBasePart(reference.InternalPath) is ResourceManager _part)
                    {
                        // found, so add ...
                        _tuple.Item2.Add(reference.InternalPath, _part);
                        reference.Resolver = _part;

                        // ... and supply
                        return reference.Resolver;
                    }
                }
            }

            // no good
            reference.Resolver = null;
            return null;
        }
        #endregion

        #region internal void RefreshBrushSetResolver()
        internal void RefreshBrushSetResolver()
        {
            var _resolvers = from _rsrc in _BrushSetResRefs
                             let _part = GetIResolveBrushCollection(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveBrushSets = new ResourceBrushSetResolver(_resolvers);
            CleanupCache();
            DoPropertyChanged(@"BrushSetResourceReferences");
            DoPropertyChanged(@"Relationships");
        }
        #endregion

        #region public void AddBrushSetResourceReference(BrushSetsResourceReference brushSetReference)
        public void AddBrushSetResourceReference(BrushSetsResourceReference brushSetReference)
        {
            if (!_BrushSetResRefs.Any(_ir => _ir.Name.Equals(brushSetReference.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _BrushSetResRefs.Add(brushSetReference);
                RefreshBrushSetResolver();
            }
        }
        #endregion

        #region public void InsertBrushSetResourceReference(int index, BrushSetsResourceReference brushSetResource)
        public void InsertBrushSetResourceReference(int index, BrushSetsResourceReference brushSetResource)
        {
            if (!_BrushSetResRefs.Any(_ir => _ir.Name.Equals(brushSetResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _BrushSetResRefs.Insert(index, brushSetResource);
                RefreshBrushSetResolver();
            }
        }
        #endregion

        #region public void RemoveBrushSetResourceReference(BrushSetsResourceReference brushSetResource)
        public void RemoveBrushSetResourceReference(BrushSetsResourceReference brushSetResource)
        {
            var _setRsrc = _BrushSetResRefs.FirstOrDefault(_ir => _ir.Name.Equals(brushSetResource.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_setRsrc != null)
            {
                // remove referebce
                _BrushSetResRefs.Remove(_setRsrc);
                RefreshBrushSetResolver();
            }
        }
        #endregion

        public int GetIndex(BrushSetsResourceReference resourceReference)
            => _BrushSetResRefs.IndexOf(resourceReference);

        public int BrushSetResourceReferenceCount => _BrushSetResRefs.Count;

        public IEnumerable<BrushSetsResourceReference> BrushSetResourceReferences
            => _BrushSetResRefs.Select(_s => _s);

        // brushes

        #region private IResolveMaterial GetIResolveMaterial(BrushesResourceReference reference)
        private IResolveMaterial GetIResolveMaterial(BrushesResourceReference reference)
        {
            var _tuple = GetPackage(reference);
            if (_tuple.Item2 != null)
            {
                if (_tuple.Item2.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _tuple.Item2[reference.InternalPath] as ResourceManager;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_tuple.Item1.FindBasePart(reference.InternalPath) is ResourceManager _part)
                    {
                        // found, so add ...
                        _tuple.Item2.Add(reference.InternalPath, _part);
                        reference.Resolver = _part;

                        // ... and supply
                        return reference.Resolver;
                    }
                }
            }

            // no good
            reference.Resolver = null;
            return null;
        }
        #endregion

        #region internal void RefreshBrushResolver()
        internal void RefreshBrushResolver()
        {
            var _resolvers = from _rsrc in _BrushResRefs
                             let _part = GetIResolveMaterial(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveBrushes = new ResourceBrushesResolver(_resolvers);
            CleanupCache();
            DoPropertyChanged(@"BrushesResourceReferences");
            DoPropertyChanged(@"Relationships");
        }
        #endregion

        #region public void AddBrushResourceReference(BrushesResourceReference brushReference)
        public void AddBrushResourceReference(BrushesResourceReference brushReference)
        {
            if (!_BrushResRefs.Any(_ir => _ir.Name.Equals(brushReference.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _BrushResRefs.Add(brushReference);
                RefreshBrushResolver();
            }
        }
        #endregion

        #region public void InsertBrushResourceReference(int index, BrushesResourceReference brushReference)
        public void InsertBrushResourceReference(int index, BrushesResourceReference brushReference)
        {
            if (!_BrushResRefs.Any(_ir => _ir.Name.Equals(brushReference.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _BrushResRefs.Insert(index, brushReference);
                RefreshBrushResolver();
            }
        }
        #endregion

        #region public void RemoveBrushResourceReference(BrushesResourceReference brushReference)
        public void RemoveBrushResourceReference(BrushesResourceReference brushReference)
        {
            var _setRsrc = _BrushResRefs.FirstOrDefault(_ir => _ir.Name.Equals(brushReference.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_setRsrc != null)
            {
                // remove referebce
                _BrushResRefs.Remove(_setRsrc);
                RefreshBrushResolver();
            }
        }
        #endregion

        public int GetIndex(BrushesResourceReference resourceReference)
            => _BrushResRefs.IndexOf(resourceReference);

        public int BrushResourceReferenceCount => _BrushResRefs.Count;

        public IEnumerable<BrushesResourceReference> BrushesResourceReferences
            => _BrushResRefs.Select(_s => _s);

        // icons

        #region private IResolveIcon GetIResolveIcon(IconsResourceReference reference)
        private IResolveIcon GetIResolveIcon(IconsResourceReference reference)
        {
            var _tuple = GetPackage(reference);
            if (_tuple.Item2 != null)
            {
                if (_tuple.Item2.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _tuple.Item2[reference.InternalPath] as ResourceManager;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_tuple.Item1.FindBasePart(reference.InternalPath) is ResourceManager _part)
                    {
                        // found, so add ...
                        _tuple.Item2.Add(reference.InternalPath, _part);
                        reference.Resolver = _part;

                        // ... and supply
                        return reference.Resolver;
                    }
                }
            }

            // no good
            reference.Resolver = null;
            return null;
        }
        #endregion

        #region internal void RefreshIconResolver()
        internal void RefreshIconResolver()
        {
            if (_IconResRefs == null)
                _IconResRefs = new List<IconsResourceReference>();
            var _resolvers = from _rsrc in _IconResRefs
                             let _part = GetIResolveIcon(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveIcons = new ResourceIconResolver(_resolvers);
            CleanupCache();
            DoPropertyChanged(@"IconResourceReferences");
            DoPropertyChanged(@"Relationships");
        }
        #endregion

        #region public void AddIconResourceReference(IconsResourceReference iconResource)
        public void AddIconResourceReference(IconsResourceReference iconResource)
        {
            if (!_IconResRefs.Any(_ir => _ir.Name.Equals(iconResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _IconResRefs.Add(iconResource);
                RefreshIconResolver();
            }
        }
        #endregion

        #region public void InsertIconResourceReference(int index, IconsResourceReference iconResource)
        public void InsertIconResourceReference(int index, IconsResourceReference iconResource)
        {
            if (!_ImageResRefs.Any(_ir => _ir.Name.Equals(iconResource.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // track reference
                _IconResRefs.Insert(index, iconResource);
                RefreshIconResolver();
            }
        }
        #endregion

        #region public void RemoveIconResourceReference(IconsResourceReference iconResource)
        public void RemoveIconResourceReference(IconsResourceReference iconResource)
        {
            var _iconRsrc = _IconResRefs.FirstOrDefault(_ir => _ir.Name.Equals(iconResource.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_iconRsrc != null)
            {
                // remove referebce
                _IconResRefs.Remove(_iconRsrc);
                RefreshImageResolver();
            }
        }
        #endregion

        public int GetIndex(IconsResourceReference resourceReference)
            => _IconResRefs.IndexOf(resourceReference);

        public int IconResourceReferenceCount => _IconResRefs.Count;

        public IEnumerable<IconsResourceReference> IconResourceReferences
            => _IconResRefs.Select(_i => _i);

        // resolution

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect) { return _ResolveImages.GetImage(key, effect); }
        public IGetImageByEffect GetIGetImageByEffect(object key) { return _ResolveImages.GetIGetImageByEffect(key); }
        public IResolveBitmapImage IResolveBitmapImageParent { get { return _ResolveImages.IResolveBitmapImageParent; } }
        public IEnumerable<BitmapImagePartListItem> ResolvableImages { get { return _ResolveImages.ResolvableImages; } }

        #endregion

        #region IResolveModel3D Members

        public Model3D GetPrivateModel3D(object key) => _ResolveModels.GetPrivateModel3D(key);
        public bool CanResolveModel3D(object key) => _ResolveModels.CanResolveModel3D(key);
        public IResolveModel3D IResolveModel3DParent => _ResolveModels.IResolveModel3DParent;
        public IEnumerable<Model3DPartListItem> ResolvableModels => _ResolveModels.ResolvableModels;

        #endregion

        #region IResolveFragment Members

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node) { return _ResolveFragments.GetFragment(fragRef, node); }
        public IResolveFragment IResolveFragmentParent { get { return _ResolveFragments.IResolveFragmentParent; } }
        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments { get { return _ResolveFragments.ResolvableFragments; } }

        #endregion

        #region IResolveMaterial Members

        public Material GetMaterial(object key, VisualEffect effect) { return _ResolveBrushes.GetMaterial(key, effect); }
        public IResolveMaterial IResolveMaterialParent { get { return _ResolveBrushes.IResolveMaterialParent; } }
        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes { get { return _ResolveBrushes.ResolvableBrushes; } }

        #endregion

        #region IResolveBrushCollection Members

        public BrushCollection GetBrushCollection(object key) { return _ResolveBrushSets.GetBrushCollection(key); }
        public IResolveBrushCollection IResolveBrushCollectionParent { get { return _ResolveBrushSets.IResolveBrushCollectionParent; } }
        public IEnumerable<BrushCollectionListItem> ResolvableBrushCollections { get { return _ResolveBrushSets.ResolvableBrushCollections; } }

        #endregion

        #region IResolveIcon Members

        public Visual GetIconVisual(string key, IIconReference iconRef) 
            => _ResolveIcons.GetIconVisual(key, iconRef);
        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
            => _ResolveIcons.GetIconMaterial(key, iconRef, detailLevel);
        public IResolveIcon IResolveIconParent => _ResolveIcons.IResolveIconParent;
        public IEnumerable<IconPartListItem> ResolvableIcons => _ResolveIcons.ResolvableIcons;

        #endregion

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            _Cache = new Dictionary<string, Tuple<CorePackage, Dictionary<string, IBasePart>>>();
            RefreshImageResolver();
            RefreshIconResolver();
            RefreshBrushResolver();
            RefreshBrushSetResolver();
            RefreshFragmentResolver();
            RefreshModelResolver();
        }

        #endregion

        #region ICorePart Members

        public string Name { get { return @"Resource References"; } }
        public IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return new PartsFolder(this, @"Images Resources", ImageResourceReferences, typeof(ImagesResourceReference));
                yield return new PartsFolder(this, @"Icons Resources", IconResourceReferences, typeof(IconsResourceReference));
                yield return new PartsFolder(this, @"Models Resources", ModelResourceReferences, typeof(ModelsResourceReference));
                yield return new PartsFolder(this, @"Fragments Resources", FragmentResourceReferences, typeof(FragmentsResourceReference));
                yield return new PartsFolder(this, @"Brush Set Resources", BrushSetResourceReferences, typeof(BrushSetsResourceReference));
                yield return new PartsFolder(this, @"Brushes Resources", BrushesResourceReferences, typeof(BrushesResourceReference));
            }
        }
        public string TypeName { get { return typeof(ResourceReferenceManager).FullName; } }

        #endregion

        #region INotifyPropertyChanged Members
        private void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public void Close()
        {
            // release all references
            _ImageResRefs.Clear();
            _ModelResRefs.Clear();
            _FragResRefs.Clear();
            _BrushSetResRefs.Clear();
            _BrushResRefs.Clear();
            _IconResRefs.Clear();

            // cleanup cache
            CleanupCache();
        }
    }
}