using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Workshop.Locale
{
    /// <summary>
    /// Interaction logic for Axes.xaml
    /// </summary>
    public partial class Axes : UserControl
    {
        public Axes()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public int Heading
        {
            get { return (int)GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register("Heading", typeof(int), typeof(Axes), new UIPropertyMetadata(1));

        public int Incline
        {
            get { return (int)GetValue(InclineProperty); }
            set { SetValue(InclineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Incline.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InclineProperty =
            DependencyProperty.Register("Incline", typeof(int), typeof(Axes), new UIPropertyMetadata(-1));
    }
}
