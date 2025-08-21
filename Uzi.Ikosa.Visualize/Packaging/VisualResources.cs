using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Packaging;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Media;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize.Packaging
{
    /// <summary>
    /// Resolvable resources
    /// </summary>
    public class VisualResources : BasePart, ICorePartNameManager, IHideCorePackagePartsFolder, IResolveIcon,
        IResolveBitmapImage, IResolveMaterial, IResolveFragment, IResolveModel3D, IResolveBrushCollection
    {
        /// <summary>Relationship type to identify a ResourceManager (http://pack.guildsmanship.com/resourcemanager)</summary>
        public const string VisualResourcesRelation = @"http://pack.guildsmanship.com/resourcemanager";

        #region construction
        /// <summary>Bind to an existing part</summary>
        public VisualResources(ICorePartNameManager manager, PackagePart part, string name)
            : base(manager, part, name)
        {
            // Load references
            using (var _rStream = Part.GetStream(FileMode.Open, FileAccess.Read))
            {
                IFormatter _fmt = new BinaryFormatter();
                _References = (ResourceReferenceManager)_fmt.Deserialize(_rStream);
            }

            CorePackagePartsFolder _getFolder(string relID)
            {
                if (part.RelationshipExists(relID))
                {
                    var _imgRel = part.GetRelationship(relID);
                    return this.GetBasePart(_imgRel) as CorePackagePartsFolder
                        ?? new CorePackagePartsFolder(this, relID, true);
                }
                return new CorePackagePartsFolder(this, relID, true);
            }

            // images
            _Images = _getFolder(@"Images");
            _ImgResolver = new ICorePartImageResolver(_Images);
            _ImgFolder = new PartsFolder(this, @"Images", _Images.Relationships, typeof(BitmapImagePart));

            // brushes
            if (part.RelationshipExists(@"Brushes"))
            {
                var _brushRel = part.GetRelationship(@"Brushes");
                _Brushes = this.GetBasePart(_brushRel) as BrushCollectionPart
                    ?? new BrushCollectionPart(this, @"Brushes");
            }
            else
            {
                _Brushes = new BrushCollectionPart(this, @"Brushes");
            }

            // brushsets
            _BrushSets = _getFolder(@"BrushSets");
            _BrushSetResolver = new ICorePartBrushCollectionResolver(_BrushSets);
            _BrsFolder = new PartsFolder(this, @"Brush Sets", _BrushSets.Relationships, typeof(BrushCollectionPart));

            // fragments
            _Fragments = _getFolder(@"Fragments");
            _FragResolver = new ICorePartMetaModelFragmentResolver(_Fragments);
            _FrgFolder = new PartsFolder(this, @"Fragments", _Fragments.Relationships, typeof(MetaModelFragment));

            // models
            _Models = _getFolder(@"Models");
            _MdlResolver = new ICorePartModel3DResolver(_Models);
            _MdlFolder = new PartsFolder(this, @"Models", _Models.Relationships, typeof(Model3DPart));

            // icons
            _Icons = _getFolder(@"Icons");
            _IconResolver = new ICorePartIconResolver(_Icons);
            _IcoFolder = new PartsFolder(this, @"Icons", _Icons.Relationships, typeof(IconPart)) { HideTree = true };

        }

        /// <summary>Create a new manager</summary>
        public VisualResources(ICorePartNameManager manager, string name)
            : base(manager, name)
        {
            _References = new ResourceReferenceManager();
            _Images = new CorePackagePartsFolder(this, @"Images", true);
            _Icons = new CorePackagePartsFolder(this, @"Icons", true);
            _Models = new CorePackagePartsFolder(this, @"Models", true);
            _Fragments = new CorePackagePartsFolder(this, @"Fragments", true);
            _BrushSets = new CorePackagePartsFolder(this, @"BrushSets", true);
            _Brushes = new BrushCollectionPart(this, @"Brushes");

            // reconnect resolvers
            _ImgResolver = new ICorePartImageResolver(_Images);
            _MdlResolver = new ICorePartModel3DResolver(_Models);
            _FragResolver = new ICorePartMetaModelFragmentResolver(_Fragments);
            _BrushSetResolver = new ICorePartBrushCollectionResolver(_BrushSets);
            _IconResolver = new ICorePartIconResolver(_Icons);

            // reconnect folders
            _ImgFolder = new PartsFolder(this, @"Images", _Images.Relationships, typeof(BitmapImagePart));
            _IcoFolder = new PartsFolder(this, @"Icons", _Icons.Relationships, typeof(IconPart));
            _FrgFolder = new PartsFolder(this, @"Fragments", _Fragments.Relationships, typeof(MetaModelFragment));
            _MdlFolder = new PartsFolder(this, @"Models", _Models.Relationships, typeof(Model3DPart));
            _BrsFolder = new PartsFolder(this, @"Brush Sets", _BrushSets.Relationships, typeof(BrushCollectionPart));
        }
        #endregion

        #region state
        private readonly ResourceReferenceManager _References;
        private readonly CorePackagePartsFolder _Images;
        private readonly CorePackagePartsFolder _Models;
        private readonly CorePackagePartsFolder _Fragments;
        private readonly CorePackagePartsFolder _BrushSets;
        private readonly CorePackagePartsFolder _Icons;
        private readonly BrushCollectionPart _Brushes;
        // resolvers
        private readonly ICorePartImageResolver _ImgResolver;
        private readonly ICorePartModel3DResolver _MdlResolver;
        private readonly ICorePartMetaModelFragmentResolver _FragResolver;
        private readonly ICorePartIconResolver _IconResolver;
        private readonly ICorePartBrushCollectionResolver _BrushSetResolver;
        // folders
        private readonly PartsFolder _ImgFolder;
        private readonly PartsFolder _FrgFolder;
        private readonly PartsFolder _MdlFolder;
        private readonly PartsFolder _BrsFolder;
        private readonly PartsFolder _IcoFolder;
        #endregion

        public BrushCollectionPart Brushes => _Brushes;

        #region public override IEnumerable<ICorePart> Relationships
        public override IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return _ImgFolder;
                yield return _IcoFolder;
                yield return _FrgFolder;
                yield return _MdlFolder;
                yield return _BrsFolder;
                yield return Brushes;
                yield return _References;
                yield break;
            }
        }
        #endregion

        public override string TypeName => typeof(VisualResources).FullName;

        #region public override void Save(Package parent)
        public override void Save(Package parent)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), $@"{Name}.resources");
            var _content = UriHelper.ConcatRelative(_folder, @"manager.references");
            _Part = parent.CreatePart(_content, @"ikosa/resourcereferencess", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, VisualResourcesRelation, Name);

            DoSave(_folder);
        }
        #endregion

        #region public override void Save(PackagePart parent, Uri baseUri)
        public override void Save(PackagePart parent, Uri baseUri)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(baseUri, $@"{Name}.resources");
            var _content = UriHelper.ConcatRelative(_folder, @"manager.references");
            _Part = parent.Package.CreatePart(_content, @"ikosa/resourcereferencess", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, VisualResourcesRelation, Name);

            DoSave(_folder);
        }
        #endregion

        #region private void DoSave(Uri _base)
        private void DoSave(Uri _base)
        {
            // references
            using (var _refStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                IFormatter _fmt = new BinaryFormatter();
                _fmt.Serialize(_refStream, _References);
            }

            _Images.Save(_Part, _base);
            _Icons.Save(_Part, _base);
            _Models.Save(_Part, _base);
            _Fragments.Save(_Part, _base);
            _BrushSets.Save(_Part, _base);
            _Brushes.Save(_Part, _base);
        }
        #endregion

        #region protected override void OnRefreshPart()
        protected override void OnRefreshPart()
        {
            if (Part != null)
            {
                // images
                var _imgRel = Part.GetRelationship(@"Images");
                _Images.RefreshPart(Part.Package.GetPart(_imgRel.TargetUri));

                // brushes
                var _brushRel = Part.GetRelationship(@"Brushes");
                _Brushes.RefreshPart(Part.Package.GetPart(_brushRel.TargetUri));

                // brushsets
                var _brushSetRel = Part.GetRelationship(@"BrushSets");
                _BrushSets.RefreshPart(Part.Package.GetPart(_brushSetRel.TargetUri));

                // fragments
                var _fragRel = Part.GetRelationship(@"Fragments");
                _Fragments.RefreshPart(Part.Package.GetPart(_fragRel.TargetUri));

                // models
                var _mdlRel = Part.GetRelationship(@"Models");
                _Models.RefreshPart(Part.Package.GetPart(_mdlRel.TargetUri));

                // icons
                var _icoRel = Part.GetRelationship(@"Icons");
                _Icons.RefreshPart(Part.Package.GetPart(_icoRel.TargetUri));

            }
        }
        #endregion

        #region ICorePartNameManager Members

        #region public bool CanUseName(string name, Type partType)
        public bool CanUseName(string name, Type partType)
        {
            if (typeof(BitmapImagePart).IsAssignableFrom(partType))
            {
                return _Images.CanUseName(name, partType);
            }
            else if (typeof(IconPart).IsAssignableFrom(partType))
            {
                return _Icons.CanUseName(name, partType);
            }
            else if (typeof(BrushCollectionPart).IsAssignableFrom(partType))
            {
                return _Brushes.CanUseName(name, partType);
            }
            else if (typeof(MetaModelFragment).IsAssignableFrom(partType))
            {
                return _Fragments.CanUseName(name, partType);
            }
            else if (typeof(Model3DPart).IsAssignableFrom(partType))
            {
                return _Models.CanUseName(name, partType);
            }

            // unknown type
            return false;
        }
        #endregion

        public void Rename(string oldName, string newName, Type partType)
        {
            // NOTE: not indexed by name
            //if (typeof(BitmapImagePart).IsAssignableFrom(partType))
            //{
            //}
            //else if (typeof(BrushCollectionPart).IsAssignableFrom(partType))
            //{
            //}
            //else if (typeof(MetaModelFragment).IsAssignableFrom(partType))
            //{
            //}
            //else if (typeof(Model3DPart).IsAssignableFrom(partType))
            //{
            //}
        }

        #endregion

        #region AddPart/RemovePart
        public void AddPart(BitmapImagePart part)
        {
            _Images.Add(part);
            _ImgFolder.ContentsChanged();
        }

        public void RemovePart(BitmapImagePart part)
        {
            _Images.Remove(part);
            _ImgFolder.ContentsChanged();
        }
        #endregion

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect)
            => _ImgResolver.GetImage(key, effect);

        public IGetImageByEffect GetIGetImageByEffect(object key)
            => _ImgResolver.GetIGetImageByEffect(key);

        public IResolveBitmapImage IResolveBitmapImageParent => _References;

        public IEnumerable<BitmapImagePartListItem> ResolvableImages
        {
            get
            {
                // self
                var _img = _ImgResolver.ResolvableImages;

                // references
                _img = _img.Union(IResolveBitmapImageParent.ResolvableImages
                    .Select(_i => new BitmapImagePartListItem
                    {
                        IsLocal = false,
                        BitmapImagePart = _i.BitmapImagePart
                    }));
                return _img;
            }
        }

        #endregion

        #region IResolveMaterial Members

        public Material GetMaterial(object key, VisualEffect effect)
            => Brushes.GetMaterial(key, effect);

        public IResolveMaterial IResolveMaterialParent => _References;

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

                // references
                _brsh = _brsh.Union(IResolveMaterialParent.ResolvableBrushes
                    .Select(_b => new BrushDefinitionListItem
                    {
                        IsLocal = false,
                        BrushDefinition = _b.BrushDefinition
                    }));
                return _brsh;
            }
        }

        #endregion

        #region AddPart/RemovePart
        public void AddPart(MetaModelFragment part)
        {
            _Fragments.Add(part);
            _FrgFolder.ContentsChanged();
        }

        public void RemovePart(MetaModelFragment part)
        {
            _Fragments.Remove(part);
            _FrgFolder.ContentsChanged();
        }
        #endregion

        #region IResolveFragment Members

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
            => _FragResolver.GetFragment(fragRef, node);

        public IResolveFragment IResolveFragmentParent => _References;

        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments
        {
            get
            {
                // self
                var _frg = _FragResolver.ResolvableFragments;

                // references
                _frg = _frg.Union(IResolveFragmentParent.ResolvableFragments
                    .Select(_f => new MetaModelFragmentListItem
                    {
                        IsLocal = false,
                        MetaModelFragment = _f.MetaModelFragment
                    }));
                return _frg;
            }
        }

        #endregion

        #region AddPart/RemovePart
        public void AddPart(Model3DPart part)
        {
            _Models.Add(part);
            _MdlFolder.ContentsChanged();
        }

        public void RemovePart(Model3DPart part)
        {
            _Models.Remove(part);
            _MdlFolder.ContentsChanged();
        }
        #endregion

        #region IResolveModel3D Members

        public Model3D GetPrivateModel3D(object key)
            => _MdlResolver.GetPrivateModel3D(key);

        public bool CanResolveModel3D(object key)
            => _MdlResolver.CanResolveModel3D(key);

        public IResolveModel3D IResolveModel3DParent => _References;

        public IEnumerable<Model3DPartListItem> ResolvableModels
        {
            get
            {
                // self
                var _mdls = _MdlResolver.ResolvableModels;

                // references
                _mdls = _mdls.Union(IResolveModel3DParent.ResolvableModels
                    .OrderBy(_m => _m.Model3DPart.Name)
                    .Select(_m => new Model3DPartListItem
                    {
                        IsLocal = false,
                        Model3DPart = _m.Model3DPart
                    }));
                return _mdls;
            }
        }

        #endregion

        #region AddPart/RemovePart
        public void AddPart(BrushCollectionPart part)
        {
            _BrushSets.Add(part);
            _BrsFolder.ContentsChanged();
        }

        public void RemovePart(BrushCollectionPart part)
        {
            _BrushSets.Remove(part);
            _BrsFolder.ContentsChanged();
        }
        #endregion

        #region IResolveBrushCollection Members

        public BrushCollection GetBrushCollection(object key)
            => _BrushSetResolver.GetBrushCollection(key);

        public IResolveBrushCollection IResolveBrushCollectionParent => _References;

        public IEnumerable<BrushCollectionListItem> ResolvableBrushCollections
        {
            get
            {
                // self
                var _brushes = _BrushSetResolver.ResolvableBrushCollections;

                // references
                _brushes = _brushes.Union(IResolveBrushCollectionParent.ResolvableBrushCollections
                    .Select(_b => new BrushCollectionListItem
                    {
                        IsLocal = false,
                        BrushCollectionPart = _b.BrushCollectionPart
                    }));
                return _brushes;
            }
        }

        #endregion

        // IHideCorePackagePartsFolder
        public bool ShouldHide(string id)
            => true;

        #region AddPart/RemovePart
        public void AddPart(IconPart part)
        {
            _Icons.Add(part);
            _IcoFolder.ContentsChanged();
        }

        public void RemovePart(IconPart part)
        {
            _Icons.Remove(part);
            _IcoFolder.ContentsChanged();
        }
        #endregion

        #region IResolveIcon Members

        public Visual GetIconVisual(string key, IIconReference iconRef)
            => _IconResolver.GetIconVisual(key, iconRef);

        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
            => _IconResolver.GetIconMaterial(key, iconRef, detailLevel);

        public IResolveIcon IResolveIconParent => _References;

        public IEnumerable<IconPartListItem> ResolvableIcons
        {
            get
            {
                // self
                var _ico = _IconResolver.ResolvableIcons;

                // references
                _ico = _ico.Union(IResolveIconParent.ResolvableIcons
                    .Select(_i => new IconPartListItem
                    {
                        IsLocal = false,
                        IconPart = _i.IconPart
                    }));
                return _ico;
            }
        }

        #endregion

        public override void Close()
        {
            _References?.Close();
            _Images?.Close();
            _Brushes?.Close();
            _BrushSets?.Close();
            _Fragments?.Close();
            _Models?.Close();
            _Icons?.Close();
        }
    }
}
