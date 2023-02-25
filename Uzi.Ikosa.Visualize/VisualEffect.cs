using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Threading;
using System.Windows;

namespace Uzi.Visualize
{
    public enum VisualEffect : byte
    {
        /// <summary>Brighter than normal</summary>
        Brighter,
        /// <summary>Normal hues and brightness</summary>
        Normal,
        /// <summary>Normal edging against shadowy</summary>
        DimTo75,
        /// <summary>Shadowy</summary>
        DimTo50,
        /// <summary>Shadowy edging against unseeable</summary>
        DimTo25,
        /// <summary>Unseeable</summary>
        Unseen,
        /// <summary>No hues</summary>
        MonoSub5,
        /// <summary>No hues</summary>
        MonoSub4,
        /// <summary>No hues</summary>
        MonoSub3,
        /// <summary>No hues</summary>
        MonoSub2,
        /// <summary>No hues</summary>
        MonoSub1,
        /// <summary>No hues</summary>
        Monochrome,
        /// <summary>Blind-sight</summary>
        FormSub5,
        /// <summary>Blind-sight</summary>
        FormSub4,
        /// <summary>Blind-sight</summary>
        FormSub3,
        /// <summary>Blind-sight</summary>
        FormSub2,
        /// <summary>Blind-sight</summary>
        FormSub1,
        /// <summary>Standard for blind-sight</summary>
        FormOnly,
        /// <summary>Do not render anything</summary>
        Skip,
        /// <summary>Used when selected in user interface</summary>
        Highlighted,

        /// <summary>Unseeable, but aware</summary>
        MonochromeDim
    }

    public static class VisualEffectProcessor
    {
        #region static private RenderTargetBitmap RenderBitmap(BitmapSource source, Visual visual)
        private static RenderTargetBitmap RenderBitmap(BitmapSource source, Visual visual)
        {
            try
            {
                // render
                var _render = new RenderTargetBitmap(source.PixelWidth, source.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                _render.Render(visual);

                // brush
                _render.Freeze();
                return _render;
            }
            catch (Exception _except)
            {
                Debug.WriteLine(_except);
                return null;
            }
        }
        #endregion

        #region public static RenderTargetBitmap RenderBitmap(this Visual source)
        public static RenderTargetBitmap RenderBitmap(this Visual source)
        {
            try
            {
                // render
                var _render = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
                _render.Render(source);

                // brush
                return _render;
            }
            catch (Exception _except)
            {
                Debug.WriteLine(_except);
                return null;
            }
        }
        #endregion

        #region public static RenderTargetBitmap RenderImage(this BitmapSource source, VisualEffect effect, bool alpha)
        public static RenderTargetBitmap RenderImage(this BitmapSource source, VisualEffect effect, bool alpha)
        {
            var _imageVisual = new Image { Source = source };
            _imageVisual.Measure(new System.Windows.Size(source.PixelWidth, source.PixelHeight));
            _imageVisual.Arrange(new System.Windows.Rect(0, 0, source.PixelWidth, source.PixelHeight));
            _imageVisual.Stretch = Stretch.UniformToFill;

            // add effect
            switch (effect)
            {
                case VisualEffect.FormOnly:
                    // chain through borders
                    _imageVisual.Effect = new EmbossedEffect
                    {
                        Amount = 5,
                        Width = 0.001
                    };
                    var _border = new Border { Child = _imageVisual };
                    _border.Effect = new BloomEffect
                    {
                        BloomIntensity = 1.25,
                        BaseIntensity = 1.5,
                        BloomSaturation = 1,
                        BaseSaturation = 1
                    };

                    return RenderBitmap(source, _border);

                case VisualEffect.Monochrome:
                    _imageVisual.Effect = new MonochromeEffect { FilterColor = Colors.White };
                    return RenderBitmap(source, _imageVisual);

                case VisualEffect.DimTo75:
                    _imageVisual.Effect = new DarknessEffect { DarknessFactor = alpha ? 0.875d : 0.50d };
                    return RenderBitmap(source, _imageVisual);

                case VisualEffect.DimTo50:
                    _imageVisual.Effect = new DarknessEffect { DarknessFactor = alpha ? 0.75d : 0.20d };
                    return RenderBitmap(source, _imageVisual);

                case VisualEffect.DimTo25:
                    _imageVisual.Effect = new DarknessEffect { DarknessFactor = alpha ? 0.5d : 0.1d };
                    return RenderBitmap(source, _imageVisual);

                case VisualEffect.MonochromeDim:
                    // TODO: ¿ alt brush Mono/Opacity ?
                    _imageVisual.Effect = new MonochromeEffect { FilterColor = Color.FromArgb(255, 32, 32, 32) };
                    return RenderBitmap(source, _imageVisual);

                case VisualEffect.Brighter:
                    _imageVisual.Effect = new DarknessEffect { DarknessFactor = 1.3 };
                    return RenderBitmap(source, _imageVisual);

                case VisualEffect.Highlighted:
                    _imageVisual.Effect = new MonochromeEffect { FilterColor = Colors.Magenta };
                    return RenderBitmap(source, _imageVisual);

            }
            return null;
        }
        #endregion

        /// <summary>Renders all variant image effects</summary>
        public static IEnumerable<(VisualEffect Effect, RenderTargetBitmap RenderTarget)> RenderImages(this BitmapSource source)
        {
            yield return (VisualEffect.Brighter, RenderImage(source, VisualEffect.Brighter, false));
            yield return (VisualEffect.DimTo75, RenderImage(source, VisualEffect.DimTo75, false));
            yield return (VisualEffect.DimTo50, RenderImage(source, VisualEffect.DimTo50, false));
            yield return (VisualEffect.DimTo25, RenderImage(source, VisualEffect.DimTo25, false));
            yield return (VisualEffect.MonochromeDim, RenderImage(source, VisualEffect.MonochromeDim, false));
            yield return (VisualEffect.Monochrome, RenderImage(source, VisualEffect.Monochrome, false));
            yield return (VisualEffect.FormOnly, RenderImage(source, VisualEffect.FormOnly, false));
            yield return (VisualEffect.Highlighted, RenderImage(source, VisualEffect.Highlighted, false));
            yield break;
        }

        public static bool IsMonochrome(this VisualEffect effect)
        {
            return (effect >= VisualEffect.MonoSub5) && (effect <= VisualEffect.Monochrome);
        }

        public static bool IsFormOnly(this VisualEffect effect)
        {
            return (effect >= VisualEffect.FormSub5) && (effect <= VisualEffect.FormOnly);
        }

        public static VisualEffect GetMonochromeLevel(double distance, double maxDistance)
        {
            var _level = 6d * distance / maxDistance;
            if (_level <= 1d)
                return VisualEffect.Monochrome;
            else if (_level <= 2d)
                return VisualEffect.MonoSub1;
            else if (_level <= 3d)
                return VisualEffect.MonoSub2;
            else if (_level <= 4d)
                return VisualEffect.MonoSub3;
            else if (_level <= 5d)
                return VisualEffect.MonoSub4;
            return VisualEffect.MonoSub5;
        }

        public static VisualEffect GetFormOnlyLevel(double distance, double maxDistance)
        {
            var _level = 6d * distance / maxDistance;
            if (_level <= 1d)
                return VisualEffect.FormOnly;
            else if (_level <= 2d)
                return VisualEffect.FormSub1;
            else if (_level <= 3d)
                return VisualEffect.FormSub2;
            else if (_level <= 4d)
                return VisualEffect.FormSub3;
            else if (_level <= 5d)
                return VisualEffect.FormSub4;
            return VisualEffect.FormSub5;
        }

        /// <summary>Ensures all six cardinal sense effects has something defined.</summary>
        public static void InitializeEffects(VisualEffect baseLine)
        {
            FrontSenseEffectExtension.EffectValue = baseLine;
            BackSenseEffectExtension.EffectValue = baseLine;
            TopSenseEffectExtension.EffectValue = baseLine;
            BottomSenseEffectExtension.EffectValue = baseLine;
            LeftSenseEffectExtension.EffectValue = baseLine;
            RightSenseEffectExtension.EffectValue = baseLine;
        }

        public static int GetEffectRank(this VisualEffect self)
        {
            switch (self)
            {
                // reserved space for future light levels?
                case VisualEffect.Brighter: return 252;
                case VisualEffect.Normal: return 228;
                case VisualEffect.DimTo75: return 203;
                case VisualEffect.DimTo50: return 178;
                case VisualEffect.DimTo25: return 153;
                case VisualEffect.Monochrome: return 63;
                case VisualEffect.MonoSub1: return 62;
                case VisualEffect.MonoSub2: return 61;
                case VisualEffect.MonoSub3: return 60;
                case VisualEffect.MonoSub4: return 59;
                case VisualEffect.MonoSub5: return 58;
                case VisualEffect.FormOnly: return 47;
                case VisualEffect.FormSub1: return 46;
                case VisualEffect.FormSub2: return 45;
                case VisualEffect.FormSub3: return 44;
                case VisualEffect.FormSub4: return 43;
                case VisualEffect.FormSub5: return 42;
                case VisualEffect.Highlighted: return 16;
                case VisualEffect.MonochromeDim: return 15;
                case VisualEffect.Unseen: return 1;
                case VisualEffect.Skip: return 0;
                default: return 0;
            }
        }
    }
}
