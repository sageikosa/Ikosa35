using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media.Effects;
using System.Windows.Media;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Effect))]
    public class IkosaVisualEffect : MarkupExtension
    {
        public VisualEffect VisualEffect { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            switch (VisualEffect)
            {
                case VisualEffect.Monochrome:
                    return new MonochromeEffect { FilterColor = Colors.White };
                case VisualEffect.FormOnly:
                    return new EmbossedEffect
                    {
                        Amount = 5,
                        Width = 0.001
                    };
                case VisualEffect.DimTo75:
                    return new DarknessEffect { DarknessFactor = 0.5 };
                case VisualEffect.DimTo50:
                    return new DarknessEffect { DarknessFactor = 0.2 };
                case VisualEffect.DimTo25:
                    return new DarknessEffect { DarknessFactor = 0.1 };
                case VisualEffect.MonochromeDim:
                    return new MonochromeEffect { FilterColor = Color.FromArgb(255, 32, 32, 32) };
                case Visualize.VisualEffect.Brighter:
                    return new DarknessEffect { DarknessFactor = 1.3 };
                case VisualEffect.Unseen:
                    return new DarknessEffect { DarknessFactor = 0 };
                case VisualEffect.Highlighted:
                    return new MonochromeEffect { FilterColor = Colors.Magenta };
            }
            return null;
        }
    }
}
