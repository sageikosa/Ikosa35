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
using Uzi.Ikosa.TypeListers;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ContainerItemBaseTab.xaml
    /// </summary>
    public partial class ContainerItemBaseTab : TabItem, IHostedTabItem
    {
        public ContainerItemBaseTab(PresentableContainerItemVM container, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = container;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        public ContainerItemBase ContainerItem => (DataContext as PresentableContainerItemVM)?.Thing;

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
