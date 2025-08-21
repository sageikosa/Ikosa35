using System;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FighterBonusFeat,
        BaseAttackRequirement(1),
        ParameterizedFeatInfo(@"Exotic Weapon Proficiency", @"Cancel -4 penalty with specific exotic weapon",
        typeof(ExoticWeaponsLister))
    ]
    public class ExoticWeaponProficiencyFeat<Wpn> : FeatBase, IWeaponProficiency where Wpn : IExoticWeapon
    {
        public ExoticWeaponProficiencyFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        public override string Name
            => $@"Exotic Weapon Proficiency ({typeof(Wpn).Name})";

        public override string Benefit
            => $@"You make attack rolls with the exotic weapon '{typeof(Wpn).Name}' normally.";

        public override bool MeetsPreRequisite(Creature creature)
        {
            // if already proficient with the weapon type, no point in going on
            if (!creature.Proficiencies.IsProficientWith<Wpn>(PowerLevel))
            {
                if (IgnorePreRequisite)
                {
                    return true;
                }

                if (typeof(IExoticWeaponHeavy).IsAssignableFrom(typeof(Wpn)))
                {
                    // must have Str 13
                    if (creature.Abilities.Strength.ValueAtPowerLevel(PowerLevel, null) >= 13)
                    {
                        return base.MeetsPreRequisite(creature); // must meet other requirements (BAB>=1)
                    }
                }
                else
                {
                    return base.MeetsPreRequisite(creature); // must meet other requirements (BAB>=1)
                }
            }
            return false;
        }

        public override string PreRequisite
            => typeof(IExoticWeaponHeavy).IsAssignableFrom(typeof(Wpn))
            ? string.Concat(@"Base attack bonus +1.", @"\nStrength 13 or higher")
            : @"Base attack bonus +1.";

        protected override void OnAdd()
        {
            base.OnAdd();
            Creature.Proficiencies.Add(this);
        }

        protected override void OnRemove()
        {
            Creature.Proficiencies.Remove(this);
            base.OnRemove();
        }

        #region IWeaponProficiency Members

        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => (type == typeof(Wpn)) && (powerLevel >= PowerLevel);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            // exotic is not a type that can be blanket proficient
            return false;
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
        {
            // ensure this is the right feat to try this
            if (IsProficientWithWeapon(weapon.GetType(), powerLevel))
            {
                // then check if this is a heavy weapon
                if (typeof(IExoticWeaponHeavy).IsAssignableFrom(weapon.GetType()))
                {
                    // since it is, we need to also check the strength
                    return (Creature.Abilities.Strength.EffectiveValue >= 13)
                        && (powerLevel >= PowerLevel);
                }

                // otherwise, we're good
                return (powerLevel >= PowerLevel);
            }

            // not our type
            return false;
        }

        public string Description
            => ItemBase.GetInfo(typeof(Wpn)).Name;

        #endregion
    }
}
