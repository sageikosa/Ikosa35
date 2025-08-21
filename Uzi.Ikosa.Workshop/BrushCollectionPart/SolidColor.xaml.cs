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
using System.Windows.Shapes;
using Uzi.Visualize;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewSolidColor.xaml
    /// </summary>
    public partial class SolidColor : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();

        public SolidColor(BrushCollection manager, string name, Color solidColor, bool exists)
        {
            InitializeComponent();
            _Manager = manager;
            _Exists = exists;
            cpSolid.SelectedColor = solidColor;
            if (_Manager == null)
            {
                lblBrushKey.Visibility = System.Windows.Visibility.Collapsed;
                txtBrushKey.Visibility = System.Windows.Visibility.Collapsed;
                txtBrushKey.IsEnabled = false;
                cpSolid.UsingAlphaChannel = true;
            }
            else
            {
                cpSolid.UsingAlphaChannel = false;
            }

            txtBrushKey.Text = name;
        }

        private BrushCollection _Manager;
        private bool _Exists;

        public BrushDefinition GetBrushDefinition()
        {
            return new SolidBrushDefinition { BrushKey = txtBrushKey.Text, Color = cpSolid.SelectedColor ?? Colors.Black };
        }

        private void cbOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_Manager == null)
            {
                e.CanExecute = true;
            }
            else if (txtBrushKey != null)
            {
                var _key = txtBrushKey.Text;
                if (!string.IsNullOrEmpty(_key))
                {
                    e.CanExecute = _Exists || !_Manager.Any(_b => _b.BrushKey.Equals(_key));
                }
            }
            e.Handled = true;
        }

        private void cbOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
