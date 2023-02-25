using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Packaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Markup;
using Uzi.Visualize;
using Ikosa.Packaging;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.IkosaPackaging
{
    /// <summary>A collection of brushes defined as a part</summary>
    public class BrushCollectionPart : BasePart, IResolveBitmapImage, ICorePartNameManager, IResolveMaterial
    {
        /// <summary>Relationship type to identify a brushset (http://pack.guildsmanship.com/visualize/brushset)</summary>
        public const string BrushSetRelation = @"http://pack.guildsmanship.com/visualize/brushset";

        #region construction
        /// <summary>Creates a collection of brushes defined from a PackagePart</summary>
        public BrushCollectionPart(ICorePartNameManager manager, PackagePart part, string id)
            : base(manager, part, id)
        {
            // track directly related images
            _Images = new Dictionary<string, BitmapImagePart>();
            foreach (var _bRef in BitmapImagePart.GetImageResources(this, Part))
            {
                if (!_Images.ContainsKey(_bRef.Name))
                {
                    _Images.Add(_bRef.Name, _bRef);
                }
            }

            _Brushes = null;
            _Parent = this.GetIResolveBitmapImage();
            _MatParent = this.GetIResolveMaterial();

            ResolveMaterialCollection();
        }

        public BrushCollectionPart(ICorePartNameManager manager, BrushCollectionPart source, string id)
            : base(manager, id)
        {
            // copy images
            _Images = new Dictionary<string, BitmapImagePart>();
            foreach (var _img in source.Images)
            {
                AddImage(new BitmapImagePart(this, _img.Value, _img.Key));
            }

            // attach resolution strategy
            _Parent = this.GetIResolveBitmapImage();
            _MatParent = this.GetIResolveMaterial();

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
                    _c.Owner = this;
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
        public BrushCollectionPart(ICorePartNameManager manager, string id)
            : base(manager, id)
        {
            _Images = new Dictionary<string, BitmapImagePart>();
            _Brushes = new BrushCollection();
            _Parent = this.GetIResolveBitmapImage();
            _MatParent = this.GetIResolveMaterial();
        }
        #endregion

        #region state
        private BrushCollection _Brushes;
        private readonly Dictionary<string, BitmapImagePart> _Images;
        private readonly IResolveBitmapImage _Parent;
        private readonly IResolveMaterial _MatParent;
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
            }
        }
        #endregion

        /// <summary>Lists all instances of BitmapImagePart related to this BrushCollectionPart</summary>
        public IEnumerable<KeyValuePair<string, BitmapImagePart>> Images { get { return _Images.AsEnumerable().OrderBy(_i => _i.Key); } }

        /// <summary>Provides upchain support fo image resolving</summary>
        public IResolveBitmapImage IResolveBitmapImageParent { get { return _Parent; } }

        #region public void AddImage(BitmapImagePart part)
        public void AddImage(BitmapImagePart part)
        {
            if (CanUseName(part.Name, typeof(BitmapImagePart)))
            {
                _Images.Add(part.Name, part);
                part.NameManager = this;
                _Brushes.Add(new ImageBrushDefinition(this) { BrushKey = part.Name, ImageKey = part.Name });
                _Brushes.RefreshAll();
                DoPropertyChanged(@"IndexedBrushes");
                DoPropertyChanged(@"Relationships");
            }
        }
        #endregion

        #region public void RemoveImage(BitmapImagePart part)
        public void RemoveImage(BitmapImagePart part)
        {
            if (_Images.ContainsKey(part.Name))
            {
                _Images.Remove(part.Name);
                _Brushes.RefreshAll();
                DoPropertyChanged(@"IndexedBrushes");
                DoPropertyChanged(@"Relationships");
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

        private void ResolveMaterialCollection()
        {
            if (Part != null)
            {
                try
                {
                    IkosaImageSource.PushResolver(this);

                    // Load material collection
                    using (var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read))
                    {
                        _Brushes = XamlReader.Load(_mStream) as BrushCollection;
                    }

                    // ensure the brush knows who owns it
                    foreach (var _iBrush in _Brushes)
                    {
                        _iBrush.Owner = this;
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

        #region IResolveBitmapImage Members

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
                    }).OrderBy(_bipli => _bipli.BitmapImagePart.Name);
                else
                    return _Images.Select(_k => new BitmapImagePartListItem
                    {
                        BitmapImagePart = _k.Value,
                        IsLocal = true
                    }).Union(_Parent.ResolvableImages.Select(_pi => new BitmapImagePartListItem
                    {
                        BitmapImagePart = _pi.BitmapImagePart,
                        IsLocal = false
                    })).OrderBy(_bipli => _bipli.BitmapImagePart.Name);
            }
        }

        #endregion

        #region ICorePart Members

        public override IEnumerable<ICorePart> Relationships
        {
            get
            {
                foreach (var _img in Images)
                    yield return _img.Value;
                yield break;
            }
        }

        public override string TypeName => GetType().FullName;

        #endregion

        #region ICorePartNameManager Members

        public bool CanUseName(string name, Type partType)
        {
            var _name = name.ToSafeString();
            return !_Images.ContainsKey(_name);
        }

        public void Rename(string oldName, string newName, Type partType)
        {
            var _img = _Images[oldName];
            if (_img != null)
            {
                _Images.Remove(oldName);
                _Images.Add(newName, _img);
                _Brushes.RefreshAll();
                DoPropertyChanged(@"IndexedBrushes");
                //DoPropertyChanged(@"Relationships");
            }
        }

        #endregion

        #region Saving
        public override void Save(Package parent)
        {
            // resolve materials before changing part
            var _brushes = BrushDefinitions;

            var _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
            var _target = UriHelper.ConcatRelative(_base, @"brushes.xaml");
            _Part = parent.CreatePart(_target, @"text/xaml+xml", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, BrushSetRelation, Name);
            _ = DoSave(_brushes, _base);
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // resolve materials before changing part
            var _brushes = BrushDefinitions;

            var _base = UriHelper.ConcatRelative(baseUri, Name);
            var _target = UriHelper.ConcatRelative(_base, Name);
            _Part = parent.Package.CreatePart(_target, @"text/xaml+xml", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, BrushSetRelation, Name);
            _ = DoSave(_brushes, _base);
        }

        private Uri DoSave(BrushCollection materials, Uri baseUri)
        {
            // save xaml (always)
            using (var _tileStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                using (var _writer = new StreamWriter(_tileStream))
                {
                    XamlWriter.Save(materials, _writer);
                }
            }

            // save related images
            var _base = UriHelper.ConcatRelative(baseUri, @"images");
            foreach (var _item in ResolvableImages.Where(_i => _i.IsLocal))
            {
                _item.BitmapImagePart.Save(_Part, _base);
            }
            return _base;
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
                    lock (_Part)
                    {
                        if (_Bytes == null)
                        {
                            using (var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read))
                            {
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

        #region IResolveBrush Members

        public Material GetMaterial(object key, VisualEffect effect)
        {
            var _brush = BrushDefinitions.FirstOrDefault(_b => _b.BrushKey.Equals(key.ToString()));
            if (_brush != null)
            {
                return _brush.GetMaterial(effect);
            }
            return null;
        }

        public IResolveMaterial IResolveMaterialParent { get { return _MatParent; } }

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

        public override void Close()
        {
            // NOTE: no open resources
        }
    }
}
