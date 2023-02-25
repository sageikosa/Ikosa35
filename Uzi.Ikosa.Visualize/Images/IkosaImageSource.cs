using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO.Packaging;
using Uzi.Visualize.Packages;

namespace Uzi.Visualize
{
    /// <summary>
    /// Provides a BitmapImage from an Ikosa source
    /// </summary>
    public class IkosaImageSource : BitmapSource
    {
        [ThreadStatic]
        private static Collection<IResolveBitmapImage> _Resolvers = new();

        [ThreadStatic]
        private static Collection<IPartResolveImage> _PartResolvers = new();

        public static Collection<IPartResolveImage> PartResolvers
        {
            get
            {
                if (_PartResolvers == null)
                    _PartResolvers = new Collection<IPartResolveImage>();
                return _PartResolvers;
            }
        }

        public static void PushResolver(IPartResolveImage resolver)
        {
            if ((resolver != null) && (!PartResolvers.Contains(resolver)))
                PartResolvers.Insert(0, resolver);
        }

        public static void PullResolver(IPartResolveImage resolver)
            => _ = PartResolvers.Remove(resolver);

        public static Collection<IResolveBitmapImage> Resolvers
        {
            get
            {
                if (_Resolvers == null)
                    _Resolvers = new Collection<IResolveBitmapImage>();
                return _Resolvers;
            }
        }

        public static void PushResolver(IResolveBitmapImage resolver)
        {
            if ((resolver != null) && (!Resolvers.Contains(resolver)))
                Resolvers.Insert(0, resolver);
        }

        public static void PullResolver(IResolveBitmapImage resolver)
            => Resolvers.Remove(resolver);

        #region private data
        private string _ImageKey = null;
        private BitmapSource _Image = null;
        #endregion

        #region public string ImageKey { get; set; }
        /// <summary>Key is serialized to XAML</summary>
        public string ImageKey
        {
            get => _ImageKey;
            set
            {
                _ImageKey = value;
                _Image = null;

                // prevent re-entrant fallback
                var _track = new Collection<IResolveBitmapImage>();
                foreach (var _res in Resolvers)
                {
                    var _rez = _res;
                    while ((_rez != null) && !_track.Contains(_rez))
                    {
                        // prevent re-entrant fallback
                        _track.Add(_rez);
                        var _img = _rez.GetImage(_ImageKey, VisualEffect.Normal);
                        if (_img != null)
                        {
                            _Image = _img;
                            break;
                        }
                        _rez = _rez.IResolveBitmapImageParent;
                    };
                }

                if (_Image == null)
                {
                    var _pTrack = new Collection<IPartResolveImage>();
                    foreach (var _res in PartResolvers)
                    {
                        var _rez = _res;
                        while ((_rez != null) && !_pTrack.Contains(_rez))
                        {
                            // prevent re-entrant fallback
                            _pTrack.Add(_rez);
                            var _img = _rez.GetImage(_ImageKey, VisualEffect.Normal);
                            if (_img != null)
                            {
                                _Image = _img;
                                break;
                            }
                            _rez = _rez.IPartResolveImageParent;
                        };
                    }
                }

                // ensure something
                if (_Image == null)
                {
                    _Image = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoImage.png"));
                }
            }
        }
        #endregion

        protected override Freezable CreateInstanceCore()
            => new IkosaImageSource { ImageKey = ImageKey };

        public override void CopyPixels(Array pixels, int stride, int offset)
            => _Image.CopyPixels(pixels, stride, offset);

        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
            => _Image.CopyPixels(sourceRect, pixels, stride, offset);

        public override void CopyPixels(Int32Rect sourceRect, IntPtr buffer, int bufferSize, int stride)
            => _Image.CopyPixels(sourceRect, buffer, bufferSize, stride);

        public override event EventHandler<ExceptionEventArgs> DecodeFailed;
        public override event EventHandler DownloadCompleted;
        public override event EventHandler<ExceptionEventArgs> DownloadFailed;
        public override event EventHandler<DownloadProgressEventArgs> DownloadProgress;

        public override double DpiX => _Image.DpiX;
        public override double DpiY => _Image.DpiY;
        public override PixelFormat Format => _Image.Format;
        public override bool IsDownloading => false;
        public override ImageMetadata Metadata => _Image.Metadata;
        public override BitmapPalette Palette => _Image.Palette;
        public override int PixelHeight => _Image.PixelHeight;
        public override int PixelWidth => _Image.PixelWidth;
    }
}
