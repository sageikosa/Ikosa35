using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Proxy
{
    public class SysNotifyVM
    {
        public SysNotifyVM(long id, SysNotify sysNotify, RelayCommand<SysNotify> remover)
        {
            _ID = id;
            _Notify = sysNotify;
            _Remover = remover;
        }

        #region data
        private readonly long _ID;
        private readonly SysNotify _Notify;
        private readonly RelayCommand<SysNotify> _Remover;
        #endregion

        public long ID => _ID;
        public SysNotify Notify => _Notify;
        public RelayCommand<SysNotify> Remover => _Remover;
    }
}
