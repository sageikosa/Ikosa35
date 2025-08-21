using Newtonsoft.Json;
using System;

namespace Uzi.Core
{
    /// <summary>
    /// Co-linkable object, to be used as a base class.  LinkableObjects allow hooks to be made to prevent de-linking.
    /// Only the BiDiPtr references the internal linking mechanism.
    /// </summary>
    /// <typeparam name="LinkType">Represents the type to which the linkable object may link</typeparam>
    [Serializable]
    public abstract class LinkableDock<LinkType> : IControlChange<LinkType>
        where LinkType : class, ILinkOwner<LinkableDock<LinkType>>
    {
        #region construction
        protected LinkableDock(string propName)
        {
            _ChangeCtrl = new ChangeController<LinkType>(this, default(LinkType));
            PropertyName = propName;
        }
        #endregion

        #region state
        private LinkType _Link = default;
        private readonly ChangeController<LinkType> _ChangeCtrl;
        #endregion

        protected LinkType Link => _Link;

        internal LinkType InternalLink => _Link;

        public string PropertyName { get; private set; }

        protected virtual void OnPreLink(LinkType newVal) { }
        protected virtual void OnLink() { }
        protected virtual bool OnWillAbortChange(LinkType newVal) => false;

        #region internal void LinkToObject(LinkType newVal)
        internal void LinkToObject(LinkType newVal)
        {
            // remember old link
            var _dropLink = _Link;

            // pre, change and post
            OnPreLink(newVal);

            // set and notify new link
            _Link = newVal;
            if (_Link != null)
            {
                _Link.LinkAdded(this);
            }

            OnLink();

            // notify old link
            if (_dropLink != null)
            {
                _dropLink.LinkDropped(this);
            }

            // notify all else
            _ChangeCtrl.DoValueChanged(newVal);
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(PropertyName));
        }
        #endregion

        internal bool WillAbortChange(LinkType newVal)
            => OnWillAbortChange(newVal) && _ChangeCtrl.WillAbortChange(newVal);

        public void AddChangeMonitor(IMonitorChange<LinkType> monitor)
            => _ChangeCtrl.AddChangeMonitor(monitor);

        public void RemoveChangeMonitor(IMonitorChange<LinkType> monitor)
            => _ChangeCtrl.RemoveChangeMonitor(monitor);

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
