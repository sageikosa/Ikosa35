using System;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(VisualEffect))]
    public class BackSenseEffectExtension : SenseEffectExtension
    {
        [ThreadStatic]
        public new static VisualEffect EffectValue = VisualEffect.Normal;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _EffectReferenced?.Invoke(typeof(BackSenseEffectExtension));
            return EffectValue;
        }
    }
}
