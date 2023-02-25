using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Uzi.Visualize
{
    public class LinearGradientBrushDefinition : BrushDefinition
    {
        public LinearGradientBrushDefinition()
        {
        }

        public LinearGradientBrushDefinition(LinearGradientBrushDefinition source)
            : base(source)
        {
            _Interpolation = source.ColorInterpolationMode;
            _Spread = source.SpreadMethod;
            _Angle = source.Angle;
            _Size = source.Size;
            _Stops = source.GradientStops.Clone();
        }

        #region private data
        private GradientStopCollection _Stops = new GradientStopCollection();
        private ColorInterpolationMode _Interpolation;
        private GradientSpreadMethod _Spread;
        private double _Angle = 0d;
        private double _Size = 1d;
        #endregion

        public override void ClearCache()
        {
            base.ClearCache();
            PreGenerateBrushes();
        }

        #region private Color Dim(Color color, float factor)
        private Color Dim(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                (byte)Math.Min(((float)color.R * factor), 255),
                (byte)Math.Min(((float)color.G * factor), 255),
                (byte)Math.Min(((float)color.B * factor), 255));
        }
        #endregion

        #region private Color Mono(Color color )
        private Color Mono(Color color)
        {
            byte _val = Convert.ToByte((
                (11 * color.R)
                + (16 * color.G)
                + (5 * color.B)
                ) / 32);
            return Color.FromArgb(color.A, _val, _val, _val);
        }
        #endregion

        #region private Color Mono(Color color, Color monoColor)
        private Color Mono(Color color, Color monoColor)
        {
            // factors
            double _redFactor = (double)monoColor.R / 255d;
            double _greenFactor = (double)monoColor.G / 255d;
            double _blueFactor = (double)monoColor.B / 255d;

            // mono value
            double _val = (
                (11 * color.R)
                + (16 * color.G)
                + (5 * color.B)
                ) / 32;

            // conversion delegate
            Func<double, byte> _capByte = (factor) =>
            {
                return Convert.ToByte(Math.Min(_val * factor, 255d));
            };

            // color
            return Color.FromArgb(color.A, _capByte(_redFactor), _capByte(_greenFactor), _capByte(_blueFactor));
        }
        #endregion

        #region private Color Average(Color color, float factor)
        private Color Average(Color color, float factor)
        {
            byte _color = Convert.ToByte((color.R + color.G + color.B) / 3.0f);
            _color = (byte)Math.Min(((float)_color * factor), 255);
            return Color.FromArgb(color.A, _color, _color, _color);
        }
        #endregion

        public GradientStopCollection GradientStops
        {
            get { return _Stops; }
            set
            {
                _Stops = value;
                _Stops.Freeze();
                ClearCache();
            }
        }

        public ColorInterpolationMode ColorInterpolationMode
        {
            get { return _Interpolation; }
            set
            {
                _Interpolation = value;
                ClearCache();
            }
        }

        public GradientSpreadMethod SpreadMethod
        {
            get { return _Spread; }
            set
            {
                _Spread = value;
                ClearCache();
            }
        }

        public double Angle
        {
            get { return _Angle; }
            set
            {
                if (_Angle != value)
                {
                    _Angle = value;
                    ClearCache();
                }
            }
        }

        public double Size
        {
            get { return _Size; }
            set
            {
                if (_Size != value)
                {
                    _Size = value;
                    ClearCache();
                }
            }
        }

        public override bool IsAlphaChannel
        {
            get { return _Stops.Any(_s => _s.Color.A < 255); }
            set { }
        }

        protected override bool NeedsPreGenerate { get { return true; } }

        #region protected override Brush OnGetBrush(VisualEffect effect)
        protected override Brush OnGetBrush(VisualEffect effect)
        {
            // defines action to recolor the copy of the stops
            Func<Color, Color> _recolor = (color) => color;
            var _opacity = IsAlphaChannel ? Opacity : 1d;

            switch (effect)
            {
                case VisualEffect.Unseen: return Brushes.Black;
                case VisualEffect.Highlighted: _recolor = (color) => Mono(color, Colors.Magenta); break;
                case VisualEffect.Brighter: _recolor = (color) => Dim(color, 1.3f); break;
                case VisualEffect.DimTo75:
                    _recolor = (color) => IsAlphaChannel ? Dim(color, 0.875f) : Dim(color, 0.5f);
                    _opacity = IsAlphaChannel ? 1d - (Opacity * 0.75d) : _opacity;
                    break;
                case VisualEffect.DimTo50:
                    _recolor = (color) => IsAlphaChannel ? Dim(color, 0.75f) : Dim(color, 0.2f);
                    _opacity = IsAlphaChannel ? 1d - (Opacity * 0.5d) : _opacity;
                    break;
                case VisualEffect.DimTo25:
                    _recolor = (color) => IsAlphaChannel ? Dim(color, 0.5f) : Dim(color, 0.1f);
                    _opacity = IsAlphaChannel ? 1d - (Opacity * 0.25d) : _opacity;
                    break;
                case VisualEffect.MonochromeDim:
                    // TODO: ¿ alt brush Mono/Opacity ?
                    _recolor = (color) => Mono(color, Color.FromArgb(255, 32, 32, 32));
                    break;

                case VisualEffect.MonoSub1:
                case VisualEffect.MonoSub2:
                case VisualEffect.MonoSub3:
                case VisualEffect.MonoSub4:
                case VisualEffect.MonoSub5:
                case VisualEffect.Monochrome:
                    _recolor = (color) => Mono(color);
                    break;

                case VisualEffect.FormSub1:
                case VisualEffect.FormSub2:
                case VisualEffect.FormSub3:
                case VisualEffect.FormSub4:
                case VisualEffect.FormSub5:
                case VisualEffect.FormOnly:
                    _recolor = (color) => Average(color, 1.35f);
                    break;
            }
            var _off = _Size / 2;
            var _brush = new LinearGradientBrush(
                new GradientStopCollection(_Stops.Select(_s => new GradientStop(_recolor(_s.Color), _s.Offset))),
                new Point(0.5 - _off, 0.5),
                new Point(0.5 + _off, 0.5))
            {
                RelativeTransform = new RotateTransform(Angle, 0.5d, 0.5d),
                SpreadMethod = _Spread,
                ColorInterpolationMode = _Interpolation,
                Opacity = _opacity
            };
            _brush.Freeze();
            return _brush;
        }
        #endregion

        public override BrushDefinition Clone()
            => new LinearGradientBrushDefinition(this);
    }
}
