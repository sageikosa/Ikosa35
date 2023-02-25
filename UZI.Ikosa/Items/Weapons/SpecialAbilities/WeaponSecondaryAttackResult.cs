using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public abstract class WeaponSecondaryAttackResult : WeaponSpecialAbility, ISecondaryAttackResult
    {
        protected WeaponSecondaryAttackResult(object source, int enhancementValue, decimal specialCost)
            : base(source, enhancementValue, specialCost)
        {
        }

        public object AttackResultSource => Source;

        public abstract IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet);

        public abstract void AttackResult(StepInteraction deliverDamageInteraction);

        private bool _PoweredUp;
        public bool PoweredUp
        {
            get { return _PoweredUp; }
            set { _PoweredUp = value; }
        }

        public abstract bool IsDamageSufficient(StepInteraction final);
    }
}
