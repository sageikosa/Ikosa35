using System;
using System.Collections.Generic;
using Uzi.Visualize;
using System.Windows.Media.Imaging;
using System.IO;
using Uzi.Visualize.Contracts;
using System.Windows;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public class BitmapImageViewModel : IGetImageByEffect
    {
        public BitmapImageViewModel(BitmapImageInfo bitmap)
        {
            _Info = bitmap;
        }

        public BitmapImageInfo Info { get { return _Info; } }

        #region static construction
        static BitmapImageViewModel()
        {
            _Black = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Ikosa.Proxy;component/Black.bmp"));
        }
        #endregion

        #region private data
        private Dictionary<VisualEffect, BitmapSource> _Images = null;
        private BitmapImageInfo _Info;
        [NonSerialized]
        private static BitmapImage _Black = null;
        // TODO: namespace resolution...
        #endregion

        #region public BitmapSource GetImage(VisualEffect effect)
        public BitmapSource GetImage(VisualEffect effect)
        {
            if (_Images == null)
            {
                ResolveImages();
            }
            if (effect == VisualEffect.Unseen)
            {
                return _Black;
            }
            return _Images[effect];
        }
        #endregion

        #region private void ResolveImages()
        private void ResolveImages()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var _memStream = new MemoryStream(_Info.Bytes);
                _memStream.Seek(0, SeekOrigin.Begin);

                // build image from the stream and keep it for XamlReader.Load
                _Images = new Dictionary<VisualEffect, BitmapSource>();
                var _normal = new BitmapImage();
                try
                {
                    _normal.BeginInit();
                    _normal.StreamSource = _memStream;
                    _normal.EndInit();
                }
                catch
                {
                    // NOTE: image may blow up
                }

                // freeze
                _normal.Freeze();
                _Images.Add(VisualEffect.Normal, _normal);

                // now add the other effects
                _Images.Add(VisualEffect.Monochrome, _normal.RenderImage(VisualEffect.Monochrome, false));
                _Images.Add(VisualEffect.MonochromeDim, _normal.RenderImage(VisualEffect.MonochromeDim, false));
                _Images.Add(VisualEffect.DimTo25, _normal.RenderImage(VisualEffect.DimTo25, false));
                _Images.Add(VisualEffect.DimTo50, _normal.RenderImage(VisualEffect.DimTo50, false));
                _Images.Add(VisualEffect.DimTo75, _normal.RenderImage(VisualEffect.DimTo75, false));
                _Images.Add(VisualEffect.FormOnly, _normal.RenderImage(VisualEffect.FormOnly, false));
                _Images.Add(VisualEffect.Brighter, _normal.RenderImage(VisualEffect.Brighter, false));
                _Images.Add(VisualEffect.Highlighted, _normal.RenderImage(VisualEffect.Highlighted, false));
            });
        }
        #endregion
    }
}