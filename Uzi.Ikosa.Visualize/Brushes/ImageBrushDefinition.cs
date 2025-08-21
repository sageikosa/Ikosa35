using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize
{
    public class ImageBrushDefinition : BrushDefinition
    {
        #region construction
        public ImageBrushDefinition()
        {
        }

        public ImageBrushDefinition(ImageBrushDefinition source)
            : base(source)
        {
            _ImageKey = source.ImageKey;
        }

        public ImageBrushDefinition(BrushCollectionPart owner)
        {
            Owner = owner;
        }
        #endregion

        #region private data
        private string _ImageKey;
        private IGetImageByEffect _Image = null;
        #endregion

        public override bool IsAlphaChannel { get { return false; } set { } }

        protected override bool NeedsPreGenerate { get { return true; } }

        public override void ClearCache()
        {
            _Image = null;
            base.ClearCache();
            ResolveImage();
            PreGenerateBrushes();
        }

        /// <summary>Key is serialized to XAML</summary>
        public string ImageKey
        {
            get { return _ImageKey; }
            set
            {
                if (_ImageKey != value)
                {
                    _ImageKey = value;
                    ClearCache();
                }
            }
        }

        #region private void ResolveImage()
        /// <summary>Ensures the image is resolved</summary>
        private void ResolveImage()
        {
            if (_Image == null)
            {
                try
                {
                    IkosaImageSource.PushResolver(Owner);

                    // prevent re-entrant fallback
                    var _track = new List<IResolveBitmapImage>();
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

                    // ensure everything
                    _Image ??= new MissingProvider();
                }
                finally
                {
                    IkosaImageSource.PullResolver(Owner);
                }
            }
        }
        #endregion

        #region protected override Brush OnGetBrush(VisualEffect effect)
        protected override Brush OnGetBrush(VisualEffect effect)
        {
            ResolveImage();
            var _alpha = Opacity < 1d;
            switch (effect)
            {
                case (VisualEffect.DimTo75):
                    {
                        // effect brushes
                        var _img = _Image.GetImage(effect);
                        _img.Freeze();
                        var _brush = new ImageBrush(_img);
                        _brush.Opacity = _alpha ? 1d - (Opacity * 0.75d) : Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

                case (VisualEffect.DimTo50):
                    {
                        // effect brushes
                        var _img = _Image.GetImage(effect);
                        _img.Freeze();
                        var _brush = new ImageBrush(_img);
                        _brush.Opacity = _alpha ? 1d - (Opacity * 0.5d) : Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

                case (VisualEffect.DimTo25):
                    {
                        // effect brushes
                        var _img = _Image.GetImage(effect);
                        _img.Freeze();
                        var _brush = new ImageBrush(_img);
                        _brush.Opacity = _alpha ? 1d - (Opacity * 0.25d) : Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

                case (VisualEffect.Brighter):
                case (VisualEffect.MonochromeDim):
                case (VisualEffect.Monochrome):
                case (VisualEffect.FormOnly):
                case (VisualEffect.Highlighted):
                    {
                        // TODO: ¿ alt brush Mono/Opacity ?
                        // effect brushes
                        var _img = _Image.GetImage(effect);
                        _img.Freeze();
                        var _brush = new ImageBrush(_img);
                        _brush.Opacity = Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

                case (VisualEffect.MonoSub1):
                case (VisualEffect.MonoSub2):
                case (VisualEffect.MonoSub3):
                case (VisualEffect.MonoSub4):
                case (VisualEffect.MonoSub5):
                    {
                        var _img = _Image.GetImage(VisualEffect.Monochrome);
                        _img.Freeze();
                        var _brush = new ImageBrush(_img);
                        _brush.Opacity = Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

                case (VisualEffect.FormSub1):
                case (VisualEffect.FormSub2):
                case (VisualEffect.FormSub3):
                case (VisualEffect.FormSub4):
                case (VisualEffect.FormSub5):
                    {
                        var _img = _Image.GetImage(VisualEffect.FormOnly);
                        _img.Freeze();
                        var _brush = new ImageBrush(_img);
                        _brush.Opacity = Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

                default: // case (VisualEffect.Normal):
                    {
                        // Normal brush
                        var _brush = new ImageBrush(_Image.GetImage(VisualEffect.Normal));
                        _brush.Opacity = Opacity;
                        _brush.Freeze();
                        return _brush;
                    }

            }
        }
        #endregion

        public override BrushDefinition Clone()
            => new ImageBrushDefinition(this);
    }
}
