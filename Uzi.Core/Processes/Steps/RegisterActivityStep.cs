using System;
using System.Diagnostics;

namespace Uzi.Core
{
    /// <summary>
    /// When processed, will register activity with local budget.  
    /// If a span, will enqueue a hold process step at the end of the pre-emption chain.
    /// </summary>
    [Serializable]
    public class RegisterActivityStep : CoreStep
    {
        /// <summary>
        /// When processed, will register activity with local budget.  
        /// If a span, will enqueue a hold process step at the end of the pre-emption chain.
        /// </summary>
        public RegisterActivityStep(CoreActivity activity, CoreActionBudget budget)
            : base(activity)
        {
            _Budget = budget;
        }

        #region private data
        private CoreActionBudget _Budget;
        #endregion

        public override string Name => @"Register Activity";
        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            var _activity = Process as CoreActivity;
            _Budget.RegisterActivity(_activity);
            return true;
        }
    }
}
