using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(VisualEffect))]
    public class FrontSenseEffectExtension : SenseEffectExtension
    {
        [ThreadStatic]
        public new static VisualEffect EffectValue = VisualEffect.Normal;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            _EffectReferenced?.Invoke(typeof(FrontSenseEffectExtension));
            return EffectValue;
        }
    }
}
