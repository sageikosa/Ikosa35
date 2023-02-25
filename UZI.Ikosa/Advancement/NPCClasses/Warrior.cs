using System;
using System.Collections.Generic;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement.NPCClasses
{
    [ClassInfo(@"Warrior", 8, 1d, 2, true, false, false)]
    [Serializable]
    public class Warrior : CharacterClass, IWeaponProficiency, IArmorProficiency, IShieldProficiency
    {
        #region ctor()
        public Warrior()
            : base(8, PowerDieCalcMethod.Average)
        {
        }

        public Warrior(PowerDieCalcMethod method)
            : base(8, method)
        {
        }
        #endregion

        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(ClimbSkill);
            yield return typeof(HandleAnimalSkill);
            yield return typeof(IntimidateSkill);
            yield return typeof(JumpSkill);
            yield return typeof(RideSkill);
            yield return typeof(SwimSkill);
            yield break;
        }

        public override string ClassName => @"Warrior";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 1d;
        public override bool HasGoodFortitude => true;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => false;

        #region IArmorProficiency Members
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
        {
            // proficient with all armor
            return (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
        {
            // proficient with all armor
            return (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        string IArmorProficiency.Description => @"All Armor";
        #endregion

        #region IWeaponProficiency Members
        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // everything but exotic weapons (generally)
            return (!typeof(IExoticWeapon).IsAssignableFrom(type)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));
        }

        public bool IsProficientWith<WpnType>(int hitDie) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), hitDie);

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
        {
            // all simple and martial weapon types
            return ((profType == WeaponProficiencyType.Simple) || (profType == WeaponProficiencyType.Martial))
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        string IWeaponProficiency.Description => @"All simple and martial weapons";
        #endregion

        #region IShieldProficiency Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
            => (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
            => IsProficientWithShield(shield.Tower, powerLevel);

        string IShieldProficiency.Description => @"All shields";
        #endregion
    }
}