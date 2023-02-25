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
    /// Interaction logic for SlottedContainerItemBaseTab.xaml
    /// </summary>
    public partial class SlottedContainerItemBaseTab : TabItem, IHostedTabItem
    {
        public SlottedContainerItemBaseTab(PresentableSlottedContainerItemVM container, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = container;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public SlottedContainerItemBase ContainerItem 
            => (DataContext as PresentableSlottedContainerItemVM)?.Thing;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed for Weight";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
