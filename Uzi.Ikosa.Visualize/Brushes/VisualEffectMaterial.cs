using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using Uzi.Visualize.Packages;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Material))]
    public class VisualEffectMaterial : MarkupExtension
    {
        [ThreadStatic]
        private static List<IResolveMaterial> _Resolvers = new();

        [ThreadStatic]
        private static List<IPartResolveMaterial> _PartResolvers = new();

        public static List<IPartResolveMaterial> PartResolvers
        {
            get
            {
                if (_PartResolvers == null)
                    _PartResolvers = new List<IPartResolveMaterial>();
                return _PartResolvers;
            }
        }

        public static void PushResolver(IPartResolveMaterial resolver)
        {
            if (!VisualEffectMaterial.PartResolvers.Contains(resolver))
                VisualEffectMaterial.PartResolvers.Insert(0, resolver);
        }

        public static void PullResolver(IPartResolveMaterial resolver)
        {
            if (VisualEffectMaterial.PartResolvers.Contains(resolver))
                VisualEffectMaterial.PartResolvers.Remove(resolver);
        }

        public static List<IResolveMaterial> Resolvers
        {
            get
            {
                if (_Resolvers == null)
                    _Resolvers = new List<IResolveMaterial>();
                return _Resolvers;
            }
        }

        public static void PushResolver(IResolveMaterial resolver)
        {
            if (!VisualEffectMaterial.Resolvers.Contains(resolver))
                VisualEffectMaterial.Resolvers.Insert(0, resolver);
        }

        public static void PullResolver(IResolveMaterial resolver)
        {
            if (VisualEffectMaterial.Resolvers.Contains(resolver))
                VisualEffectMaterial.Resolvers.Remove(resolver);
        }

        #region public static Action<string> ReferencedKey { get { return _KeyFound; } set { _KeyFound = value; } }
        [ThreadStatic]
        private static Action<string> _KeyReferenced = null;

        /// <summary>Thread static tracking callback</summary>
        public static Action<string> ReferencedKey { get => _KeyReferenced; set { _KeyReferenced = value; } }
        #endregion

        public string Key { get; set; }

        public VisualEffect VisualEffect { get; set; }

        /// <summary>Non-null and starts with &quot;#&quot;</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsLiteral(string key)
            => key?.StartsWith(@"#") ?? false;

        /// <summary>Non-null and starts with &quot;#&quot; and contains &quot;|&quot; or &quot;-&quot;</summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static bool IsGradient(string key)
            => IsLiteral(key) 
            && (key.Contains(@"|") || key.Contains(@"-"));

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // TODO: =System.Brush
            if ((_KeyReferenced != null) && !IsLiteral(Key))
                _KeyReferenced(Key);

            return VisualEffectMaterial.ResolveMaterial(Key, VisualEffect);
        }

        public static Material ResolveMaterial(string key, VisualEffect effect)
        {
            try
            {
                if (IsLiteral(key))
                {
                    Color _getColor(string literal) => (Color)ColorConverter.ConvertFromString(literal);

                    if (IsGradient(key))
                    {
                        #region gradient handling
                        string[] _colors = null;
                        LinearGradientBrush _brush = null;

                        // resolve gradients
                        if (key.Contains(@"|"))
                        {
                            // up-down
                            _colors = key.Split('|');
                            _brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0.5, 0),
                                EndPoint = new Point(0.5, 1)
                            };
                        }
                        else
                        {
                            // left-right
                            _colors = key.Split('-');
                            _brush = new LinearGradientBrush
                            {
                                StartPoint = new Point(0, 0.5),
                                EndPoint = new Point(1, 0.5)
                            };
                        }

                        // gradient stops
                        var _stop = 0.0d;
                        var _step = 1.0d / (_colors.Length - 1);
                        foreach (var _c in _colors)
                        {
                            _brush.GradientStops.Add(new GradientStop(_getColor(_c), _stop));
                            _stop += _step;
                        }

                        // return material
                        var _material = new DiffuseMaterial(_brush);
                        _material.Freeze();
                        return _material;
                        #endregion
                    }
                    else
                    {
                        #region solid handling
                        // resolve literal solid color materials
                        var _material = new DiffuseMaterial(new SolidColorBrush(_getColor(key)));
                        _material.Freeze();
                        return _material;
                        #endregion
                    }
                }

                // TODO: =System.Brush
                var _track = new List<IResolveMaterial>();
                foreach (var _res in VisualEffectMaterial.Resolvers)
                {
                    var _rez = _res;
                    while ((_rez != null) && !_track.Contains(_rez))
                    {
                        _track.Add(_rez);
                        var _material = _rez.GetMaterial(key, effect);
                        if (_material != null)
                            return _material;
                        _rez = _rez.IResolveMaterialParent;
                    }
                }

                var _pTrack = new List<IPartResolveMaterial>();
                foreach (var _res in VisualEffectMaterial.PartResolvers)
                {
                    var _rez = _res;
                    while ((_rez != null) && !_pTrack.Contains(_rez))
                    {
                        _pTrack.Add(_rez);
                        var _material = _rez.GetMaterial(key, effect);
                        if (_material != null)
                            return _material;
                        _rez = _rez.IPartResolveMaterialParent;
                    }
                }
            }
            catch
            {
            }

            // return something useful
            return effect == VisualEffect.Highlighted
                ? BrushDefinition.MissingSelectedMaterial
                : BrushDefinition.MissingMaterial;
        }
    }
}
