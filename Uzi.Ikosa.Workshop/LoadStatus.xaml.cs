using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LoadStatus.xaml
    /// </summary>
    public partial class LoadStatus : Window
    {
        private static LoadStatus _Status;
        private int _Count = 0;

        public static void StartLoadStatusWindow()
        {
            // Create a thread
            var _thread = new Thread(new ThreadStart(() =>
            {
                // Create our context, and install it:
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(
                        Dispatcher.CurrentDispatcher));

                _Status = new LoadStatus();
                // When the window closes, shut down the dispatcher
                _Status.Closed += (s, e) =>
                   Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                BasePartHelper.LoadMessage =
                (msg) =>
                {
                    _Status?.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            _Status._Count++;
                            _Status.tbStatus.Text = $@"{_Status._Count}: {msg}";
                            //Debug.WriteLine($@"{_Status._Count}: {msg}");
                        }));
                };

                _Status.Show();
                // Start the Dispatcher Processing
                Dispatcher.Run();
            }));

            // Setup and start thread as before
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public static void StopLoadStatusWindow()
        {
            _Status?.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    _Status.Close();
                    BasePartHelper.LoadMessage = null;
                    _Status = null;
                }));
        }

        private LoadStatus()
        {
            InitializeComponent();
        }


    }
}
