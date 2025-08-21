using System.Windows;

namespace Uzi.Visualize
{
    public class OpenableState
    {
        public static double GetOpenableValue(DependencyObject obj)
        {
            return (double)obj.GetValue(ValueProperty);
        }

        public static void SetOpenableValue(DependencyObject obj, double value)
        {
            obj.SetValue(ValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for Rotation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(@"Value", typeof(double), typeof(OpenableState),
            new UIPropertyMetadata(0d));
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.RegisterAttached(@"Value2", typeof(double), typeof(OpenableState),
            new UIPropertyMetadata(0d));
        public static readonly DependencyProperty Value3Property =
            DependencyProperty.RegisterAttached(@"Value3", typeof(double), typeof(OpenableState),
            new UIPropertyMetadata(0d));
        public static readonly DependencyProperty Value4Property =
            DependencyProperty.RegisterAttached(@"Value4", typeof(double), typeof(OpenableState),
            new UIPropertyMetadata(0d));
        public static readonly DependencyProperty Value5Property =
            DependencyProperty.RegisterAttached(@"Value5", typeof(double), typeof(OpenableState),
            new UIPropertyMetadata(0d));
    }
}
