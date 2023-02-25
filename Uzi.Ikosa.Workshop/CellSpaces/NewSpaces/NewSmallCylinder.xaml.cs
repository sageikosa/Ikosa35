using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewSmallCylinder.xaml
    /// </summary>
    public partial class NewSmallCylinder : Window
    {
        public NewSmallCylinder(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
            Resources.Add(@"solidMaterials", map.AllCellMaterials.OfType<SolidCellMaterial>().ToList());
            Resources.Add(@"fillMaterials", map.AllCellMaterials.Where(_m => !(_m is SolidCellMaterial)).ToList());
        }

        private LocalMap _Map;

        public SmallCylinderSpace GetSmallCylinderSpace(CellSpace parent)
        {
            var _cSpace = new SmallCylinderSpace(
                cboSolidMaterial.SelectedItem as SolidCellMaterial, cboSolidTiling.SelectedItem as TileSet,
                cboFillMaterial.SelectedItem as CellMaterial, cboFillTiling.SelectedItem as TileSet)
            {
                Name = txtName.Text,
                Parent = parent
            };
            return _cSpace;
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
                || (cboSolidMaterial.SelectedItem == null)
                || (cboFillMaterial.SelectedItem == null))
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
