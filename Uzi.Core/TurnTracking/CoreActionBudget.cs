using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreActionBudget
    {
        #region ctor()
        protected CoreActionBudget(CoreActor actor)
        {
            _Actor = actor;
            _BudgetItems = new BudgetItemSet(this);
            _ActStack = new Stack<CoreActivity>();
        }
        #endregion

        #region state
        private readonly CoreActor _Actor;
        private readonly BudgetItemSet _BudgetItems;
        private Stack<CoreActivity> _ActStack;
        #endregion

        public CoreActor Actor => _Actor;

        /// <summary>Extra state information</summary>
        public BudgetItemSet BudgetItems => _BudgetItems;

        /// <summary>Actions requiring some degree of effort (flag is true for external)</summary>
        public abstract IEnumerable<ActionResult> GetActions();

        /// <summary>Current activity at top of stack</summary>
        public CoreActivity TopActivity
            => _ActStack.Count > 0 ? _ActStack.Peek() : null;

        /// <summary>True if there is an activity for this budget</summary>
        public bool HasActivity
            => _ActStack.Any();

        public IEnumerable<CoreActivity> Activities
            => _ActStack.Select(_a => _a);

        public void ClearActivities()
        {
            while (_ActStack.Any())
            {
                var _next = _ActStack.Pop();
                _next?.Action.DoClearStack(this, _next);
            }
        }

        protected abstract void OnRegisterActivity(CoreActivity activity);

        public void RegisterActivity(CoreActivity activity)
        {
            if (activity.Action.IsStackBase(activity))
            {
                _ActStack.Push(activity);
                activity.Action.DoSetStackBase(this, activity);
            }
            else
            {
                while (_ActStack.Any() && activity.Action.WillClearStack(this, activity))
                {
                    var _top = _ActStack.Pop();
                    _top?.Action.DoClearStack(this, _top);
                }
            }

            OnRegisterActivity(activity);
        }
    }
}
