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
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for AmmoBundleEditor.xaml
    /// </summary>
    public partial class AmmoBundleEditor : UserControl
    {
        public AmmoBundleEditor()
        {
            InitializeComponent();
            DataContextChanged += AmmoBundleEditor_DataContextChanged;
        }

        private IAmmunitionBundle _Bundle => (DataContext as PresentableContext)?.CoreObject as IAmmunitionBundle;
        private PresentableCreatureVM _Possessor => (DataContext as PresentableContext)?.Possessor;

        private void AmmoBundleEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_Bundle != null)
            {
                var _bundle = _Bundle;
                cboSize.SelectedIndex = _bundle.ItemSizer.ExpectedCreatureSize.Order + 4;
            }
        }

        private void btnContents_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new AmmoContainerContentsList()
            {
                DataContext = new AmmoBundleVM(_Bundle, _Possessor),
                Owner = Window.GetWindow(this)
            };
            _dlg.ShowDialog();
            e.Handled = true;
        }

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Bundle != null)
            {
                var _bundle = _Bundle;
                var _cbiSize = cboSize.SelectedItem as ComboBoxItem;
                _bundle.ItemSizer.ExpectedCreatureSize = _cbiSize.Tag as Size;
            }
        }
    }
}
