using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Ranger", 8, 1d, 6, true, true, false)]
    [Serializable]
    public class Ranger : PreparedCaster, IWeaponProficiency, IArmorProficiency, IShieldProficiency
    {
        #region Construction
        public Ranger()
            : this(PowerDieCalcMethod.Average)
        {
        }

        public Ranger(PowerDieCalcMethod method)
            : base(8, method)
        {
            _SpellDifficultyBase = new ConstDeltable(10);
        }
        #endregion

        #region data
        private IDeltable _SpellDifficultyBase;
        #endregion

        public override bool MustRestToRecharge => false;
        public override MagicType MagicType => MagicType.Divine;
        public override IDeltable SpellDifficultyBase => _SpellDifficultyBase;
        public override Type CasterClassType => typeof(Ranger);

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(ClimbSkill);
            yield return typeof(ConcentrationSkill);
            yield return typeof(HandleAnimalSkill);
            yield return typeof(HealSkill);
            yield return typeof(StealthSkill);
            yield return typeof(JumpSkill);
            yield return typeof(ListenSkill);
            yield return typeof(SilentStealthSkill);
            yield return typeof(RideSkill);
            yield return typeof(SearchSkill);
            yield return typeof(SpotSkill);
            yield return typeof(SurvivalSkill);
            yield return typeof(SwimSkill);
            yield return typeof(UseRopeSkill);
            foreach (var _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
            {
                yield return _skillType;
            }
            yield return typeof(KnowDungeoneering);
            yield return typeof(KnowGeography);
            yield return typeof(KnowNature);
            foreach (var _skillType in SubSkillLister.SubSkillTypes<ProfessionFocus>(typeof(ProfessionSkill<>)))
            {
                yield return _skillType;
            }
            yield break;
        }
        #endregion

        public override CastingAbilityBase SpellDifficultyAbility
            => Creature?.Abilities.Wisdom;

        public override CastingAbilityBase BonusSpellAbility
            => Creature?.Abilities.Wisdom;

        public override string ClassName => @"Ranger";
        public override string ClassIconKey => $@"{ClassName}_class";
        public override int SkillPointsPerLevel => 6;
        public override double BABProgression => 1.0d;
        public override bool HasGoodFortitude => true;
        public override bool HasGoodReflex => true;
        public override bool HasGoodWill => false;

        #region protected override void OnAdd()
        protected override void OnAdd()
        {
            base.OnAdd();
            _SpellDifficultyBase.Deltas.Add(SpellDifficultyAbility.IModifier);
            _SpellDifficultyBase.Deltas.Add((IQualifyDelta)Creature.ExtraSpellDifficulty);
        }
        #endregion

        #region protected override void OnRemove()
        protected override void OnRemove()
        {
            // remove deltas provided by creature, so they do not continue to anchor this class
            _SpellDifficultyBase.Deltas.Remove(SpellDifficultyAbility.IModifier);
            _SpellDifficultyBase.Deltas.Remove((IQualifyDelta)Creature.ExtraSpellDifficulty);
            base.OnRemove();
        }
        #endregion

        public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
            => (from _csl in Campaign.SystemCampaign.SpellLists[GetType().FullName]
                from _cSpell in _csl.Value
                    // TODO: filter by alignment and descriptor compatibility
                select _cSpell)
            .ToList();

        public override IEnumerable<ClassSpell> UsableSpells
            => PreparableSpells(0);

        #region protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int setIndex, int level)
        /// <summary>setIndex==0: standard spells; setIndex==1: influence spells</summary>
        protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int setIndex, int level)
        {
            if (setIndex == 0)
            {
                switch (level)
                {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                        break;
                    case 4:
                    case 5:
                        yield return (1, 0);
                        break;
                    case 6:
                    case 7:
                        yield return (1, 1);
                        break;
                    case 8:
                    case 9:
                        yield return (1, 1);
                        yield return (2, 0);
                        break;
                    case 10:
                        yield return (1, 1);
                        yield return (2, 1);
                        break;
                    case 11:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 0);
                        break;
                    case 12:
                    case 13:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        break;
                    case 14:
                        yield return (1, 2);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 0);
                        break;
                    case 15:
                        yield return (1, 2);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        break;
                    case 16:
                        yield return (1, 2);
                        yield return (2, 2);
                        yield return (3, 1);
                        yield return (4, 1);
                        break;
                    case 17:
                        yield return (1, 2);
                        yield return (2, 2);
                        yield return (3, 2);
                        yield return (4, 1);
                        break;
                    case 18:
                        yield return (1, 3);
                        yield return (2, 2);
                        yield return (3, 2);
                        yield return (4, 1);
                        break;
                    case 19:
                        yield return (1, 3);
                        yield return (2, 3);
                        yield return (3, 3);
                        yield return (4, 2);
                        break;
                    default:
                        yield return (1, 3);
                        yield return (2, 3);
                        yield return (3, 3);
                        yield return (4, 3);
                        break;
                }
            }
            yield break;
        }
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => ((profType == WeaponProficiencyType.Simple) || (profType == WeaponProficiencyType.Martial))
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        // everything but exotic weapons (generally)
        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => (!typeof(IExoticWeapon).IsAssignableFrom(type)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));

        string IWeaponProficiency.Description
            => @"All simple and martial weapons";

        #endregion

        #region IArmorProficiency Members

        // proficient with light armor
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
            => (profType < ArmorProficiencyType.Medium) 
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        // proficient with light armor
        public bool IsProficientWith(ArmorBase armor, int powerLevel)
            => (armor.ProficiencyType < ArmorProficiencyType.Medium) 
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        string IArmorProficiency.Description
            => @"Light Armor";

        #endregion

        #region IShieldProficiency Members

        public bool IsProficientWithShield(bool tower, int powerLevel)
            => !tower && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
            => IsProficientWithShield(shield.Tower, powerLevel);

        string IShieldProficiency.Description => @"Normal Shields";

        #endregion

        // ???
        public static ItemCaster CreateItemCaster(int level, Alignment alignment = null)
            => new ItemCaster(MagicType.Divine, level, alignment ?? Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Ranger));

        // TODO: requirements
        // TODO: features
        // TODO: actions
    }
}
