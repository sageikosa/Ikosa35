using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client
{
    /// <summary>
    /// Interaction logic for MasterLogView.xaml
    /// </summary>
    public partial class MasterLogView : Window
    {
        private MasterLogView(IsMasterModel isMaster)
        {
            InitializeComponent();
            var _log = new MasterLog(isMaster, Dispatcher,
                () => Close(),
                () =>
                {
                    if (WindowState == WindowState.Minimized)
                        WindowState = WindowState.Normal;
                    Activate();
                });
            isMaster.MasterLog = _log;
            DataContext = _log;
            Closed += MasterLogView_Closed;
            logMaster.PrincipalPrefixer = (id) => isMaster.Proxies.GetPrincipalPrefix(id);
        }

        public MasterLog MasterLog => DataContext as MasterLog;

        private void MasterLogView_Closed(object sender, EventArgs e)
        {
            try
            {
                MasterLog.Master.MasterLog = null;
                Closed -= MasterLogView_Closed;
                Dispatcher.InvokeShutdown();
            }
            catch
            {
            }
        }

        public static void StartMasterLog(IsMasterModel master)
        {
            var _start = new Thread(() => DoStartMasterLog(master));
            _start.SetApartmentState(ApartmentState.STA);
            _start.Start();
        }

        private static void DoStartMasterLog(IsMasterModel master)
        {
            var _view = new MasterLogView(master);
            _view.Show();
            System.Windows.Threading.Dispatcher.Run();
        }
    }
}
