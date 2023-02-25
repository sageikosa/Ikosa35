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
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for LogItems.xaml
    /// </summary>
    public partial class LogItems : UserControl
    {
        public static RoutedCommand ClearLogCommand = new RoutedCommand();

        public LogItems()
        {
            try { InitializeComponent(); } catch { }
        }

        #region public ProxyModel Proxies { get; set; } (DEPENDENCY)
        public ProxyModel Proxies
        {
            get => GetValue(ProxiesProperty) as ProxyModel;
            set => SetValue(ProxiesProperty, value);
        }

        // Using a DependencyProperty as the backing store for Proxies.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProxiesProperty =
            DependencyProperty.Register(@"Proxies", typeof(ProxyModel), typeof(LogItems), new PropertyMetadata(null));
        #endregion

        private bool IsLoggedIn
            => Proxies?.IsLoggedIn ?? false;

        #region ClearLog
        private void cbClearLog_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsLoggedIn;
            e.Handled = true;
        }

        private void cbClearLog_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ActorModel)?.ClearNotifications();
            e.Handled = true;
        }
        #endregion
    }
}
