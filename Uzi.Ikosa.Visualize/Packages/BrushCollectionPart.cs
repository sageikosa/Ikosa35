using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Ikosa.Packaging;
using System.IO.Compression;

namespace Uzi.Visualize.Packages
{
    /// <summary>A collection of brushes defined as a part</summary>
    public class BrushCollectionPart : StorablePart, IPartResolveImage, IRetrievablePartNameManager, IPartResolveMaterial
    {
        #region construction
        public BrushCollectionPart(IRetrievablePartNameManager manager, BrushCollectionPart source, string id)
            : base(manager, id)
        {
            // copy images
            _Images = new Dictionary<string, BitmapImagePart>();
            foreach (var _img in source.Images)
            {
                AddImage(new BitmapImagePart(this, _img.Value, _img.Key));
            }

            // attach resolution strategy
            _Parent = this.GetIPartResolveBitmapImage();
            _MatParent = this.GetIPartResolveMaterial();

            // copy brushes
            _Brushes = new BrushCollection();
            try
            {
                IkosaImageSource.PushResolver(this);

                // copy material collection
                foreach (var _b in source.BrushDefinitions)
                {
                    var _c = _b.Clone();
                    // ensure the brush knows who owns it
                    _c.PartOwner = this;
                    _c.ClearCache();
                    BrushDefinitions.Add(_c);
                }
            }
            finally
            {
                IkosaImageSource.PullResolver(this);
            }
        }

        /// <summary>Creates a new collection of brushes</summary>
        public BrushCollectionPart(IRetrievablePartNameManager manager, string id)
            : base(manager, id)
        {
            _Images = new Dictionary<string, BitmapImagePart>();
            _Brushes = new BrushCollection();
            _Parent = this.GetIPartResolveBitmapImage();
            _MatParent = this.GetIPartResolveMaterial();
        }
        #endregion

        #region state
        private ZipArchiveEntry _Entry;
        private BrushCollection _Brushes;
        private readonly Dictionary<string, BitmapImagePart> _Images;
        private readonly IPartResolveImage _Parent;
        private readonly IPartResolveMaterial _MatParent;
        #endregion

        /// <summary>Lists all instances of BitmapImagePart related to this BrushCollectionPart</summary>
        public IEnumerable<KeyValuePair<string, BitmapImagePart>> Images
            => _Images.AsEnumerable().OrderBy(_i => _i.Key);

        /// <summary>Provides upchain support fo image resolving</summary>
        public IPartResolveImage IPartResolveImageParent => _Parent;

        #region public void AddImage(BitmapImagePart part)
        public void AddImage(BitmapImagePart part)
        {
            if (CanUseName(part.PartName))
            {
                _Images.Add(part.PartName, part);
                part.PartNameManager = this;
                _Brushes.Add(new ImageBrushDefinition(this) { BrushKey = part.PartName, ImageKey = part.PartName });
                _Brushes.RefreshAll();
                DoPropertyChanged(nameof(IndexedBrushes));
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        #region public void RemoveImage(BitmapImagePart part)
        public void RemoveImage(BitmapImagePart part)
        {
            if (_Images.ContainsKey(part.PartName))
            {
                _Images.Remove(part.PartName);
                _Brushes.RefreshAll();
                DoPropertyChanged(nameof(IndexedBrushes));
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        #region public IEnumerable<KeyValuePair<int, BrushDefinition>> IndexedBrushes { get; }
        /// <summary>Used for WPF DataBinding</summary>
        public IEnumerable<KeyValuePair<int, BrushDefinition>> IndexedBrushes
        {
            get
            {
                for (var _mx = 0; _mx < BrushDefinitions.Count; _mx++)
                {
                    yield return new KeyValuePair<int, BrushDefinition>(_mx, BrushDefinitions[_mx]);
                }
                yield break;
            }
        }
        #endregion

        #region public BrushCollection BrushDefinitions { get; }
        public BrushCollection BrushDefinitions
        {
            get
            {
                if (_Brushes == null)
                {
                    ResolveMaterialCollection();
                }
                return _Brushes;
            }
        }

        private Stream GetArchiveStream()
            => _Entry.Open();

        private void ResolveMaterialCollection()
        {
            if (_Entry != null)
            {
                try
                {
                    IkosaImageSource.PushResolver(this);

                    // Load material collection
                    using (var _mStream = GetArchiveStream())
                    {
                        _Brushes = XamlReader.Load(_mStream) as BrushCollection;
                    }

                    // ensure the brush knows who owns it
                    foreach (var _iBrush in _Brushes)
                    {
                        _iBrush.PartOwner = this;
                        _iBrush.ClearCache();
                    }
                }
                finally
                {
                    IkosaImageSource.PullResolver(this);
                }
            }
            else
            {
                _Brushes = new BrushCollection();
            }
        }
        #endregion

        #region IPartResolveImage Members

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            if ((_Images != null) && (_Images.Count > 0))
            {
                var _key = key.ToString();
                if (_Images.ContainsKey(_key))
                    return _Images[_key];
            }
            return null;
        }

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            if ((_Images != null) && (_Images.Count > 0))
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
                if (_Parent == null)
                    return _Images.Select(_k => new BitmapImagePartListItem
                    {
                        BitmapImagePart = _k.Value,
                        IsLocal = true
                    }).OrderBy(_bipli => _bipli.BitmapImagePart.PartName);
                else
                    return _Images.Select(_k => new BitmapImagePartListItem
                    {
                        BitmapImagePart = _k.Value,
                        IsLocal = true
                    }).Union(_Parent.ResolvableImages.Select(_pi => new BitmapImagePartListItem
                    {
                        BitmapImagePart = _pi.BitmapImagePart,
                        IsLocal = false
                    })).OrderBy(_bipli => _bipli.BitmapImagePart.PartName);
            }
        }

        #endregion

        #region IRetrievablePart Members

        public override IEnumerable<IRetrievablePart> Parts
        {
            get
            {
                foreach (var _img in Images)
                    yield return _img.Value;
                yield break;
            }
        }

        public override string PartType => GetType().FullName;

        #endregion

        #region IRetrievablePartNameManager Members

        public bool CanUseName(string name)
        {
            var _name = name.ToSafeString();
            return !_Images.ContainsKey(_name);
        }

        public void Rename(string oldName, string newName)
        {
            var _img = _Images[oldName];
            if (_img != null)
            {
                _Images.Remove(oldName);
                _Images.Add(newName, _img);
                _Brushes.RefreshAll();
                DoPropertyChanged(nameof(IndexedBrushes));
                DoPropertyChanged(nameof(Parts));
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
                    lock (_Brushes)
                    {
                        if (_Bytes == null)
                        {
                            if (_Entry != null)
                            {
                                using var _mStream = _Entry.Open();
                                _Bytes = new byte[_mStream.Length];
                                _mStream.Read(_Bytes, 0, (int)_mStream.Length);
                            }
                        }
                    }
                }
                return _Bytes;
            }
        }
        #endregion

        #region IPartResolveMaterial Members

        public Material GetMaterial(object key, VisualEffect effect)
        {
            var _brush = BrushDefinitions.FirstOrDefault(_b => _b.BrushKey.Equals(key.ToString()));
            if (_brush != null)
            {
                return _brush.GetMaterial(effect);
            }
            return null;
        }

        public IPartResolveMaterial IPartResolveMaterialParent => _MatParent;

        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes
        {
            get
            {
                if (_MatParent == null)
                    return _Brushes.Select(_b => new BrushDefinitionListItem
                    {
                        BrushDefinition = _b,
                        IsLocal = true
                    });
                else
                    return _Brushes.Select(_b => new BrushDefinitionListItem
                    {
                        BrushDefinition = _b,
                        IsLocal = true
                    }).Union(_MatParent.ResolvableBrushes.Select(_pb => new BrushDefinitionListItem
                    {
                        BrushDefinition = _pb.BrushDefinition,
                        IsLocal = false
                    }));
            }
        }

        #endregion

        public override void ClosePart()
        {
            // NOTE: no open resources
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            // xaml
            var _entryPath = $@"{parentPath}/{PartName}/{PartName}";
            var _entry = archive.CreateEntry(_entryPath);
            using var _saveStream = _entry.Open();
            using var _writer = new StreamWriter(_saveStream);
            XamlWriter.Save(BrushDefinitions, _writer);

            // images
            var _imagesPath = $@"{parentPath}/{PartName}/Images";
            foreach (var _img in ResolvableImages.Where(_i => _i.IsLocal))
            {
                _img.BitmapImagePart.StorePart(archive, _imagesPath);
            }
            _Entry = _entry;
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            // xaml
            var _entryPath = $@"{parentPath}/{PartName}/{PartName}";
            _Entry = archive.GetEntry(_entryPath);
            _Bytes = null;

            // images
            var _imagesPath = $@"{parentPath}/{PartName}/Images";
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
        }
    }

    public class BrushCollectionPartListItem
    {
        public BrushCollectionPart BrushCollectionPart { get; set; }
        public bool IsLocal { get; set; }
    }
}
