using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Windows.Media;
using System.Windows.Markup;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using Uzi.Visualize.Contracts;
using Newtonsoft.Json;
using Uzi.Visualize.Contracts.Tactical;
using Ikosa.Packaging;
using System.IO.Compression;

namespace Uzi.Visualize.Packages
{
    public class IconPart : StorablePart
    {
        #region ctor
        public IconPart(IRetrievablePartNameManager manager, FileInfo fileInfo)
            : base(manager, fileInfo.Name.Replace(@" ", @"_"))
        {
            _Info = null;
            _Entry = null;
            _FileInfo = fileInfo;
        }

        public IconPart(IRetrievablePartNameManager manager, string id)
            : base(manager, id.Replace(@" ", @"_"))
        {
            _Info = null;
            _Entry = null;
            _FileInfo = null;
        }

        public IconPart(IconInfo info)
            : base(null, info.Name)
        {
            _Info = info;
            _Entry = null;
            _FileInfo = null;
        }
        #endregion

        #region thread static data
        [ThreadStatic]
        private static ResourceDictionary _Lowest;
        [ThreadStatic]
        private static ResourceDictionary _Low;
        [ThreadStatic]
        private static ResourceDictionary _Normal;
        [ThreadStatic]
        private static ResourceDictionary _High;
        [ThreadStatic]
        private static ResourceDictionary _Highest;
        #endregion

        #region data
        private ZipArchiveEntry _Entry;
        private readonly IconInfo _Info;
        private readonly FileInfo _FileInfo;
        #endregion

        #region private ResourceDictionary GetDetailDictionary(IconDetailLevel detailLevel)
        private ResourceDictionary GetDetailDictionary(IconDetailLevel detailLevel)
        {
            ResourceDictionary _makeDictionary(double thickness)
            {
                var _resources = new ResourceDictionary();

                var _style = new Style(typeof(Line));
                _style.Setters.Add(new Setter(Line.StrokeThicknessProperty, thickness));
                _resources.Add(typeof(Line), _style);

                _style = new Style(typeof(Rectangle));
                _style.Setters.Add(new Setter(Rectangle.StrokeThicknessProperty, thickness));
                _resources.Add(typeof(Rectangle), _style);

                _style = new Style(typeof(Ellipse));
                _style.Setters.Add(new Setter(Ellipse.StrokeThicknessProperty, thickness));
                _resources.Add(typeof(Ellipse), _style);

                _style = new Style(typeof(System.Windows.Shapes.Path));
                _style.Setters.Add(new Setter(Shape.StrokeThicknessProperty, thickness));
                _resources.Add(typeof(System.Windows.Shapes.Path), _style);

                return _resources;
            };

            switch (detailLevel)
            {
                case IconDetailLevel.Lowest:
                    if (_Lowest == null)
                        _Lowest = _makeDictionary(2d);
                    return _Lowest;

                case IconDetailLevel.Low:
                    if (_Low == null)
                        _Low = _makeDictionary(1.5d);
                    return _Low;

                case IconDetailLevel.High:
                    if (_High == null)
                        _High = _makeDictionary(0.5d);
                    return _High;

                case IconDetailLevel.Highest:
                    if (_Highest == null)
                        _Highest = _makeDictionary(0.25d);
                    return _Highest;

                case IconDetailLevel.Normal:
                default:
                    if (_Normal == null)
                        _Normal = _makeDictionary(1d);
                    return _Normal;
            }
        }
        #endregion

        private Stream GetArchiveStream()
            => _Entry.Open();

        private Stream GetCurrentStream()
        {
            if (_Entry != null)
            {
                return GetArchiveStream();
            }
            if (_FileInfo != null)
            {
                return _FileInfo.OpenRead();
            }
            else if (_Info != null)
            {
                return new MemoryStream(_Info.Bytes);
            }
            return null;
        }

        #region protected DataTemplate ResolveFromStream()
        protected DataTemplate ResolveFromStream(Stream stream, IDictionary<string, string> colorMap)
        {
            ColorVal.SetKeyedColors(colorMap);
            stream.Seek(0, SeekOrigin.Begin);
            var _load = XamlReader.Load(stream);
            return _load as DataTemplate;
        }
        #endregion

        #region protected DataTemplate ResolveFromStream()
        protected DataTemplate ResolveFromStream(IDictionary<string, string> colorMap)
        {
            using var _stream = GetCurrentStream();
            return ResolveFromStream(_stream, colorMap);
        }
        #endregion

        private Visual GetVisual(IconDetailLevel detail, IIconReference iconRef)
        {
            var _visual = GetIconContent(iconRef);
            _visual.Resources.Add(@"dictStyles", GetDetailDictionary(detail));
            _visual.CacheMode = new BitmapCache(2d);
            return _visual;
        }

        private Material GetMaterial(IconDetailLevel detail, IIconReference iconRef)
            => new DiffuseMaterial(new VisualBrush(GetVisual(detail, iconRef)) { Opacity = 0.9d, Stretch = Stretch.Uniform });

        public Visual Icon => GetIconContent(new IconReferenceInfo { IconScale = 1 });

        public ContentControl GetIconContent(IIconReference iconRef)
        {
            var _transform = new TransformGroup()
            {
                Children = new TransformCollection
                {
                    new RotateTransform(iconRef.IconAngle),
                    new ScaleTransform(iconRef.IconScale, iconRef.IconScale)
                }
            };
            _transform.Freeze();
            var _control = new ContentControl
            {
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = _transform,
                Content = new object(),
                ContentTemplate = ResolveFromStream(iconRef.IconColorMap)
            };
            return _control;
        }

        public Material GetIconMaterial(IconDetailLevel detailLevel, IIconReference iconRef)
            => GetMaterial(detailLevel, iconRef);

        public override IEnumerable<IRetrievablePart> Parts => Enumerable.Empty<IRetrievablePart>();
        public override string PartType => GetType().FullName;

        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region public byte[] StreamBytes { get; }
        private byte[] _Bytes;
        public byte[] StreamBytes
        {
            get
            {
                if (_Bytes == null)
                {
                    lock (this)
                    {
                        if (_Bytes == null)
                        {
                            if (_Entry != null)
                            {
                                using var _aStream = GetArchiveStream();
                                _Bytes = new byte[_aStream.Length];
                                _aStream.Read(_Bytes, 0, (int)_aStream.Length);
                            }
                            else if (_FileInfo != null)
                            {
                                using var _fStream = _FileInfo.OpenRead();
                                _Bytes = new byte[_fStream.Length];
                                _fStream.Read(_Bytes, 0, (int)_fStream.Length);
                            }
                            else if (_Info != null)
                            {
                                _Bytes = _Info.Bytes;
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
            using var _srcStream = GetCurrentStream();
            var _entry = archive.CreateEntry($@"{parentPath}/{PartName}");
            using var _saveStream = _entry.Open();
            StreamHelper.CopyStream(_srcStream, _saveStream);
            _Entry = _entry;
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            _Entry = archive.GetEntry($@"{parentPath}/{PartName}");
            _Bytes = null;
        }
    }

    public class IconPartListItem
    {
        public IconPart IconPart { get; set; }
        public bool IsLocal { get; set; }
    }
}
