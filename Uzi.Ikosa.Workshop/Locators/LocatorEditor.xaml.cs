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
using Uzi.Ikosa.Tactical;
using System.ComponentModel;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Ikosa.UI;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LocatorEditor.xaml
    /// </summary>
    public partial class LocatorEditor : TabItem, IPackageItem, IHostedTabItem
    {
        #region construction
        public LocatorEditor(Locator locator, IHostTabControl host)
        {
            InitializeComponent();
            DataContext = this;
            Content = new LocatorEditorControl(locator);
            _Locator = locator;
            _Host = host;
        }
        #endregion

        private IHostTabControl _Host;

        private Locator _Locator;
        public Locator Locator => _Locator; 

        public object PackageItem => _Locator; 

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _Host.RemoveTabItem(this);
        }

        #region IHostedTabItem Members
        public void CloseTabItem()
        {
            (Content as LocatorEditorControl).CloseTabItem();
        }
        #endregion
    }
}