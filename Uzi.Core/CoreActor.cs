using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreActor : CoreObject, IControlChange<InteractionAlteration>, IControlChange<CoreActivity>, ICoreImagery
    {
        #region Construction
        protected CoreActor(string name)
            : base(name)
        {
            _Possess = new PossessionSet(this);
            _ObjLoad = GetInitialObjectLoad();
            _Actions = new ActionRepetoire(this);
            _InteractAlterCtrl = new ChangeController<InteractionAlteration>(this, null);
            _ActivityCtrl = new ChangeController<CoreActivity>(this, null);
        }
        #endregion

        protected abstract ObjectLoad GetInitialObjectLoad();

        #region Private data
        protected PossessionSet _Possess;
        private ObjectLoad _ObjLoad;
        private ActionRepetoire _Actions;
        private ChangeController<CoreActivity> _ActivityCtrl;
        #endregion

        /// <summary>All objects owned (carried or not)</summary>
        public PossessionSet Possessions => _Possess;

        /// <summary>All objects being carried</summary>
        public ObjectLoad ObjectLoad => _ObjLoad;

        /// <summary>All actions available to the actor</summary>
        public ActionRepetoire Actions => _Actions;

        #region IControlChange<InteractionAlteration> Members
        private ChangeController<InteractionAlteration> _InteractAlterCtrl;

        /// <summary>Effects that modify interaction alterations hook into this on the actor</summary>
        internal ChangeController<InteractionAlteration> InteractAlterationController
            => _InteractAlterCtrl;

        /// <summary>Adds ability to monitor alterations to interactions that start from this actor</summary>
        public void AddChangeMonitor(IMonitorChange<InteractionAlteration> monitor)
        {
            _InteractAlterCtrl.AddChangeMonitor(monitor);
        }

        /// <summary>Removes previously added interaction alteration monitors</summary>
        public void RemoveChangeMonitor(IMonitorChange<InteractionAlteration> monitor)
        {
            _InteractAlterCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region IControlChange<CoreActivity> Members
        internal ChangeController<CoreActivity> BaseActivityController => _ActivityCtrl;
        public void AddChangeMonitor(IMonitorChange<CoreActivity> monitor)
        {
            _ActivityCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<CoreActivity> monitor)
        {
            _ActivityCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        public abstract CoreActionBudget CreateActionBudget(CoreTurnTick tick);

        public override bool IsTargetable => true;

        /// <summary>Yields all ICoreObjects in the actor's ObjectLoad</summary>
        public override IEnumerable<ICoreObject> Connected
            => _ObjLoad.Select(_o => _o);

        protected abstract string DefaultImage { get; }

        #region public IEnumerable<string> ImageKeys
        public IEnumerable<string> ImageKeys
        {
            get
            {
                // provide any imagery
                foreach (var _iKey in Adjuncts.OfType<ImageKeyAdjunct>()
                    .OrderByDescending(_ik => _ik.Order))
                    yield return _iKey.Key;

                // default
                var _default = DefaultImage;
                if (!string.IsNullOrEmpty(_default))
                    yield return _default;

                yield break;
            }
        }
        #endregion

        public void SendSysNotify(SysNotify notify)
        {
            var _mgr = ProcessManager;
            if (_mgr != null)
            {
                var _step = _mgr.CurrentStep;
                if (_step != null)
                {
                    // enqueue if possible
                    _step.EnqueueNotify(notify, new Guid[] { ID });
                }
                else
                {
                    // otherwise, send direct...
                    _mgr.SendSysStatus(notify, new Guid[] { ID });
                }
            }
        }
    }
}
