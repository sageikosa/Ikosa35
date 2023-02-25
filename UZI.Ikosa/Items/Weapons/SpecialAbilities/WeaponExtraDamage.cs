using System;
using System.Collections.Generic;
using Uzi.Ikosa.Actions;
using Uzi.Core;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public abstract class WeaponExtraDamage: WeaponSpecialAbility, IWeaponExtraDamage
    {
        protected WeaponExtraDamage(object source, int enhancementVal, decimal specialCost)
            : base(source, enhancementVal, specialCost)
        {
        }

        private bool _PoweredUp;
        public bool PoweredUp
        {
            get { return _PoweredUp; }
            set { _PoweredUp = value; }
        }

        // TODO: action (command-word) to power up ability

        public abstract IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet);
    }
}
