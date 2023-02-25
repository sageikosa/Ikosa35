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
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for AmmoContainerEditor.xaml
    /// </summary>
    public partial class AmmoContainerEditor : UserControl
    {
        public AmmoContainerEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(AmmoContainerEditor_DataContextChanged);
        }

        private IAmmunitionContainer _AmmoContainer
            => (DataContext as PresentableContext)?.CoreObject as IAmmunitionContainer;

        private PresentableCreatureVM _Possessor => (DataContext as PresentableContext)?.Possessor;

        void AmmoContainerEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_AmmoContainer != null)
            {
                var _aCont = _AmmoContainer;
                cboSize.SelectedIndex = _aCont.ItemSizer.ExpectedCreatureSize.Order + 4;
            }
        }

        #region double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!double.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed";
                _txt.Text = 0.ToString();
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_AmmoContainer != null)
            {
                var _cbiSize = cboSize.SelectedItem as ComboBoxItem;
                _AmmoContainer.ItemSizer.ExpectedCreatureSize = _cbiSize.Tag as Size;
            }
        }

        private void btnContents_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new AmmoContainerContentsList()
            {
                DataContext = new AmmoBundleVM(_AmmoContainer, _Possessor),
                Owner = Window.GetWindow(this)
            };
            _dlg.ShowDialog();
            e.Handled = true;
        }
    }
}
