using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// BaseAction is the basis for available actions an actor can make.
    /// </summary>
    [Serializable]
    public abstract class CoreAction : ISourcedObject
    {
        #region construction
        protected CoreAction(IActionSource source)
        {
            _Source = source;
        }
        #endregion

        #region private data
        private IActionSource _Source;
        #endregion

        public abstract string Key { get; }
        public abstract string DisplayName(CoreActor observer);

        public virtual string WorkshopName => DisplayName(null);

        public object Source
            => _Source;

        public IActionSource ActionSource
            => _Source;

        public virtual string Description
            => string.Empty;

        /// <summary>Create an InfoStep for the actor performing the activity</summary>
        public NotifyStep CreateActivityFeedback(CoreActivity activity, SysNotify notify)
        {
            return new NotifyStep(activity, notify)
            {
                InfoReceivers = new Guid[] { activity.Actor.ID }
            };
        }

        #region public int ActionClassLevel(CoreActor actor, object source)
        /// <summary>
        /// returns the class level for the source of the action for the actor under the "source" condition (typically a range or aiming mode)
        /// </summary>
        public int CoreActionClassLevel(CoreActor actor, object source)
        {
            var _iData = new ActionInteractData(this);
            var _interact = new Interaction(actor, source, null, _iData);
            return ActionSource.ActionClassLevel.QualifiedValue(_interact);
        }
        #endregion

        /// <summary>True if this action will stay on the budget stack</summary>
        public abstract bool IsStackBase(CoreActivity activity);

        /// <summary>True if this action will pop the topmost activity on the budget stack (true by default)</summary>
        public virtual bool WillClearStack(CoreActionBudget budget, CoreActivity activity)
            => true;

        /// <summary>Called when a stackbase action is popped from the stack</summary>
        public virtual void DoClearStack(CoreActionBudget budget, CoreActivity activity) { }

        /// <summary>Called when a stackbase action is pushed onto the stack</summary>
        public virtual void DoSetStackBase(CoreActionBudget budget, CoreActivity activity) { }

        public virtual void ActivityFinalization(CoreActivity activity, bool deactivated) { }

        public virtual void ProcessManagerInitialized(CoreActivity activity) { }

        public abstract ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer);

        /// <summary>This lets us know if we should present the action in a UI</summary>
        public virtual ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            // TODO: consider using knowledge tokens to filter this
            return new ActivityResponse(true);
        }

        #region CanPerform support for BaseActivity
        /// <summary>Determine whether the action an be performed.  Override in derived classes.</summary>
        protected virtual ActivityResponse OnCanPerformActivity(CoreActivity activity)
        {
            return new ActivityResponse(true);
        }

        /// <summary>Called by base Core Activity</summary>
        internal ActivityResponse DoCanPerformActivity(CoreActivity activity)
        {
            // derived implementations?
            if (activity.Action.GetType() == GetType())
            {
                return OnCanPerformActivity(activity);
            }
            return new ActivityResponse(false);
        }
        #endregion

        #region Perform support for BaseActivity
        /// <summary>Attempt to perform the specified activity.  Override in derived classes.</summary>
        protected abstract CoreStep OnPerformActivity(CoreActivity activity);

        /// <summary>Called by BaseActivity</summary>
        internal CoreStep DoPerformActivity(CoreActivity activity)
        {
            // derived implementations?
            if (activity.Action.GetType() == GetType())
            {
                return OnPerformActivity(activity);
            }
            return null;
        }
        #endregion

        public abstract IEnumerable<AimingMode> AimingMode(CoreActivity activity);

        public virtual IEnumerable<AimTarget> ConvertTargets(CoreActor actor, 
            AimTargetInfo[] targets, IInteractProvider provider)
        {
            var _activity = new CoreActivity(actor, this, null);
            return from _aim in AimingMode(_activity)
                   from _target in _aim.GetTargets(actor, this, targets, provider)
                   select _target;
        }

        public Derived To<Derived>() where Derived : CoreAction
        {
            return this as Derived;
        }

    }
}
