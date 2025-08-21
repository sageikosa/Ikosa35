using System;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(VisualEffect))]
    public class SenseEffectExtension:MarkupExtension
    {
        [ThreadStatic]
        public static VisualEffect EffectValue = VisualEffect.Normal;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _EffectReferenced?.Invoke(typeof(SenseEffectExtension));
            return EffectValue;
        }

        #region public static Action<Type> ReferencedEffect { get { return _EffectReferenced; } set { _EffectReferenced = value; } }
        [ThreadStatic]
        protected static Action<Type> _EffectReferenced = null;

        /// <summary>Thread static tracking callback</summary>
        public static Action<Type> ReferencedEffect { get { return _EffectReferenced; } set { _EffectReferenced = value; } }
        #endregion
    }
}
