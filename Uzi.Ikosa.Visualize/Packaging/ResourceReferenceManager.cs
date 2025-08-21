using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Runtime.Serialization;
using Uzi.Packaging;
using System.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize.Packaging
{
    [Serializable]
    public class ResourceReferenceManager : INotifyPropertyChanged, ICorePart, IResolveIcon, IListPackagePartReferences,
        IResolveBitmapImage, IResolveModel3D, IResolveFragment, IResolveMaterial, IResolveBrushCollection, IDeserializationCallback
    {
        public ResourceReferenceManager()
        {
            PackageManager.Manager.AddPackagePartReferenceLister(this);
        }

        #region non serialized processing state
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

        public void OnDeserialization(object sender)
        {
            PackageManager.Manager.AddPackagePartReferenceLister(this);
            RefreshModelResolver();
            RefreshImageResolver();
            RefreshIconResolver();
            RefreshFragmentResolver();
            RefreshBrushSetResolver();
            RefreshBrushResolver();
        }

        #region state
        private readonly List<ImagesResourceReference> _ImageResRefs = [];
        private readonly List<ModelsResourceReference> _ModelResRefs = [];
        private readonly List<FragmentsResourceReference> _FragResRefs = [];
        private readonly List<BrushSetsResourceReference> _BrushSetResRefs = [];
        private readonly List<BrushesResourceReference> _BrushResRefs = [];
        private List<IconsResourceReference> _IconResRefs = [];
        #endregion

        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public IEnumerable<IPackagePartReference> AllReferences
            => _ImageResRefs.AsEnumerable<IPackagePartReference>()
            .Concat(_ModelResRefs)
            .Concat(_FragResRefs)
            .Concat(_BrushSetResRefs)
            .Concat(_BrushResRefs)
            .Concat(_IconResRefs);

        // ICorePart Members

        public IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return new PartsFolder(this, @"Images Resources", ImageResourceReferences, typeof(ImagesResourceReference)) { NameSort = false };
                yield return new PartsFolder(this, @"Icons Resources", IconResourceReferences, typeof(IconsResourceReference)) { NameSort = false };
                yield return new PartsFolder(this, @"Models Resources", ModelResourceReferences, typeof(ModelsResourceReference)) { NameSort = false };
                yield return new PartsFolder(this, @"Fragments Resources", FragmentResourceReferences, typeof(FragmentsResourceReference)) { NameSort = false };
                yield return new PartsFolder(this, @"Brush Set Resources", BrushSetResourceReferences, typeof(BrushSetsResourceReference)) { NameSort = false };
                yield return new PartsFolder(this, @"Brushes Resources", BrushesResourceReferences, typeof(BrushesResourceReference)) { NameSort = false };
            }
        }

        public string Name => @"Resource References";
        public string TypeName => typeof(ResourceReferenceManager).FullName;

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
            PackageManager.Manager?.CleanupCache();
            PackageManager.Manager?.RemovePackagePartReferenceLister(this);
        }

        // bitmap Images

        #region private IResolveBitmapImage GetIResolveBitmapImage(ImagesResourceReference reference)
        private IResolveBitmapImage GetIResolveBitmapImage(ImagesResourceReference reference)
        {
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as VisualResources;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is VisualResources _part)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _part);
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
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(ImageResourceReferences));
            DoPropertyChanged(nameof(Relationships));
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
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as VisualResources;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is VisualResources _part)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _part);
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
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(ModelResourceReferences));
            DoPropertyChanged(nameof(Relationships));
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
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as VisualResources;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is VisualResources _part)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _part);
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
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(FragmentResourceReferences));
            DoPropertyChanged(nameof(Relationships));
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
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as VisualResources;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is VisualResources _part)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _part);
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
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(BrushSetResourceReferences));
            DoPropertyChanged(nameof(Relationships));
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
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as VisualResources;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is VisualResources _part)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _part);
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
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(BrushesResourceReferences));
            DoPropertyChanged(nameof(Relationships));
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
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as VisualResources;
                    reference.Resolver = _part;
                    return reference.Resolver;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is VisualResources _part)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _part);
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
            _IconResRefs ??= [];
            var _resolvers = from _rsrc in _IconResRefs
                             let _part = GetIResolveIcon(_rsrc)
                             where (_part != null)
                             select _part;
            _ResolveIcons = new ResourceIconResolver(_resolvers);
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(IconResourceReferences));
            DoPropertyChanged(nameof(Relationships));
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
    }
}
