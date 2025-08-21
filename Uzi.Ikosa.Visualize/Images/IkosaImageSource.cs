using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;

namespace Uzi.Visualize
{
    /// <summary>
    /// Provides a BitmapImage from an Ikosa source
    /// </summary>
    public class IkosaImageSource : BitmapSource
    {
        [ThreadStatic]
        private static Collection<IResolveBitmapImage> _Resolvers = [];

        public static Collection<IResolveBitmapImage> Resolvers
        {
            get
            {
                _Resolvers ??= [];
                return _Resolvers;
            }
        }

        public static void PushResolver(IResolveBitmapImage resolver)
        {
            if ((resolver != null) && (!Resolvers.Contains(resolver)))
            {
                Resolvers.Insert(0, resolver);
            }
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

                // ensure something
                _Image ??= new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoImage.png"));
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
