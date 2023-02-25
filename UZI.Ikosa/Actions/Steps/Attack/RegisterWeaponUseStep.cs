using System;
using Uzi.Core;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>
    /// Used to inform full attack budget that a weapon has been used, so that iterative attacks can be tracked.
    /// </summary>
    [Serializable]
    public class RegisterWeaponUseStep : CoreStep
    {
        /// <summary>
        /// Used to inform full attack budget that a weapon has been used, so that iterative attacks can be tracked.
        /// </summary>
        public RegisterWeaponUseStep(CoreActivity activity, FullAttackBudget atkBudget, AttackActionBase attack)
            : base(activity)
        {
            _AtkBudget = atkBudget;
            _AttackAction = attack;
        }

        private FullAttackBudget _AtkBudget = null;
        private AttackActionBase _AttackAction;

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }

        protected override bool OnDoStep()
        {
            if ((_AtkBudget != null) && (_AttackAction != null))
            {
                // use weapon (head)
                _AtkBudget.UseAttack(_AttackAction);
            }

            // done
            return true;
        }
    }
}
