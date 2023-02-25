using System.Windows;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    public class PortalOutsideSenseEffect
    {
        public static VisualEffect GetVisualEffectValue(DependencyObject obj)
        {
            return (VisualEffect)obj.GetValue(ValueProperty);
        }

        public static void SetVisualEffectValue(DependencyObject obj, VisualEffect value)
        {
            obj.SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(@"Value", typeof(VisualEffect), typeof(PortalOutsideSenseEffect),
            new UIPropertyMetadata(VisualEffect.Normal));
    }
}
