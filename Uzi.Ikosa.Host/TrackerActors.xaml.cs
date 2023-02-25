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
using System.Windows.Shapes;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Services;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Host
{
    /// <summary>
    /// Interaction logic for TrackerActors.xaml
    /// </summary>
    public partial class TrackerActors : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();

        public TrackerActors(IList<CreatureLoginInfo> creatures)
        {
            InitializeComponent();
            lstActors.ItemsSource = creatures;
        }

        public IEnumerable<CreatureLoginInfo> GetCreatures()
        {
            foreach (var _item in lstActors.SelectedItems)
            {
                yield return _item as CreatureLoginInfo;
            }
            yield break;
        }

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstActors != null) && (lstActors.SelectedItems.Count > 0);
            e.Handled = true;
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            e.Handled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
            e.Handled = true;
        }
    }
}
