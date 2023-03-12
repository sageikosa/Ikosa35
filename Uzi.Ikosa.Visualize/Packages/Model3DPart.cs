using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using System.Collections.Specialized;
using Uzi.Visualize;
using Newtonsoft.Json;
using Ikosa.Packaging;
using System.IO.Compression;

namespace Uzi.Visualize.Packages
{
    public class Model3DPart : StorablePart, IPartResolveImage, IRetrievablePartNameManager
    {
        #region state
        private ZipArchiveEntry _Entry;
        private readonly FileInfo _FileInfo;
        private readonly Model3DPart _Source;
        private Dictionary<string, BitmapImagePart> _Images;
        private readonly IPartResolveImage _Parent;
        private readonly BrushCollectionPart _Brushes = null;

        // Visual Effect Material Keys
        protected SortedSet<string> _VEMKeys = null;
        protected List<Type> _SETypes = null;
        protected List<(ModelCacheSelector selector, Model3D model)> _ModelCache = new();
        #endregion

        #region construction
        public Model3DPart(IRetrievablePartNameManager manager, string id)
            : base(manager, id)
        {
            _Images = new Dictionary<string, BitmapImagePart>();
            _Parent = this.GetIPartResolveBitmapImage();
        }

        public Model3DPart(IRetrievablePartNameManager manager, FileInfo fileInfo)
            : this(manager, fileInfo.Name.Replace(@" ", @"_"))
        {
            _FileInfo = fileInfo;
            _Brushes = new BrushCollectionPart(this, @"Brushes");
        }

        public Model3DPart(IRetrievablePartNameManager manager, Model3DPart source, string name)
            : this(manager, name)
        {
            _Source = source;

            // copy images...
            foreach (var _img in source.Images)
            {
                AddImage(new BitmapImagePart(this, _img.Value, _img.Key));
            }

            // copy brushes...
            _Brushes = new BrushCollectionPart(this, source.Brushes, @"Brushes");
        }
        #endregion

        /// <summary>Lists all instances of BitmapImagePart related to this Model3DPart</summary>
        public IEnumerable<KeyValuePair<string, BitmapImagePart>> Images => _Images.AsEnumerable();

        /// <summary>Reloads model from part stream or file</summary>
        public void RefreshModel()
        {
            _ModelCache.Clear();
        }

        #region IRetrievablePart Members

        public override IEnumerable<IRetrievablePart> Parts
        {
            get
            {
                yield return _Brushes;
                if (_Images != null)
                    foreach (var _img in Images)
                        yield return _img.Value;
                yield break;
            }
        }
        public override string PartType => GetType().FullName;

        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IRetrievablePartNameManager Members

        public virtual bool CanUseName(string name)
        {
            var _name = name.ToSafeString();
            return (_Images == null) || !_Images.ContainsKey(_name);
        }

        public virtual void Rename(string oldName, string newName)
        {
            if (_Images != null)
            {
                var _img = _Images[oldName];
                if (_img != null)
                {
                    _Images.Remove(oldName);
                    _Images.Add(newName, _img);
                }
            }
        }

        #endregion

        #region public void AddImage(BitmapImagePart part)
        public void AddImage(BitmapImagePart part)
        {
            if (CanUseName(part.PartName))
            {
                if (_Images == null)
                    _Images = new Dictionary<string, BitmapImagePart>();
                _Images.Add(part.PartName, part);
                part.PartNameManager = this;
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        #region public void RemoveImage(BitmapImagePart part)
        public void RemoveImage(BitmapImagePart part)
        {
            if (_Images?.ContainsKey(part.PartName) ?? false)
            {
                _Images.Remove(part.PartName);
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        public BrushCollectionPart Brushes => _Brushes;

        /// <summary>VisualEffectMaterial keys found in the model</summary>
        public IList<string> VisualEffectMaterialKeys
        {
            get
            {
                if (_VEMKeys == null)
                    ResolveModel();
                return _VEMKeys.ToList();
            }
        }

        #region public virtual Model3D ResolveModel()
        public virtual Model3D ResolveModel()
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

            // only need resolvers for a limited time
            try
            {
                VisualEffectMaterial.PushResolver(_Brushes);
                IkosaImageSource.PushResolver(this);

                // first load ever!
                if (_VEMKeys == null)
                {
                    // referenced keys
                    _VEMKeys = new SortedSet<string>();
                    VisualEffectMaterial.ReferencedKey = (key) => { if (!_VEMKeys.Contains(key)) _VEMKeys.Add(key); };
                }
                if (_SETypes == null)
                {
                    // referenced sense extensions
                    _SETypes = new List<Type>();
                    SenseEffectExtension.ReferencedEffect = (type) => { if (!_SETypes.Contains(type)) _SETypes.Add(type); };
                }

                // resolve
                var _resolve = ResolveFromStream();
                if (!_ModelCache.Any(_ce => _ce.selector.Equals(_selector)))
                {
                    _resolve.Freeze();
                    _ModelCache.Add((_selector.GetCacheKey(), _resolve));
                }

                // cache...
                return _resolve;
            }
            finally
            {
                SenseEffectExtension.ReferencedEffect = null;
                VisualEffectMaterial.ReferencedKey = null;
                VisualEffectMaterial.PullResolver(_Brushes);
                IkosaImageSource.PullResolver(this);
            }
        }
        #endregion

        private Stream GetArchiveStream()
            => _Entry.Open();

        #region protected Model3D ResolveFromStream()
        protected Model3D ResolveFromStream()
        {
            Model3D _mdl = null;
            if (_Entry != null)
            {
                // load model
                using var _mStream = GetArchiveStream();
                _mdl = XamlReader.Load(_mStream) as Model3D;
            }
            else if (_FileInfo != null)
            {
                // load model
                using var _fStream = _FileInfo.OpenRead();
                _mdl = XamlReader.Load(_fStream) as Model3D;
            }
            else if (_Source != null)
            {
                _mdl = _Source.ResolveFromStream();
            }
            return _mdl;
        }
        #endregion

        #region IPartResolveImage Members

        public IPartResolveImage IPartResolveImageParent => _Parent;

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            if ((_Images != null) && _Images.Any())
            {
                var _key = key.ToString();
                if (_Images.ContainsKey(_key))
                    return _Images[_key];
            }
            return null;
        }

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            if ((_Images != null) && _Images.Any())
            {
                var _key = key.ToString();
                if (_Images.ContainsKey(_key))
                    return _Images[_key].GetImage(effect);
            }
            return null;
        }

        public IEnumerable<BitmapImagePartListItem> ResolvableImages
        {
            get
            {
                // our own and anything available through parent's
                return _Images.Select(_k => new BitmapImagePartListItem
                {
                    BitmapImagePart = _k.Value,
                    IsLocal = true
                }).Union(_Parent.ResolvableImages.Select(_pi => new BitmapImagePartListItem
                {
                    BitmapImagePart = _pi.BitmapImagePart,
                    IsLocal = false
                }));
            }
        }
        #endregion

        #region public byte[] StreamBytes { get; }
        private byte[] _Bytes;
        public byte[] StreamBytes
        {
            get
            {
                if (_Bytes == null)
                {
                    lock (_ModelCache)
                    {
                        if (_Bytes == null)
                        {
                            if (_Entry != null)
                            {
                                using var _mStream = GetArchiveStream();
                                _Bytes = new byte[_mStream.Length];
                                _mStream.Read(_Bytes, 0, (int)_mStream.Length);
                            }
                            else if (_FileInfo != null)
                            {
                                using var _fStream = _FileInfo.OpenRead();
                                _Bytes = new byte[_fStream.Length];
                                _fStream.Read(_Bytes, 0, (int)_fStream.Length);
                            }
                            else if (_Source != null)
                            {
                                _Bytes = _Source.StreamBytes;
                            }
                        }
                    }
                }
                return _Bytes;
            }
        }
        #endregion

        public override void ClosePart()
        {
            // NOTE: no open resources
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            var _partPath = $@"{parentPath}/{PartName}";
            var _entryPath = $@"{_partPath}/{SaveTarget}";
            var _entry = archive.CreateEntry(_entryPath);
            using var _mdlStream =
                (_Entry != null)
                ? GetArchiveStream()
                : (_FileInfo != null)
                ? _FileInfo.OpenRead()
                : (_Source != null) ? new MemoryStream(_Source.StreamBytes)
                : null;
            if (_mdlStream != null)
            {
                using var _saveStream = _entry.Open();
                StreamHelper.CopyStream(_mdlStream, _saveStream);
            }

            // images
            var _imagesPath = $@"{_partPath}/Images";
            foreach (var _img in ResolvableImages.Where(_i => _i.IsLocal))
            {
                _img.BitmapImagePart.StorePart(archive, _imagesPath);
            }

            // brushes
            _Brushes.StorePart(archive, _partPath);
            _Entry = _entry;
        }

        protected virtual string SaveTarget => @"model.xaml";

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            // xaml
            var _partPath = $@"{parentPath}/{PartName}";
            var _entryPath = $@"{_partPath}/{SaveTarget}";
            _Entry = archive.GetEntry(_entryPath);
            _Bytes = null;

            // images
            var _imagesPath = $@"{_partPath}/Images";
            foreach (var _iRef in archive.Entries
                .Where(_e
                // starts with image path
                => _e.FullName.StartsWith(_imagesPath)
                // only has image path and final name with a separator
                && string.Equals(string.Concat(_imagesPath, @"/", _e.Name), _e.FullName)))
            {
                if (!_Images.TryGetValue(_iRef.Name, out var _img))
                {
                    _img = new BitmapImagePart(this, _iRef.Name);
                    _Images.Add(_iRef.Name, _img);
                }
                _img.ReloadPart(archive, _imagesPath);
            }

            // brushes
            _Brushes.ReloadPart(archive, _partPath);
        }
    }

    public class Model3DPartListItem
    {
        public Model3DPart Model3DPart { get; set; }
        public bool IsLocal { get; set; }
    }
}
