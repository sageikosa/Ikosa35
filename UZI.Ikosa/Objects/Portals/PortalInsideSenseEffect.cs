using System.Windows;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    public class PortalInsideSenseEffect
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
            DependencyProperty.RegisterAttached(@"Value", typeof(VisualEffect), typeof(PortalInsideSenseEffect),
            new UIPropertyMetadata(VisualEffect.Normal));
    }
}
