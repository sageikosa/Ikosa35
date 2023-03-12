using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;
using Newtonsoft.Json;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize.Packages.References
{
    public class ReferencedResourceManager : StorablePart, IListPartReferences, IPartResolveIcon,
        IPartResolveImage, IPartResolveMaterial, IPartResolveFragment, IPartResolveModel3D, IPartResolveBrushCollection
    {
        public ReferencedResourceManager(IRetrievablePartNameManager manager)
            : base(manager, @"Visual Resources")
        {
            _ImageReferences = new();
            _ModelReferences = new();
            _FragmentReferences = new();
            _BrushSetReferences = new();
            _BrushReferences = new();
            _IconReferences = new();

            _ImageResources = new();
            _ModelResources = new();
            _FragmentResources = new();
            _BrushSetResources = new();
            _BrushResources = new();
            _IconResources = new();
            IkosaPackageManager.Manager.AddPackagePartReferenceLister(this);
        }

        private readonly Dictionary<string, PackageReference> _ImageReferences;
        private readonly Dictionary<string, PackageReference> _ModelReferences;
        private readonly Dictionary<string, PackageReference> _FragmentReferences;
        private readonly Dictionary<string, PackageReference> _BrushSetReferences;
        private readonly Dictionary<string, PackageReference> _BrushReferences;
        private readonly Dictionary<string, PackageReference> _IconReferences;

        private readonly Dictionary<string, VisualResourcePart> _ImageResources;
        private readonly Dictionary<string, VisualResourcePart> _ModelResources;
        private readonly Dictionary<string, VisualResourcePart> _FragmentResources;
        private readonly Dictionary<string, VisualResourcePart> _BrushSetResources;
        private readonly Dictionary<string, VisualResourcePart> _BrushResources;
        private readonly Dictionary<string, VisualResourcePart> _IconResources;

        private (Dictionary<string, PackageReference> references, Dictionary<string, VisualResourcePart> resources) GetDictionaries(string resolvableName)
            => resolvableName switch
            {
                nameof(ResolvableImages) => (_ImageReferences, _ImageResources),
                nameof(ResolvableModels) => (_ModelReferences, _ModelResources),
                nameof(ResolvableFragments) => (_FragmentReferences, _FragmentResources),
                nameof(ResolvableBrushCollections) => (_BrushSetReferences, _BrushSetResources),
                nameof(ResolvableBrushes) => (_BrushReferences, _BrushResources),
                _ => (_IconReferences, _IconResources)
            };

        // TODO: manage references: insert, append, re-order, set VisualresourcePart...

        public void SetReference(string resolvableName, PackageReference reference, VisualResourcePart part)
        {
            var _path = reference.PackagePath;
            var (_refs, _rsrcs) = GetDictionaries(resolvableName);
            _refs[reference.ReferenceName] = reference;
            _rsrcs[_path] = part;
            IkosaPackageManager.Manager?.CleanupCache();
        }

        public bool RemoveReference(string resolvableName, PackageReference reference)
        {
            try
            {
                var _path = reference.PackagePath;
                var (_refs, _rsrcs) = GetDictionaries(resolvableName);
                _refs.Remove(_path);
                _rsrcs.Remove(_path);
                return true;
            }
            finally
            {
                IkosaPackageManager.Manager?.CleanupCache();
            }
        }

        // TODO: express dictionaries as data-bindable properties

        // storable parts overrides 

        public override IEnumerable<IRetrievablePart> Parts => Enumerable.Empty<IRetrievablePart>();

        public override string PartType => GetType().FullName;

        public override void ClosePart()
        {
            // release all
            _ImageReferences.Clear();
            _ModelReferences.Clear();
            _FragmentReferences.Clear();
            _BrushSetReferences.Clear();
            _BrushReferences.Clear();
            _IconReferences.Clear();

            _ImageResources.Clear();
            _ModelResources.Clear();
            _FragmentResources.Clear();
            _BrushSetResources.Clear();
            _BrushResources.Clear();
            _IconResources.Clear();

            // cleanup
            IkosaPackageManager.Manager?.CleanupCache();
            IkosaPackageManager.Manager?.RemovePackagePartReferenceLister(this);
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            // prepare stream
            var _path = $@"{parentPath}/references.json";
            var _entry = archive.GetEntry(_path);
            using var _stream = _entry.Open();
            using var _sReader = new StreamReader(_stream);
            using var _jReader = new JsonTextReader(_sReader);

            // deserialize
            var _serializer = IkosaPackageManager.GetJsonSerializer();
            var _packed = _serializer.Deserialize<Dictionary<string, List<PackageReference>>>(_jReader);

            void _resolveResources(string resolvableName)
            {
                var (_refs, _rsrcs) = GetDictionaries(resolvableName);

                // _content dictionary indexed by name
                if (_packed.TryGetValue(resolvableName, out var _content))
                {
                    // clear target
                    _refs.Clear();
                    _rsrcs.Clear();

                    foreach (var _ref in _content)
                    {
                        // resolve
                        var _part = IkosaPackageManager.Manager.GetStorablePart<VisualResourcePart>(_ref);
                        if (_part != null)
                        {
                            SetReference(resolvableName, _ref, _part);
                        }
                    }
                }
            }

            // resolve each set
            _resolveResources(nameof(ResolvableImages));
            _resolveResources(nameof(ResolvableModels));
            _resolveResources(nameof(ResolvableFragments));
            _resolveResources(nameof(ResolvableBrushCollections));
            _resolveResources(nameof(ResolvableBrushes));
            _resolveResources(nameof(ResolvableIcons));

            IkosaPackageManager.Manager.CleanupCache();
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            // format into a single object, stripped of volatile parts
            var _packed = new Dictionary<string, List<PackageReference>>()
            {
                { nameof(ResolvableImages),              _ImageReferences.Select(_kvp => _kvp.Value).ToList() },
                { nameof(ResolvableModels),              _ModelReferences.Select(_kvp => _kvp.Value).ToList() },
                { nameof(ResolvableFragments),        _FragmentReferences.Select(_kvp => _kvp.Value).ToList() },
                { nameof(ResolvableBrushCollections), _BrushSetReferences.Select(_kvp => _kvp.Value).ToList() },
                { nameof(ResolvableBrushes),             _BrushReferences.Select(_kvp => _kvp.Value).ToList() },
                { nameof(ResolvableIcons),                _IconReferences.Select(_kvp => _kvp.Value).ToList() },
            };

            // prepare stream
            var _path = $@"{parentPath}/references.json";
            var _entry = archive.CreateEntry(_path);
            using var _stream = _entry.Open();
            using var _writer = new StreamWriter(_stream);

            // serialize
            var _serializer = IkosaPackageManager.GetJsonSerializer();
            _serializer.Serialize(_writer, _packed);
        }

        // ---------- IListPartReferences

        public IEnumerable<IStorablePart> AllReferences
            => _ImageResources
            .Concat(_ModelResources)
            .Concat(_FragmentResources)
            .Concat(_BrushSetResources)
            .Concat(_BrushResources)
            .Concat(_IconResources)
            .Select(_kvp => _kvp.Value);

        // ---------- IPartResolveImage

        public IPartResolveImage IPartResolveImageParent => null;

        public IEnumerable<BitmapImagePartListItem> ResolvableImages
            => _ImageResources.SelectMany(_ir => _ir.Value.ResolvableImages);

        public BitmapSource GetImage(object key, VisualEffect effect)
            => (from _ir in _ImageResources
                let _img = _ir.Value.GetImage(key, effect)
                select _img).FirstOrDefault();

        public IGetImageByEffect GetIGetImageByEffect(object key)
            => (from _ir in _ImageResources
                let _img = _ir.Value.GetIGetImageByEffect(key)
                select _img).FirstOrDefault();

        // ---------- IPartResolveMaterial

        public IPartResolveMaterial IPartResolveMaterialParent => null
            ;
        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes
            => _BrushResources.SelectMany(_br => _br.Value.ResolvableBrushes);

        public Material GetMaterial(object key, VisualEffect effect)
            => (from _br in _BrushResources
                let _mat = _br.Value.GetMaterial(key, effect)
                select _mat).FirstOrDefault();

        // ---------- IPartResolveFragment

        public IPartResolveFragment IPartResolveFragmentParent => null;

        public IEnumerable<MetaModelFragmentPartListItem> ResolvableFragments
            => _FragmentResources.SelectMany(_fr => _fr.Value.ResolvableFragments);

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
            => (from _fr in _FragmentResources
                let _mdl = _fr.Value.GetFragment(fragRef, node)
                select _mdl).FirstOrDefault();

        // ---------- IPartResolveModel3D

        /// <summary>Used to provide lists of models that are expected to resolve at runtime</summary>
        public IPartResolveModel3D IPartResolveModel3DParent => throw new NotImplementedException();

        /// <summary>Used to provide lists of models that are expected to resolve at runtime</summary>
        public IEnumerable<Model3DPartListItem> ResolvableModels
            => _ModelResources.SelectMany(_mr => _mr.Value.ResolvableModels);

        /// <summary>Gets a model 3D distinct for the caller (rematerializes for each call)</summary>
        public Model3D GetPrivateModel3D(object key)
            => (from _mr in _ModelResources
                let _mdl = _mr.Value.GetPrivateModel3D(key)
                select _mdl).FirstOrDefault();

        /// <summary>True if model 3D is defined for this resolver</summary>
        public bool CanResolveModel3D(object key)
            => _ModelResources.Any(_mr => _mr.Value.CanResolveModel3D(key));

        // ---------- IPartResolveIcon

        public IPartResolveIcon IPartResolveIconParent => null;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => _IconResources.SelectMany(_ir => _ir.Value.ResolvableIcons);

        public Visual GetIconVisual(string key, IIconReference iconRef)
            => (from _ir in _IconResources
                let _ico = _ir.Value.GetIconVisual(key, iconRef)
                select _ico).FirstOrDefault();

        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
            => (from _ir in _IconResources
                let _ico = _ir.Value.GetIconMaterial(key, iconRef, detailLevel)
                select _ico).FirstOrDefault();

        // ---------- IPartResolveBrushCollection

        public IPartResolveBrushCollection IPartResolveBrushCollectionParent => null;

        public IEnumerable<BrushCollectionPartListItem> ResolvableBrushCollections
            => _BrushSetResources.SelectMany(_bsr => _bsr.Value.ResolvableBrushCollections);

        public BrushCollection GetBrushCollection(object key)
            => (from _bsr in _BrushSetResources
                let _bs = _bsr.Value.GetBrushCollection(key)
                select _bs).FirstOrDefault();
    }
}
