using System;
using System.Windows.Media;

namespace Uzi.Visualize
{
    public class SolidBrushDefinition : BrushDefinition
    {
        public SolidBrushDefinition()
        {
        }

        public SolidBrushDefinition(SolidBrushDefinition source)
            : base(source)
        {
            _Color = source.Color;
        }

        #region private data
        private Color _Color;
        #endregion

        private Color Dim(float factor)
        {
            return Color.FromArgb(Color.A,
                (byte)Math.Min(((float)Color.R * factor), 255),
                (byte)Math.Min(((float)Color.G * factor), 255),
                (byte)Math.Min(((float)Color.B * factor), 255));
        }

        private Color Mono()
        {
            byte _val = Convert.ToByte((
                (11 * Color.R)
                + (16 * Color.G)
                + (5 * Color.B)
                ) / 32);
            return Color.FromArgb(Color.A, _val, _val, _val);
        }

        public override void ClearCache()
        {
            base.ClearCache();
            PreGenerateBrushes();
        }

        private Color Mono(Color monoColor)
        {
            // factors
            double _redFactor = (double)monoColor.R / 255d;
            double _greenFactor = (double)monoColor.G / 255d;
            double _blueFactor = (double)monoColor.B / 255d;

            // mono value
            double _val = (
                (11 * Color.R)
                + (16 * Color.G)
                + (5 * Color.B)
                ) / 32;

            // conversion delegate
            byte _capByte(double factor) => Convert.ToByte(Math.Min(_val * factor, 255d));

            // color
            return Color.FromArgb(Color.A, _capByte(_redFactor), _capByte(_greenFactor), _capByte(_blueFactor));
        }

        private Color Average(float factor)
        {
            byte _color = Convert.ToByte((Color.R + Color.G + Color.B) / 3.0f);
            _color = (byte)Math.Min(((float)_color * factor), 255);
            return Color.FromArgb(Color.A, _color, _color, _color);
        }

        public Color Color
        {
            get { return _Color; }
            set
            {
                if (_Color != value)
                {
                    _Color = value;
                    ClearCache();
                }
            }
        }

        protected override bool NeedsPreGenerate { get { return true; } }

        public override bool IsAlphaChannel { get { return Color.A < byte.MaxValue; } set { } }

        #region protected override Brush OnGetBrush(VisualEffect effect)
        protected override Brush OnGetBrush(VisualEffect effect)
        {
            switch (effect)
            {
                case VisualEffect.Unseen:
                    return Brushes.Black;

                case VisualEffect.MonoSub1:
                case VisualEffect.MonoSub2:
                case VisualEffect.MonoSub3:
                case VisualEffect.MonoSub4:
                case VisualEffect.MonoSub5:
                case VisualEffect.Monochrome:
                    {
                        var _brush = new SolidColorBrush(Mono());
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = Opacity;
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.Highlighted:
                    {
                        var _brush = new SolidColorBrush(Mono(Colors.Magenta));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = Opacity;
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.DimTo75:
                    {
                        var _brush = IsAlphaChannel
                            ? new SolidColorBrush(Dim(0.875f))
                            : new SolidColorBrush(Dim(0.5f));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = 1d - (Opacity * 0.75d);
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.DimTo50:
                    {
                        var _brush = IsAlphaChannel
                            ? new SolidColorBrush(Dim(0.75f))
                            : new SolidColorBrush(Dim(0.2f));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = 1d - (Opacity * 0.5d);
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.DimTo25:
                    {
                        var _brush = IsAlphaChannel
                            ? new SolidColorBrush(Dim(0.5f))
                            : new SolidColorBrush(Dim(0.1f));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = 1d - (Opacity * 0.25d);
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.MonochromeDim:
                    {
                        // TODO: ¿ alt brush Mono/Opacity ?
                        var _brush = new SolidColorBrush(Mono(Color.FromArgb(255, 32, 32, 32)));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = Opacity;
                        }
                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.FormSub1:
                case VisualEffect.FormSub2:
                case VisualEffect.FormSub3:
                case VisualEffect.FormSub4:
                case VisualEffect.FormSub5:
                case VisualEffect.FormOnly:
                    {
                        var _brush = new SolidColorBrush(Average(1.35f));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = Opacity;
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }

                case VisualEffect.Brighter:
                    {
                        var _brush = new SolidColorBrush(Dim(1.3f));
                        if (!IsAlphaChannel)
                        {
                            _brush.Opacity = Opacity;
                        }

                        if (_brush.CanFreeze)
                        {
                            _brush.Freeze();
                        }

                        return _brush;
                    }
            }
            var _ret = new SolidColorBrush(Color);
            if (!IsAlphaChannel)
            {
                _ret.Opacity = Opacity;
            }

            _ret.Freeze();
            return _ret;
        }
        #endregion

        public override BrushDefinition Clone()
            => new SolidBrushDefinition(this);
    }
}
