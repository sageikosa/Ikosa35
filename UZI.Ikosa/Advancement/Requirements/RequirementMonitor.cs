using System;
using Uzi.Core;

namespace Uzi.Ikosa.Advancement
{
    /// <summary>non-generic abstract base for requirement monitors</summary>
    [Serializable]
    public abstract class RequirementMonitor
    {
        protected RequirementMonitor(IRefreshable target, Creature owner)
        {
            _Target = target;
            _Owner = owner;
        }

        private IRefreshable _Target;
        public IRefreshable Target { get { return _Target; } }

        private Creature _Owner;
        public Creature Owner { get { return _Owner; } }

        protected bool _MeetsRequirement = false;

        public abstract void DoTerminate();
    }

    /// <summary>This class is designed to watch for changes in requirement status and then refresh the target</summary>
    [Serializable]
    public abstract class FlexRequirementMonitor<AttrType, MonitorType> : RequirementMonitor, IMonitorChange<MonitorType>
        where AttrType : RequirementAttribute
    {
        protected FlexRequirementMonitor(AttrType attr, IRefreshable target, Creature owner)
            : base(target, owner)
        {
            _ReqAttr = attr;
            _MeetsRequirement = _ReqAttr.MeetsRequirement(owner);
        }

        protected AttrType _ReqAttr;

        #region IMonitorChange<MonitorType> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<MonitorType> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<MonitorType> args) { }

        public virtual void ValueChanged(object sender, ChangeValueEventArgs<MonitorType> args)
        {
            if (_MeetsRequirement != _ReqAttr.MeetsRequirement(Owner))
            {
                // requirement changed...check all requirements
                _MeetsRequirement = !_MeetsRequirement;
                this.Target.Refresh();
            }
        }
        #endregion
    }
}
