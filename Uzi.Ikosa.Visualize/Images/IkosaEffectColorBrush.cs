using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Brush))]
    public class IkosaEffectColorBrush : MarkupExtension
    {
        public SolidColorBrush SolidColor { get; set; }
        public VisualEffect VisualEffect { get; set; }

        private Color Dim(float factor)
        {
            return Color.FromArgb(SolidColor.Color.A,
                (byte)Math.Min(((float)SolidColor.Color.R * factor),255),
                (byte)Math.Min(((float)SolidColor.Color.G * factor), 255),
                (byte)Math.Min(((float)SolidColor.Color.B * factor), 255));
        }

        private Color Mono()
        {
            byte _val = Convert.ToByte((
                (11 * SolidColor.Color.R) 
                + (16 * SolidColor.Color.G) 
                + (5 * SolidColor.Color.B)
                ) / 32);
            return Color.FromArgb(SolidColor.Color.A, _val, _val, _val);
        }

        private Color Mono(Color monoColor)
        {
            // factors
            double _redFactor = (double)monoColor.R / 255d;
            double _greenFactor = (double)monoColor.G / 255d;
            double _blueFactor = (double)monoColor.B / 255d;

            // mono value
            double _val = (
                (11 * SolidColor.Color.R)
                + (16 * SolidColor.Color.G)
                + (5 * SolidColor.Color.B)
                ) / 32;

            // conversion delegate
            Func<double, byte> _capByte = (factor) =>
                {
                    return Convert.ToByte(Math.Min(_val * factor, 255d));
                };

            // color
            return Color.FromArgb(SolidColor.Color.A, _capByte(_redFactor), _capByte(_greenFactor), _capByte(_blueFactor));
        }

        private Color Average(float factor)
        {
            byte _color = Convert.ToByte((SolidColor.Color.R + SolidColor.Color.G + SolidColor.Color.B) / 3.0f);
            _color = (byte)Math.Min(((float)_color * factor), 255);
            return Color.FromArgb(SolidColor.Color.A, _color, _color, _color);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            switch (VisualEffect)
            {
                case VisualEffect.Unseen:
                    return Brushes.Black;

                case VisualEffect.Monochrome:
                    {
                        // TODO: now that TextureCoordinates were shown to be culprit...
                        // TODO: ...go back to a visual brush solution, so colors will match
                        var _brush = new SolidColorBrush(Mono());
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.Highlighted:
                    {
                        // TODO: now that TextureCoordinates were shown to be culprit...
                        // TODO: ...go back to a visual brush solution, so colors will match
                        var _brush = new SolidColorBrush(Mono(Colors.Magenta));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.DimTo75:
                    {
                        var _brush = new SolidColorBrush(Dim(0.5f));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.DimTo50:
                    {
                        var _brush = new SolidColorBrush(Dim(0.2f));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.DimTo25:
                    {
                        var _brush = new SolidColorBrush(Dim(0.1f));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.MonochromeDim:
                    {
                        // TODO: now that TextureCoordinates were shown to be culprit...
                        // TODO: ...go back to a visual brush solution, so colors will match
                        var _brush = new SolidColorBrush(Mono(Color.FromArgb(255,32,32,32)));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.FormOnly:
                    {
                        // NOTE: until I feel like caching a range of brushes (8-16?), this is too expensive to use
                        //Rectangle _rect = new Rectangle
                        //{
                        //    Stroke = Brushes.Black,
                        //    StrokeThickness = 1,
                        //    Fill = new SolidColorBrush(Average()),
                        //    Width = 96,
                        //    Height = 96,
                        //    Effect = new ShaderEffectLibrary.BloomEffect
                        //    {
                        //        BloomIntensity = 1.25,
                        //        BaseIntensity = 1.5,
                        //        BloomSaturation = 1,
                        //        BaseSaturation = 1
                        //    }
                        //};
                        //var _brush = new VisualBrush(_rect);
                        var _brush = new SolidColorBrush(Average(1.35f));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }

                case VisualEffect.Brighter:
                    {
                        var _brush = new SolidColorBrush(Dim(1.3f));
                        if (_brush.CanFreeze)
                            _brush.Freeze();
                        return _brush;
                    }
            }
            return SolidColor;
        }
    }
}