using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Proxy
{
    public class NotificationTopic : ViewModelBase
    {
        public NotificationTopic(string topic, RelayCommand<NotificationTopic> remover)
        {
            _Topic = topic;
            _Notifies = new List<SysNotify>();
            _Remover = remover;
            _NotifyRemover = new RelayCommand<SysNotify>((notify) => Remove(notify));
        }

        #region data
        private readonly List<SysNotify> _Notifies;
        private readonly string _Topic;
        private readonly RelayCommand<NotificationTopic> _Remover;
        private readonly RelayCommand<SysNotify> _NotifyRemover;
        #endregion

        public string Topic => _Topic;
        public RelayCommand<NotificationTopic> Remover => _Remover;

        public IEnumerable<SysNotifyVM> SysNotifyVMs
        {
            get
            {
                // implicitly type
                List<SysNotifyVM> _notifies = null;
                lock (_Notifies)
                {
                    // copy
                    _notifies = _Notifies.Select(_n => new SysNotifyVM(0, _n, _NotifyRemover)).ToList();
                }

                // return
                return _notifies;
            }
        }

        public void Add(SysNotify notify)
        {
            lock (_Notifies)
            {
                // snapshot
                switch (notify)
                {
                    case ConditionNotify _condition:
                        if (_condition.IsEnding)
                        {
                            var _cEnd = _condition.Infos.FirstOrDefault()?.Message ?? string.Empty;
                            var _old = (from _c in _Notifies.OfType<ConditionNotify>()
                                        where _c.Infos.Any(_i => _i.Message == _cEnd)
                                        select _c).FirstOrDefault();
                            if (_old != null)
                            {
                                _Notifies.Remove(_old);
                            }
                            if (_Notifies.Count == 0)
                                Remover?.Execute(this);
                        }
                        else
                        {
                            _Notifies.Insert(0, notify);
                        }
                        break;

                    default:
                        _Notifies.Insert(0, notify);
                        break;
                }
            }
            DoPropertyChanged(nameof(SysNotifyVMs));
        }

        public void Remove(SysNotify notify)
        {
            lock (_Notifies)
            {
                if (_Notifies.Contains(notify))
                    _Notifies.Remove(notify);
                if (_Notifies.Count == 0)
                    Remover?.Execute(this);
            }
            DoPropertyChanged(nameof(SysNotifyVMs));
        }
    }
}
