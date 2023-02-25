using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Proxy
{
    public class NotificationVM : ViewModelBase
    {
        public NotificationVM()
        {
            _Topics = new List<NotificationTopic>();
            _Clear = new RelayCommand(() => Clear());
            _TopicRemover = new RelayCommand<NotificationTopic>((topic) => Remove(topic));
        }

        #region data
        private List<NotificationTopic> _Topics;
        private readonly RelayCommand _Clear;
        private readonly RelayCommand<NotificationTopic> _TopicRemover;
        #endregion

        public RelayCommand ClearCommand => _Clear;

        #region public IEnumerable<NotificationTopic> Topics { get; }
        public IEnumerable<NotificationTopic> Topics
        {
            get
            {
                // implicitly type
                var _snapShot = _Topics;
                lock (_Topics)
                {
                    // copy
                    _snapShot = _Topics.Select(_t => _t).ToList();
                }

                // return
                return _snapShot;
            }
        }
        #endregion

        public void Clear()
        {
            // create new
            var _topics = new List<NotificationTopic>();
            _Topics = _topics;
            DoPropertyChanged(nameof(Topics));
        }

        #region public void Remove(NotificationTopic topic)
        public void Remove(NotificationTopic topic)
        {
            var _removed = false;
            lock (_Topics)
            {
                if (_Topics.Contains(topic))
                {
                    _removed = true;
                    _Topics.Remove(topic);
                }
            }

            // update topics
            if (_removed)
                DoPropertyChanged(nameof(Topics));
        }
        #endregion

        public void Add(SysNotify sysNotify)
            => AddRange(new SysNotify[] { sysNotify });

        public void AddRange(IEnumerable<SysNotify> sysNotifies)
        {
            NotificationTopic _topic = null;
            var _newTopics = false;
            foreach (var _sysNotify in sysNotifies)
            {
                lock (_Topics)
                {
                    _topic = _Topics.FirstOrDefault(_t => _t.Topic == _sysNotify.Topic);
                    if (_topic == null)
                    {
                        // add to copy of current
                        _topic = new NotificationTopic(_sysNotify.Topic, _TopicRemover);
                        _Topics.Insert(0, _topic);

                        // any new topics being made?
                        _newTopics = true;
                    }
                    else
                    {
                        // bump to top
                        _Topics.Remove(_topic);
                        _Topics.Insert(0, _topic);
                    }
                }

                // add notify to topic
                _topic.Add(_sysNotify);
            }

            // update topics
            if (_newTopics)
                DoPropertyChanged(nameof(Topics));
        }
    }
}
