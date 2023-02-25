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
using Uzi.Ikosa.UI;
using Uzi.Ikosa.Services;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for UsersTab.xaml
    /// </summary>
    public partial class UsersTab : TabItem, IPackageItem, IHostedTabItem
    {
        public UsersTab(UserDefinitionsPart users, IHostTabControl host)
        {
            InitializeComponent();
            _Users = users;
            _Host = host;
            this.DataContext = _Users;
        }

        private IHostTabControl _Host;
        private UserDefinitionsPart _Users;

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IPackageItem Members

        public object PackageItem
        {
            get { return _Users; }
        }

        #endregion

        #region IHostedTabItem Members
        public void CloseTabItem() { }
        #endregion
    }
}
