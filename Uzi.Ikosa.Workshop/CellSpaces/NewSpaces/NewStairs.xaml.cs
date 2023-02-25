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
    /// Interaction logic for NewStairs.xaml
    /// </summary>
    public partial class NewStairs : Window
    {
        public NewStairs(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            this.Resources.Add(@"roomMaterials", map.AllCellMaterials);
        }

        private LocalMap _Map;

        public Stairs GetStairs(CellSpace parent)
        {
            return new Stairs(cboMaterial.SelectedItem as CellMaterial, cboTiling.SelectedItem as TileSet,
                cboPlusMaterial.SelectedItem as CellMaterial, cboPlusTiling.SelectedItem as TileSet,
                int.Parse(txtSteps.Text))
            {
                Name = txtName.Text
            };
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_Map != null)
            {
                e.CanExecute = _Map.CanUseName(txtName.Text, typeof(CellSpace));
            }
            e.Handled = true;
        }

        #region Int32 text field validation
        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            int _out = 0;
            if (!int.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 2)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Need at least 2 steps";
                _txt.Text = 0.ToString();
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion
    }
}
