using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(VisualEffect))]
    public class BottomSenseEffectExtension : SenseEffectExtension
    {
        [ThreadStatic]
        public new static VisualEffect EffectValue = VisualEffect.Normal;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _EffectReferenced?.Invoke(typeof(BottomSenseEffectExtension));
            return EffectValue;
        }
    }
}
