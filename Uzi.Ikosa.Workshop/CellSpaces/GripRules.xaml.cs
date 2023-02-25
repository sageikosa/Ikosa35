using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for GripRules.xaml
    /// </summary>
    public partial class GripRules : UserControl
    {
        public GripRules()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            var _space = DataContext as CellSpace;
            var _materials = _space.AllMaterials.ToList();
            if (_materials.Count > 1)
            {
                _space.GripRules.InitializeMaterials(_materials[0], _materials[1]);
            }
            else
            {
                _space.GripRules.InitializeUniform(_space.CellMaterial);
            }
            lstRules.Items.Refresh();
        }
    }
}
