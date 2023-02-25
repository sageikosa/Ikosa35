using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Collections.Concurrent;
using Uzi.Visualize.Packaging;
using Uzi.Visualize.Packages;

namespace Uzi.Visualize
{
    public abstract class BrushDefinition 
    {
        #region ctor()
        protected BrushDefinition()
        {
            _Brushes = new Dictionary<VisualEffect, Brush>();
            _Materials = new Dictionary<VisualEffect, Material>();
        }

        protected BrushDefinition(BrushDefinition source)
        {
            _Brushes = new Dictionary<VisualEffect, Brush>();
            _Materials = new Dictionary<VisualEffect, Material>();
            _Opacity = source.Opacity;
            BrushKey = source.BrushKey;
        }

        static BrushDefinition()
        {
            _Black = new DiffuseMaterial(Brushes.Black);
            _Black.Freeze();
            _Missing = new DiffuseMaterial(new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoImage.png"))));
            _Missing.Freeze();
            _MissingSelected = TintMaterial(new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/Images/NoImage.png"))), Colors.Magenta);
            _MissingSelected.Freeze();
        }
        #endregion

        #region private data
        private static Material _Black = null;
        private static Material _Missing = null;
        private static Material _MissingSelected = null;
        private Dictionary<VisualEffect, Brush> _Brushes;
        private Dictionary<VisualEffect, Material> _Materials;
        private double _Opacity = 1d;
        #endregion

        public string BrushKey { get; set; }

        /// <summary>Brush handles alpha channel</summary>
        public abstract bool IsAlphaChannel { get; set; }

        public double Opacity
        {
            get { return _Opacity; }
            set
            {
                if (_Opacity != value)
                {
                    _Opacity = value;
                    ClearCache();
                }
            }
        }

        public static Material MissingMaterial => _Missing;
        public static Material MissingSelectedMaterial => _MissingSelected;

        public bool ShouldSerializeOwner()
            => false;

        public IPartResolveImage PartOwner { get; set; }
        public IResolveBitmapImage Owner { get; set; }

        public Material Normal
            => GetMaterial(VisualEffect.Normal);

        public Brush Brush
            => GetBrush(VisualEffect.Normal);

        public virtual void ClearCache()
        {
            _Brushes.Clear();
            _Materials.Clear();
        }

        #region private IEnumerable<VisualEffect> GeneratedEffects()
        private IEnumerable<VisualEffect> GeneratedEffects()
        {
            yield return VisualEffect.Normal;
            yield return VisualEffect.Brighter;
            yield return VisualEffect.DimTo25;
            yield return VisualEffect.DimTo50;
            yield return VisualEffect.DimTo75;
            yield return VisualEffect.FormOnly;
            yield return VisualEffect.FormSub1;
            yield return VisualEffect.FormSub2;
            yield return VisualEffect.FormSub3;
            yield return VisualEffect.FormSub4;
            yield return VisualEffect.FormSub5;
            yield return VisualEffect.Highlighted;
            yield return VisualEffect.Monochrome;
            yield return VisualEffect.MonochromeDim;
            yield return VisualEffect.MonoSub1;
            yield return VisualEffect.MonoSub2;
            yield return VisualEffect.MonoSub3;
            yield return VisualEffect.MonoSub4;
            yield return VisualEffect.MonoSub5;
            yield break;
        }
        #endregion

        protected abstract bool NeedsPreGenerate { get; }

        protected void PreGenerateBrushes()
        {
            if (NeedsPreGenerate)
                foreach (var _effect in GeneratedEffects())
                {
                    var _newBrush = OnGetBrush(_effect);
                    _Brushes.Add(_effect, _newBrush);
                    _Materials.Add(_effect, OnGetMaterial(_effect));
                }
        }

        #region public Brush GetBrush(VisualEffect effect)
        protected abstract Brush OnGetBrush(VisualEffect effect);

        public Brush GetBrush(VisualEffect effect)
        {
            if (effect == VisualEffect.Unseen)
                return Brushes.Black;

            if (!_Brushes.ContainsKey(effect) && !NeedsPreGenerate)
            {
                var _newBrush = OnGetBrush(effect);
                _Brushes.Add(effect, _newBrush);
                return _newBrush;
            }
            return _Brushes[effect];
        }
        #endregion

        #region protected Material GetMaterial(VisualEffect effect)
        protected static DiffuseMaterial TintMaterial(Brush brush, Color color)
        {
            var _mat = new DiffuseMaterial(brush)
            {
                AmbientColor = color
            };
            _mat.Freeze();
            return _mat;
        }

        protected static DiffuseMaterial SubMaterial(Brush brush, byte seed)
        {
            var _mat = new DiffuseMaterial(brush)
            {
                AmbientColor = Color.FromArgb(255, seed, seed, seed)
            };
            _mat.Freeze();
            return _mat;
        }

        protected virtual Material OnGetMaterial(VisualEffect effect)
        {
            switch (effect)
            {
                case (VisualEffect.Normal):
                case (VisualEffect.DimTo75):
                case (VisualEffect.DimTo50):
                case (VisualEffect.DimTo25):
                case (VisualEffect.Highlighted):
                    {
                        var _brush = GetBrush(VisualEffect.Normal);
                        switch (effect)
                        {
                            case (VisualEffect.DimTo75): return SubMaterial(_brush, 127);
                            case (VisualEffect.DimTo50): return SubMaterial(_brush, 50);
                            case (VisualEffect.DimTo25): return SubMaterial(_brush, 25);
                            case (VisualEffect.Highlighted): return TintMaterial(_brush, Colors.Magenta);
                        }
                        var _material = new DiffuseMaterial(_brush);
                        _material.Freeze();
                        return _material;
                    }

                case (VisualEffect.Brighter):
                case (VisualEffect.MonochromeDim):
                case (VisualEffect.Monochrome):
                case (VisualEffect.FormOnly):
                    {
                        var _material = new DiffuseMaterial(GetBrush(effect));
                        _material.Freeze();
                        return _material;
                    }

                case (VisualEffect.MonoSub1):
                case (VisualEffect.MonoSub2):
                case (VisualEffect.MonoSub3):
                case (VisualEffect.MonoSub4):
                case (VisualEffect.MonoSub5):
                    {
                        var _brush = GetBrush(VisualEffect.Monochrome);
                        switch (effect)
                        {
                            case VisualEffect.MonoSub1: return SubMaterial(_brush, 225);
                            case VisualEffect.MonoSub2: return SubMaterial(_brush, 195);
                            case VisualEffect.MonoSub3: return SubMaterial(_brush, 165);
                            case VisualEffect.MonoSub4: return SubMaterial(_brush, 135);
                            default: return SubMaterial(_brush, 105);
                        }
                    }

                case (VisualEffect.FormSub1):
                case (VisualEffect.FormSub2):
                case (VisualEffect.FormSub3):
                case (VisualEffect.FormSub4):
                case (VisualEffect.FormSub5):
                    {
                        var _brush = GetBrush(VisualEffect.FormOnly);
                        switch (effect)
                        {
                            case VisualEffect.FormSub1: return SubMaterial(_brush, 235);
                            case VisualEffect.FormSub2: return SubMaterial(_brush, 215);
                            case VisualEffect.FormSub3: return SubMaterial(_brush, 195);
                            case VisualEffect.FormSub4: return SubMaterial(_brush, 175);
                            default: return SubMaterial(_brush, 155);
                        }
                    }
            }
            return BrushDefinition.MissingMaterial;
        }

        public Material GetMaterial(VisualEffect effect)
        {
            if (effect == VisualEffect.Unseen)
                return _Black;

            if (!_Materials.ContainsKey(effect) && !NeedsPreGenerate)
                _Materials.Add(effect, OnGetMaterial(effect));
            return _Materials[effect];
        }
        #endregion

        public abstract BrushDefinition Clone();
    }
}
