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
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CloseableContainerTab.xaml
    /// </summary>
    public partial class CloseableContainerTab : TabItem, IHostedTabItem
    {
        public CloseableContainerTab(PresentableCloseableContainerVM container, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = container;
            _Host = host;
        }

        private readonly IHostTabControl _Host;

        // TODO: data context must be the tab, CloseableContainerObject must be held as a private variable
        public CloseableContainerObject CloseableContainerObject => (DataContext as PresentableCloseableContainerVM).Thing;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
