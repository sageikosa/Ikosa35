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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Items.Wealth;
using System.Collections;
using Uzi.Ikosa.Items.Materials;
using Uzi.Core.Dice;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for GemEditor.xaml
    /// </summary>
    public partial class GemEditor : UserControl
    {
        public GemEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(GemEditor_DataContextChanged);
        }

        private Gem Gem => (DataContext as GemVM)?.Thing;

        void GemEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Gem != null)
                cboMaterial.SelectedItem = cboMaterial.Items.OfType<ComboBoxItem>().FirstOrDefault(_cbi => _cbi.Tag.GetType().Equals(Gem.ItemMaterial.GetType()));
        }

        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Gem != null)
                Gem.ItemMaterial = (cboMaterial.SelectedItem as ComboBoxItem).Tag as Material;
        }

        #region Double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!double.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Cannot be zero or negative";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region Double text field validation
        private void txtDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!decimal.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Cannot be zero or negative";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        private void btnRandomPrice_Click(object sender, RoutedEventArgs e)
        {
            Gem.Price.CorePrice = (btnRandomPrice.Content as Roller).RollValue(Guid.Empty, @"Gem", @"Value");
        }
    }
}
