using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ikosa.Packaging;
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

namespace Uzi.Visualize.IkosaPackaging
{
    public class IconPart : BasePart
    {
        public const string IconRelation = @"http://pack.guildsmanship.com/visualize/icon";

        #region ctor
        public IconPart(ICorePartNameManager manager, FileInfo fileInfo)
            : base(manager, fileInfo.Name.Replace(@" ", @"_"))
        {
            _Info = null;
            _FileInfo = fileInfo;
        }

        public IconPart(ICorePartNameManager manager, PackagePart part, string id)
            : base(manager, part, id.Replace(@" ", @"_"))
        {
            _Info = null;
            _FileInfo = null;
        }

        public IconPart(IconInfo info)
            : base(null, info.Name)
        {
            _Info = info;
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

        private Stream StorageStream()
        {
            if (Part != null)
                return Part.GetStream(FileMode.Open, FileAccess.Read);
            else if (_FileInfo != null)
                return _FileInfo.OpenRead();
            else if (_Info != null)
                return new MemoryStream(_Info.Bytes);
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
            using var _stream = StorageStream();
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

        #region public static IEnumerable<IconPart> GetIconPartResources(ICorePartNameManager manager, PackagePart part)
        /// <summary>Pre-loads icons from related package parts for IResolveIcon</summary>
        /// <param name="part">part with (possible) IconPart relations</param>
        public static IEnumerable<IconPart> GetIconPartResources(ICorePartNameManager manager, PackagePart part)
        {
            foreach (var _imgRel in part.GetRelationshipsByType(MetaModelFragment.MetaModelFragmentRelation))
            {
                var _imgPart = part.Package.GetPart(_imgRel.TargetUri);
                yield return new IconPart(manager, _imgPart, _imgRel.Id);
            }
        }
        #endregion

        #region ICorePart Members
        public override IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public override string TypeName => GetType().FullName;

        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region Saving
        public override void Save(Package parent)
        {
            // re-Resolve icon before changing parts
            using var _iStream = (Part != null
                ? Part.GetStream(FileMode.Open, FileAccess.Read)
                : (_FileInfo != null) ? (Stream)_FileInfo.OpenRead()
                : (_Info != null ? new MemoryStream(_Info.Bytes)
                : null));
            if (_iStream != null)
            {
                Uri _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
                _Part = parent.CreatePart(_base, @"text/xaml+xml", CompressionOption.Normal);
                parent.CreateRelationship(_base, TargetMode.Internal, IconRelation, Name);

                DoSave(_iStream);
            }
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // re-Resolve icon before changing parts
            using (var _iStream = (Part != null
                ? Part.GetStream(FileMode.Open, FileAccess.Read)
                : (_FileInfo != null) ? (Stream)_FileInfo.OpenRead()
                : (_Info != null ? new MemoryStream(_Info.Bytes)
                : null)))
            {
                if (_iStream != null)
                {
                    var _base = UriHelper.ConcatRelative(baseUri, Name);
                    _Part = parent.Package.CreatePart(_base, @"text/xaml+xml", CompressionOption.Normal);
                    parent.CreateRelationship(_base, TargetMode.Internal, IconRelation, Name);

                    DoSave(_iStream);
                }
            }
        }

        protected virtual void DoSave(Stream iconStream)
        {
            // save xaml
            using (var _saveStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                StreamHelper.CopyStream(iconStream, _saveStream);
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
                    lock (this)
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

        protected override void OnRefreshPart() { }

        public override void Close()
        {
            // NOTE: no open resources
        }
    }

    public class IconPartListItem
    {
        public IconPart IconPart { get; set; }
        public bool IsLocal { get; set; }
    }
}
