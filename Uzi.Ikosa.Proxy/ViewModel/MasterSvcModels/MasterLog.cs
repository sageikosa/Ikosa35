using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MasterLog : ViewModelBase
    {
        public MasterLog(IsMasterModel masterModel, Dispatcher dispatcher, Action shutdown, Action doFocus)
        {
            _Master = masterModel;
            _Dispatcher = dispatcher;
            _Shutdown = shutdown;
            _DoFocus = doFocus;
            _Notifies = new ObservableCollection<SysNotifyVM>();
        }

        #region data
        private readonly IsMasterModel _Master;
        private readonly Dispatcher _Dispatcher;
        private readonly Action _Shutdown;
        private readonly Action _DoFocus;
        private readonly ObservableCollection<SysNotifyVM> _Notifies;
        private long _NotifyID = 0;
        #endregion

        public IsMasterModel Master => _Master;
        public ProxyModel Proxies => _Master.Proxies;

        public Dispatcher Dispatcher => _Dispatcher;
        public void DoShutdown() => _Dispatcher.BeginInvoke(DispatcherPriority.Normal, _Shutdown);
        public void DoFocus() => _Dispatcher.BeginInvoke(DispatcherPriority.Normal, _DoFocus);
        public ObservableCollection<SysNotifyVM> Notifies => _Notifies;

        public long GetNextNotifyID()
        {
            _NotifyID++;
            return _NotifyID;
        }
    }
}
