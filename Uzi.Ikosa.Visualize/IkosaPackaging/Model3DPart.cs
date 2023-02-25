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

namespace Uzi.Visualize.IkosaPackaging
{
    public class Model3DPart : BasePart, IResolveBitmapImage, ICorePartNameManager
    {
        /// <summary>Relationship to map models (http://pack.guildsmanship.com/visualize/model3d)</summary>
        public const string ModelRelation = @"Ikosa.Visualize.Model3DPart";

        #region construction
        public Model3DPart(ICorePartNameManager manager, FileInfo fileInfo)
            : base(manager, fileInfo.Name.Replace(@" ", @"_"))
        {
            _FileInfo = fileInfo;
            _Images = new Dictionary<string, BitmapImagePart>();
            _Parent = this.GetIResolveBitmapImage();

            _Brushes = new BrushCollectionPart(this, @"Brushes");
        }

        public Model3DPart(ICorePartNameManager manager, Model3DPart source, string name)
            : base(manager, name)
        {
            _Source = source;
            _Parent = this.GetIResolveBitmapImage();

            // copy images...
            _Images = new Dictionary<string, BitmapImagePart>();
            foreach (var _img in source.Images)
            {
                AddImage(new BitmapImagePart(this, _img.Value, _img.Key));
            }

            // copy brushes...
            _Brushes = new BrushCollectionPart(this, source.Brushes, @"Brushes");
        }

        public Model3DPart(ICorePartNameManager manager, ZipArchiveEntry part)
            : base(manager, part)
        {
            _Images = new Dictionary<string, BitmapImagePart>();
            _Parent = this.GetIResolveBitmapImage();

            var _related =  Part.RestoreParts(this).ToList();
            foreach (var _bRef in _related.OfType<BitmapImagePart>())
            {
                if (!_Images.ContainsKey(_bRef.Name))
                {
                    _Images.Add(_bRef.Name, _bRef);
                }
            }

            // brushes
            _Brushes = _related.OfType<BrushCollectionPart>().FirstOrDefault();
            if (_Brushes == null)
                _Brushes = new BrushCollectionPart(this, @"Brushes");
        }
        #endregion

        #region data
        private readonly FileInfo _FileInfo;
        private readonly Model3DPart _Source;
        private Dictionary<string, BitmapImagePart> _Images;
        private readonly IResolveBitmapImage _Parent;
        private readonly BrushCollectionPart _Brushes = null;

        // Visual Effect Material Keys
        protected SortedSet<string> _VEMKeys = null;
        protected List<Type> _SETypes = null;
        protected List<Tuple<ModelCacheSelector, Model3D>> _ModelCache = new List<Tuple<ModelCacheSelector, Model3D>>();
        #endregion

        /// <summary>Lists all instances of BitmapImagePart related to this Model3DPart</summary>
        public IEnumerable<KeyValuePair<string, BitmapImagePart>> Images { get { return _Images.AsEnumerable(); } }

        /// <summary>Reloads model from part stream or file</summary>
        public void RefreshModel()
        {
            _ModelCache.Clear();
        }

        #region public void AddImage(BitmapImagePart part)
        public void AddImage(BitmapImagePart part)
        {
            if (CanUseName(part.Name, typeof(BitmapImagePart)))
            {
                if (_Images == null)
                    _Images = new Dictionary<string, BitmapImagePart>();
                _Images.Add(part.Name, part);
                part.NameManager = this;
                DoPropertyChanged(@"Relationships");
            }
        }
        #endregion

        #region public void RemoveImage(BitmapImagePart part)
        public void RemoveImage(BitmapImagePart part)
        {
            if ((_Images != null) && _Images.ContainsKey(part.Name))
            {
                _Images.Remove(part.Name);
                DoPropertyChanged(@"Relationships");
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
                var _entry = _ModelCache
                    .FirstOrDefault(_ce => _ce.Item1.Equals(_selector));
                if (_entry != null)
                    return _entry.Item2;
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
                if (_ModelCache
                    .FirstOrDefault(_ce => _ce.Item1.Equals(_selector)) == null)
                {
                    _resolve.Freeze();
                    _ModelCache.Add(new Tuple<ModelCacheSelector, Model3D>(_selector.GetCacheKey(), _resolve));
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

        #region protected Model3D ResolveFromStream()
        protected Model3D ResolveFromStream()
        {
            Model3D _mdl = null;
            if (Part != null)
            {
                // load model
                using var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read);
                _mdl = XamlReader.Load(_mStream) as Model3D;
            }
            else if (_FileInfo != null)
            {
                using var _fStream = _FileInfo.OpenRead();
                // load model
                _mdl = XamlReader.Load(_fStream) as Model3D;
            }
            else if (_Source != null)
            {
                _mdl = _Source.ResolveFromStream();
            }
            return _mdl;
        }
        #endregion

        #region IResolveBitmapImage Members

        public IResolveBitmapImage IResolveBitmapImageParent { get { return _Parent; } }

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

        #region ICorePart Members

        public override IEnumerable<ICorePart> Parts
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

        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region ICorePartNameManager Members

        public virtual bool CanUseName(string name, Type partType)
        {
            var _name = name.ToSafeString();
            if (partType == typeof(BitmapImagePart))
                return ((_Images == null) || !_Images.ContainsKey(_name));
            return false;
        }

        public virtual void Rename(string oldName, string newName, Type partType)
        {
            if ((_Images != null) && (partType == typeof(BitmapImagePart)))
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

        #region Saving
        public override void Save(Package parent)
        {
            // re-Resolve Model before changing parts
            using var _mStream = (
                Part != null ? Part.GetStream(FileMode.Open, FileAccess.Read)
                : (_FileInfo != null) ? (Stream)_FileInfo.OpenRead()
                : (_Source != null) ? new MemoryStream(_Source.StreamBytes)
                : null);
            if (_mStream != null)
            {
                var _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
                var _newPart = true;
                var _target = UriHelper.ConcatRelative(_base, SaveTarget);
                _Part = parent.CreatePart(_target, @"text/xaml+xml", CompressionOption.Normal);
                parent.CreateRelationship(_target, TargetMode.Internal, SaveRelation, Name);

                DoSave(_mStream, _base, _newPart);
            }
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // re-Resolve Model before changing parts
            using var _mStream = (
                Part != null ? Part.GetStream(FileMode.Open, FileAccess.Read)
                : (_FileInfo != null) ? (Stream)_FileInfo.OpenRead()
                : (_Source != null) ? new MemoryStream(_Source.StreamBytes)
                : null);
            if (_mStream != null)
            {
                var _base = UriHelper.ConcatRelative(baseUri, Name);
                var _newPart = true;
                var _target = UriHelper.ConcatRelative(_base, SaveTarget);
                _Part = parent.Package.CreatePart(_target, @"text/xaml+xml", CompressionOption.Normal);
                parent.CreateRelationship(_target, TargetMode.Internal, SaveRelation, Name);

                DoSave(_mStream, _base, _newPart);
            }
        }

        protected virtual string SaveTarget => @"model.xaml";
        protected virtual string SaveRelation => ModelRelation;

        protected virtual void DoSave(Stream modelStream, Uri baseUri, bool newPart)
        {
            if (newPart)
            {
                // save xaml
                using var _saveStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite);
                StreamHelper.CopyStream(modelStream, _saveStream);
            }

            // save related images
            var _base = UriHelper.ConcatRelative(baseUri, @"images");
            foreach (var _item in ResolvableImages.Where(_i => _i.IsLocal))
            {
                _item.BitmapImagePart.Save(_Part, _base);
            }

            _Brushes.Save(_Part, baseUri);
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
                            if (Part != null)
                            {
                                using var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read);
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

        #region protected override void OnRefreshPart()
        protected override void OnRefreshPart()
        {
            // if this wasn't cleared, update and add
            if (Part != null)
            {
                foreach (var _bRef in BitmapImagePart.GetImageResources(this, Part))
                {
                    if (_Images.ContainsKey(_bRef.Name))
                        _Images[_bRef.Name].RefreshPart(_bRef.Part);
                    else
                        _Images.Add(_bRef.Name, _bRef);
                }

                // brushes
                var _brushRel = Part.GetRelationship(@"Brushes");
                _Brushes.RefreshPart(Part.Package.GetPart(_brushRel.TargetUri));
            }
        }
        #endregion

        public override void Close()
        {
            // NOTE: no open resources
        }
    }
}
