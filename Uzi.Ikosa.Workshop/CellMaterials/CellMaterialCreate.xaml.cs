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
    /// Interaction logic for CellMaterialCreate.xaml
    /// </summary>
    public partial class CellMaterialCreate : Window
    {
        public CellMaterialCreate(LocalMap map)
        {
            InitializeComponent();
            _Map = map;
        }

        private LocalMap _Map;

        public CellMaterial CreatedMaterial()
        {
            if (!string.IsNullOrEmpty(txtName.Text))
            {
                switch (cboCMType.SelectedIndex)
                {
                    case 0:
                        if (cboSolidTemplate.SelectedItem != null)
                        {
                            return new SolidCellMaterial(txtName.Text, cboSolidTemplate.SelectedItem as Uzi.Ikosa.Items.Materials.Material, _Map);
                        }
                        else
                        {
                            MessageBox.Show(@"No solid material template selected");
                            break;
                        }
                    case 1:
                        return new LiquidCellMaterial(txtName.Text, @"4d8*10", false, -2, true, true, _Map);
                    default:
                        return new GasCellMaterial(txtName.Text, _Map);
                }
            }
            else
            {
                MessageBox.Show(@"No name provided");
            }
            return null;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
