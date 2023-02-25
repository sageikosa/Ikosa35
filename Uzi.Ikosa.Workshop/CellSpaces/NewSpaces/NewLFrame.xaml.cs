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
    /// Interaction logic for NewLFrame.xaml
    /// </summary>
    public partial class NewLFrame : Window
    {
        public NewLFrame(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            this.Resources.Add(@"roomMaterials", map.AllCellMaterials);
        }

        public LFrame GetLFrame(CellSpace parent)
        {
            return new LFrame(cboMaterial.SelectedItem as CellMaterial, cboTiling.SelectedItem as TileSet,
                cboSlvPMaterial.SelectedItem as CellMaterial, cboSlvPTiling.SelectedItem as TileSet,
                _PrimeOffset, _SecondOffset, _Thickness) { Name = txtName.Text, Parent = parent };
        }

        private LocalMap _Map;
        private double _PrimeOffset = 2.5;
        private double _SecondOffset = 2.5;
        private double _Thickness = 1;

        #region double text field validation
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

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Offset cannot be less than 0";
                return;
            }
            if (_out >= 5)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Offset cannot exceed 5";
                return;
            }

            if (_txt == txtOffset)
            {
                _PrimeOffset = _out;
            }
            else if (_txt == txtSecondOffset)
            {
                _SecondOffset = _out;
            }
            else
            {
                _Thickness = _out;
            }
            _txt.ToolTip = null;
            _txt.Tag = null;
        }
        #endregion

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
            if (string.IsNullOrEmpty(txtName.Text) || (cboMaterial.SelectedItem == null) || (cboSlvPMaterial.SelectedItem == null))
            {
                txtMessage.Text = @"Missing name and/or material";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            if ((txtOffset.Tag != null) || (txtSecondOffset.Tag != null))
            {
                txtMessage.Text = @"Offset is invalid";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            txtMessage.Text = string.Empty;
            e.CanExecute = _Map.CanUseName(txtName.Text, typeof(CellSpace));
            e.Handled = true;
        }
    }
}
