using Newtonsoft.Json;
using System;

namespace Uzi.Core
{
    /// <summary>Represents a link between an object and its owner</summary>
    /// <typeparam name="OwnType">represents the type of the owner of the pointer</typeparam>
    [Serializable]
    public class BiDiPtr<DockType, OwnType> : IControlChange<DockType>
        where DockType : LinkableDock<OwnType>
        where OwnType : class, ILinkOwner<LinkableDock<OwnType>>
    {
        #region Construction
        public BiDiPtr(OwnType ptrOwner)
        {
            _Owner = ptrOwner;
            _LinkDock = default;
            _ChangeCtrl = new ChangeController<DockType>(this, null);
        }

        public BiDiPtr(OwnType ptrOwner, DockType val)
        {
            _Owner = ptrOwner;
            _LinkDock = val;
            _ChangeCtrl = new ChangeController<DockType>(this, null);
        }
        #endregion

        #region state
        /// <summary>Owner of the pointer</summary>
        protected OwnType _Owner;
        private readonly ChangeController<DockType> _ChangeCtrl;
        protected DockType _LinkDock;
        #endregion

        #region public DockType LinkDock { get; set; }
        /// <summary>
        /// Get the pointer referrent, or set the pointer value (may be aborted by an event handler).
        /// The referent will be told to link to this owner.
        /// </summary>
        public DockType LinkDock
        {
            get
            {
                return _LinkDock;
            }
            set
            {
                // check to see if the old or new value refuses the change
                if (_LinkDock != value)
                {
                    if (!WillAbortChange(value))
                    {
                        var _oldLink = _LinkDock;
                        _LinkDock = value;              // set the new value
                        if (_LinkDock != null)
                        {
                            // attach to the new dock (this will also release the old one)
                            _LinkDock.LinkToObject(_Owner);
                        }
                        else if ((_oldLink != null) && (_oldLink.InternalLink == Owner))
                        {
                            // just release the old dock
                            _oldLink.LinkToObject(null);
                        }

                        // notify listeners
                        _ChangeCtrl.DoValueChanged(_LinkDock);
                        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(LinkDock)));
                    }
                }
            }
        }
        #endregion

        /// <summary>The owner of the pointer</summary>
        public OwnType Owner => _Owner;

        #region public bool WillAbortChange(DockType newVal)
        /// <summary>
        /// If there is no change, none will be aborted.
        /// Makes sure the old reference will release.
        /// Makes sure the new reference does not abort.
        /// </summary>
        public bool WillAbortChange(DockType newVal)
        {
            if (_LinkDock == newVal)
            {
                // no change
                return false;
            }

            if (_LinkDock != null)
            {
                // test if old value will release
                if (_LinkDock.WillAbortChange(default))
                    return true;
            }

            if (newVal != null)
            {
                // test if new value will allow
                if (newVal.WillAbortChange(_Owner))
                    return true;
            }

            // see if anything has registered to test for an abort
            return _ChangeCtrl.WillAbortChange(newVal);
        }
        #endregion

        public void AddChangeMonitor(IMonitorChange<DockType> monitor)
        {
            _ChangeCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<DockType> monitor)
        {
            _ChangeCtrl.RemoveChangeMonitor(monitor);
        }

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
