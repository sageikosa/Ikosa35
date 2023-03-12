using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Visualize.Packages.References;

namespace Uzi.Visualize.Packages
{
    public class VisualResourcePart : StorablePart, IRetrievablePartNameManager, IPartResolveIcon,
        IPartResolveImage, IPartResolveMaterial, IPartResolveFragment, IPartResolveModel3D, IPartResolveBrushCollection
    {
        public VisualResourcePart(IRetrievablePartNameManager manager, string name)
            : base(manager, name)
        {
            _Resources = new ReferencedResourceManager(this);

            _Images = new ArchivePartsFolder(this, @"Images", true,
                (manager, name, archive, parentPath) => new BitmapImagePart(manager, name));
            _Icons = new ArchivePartsFolder(this, @"Icons", true,
                (manager, name, archive, parentPath) => new IconPart(manager, name));
            _Fragments = new ArchivePartsFolder(this, @"Fragments", true,
                (manager, name, archive, parentPath) => new MetaModelFragment(manager, name));
            _BrushSets = new ArchivePartsFolder(this, @"BrushSets", true,
                (manager, name, archive, parentPath) => new BrushCollectionPart(manager, name));
            _Brushes = new BrushCollectionPart(this, @"Brushes");
            _Models = new ArchivePartsFolder(this, @"Models", true,
                ModelPartFactory.GetModelPart);

            // reconnect folders
            _ImgFolder = new RetrievableFolder(this, @"Images", _Images.Parts, typeof(BitmapImagePart).FullName);
            _IcoFolder = new RetrievableFolder(this, @"Icons", _Icons.Parts, typeof(IconPart).FullName);
            _FrgFolder = new RetrievableFolder(this, @"Fragments", _Fragments.Parts, typeof(MetaModelFragment).FullName);
            _BrsFolder = new RetrievableFolder(this, @"Brush Sets", _BrushSets.Parts, typeof(BrushCollectionPart).FullName);
            _MdlFolder = new RetrievableFolder(this, @"Models", _Models.Parts, typeof(Model3DPart).FullName);
        }

        #region state
        private readonly ArchivePartsFolder _Images;
        private readonly ArchivePartsFolder _Models;
        private readonly ArchivePartsFolder _Fragments;
        private readonly ArchivePartsFolder _BrushSets;
        private readonly ArchivePartsFolder _Icons;
        private readonly BrushCollectionPart _Brushes;
        private readonly ReferencedResourceManager _Resources;

        // folders
        private readonly RetrievableFolder _ImgFolder;
        private readonly RetrievableFolder _FrgFolder;
        private readonly RetrievableFolder _MdlFolder;
        private readonly RetrievableFolder _BrsFolder;
        private readonly RetrievableFolder _IcoFolder;
        #endregion

        public override IEnumerable<IRetrievablePart> Parts
        {
            get
            {
                yield return _ImgFolder;
                yield return _IcoFolder;
                yield return _FrgFolder;
                yield return _BrsFolder;
                yield return _Brushes;
                yield return _MdlFolder;
                yield return _Resources;
                yield break;
            }
        }

        public override string PartType => typeof(VisualResourcePart).FullName;

        public override void ClosePart()
        {
            //_References?.Close();
            _Images?.ClosePart();
            _Brushes?.ClosePart();
            _BrushSets?.ClosePart();
            _Fragments?.ClosePart();
            _Models?.ClosePart();
            _Icons?.ClosePart();
            _Resources.ClosePart();
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            var _parentPath = $@"{parentPath}/{PartName}";
            _Images.ReloadPart(archive, _parentPath);
            _Brushes.ReloadPart(archive, _parentPath);
            _BrushSets.ReloadPart(archive, _parentPath);
            _Fragments.ReloadPart(archive, _parentPath);
            _Models.ReloadPart(archive, _parentPath);
            _Icons.ReloadPart(archive, _parentPath);
            _Resources.ReloadPart(archive, _parentPath);
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            var _parentPath = $@"{parentPath}/{PartName}";
            _Images.StorePart(archive, _parentPath);
            _Brushes.StorePart(archive, _parentPath);
            _BrushSets.StorePart(archive, _parentPath);
            _Fragments.StorePart(archive, _parentPath);
            _Models.StorePart(archive, _parentPath);
            _Icons.StorePart(archive, _parentPath);
            _Resources.StorePart(archive, _parentPath);
        }

        public bool CanUseName(string name) => true;
        public void Rename(string oldName, string newName) { }

        // image resolution

        public IPartResolveImage IPartResolveImageParent => _Resources;

        public BitmapSource GetImage(object key, VisualEffect effect)
            => GetIGetImageByEffect(key)?.GetImage(effect);

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            if (_Images.Count > 0)
            {
                var _key = key.ToString();
                return _Images.Parts.OfType<BitmapImagePart>()
                    .FirstOrDefault(_p => _key.Equals(_p.PartName, StringComparison.OrdinalIgnoreCase));
            }
            return null;
        }

        public IEnumerable<BitmapImagePartListItem> ResolvableImages
            => _Images.Parts.OfType<BitmapImagePart>()
            .Select(_i => new BitmapImagePartListItem
            {
                IsLocal = true,
                BitmapImagePart = _i
            });

        #region AddImage/RemoveImage
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

        // material resolutions

        public IPartResolveMaterial IPartResolveMaterialParent => _Resources;

        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes
            => _Brushes.Parts.OfType<BrushDefinition>()
            .Select(_bd => new BrushDefinitionListItem
            {
                IsLocal = true,
                BrushDefinition = _bd
            });

        public Material GetMaterial(object key, VisualEffect effect)
            => _Brushes.GetMaterial(key, effect);

        public BrushCollectionPart Brushes => _Brushes;

        // fragment resolution

        public IPartResolveFragment IPartResolveFragmentParent => _Resources;

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
        {
            var _group = new Model3DGroup();

            if (_Fragments.Parts.OfType<MetaModelFragment>()
                .FirstOrDefault(_mmf => _mmf.PartName == node.FragmentKey) is MetaModelFragment _fragPart)
            {
                var _mdl = _fragPart.ResolveModel(node);

                // collect and end resolve of fragment
                if (_mdl != null)
                {
                    _group.Children.Add(fragRef.ApplyTransforms(node, _mdl));
                }
            }

            // return smallest possible grouping
            if (_group.Children.Count == 1)
                return _group.Children.First();
            else if (_group.Children.Any())
                return _group;
            return null;
        }

        public IEnumerable<MetaModelFragmentPartListItem> ResolvableFragments
            => _Fragments.Parts.OfType<MetaModelFragment>()
            .Select(_mmf => new MetaModelFragmentPartListItem
            {
                IsLocal = true,
                MetaModelFragment = _mmf
            });

        #region AddFragment/RemoveFragment
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

        // Model3DPart resolution

        public IPartResolveModel3D IPartResolveModel3DParent => _Resources;

        public Model3D GetPrivateModel3D(object key)
        {
            string _key = key.ToString();
            if (_Models.Parts.OfType<Model3DPart>()
                .FirstOrDefault(_mdl => _mdl.PartName.Equals(_key, StringComparison.OrdinalIgnoreCase)) is Model3DPart _mdl)
            {
                return _mdl.ResolveModel();
            }
            return null;
        }

        public bool CanResolveModel3D(object key)
        {
            string _key = key.ToString();
            if (_Models.Parts.OfType<Model3DPart>()
                .Any(_mdl => _mdl.PartName.Equals(_key, StringComparison.OrdinalIgnoreCase)))
                return true;
            return false;
        }

        public IEnumerable<Model3DPartListItem> ResolvableModels
            => _Models.Parts.OfType<Model3DPart>()
            .Select(_mmf => new Model3DPartListItem
            {
                IsLocal = true,
                Model3DPart = _mmf
            });

        #region AddModel3DPart/RemoveModel3DPart
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

        // icon resolution

        public IPartResolveIcon IPartResolveIconParent => _Resources;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => _Icons.Parts.OfType<IconPart>()
            .Select(_i => new IconPartListItem
            {
                IsLocal = true,
                IconPart = _i
            });

        public Visual GetIconVisual(string key, IIconReference iconRef)
        {
            string _key = key.ToString();
            if (_Icons.Parts.OfType<IconPart>()
                .FirstOrDefault(_ip => _ip.PartName.Equals(_key, StringComparison.OrdinalIgnoreCase)) is IconPart _ico)
            {
                return _ico.GetIconContent(iconRef);
            }
            return null;
        }

        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
        {
            string _key = key.ToString();
            if (_Icons.Parts.OfType<IconPart>()
                .FirstOrDefault(_ip => _ip.PartName.Equals(_key, StringComparison.OrdinalIgnoreCase)) is IconPart _ico)
            {
                return _ico.GetIconMaterial(detailLevel, iconRef);
            }
            return null;
        }

        #region AddIconPart/RemoveIconPart
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

        // brush set resolution

        public IPartResolveBrushCollection IPartResolveBrushCollectionParent => _Resources;

        public IEnumerable<BrushCollectionPartListItem> ResolvableBrushCollections
            => _BrushSets.Parts.OfType<BrushCollectionPart>()
            .Select(_bc => new BrushCollectionPartListItem
            {
                IsLocal = true,
                BrushCollectionPart = _bc
            });

        public BrushCollection GetBrushCollection(object key)
        {
            string _key = key.ToString();
            if (_BrushSets.Parts.OfType<BrushCollectionPart>()
                .FirstOrDefault(_bs => _bs.PartName.Equals(_key, StringComparison.OrdinalIgnoreCase)) is BrushCollectionPart _bc)
            {
                return _bc.BrushDefinitions;
            }
            return null;
        }

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
    }
}
