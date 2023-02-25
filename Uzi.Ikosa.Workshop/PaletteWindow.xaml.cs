using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for PaletteWindow.xaml
    /// </summary>
    public partial class PaletteWindow : Window
    {
        public PaletteWindow(RibbonToggleButton controller, Control control, string title)
        {
            InitializeComponent();
            _Controller = controller;
            txtTitle.Text = title;
            gridPlaceHolder.Children.Add(control);
        }

        RibbonToggleButton _Controller = null;

        private void brdChrome_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Controller.IsChecked = false;
        }
    }
}
