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
    /// Interaction logic for ContainerItemBaseEditor.xaml
    /// </summary>
    public partial class ContainerItemBaseEditor : UserControl
    {
        public ContainerItemBaseEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(_DataContextChanged);
        }

        private ContainerItemBase _Container => (DataContext as PresentableContainerItemVM)?.Thing;

        void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _cont = _Container;
            if (_cont != null)
            {
                cboSize.SelectedIndex = _cont.ItemSizer.ExpectedCreatureSize.Order + 4;
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
            if (_Container != null)
            {
                var _cbItem = cboSize.SelectedItem as ComboBoxItem;
                _Container.ItemSizer.ExpectedCreatureSize = _cbItem.Tag as Size;
            }
        }
    }
}
