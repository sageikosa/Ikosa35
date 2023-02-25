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
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewWedgeCorner.xaml
    /// </summary>
    public partial class NewWedgeCorner : Window
    {
        public NewWedgeCorner(LocalMap map, string title)
        {
            InitializeComponent();
            this.Title = title;
            _Map = map;
            this.Resources.Add(@"roomMaterials", map.AllCellMaterials);
        }

        private LocalMap _Map;

        public WedgeCellSpace GetWedgeCellSpace(CellSpace parent)
        {
            return new WedgeCellSpace(cboMaterial.SelectedItem as CellMaterial, cboTiling.SelectedItem as TileSet,
                cboSlvPMaterial.SelectedItem as CellMaterial, cboSlvPTiling.SelectedItem as TileSet,
                _PrimeOffset, _SecondOffset) { Name = txtName.Text, Parent = parent };
        }

        public CornerCellSpace GetCornerCellSpace(CellSpace parent)
        {
            return new CornerCellSpace(cboMaterial.SelectedItem as CellMaterial, cboTiling.SelectedItem as TileSet,
                cboSlvPMaterial.SelectedItem as CellMaterial, cboSlvPTiling.SelectedItem as TileSet,
                _PrimeOffset, _SecondOffset) { Name = txtName.Text, Parent = parent };
        }

        private double _PrimeOffset = 2.5;
        private double _SecondOffset = 2.5;

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

            if (_out <= -5)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Offset cannot be less than -5";
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
            else
            {
                _SecondOffset = _out;
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
