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
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewNormalPanel.xaml
    /// </summary>
    public partial class NewNormalPanel : Window
    {
        public NewNormalPanel(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            this.Resources.Add(@"roomMaterials", map.AllCellMaterials.OfType<SolidCellMaterial>());
            txtThickness.Text = @"0.25";
        }

        private LocalMap _Map;
        private double _Thickness = 0.25;

        public NormalPanel GetNormalPanel()
        {
            return new NormalPanel(txtName.Text, cboMaterial.SelectedItem as SolidCellMaterial, 
                cboTiling.SelectedItem as TileSet, _Thickness);
        }

        private void txtDouble_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Thickness cannot be negative";
                return;
            }
            if (_out > 0.25)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Thickness cannot exceed 0.25";
                return;
            }

            _Thickness = _out;
            _txt.ToolTip = null;
            _txt.Tag = null;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // values
            if (string.IsNullOrEmpty(txtName.Text) || (cboMaterial.SelectedItem == null))
            {
                txtMessage.Text = @"Missing name and/or material";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            if (txtThickness.Tag != null)
            {
                txtMessage.Text = @"Thickness is invalid";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            txtMessage.Text = string.Empty;
            e.CanExecute = _Map.CanUseName(txtName.Text, typeof(BasePanel));
            e.Handled = true;
        }
    }
}
