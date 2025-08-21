using System;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;

namespace Uzi.Visualize
{
    public class IkosaEffectImageSource : BitmapSource
    {
        #region private data
        private string _ImageKey = null;
        private IGetImageByEffect _Image = null;
        private BitmapSource _Source = null;
        #endregion

        public IGetImageByEffect BitmapImagePart { get { return _Image; } }

        public VisualEffect VisualEffect
        {
            get { return (VisualEffect)GetValue(EffectProperty); }
            set { SetValue(EffectProperty, value); }
        }

        private static void EffectChanged(DependencyObject depObject, DependencyPropertyChangedEventArgs args)
        {
            if (depObject is IkosaEffectImageSource)
            {
                var _effImageSource = depObject as IkosaEffectImageSource;
                if (_effImageSource._Image != null)
                {
                    _effImageSource.RenderImage();
                }
            }
        }

        private void RenderImage()
        {
            // ensure something
            if (_Image != null)
            {
                // render image
                _Source = _Image.GetImage(this.VisualEffect);
            }
            else
            {
                if (!String.IsNullOrEmpty(ImageKey) && ImageKey.Equals(@"[FORM]"))
                {
                    _Source = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/Outline.bmp"));
                }
                else
                {
                    _Source = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoImage.png"));
                }
            }
        }

        // Using a DependencyProperty as the backing store for Effect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EffectProperty =
            DependencyProperty.Register(@"VisualEffect", typeof(VisualEffect), typeof(IkosaEffectImageSource),
            new UIPropertyMetadata(VisualEffect.Normal, new PropertyChangedCallback(EffectChanged)));

        #region public string ImageKey { get; set; }
        /// <summary>Key is serialized to XAML</summary>
        public string ImageKey
        {
            get { return _ImageKey; }
            set
            {
                // prevent re-entrant fallback
                Collection<IResolveBitmapImage> _track = [];
                _ImageKey = value;
                _Image = null;
                foreach (var _res in IkosaImageSource.Resolvers.ToList())
                {
                    var _rez = _res;
                    while ((_rez != null) && !_track.Contains(_rez))
                    {
                        // prevent re-entrant fallback
                        _track.Add(_rez);
                        var _img = _rez.GetIGetImageByEffect(_ImageKey);
                        if (_img != null)
                        {
                            _Image = _img;
                            break;
                        }
                        _rez = _rez.IResolveBitmapImageParent;
                    };
                }

                RenderImage();
            }
        }
        #endregion

        protected override Freezable CreateInstanceCore()
        {
            return new IkosaEffectImageSource { ImageKey = this.ImageKey, VisualEffect= this.VisualEffect };
        }

        public override void CopyPixels(Array pixels, int stride, int offset)
        {
            _Source.CopyPixels(pixels, stride, offset);
        }

        public override void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
        {
            _Source.CopyPixels(sourceRect, pixels, stride, offset);
        }

        public override void CopyPixels(Int32Rect sourceRect, IntPtr buffer, int bufferSize, int stride)
        {
            _Source.CopyPixels(sourceRect, buffer, bufferSize, stride);
        }

        public override event EventHandler<ExceptionEventArgs> DecodeFailed;
        public override event EventHandler DownloadCompleted;
        public override event EventHandler<ExceptionEventArgs> DownloadFailed;
        public override event EventHandler<DownloadProgressEventArgs> DownloadProgress;

        public override double DpiX { get { return _Source.DpiX; } }
        public override double DpiY { get { return _Source.DpiY; } }
        public override PixelFormat Format { get { return _Source.Format; } }
        public override bool IsDownloading { get { return false; } }
        public override ImageMetadata Metadata { get { return _Source.Metadata; } }
        public override BitmapPalette Palette { get { return _Source.Palette; } }
        public override int PixelHeight { get { return _Source.PixelHeight; } }
        public override int PixelWidth { get { return _Source.PixelWidth; } }
    }
}
