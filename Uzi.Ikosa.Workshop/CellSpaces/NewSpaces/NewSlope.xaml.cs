using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewSlope.xaml
    /// </summary>
    public partial class NewSlope : Window
    {
        public NewSlope(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            Resources.Add(@"roomMaterials", map.AllCellMaterials);
        }

        private LocalMap _Map;

        public SlopeCellSpace GetSlopeCellSpace(CellSpace parent)
        {
            var _cSpace = new SlopeCellSpace(cboMaterial.SelectedItem as CellMaterial, cboTiling.SelectedItem as TileSet,
                cboSlvPMaterial.SelectedItem as CellMaterial, cboSlvPTiling.SelectedItem as TileSet)
            {
                Name = txtName.Text,
                Parent = parent
            };
            _cSpace.CellEdge.Material = cboEdgeMaterial.SelectedItem as CellMaterial;
            _cSpace.CellEdge.Tiling = cboEdgeTiling.SelectedItem as TileSet;
            _cSpace.CellEdge.Width = sldrOffset.Value;
            return _cSpace;
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

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Offset cannot be 0 or negative";
                return;
            }
            if (_out >= 5)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Offset cannot exceed 5";
                return;
            }
            _txt.ToolTip = null;
            _txt.Tag = null;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // values
            if (string.IsNullOrEmpty(txtName.Text)
                || (cboMaterial.SelectedItem == null)
                || (cboSlvPMaterial.SelectedItem == null)
                || (cboEdgeMaterial.SelectedItem == null))
            {
                txtMessage.Text = @"Missing name and/or material";
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
