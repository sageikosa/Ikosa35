using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for AddActor.xaml
    /// </summary>
    public partial class AddActor : Window
    {
        public AddActor(ResourceDictionary resources, ProxyModel proxies)
        {
            InitializeComponent();
            _Resources = resources;
            _Proxies = proxies;
            FetchAvailable();
        }

        private ProxyModel _Proxies = null;
        private ResourceDictionary _Resources = null;

        #region private void FetchAvailable()
        private void FetchAvailable()
        {
            try
            {
                // get available
                var _critters = _Proxies.GetAvailableCreatures();
                lstCritters.ItemsSource = _critters;
                lstCritters.Focus();
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                _Proxies = null;
            }
        }
        #endregion

        #region private void cbLogin_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbLogin_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (lstCritters != null)
                e.CanExecute = lstCritters.SelectedItem != null;
            e.Handled = true;
        }
        #endregion

        #region private void cbLogin_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbLogin_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (lstCritters.SelectedItem is CreatureLoginInfo _critter)
                {
                    _Proxies.LoginCreature(_critter,
                        (found) => ActorView.StartActorView(new ActorModel(_Proxies, found, true)),
                        (master) => MasterLogView.StartMasterLog(master));
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                _Proxies = null;
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
