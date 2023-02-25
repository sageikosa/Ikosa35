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
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for TurnTrackerPanel.xaml
    /// </summary>
    public partial class TurnTrackerPanel : UserControl
    {
        public TurnTrackerPanel()
        {
            try { InitializeComponent(); } catch { }
        }

        private IsMasterModel MasterModel => DataContext as IsMasterModel;
    }
}
