using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement.NPCClasses
{
    [ClassInfo(@"Commoner", 4, 0.5d, 2, false, false, false)]
    [Serializable]
    public class Commoner : CharacterClass, IWeaponProficiency
    {
        #region ctor()
        public Commoner()
            : base(4, PowerDieCalcMethod.Average)
        {
        }

        public Commoner(PowerDieCalcMethod method)
            : base(4, method)
        {
        }
        #endregion

        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(ClimbSkill);
            yield return typeof(HandleAnimalSkill);
            yield return typeof(JumpSkill);
            yield return typeof(ListenSkill);
            yield return typeof(RideSkill);
            yield return typeof(SpotSkill);
            yield return typeof(SwimSkill);
            yield return typeof(UseRopeSkill);
            foreach (var _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
            {
                yield return _skillType;
            }
            foreach (var _skillType in SubSkillLister.SubSkillTypes<ProfessionFocus>(typeof(ProfessionSkill<>)))
            {
                yield return _skillType;
            }
            yield break;
        }

        public override string ClassName => @"Commoner";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.5d;
        public override bool HasGoodFortitude => false;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => false;

        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            if ((level == 1) && (LockedLevel < 1))
            {
                yield return new AdvancementRequirement(
                    new RequirementKey(@"Commoner.Weapon"), @"Weapon Proficiency", @"Commoner is proficient with one simple weapon",
                    WeaponTypes, SetWeaponType, CheckWeaponType)
                {
                    CurrentValue = (_SimpleWeaponType != null) ? Features(1).FirstOrDefault() : null
                };
            }
            yield break;
        }

        private IEnumerable<IAdvancementOption> WeaponTypes(IResolveRequirement target, object key)
        {
            foreach (var _wpn in Campaign.SystemCampaign.SimpleWeapons)
            {
                yield return new AdvancementParameter<Type>(target, _wpn.Key, _wpn.Value.Info.Description, _wpn.Value.ItemType);
            }
            yield break;
        }

        private bool SetWeaponType(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<Type> _option)
            {
                // assume the weaponType is OK
                if (_option.ParameterValue.IsSubclassOf(typeof(WeaponBase)))
                {
                    _SimpleWeaponType = _option.ParameterValue;
                }
            }
            return false;
        }

        private bool CheckWeaponType(RequirementKey key)
            => _SimpleWeaponType != null;

        public override bool CanLockLevel(int level)
        {
            // cannot lock if weapon proficiency is not set
            if (_SimpleWeaponType == null)
            {
                return false;
            }
            return base.CanLockLevel(level);
        }

        protected override void OnUnlockOneLevel()
        {
            // clear simple weapon proficiency at level 1
            if (LockedLevel < 1)
            {
                _SimpleWeaponType = null;
            }
            base.OnUnlockOneLevel();
        }

        private Type _SimpleWeaponType = null;
        public Type SimpleWeaponType
        {
            get => _SimpleWeaponType;
            set
            {
                // set if level isn't locked
                if (LockedLevel < 1)
                {
                    // must be a weapon, but not martial or exotic
                    if (typeof(IWeapon).IsAssignableFrom(value)
                        && (!typeof(IMartialWeapon).IsAssignableFrom(value))
                        && (!typeof(IExoticWeapon).IsAssignableFrom(value)))
                    {
                        _SimpleWeaponType = value;
                    }
                }
            }
        }

        public override IEnumerable<IFeature> Features(int level)
        {
            if (level == 1)
            {
                if (_SimpleWeaponType != null)
                {
                    ItemInfoAttribute _info = WeaponBase.GetInfo(_SimpleWeaponType);
                    yield return new Feature(
                        $@"Weapon Proficiency: {_info.Name}",
                        $@"Proficient with one simple weapon.  {_info.Name}: {_info.Description}");
                }
                else
                {
                    yield return new Feature(
                        @"Weapon Proficiency: (Not Set)",
                        @"Proficient with one simple weapon.  (Not Set): (No Info)");
                }
            }
            yield break;
        }

        #region IWeaponProficiency Members
        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => false;

        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => (_SimpleWeaponType == null)
            ? false
            : (type.Equals(_SimpleWeaponType)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public string Description
            => (_SimpleWeaponType != null)
            ? ItemBase.GetInfo(_SimpleWeaponType).Name
            : @"(Weapon Not Set)";
        #endregion
    }
}
