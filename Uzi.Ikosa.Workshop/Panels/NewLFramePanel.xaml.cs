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
    /// Interaction logic for NewLFramePanel.xaml
    /// </summary>
    public partial class NewLFramePanel : Window
    {
        public NewLFramePanel(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            this.Resources.Add(@"roomMaterials", map.AllCellMaterials.OfType<SolidCellMaterial>());
            txtThickness.Text = @"0.25";
            txtHorizontal.Text = @"1.5";
            txtVertical.Text = @"2.5";
        }

        private LocalMap _Map;
        private double _Thickness = 0.25;
        private double _Horizontal = 1.5d;
        private double _Vertical = 2.5d;

        public LFramePanel GetLFramePanel()
        {
            return new LFramePanel(txtName.Text, cboMaterial.SelectedItem as SolidCellMaterial,
                cboTiling.SelectedItem as TileSet, _Thickness, _Horizontal, _Vertical);
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

            if (_txt == txtThickness)
            {
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
            }
            else if ((_txt == txtHorizontal) || (_txt == txtVertical))
            {
                if (_out < 0.25)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Offset cannot be less then 0.25";
                    return;
                }
                if (_out > 5)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Offset cannot exceed 5";
                    return;
                }

                if (_txt == txtHorizontal)
                {
                    _Horizontal = _out;
                }
                else if (_txt == txtVertical)
                {
                    _Vertical = _out;
                }
            }
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

            if (txtHorizontal.Tag != null)
            {
                txtMessage.Text = @"Horizontal is invalid";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            if (txtVertical.Tag != null)
            {
                txtMessage.Text = @"Vertical is invalid";
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
