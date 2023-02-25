using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.TypeListers;
using Uzi.Core;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Sorcerer", 4, 0.5d, 2, false, false, true)]
    [Serializable]
    public class Sorcerer : SpontaneousCaster, IWeaponProficiency
    {
        #region construction
        public Sorcerer()
            : base(4)
        {
        }

        public Sorcerer(PowerDieCalcMethod calcMethod) :
            base(4, calcMethod)
        {
        }
        #endregion

        #region public override IEnumerable<KeyValuePair<int, int>> KnownSpellsPerLevel(int level)
        public override IEnumerable<KeyValuePair<int, int>> KnownSpellsPerLevel(int level)
        {
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    yield return new KeyValuePair<int, int>(0, 4);
                    yield return new KeyValuePair<int, int>(1, 2);
                    break;
                case 2:
                    yield return new KeyValuePair<int, int>(0, 5);
                    yield return new KeyValuePair<int, int>(1, 2);
                    break;
                case 3:
                    yield return new KeyValuePair<int, int>(0, 5);
                    yield return new KeyValuePair<int, int>(1, 3);
                    break;
                case 4:
                    yield return new KeyValuePair<int, int>(0, 6);
                    yield return new KeyValuePair<int, int>(1, 3);
                    yield return new KeyValuePair<int, int>(2, 1);
                    break;
                case 5:
                    yield return new KeyValuePair<int, int>(0, 6);
                    yield return new KeyValuePair<int, int>(1, 4);
                    yield return new KeyValuePair<int, int>(2, 2);
                    break;
                case 6:
                    yield return new KeyValuePair<int, int>(0, 7);
                    yield return new KeyValuePair<int, int>(1, 4);
                    yield return new KeyValuePair<int, int>(2, 2);
                    yield return new KeyValuePair<int, int>(3, 1);
                    break;
                case 7:
                    yield return new KeyValuePair<int, int>(0, 7);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 3);
                    yield return new KeyValuePair<int, int>(3, 2);
                    break;
                case 8:
                    yield return new KeyValuePair<int, int>(0, 8);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 3);
                    yield return new KeyValuePair<int, int>(3, 2);
                    yield return new KeyValuePair<int, int>(4, 1);
                    break;
                case 9:
                    yield return new KeyValuePair<int, int>(0, 8);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 4);
                    yield return new KeyValuePair<int, int>(3, 3);
                    yield return new KeyValuePair<int, int>(4, 2);
                    break;
                case 10:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 4);
                    yield return new KeyValuePair<int, int>(3, 3);
                    yield return new KeyValuePair<int, int>(4, 2);
                    yield return new KeyValuePair<int, int>(5, 1);
                    break;
                case 11:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 3);
                    yield return new KeyValuePair<int, int>(5, 2);
                    break;
                case 12:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 3);
                    yield return new KeyValuePair<int, int>(5, 2);
                    yield return new KeyValuePair<int, int>(6, 1);
                    break;
                case 13:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 3);
                    yield return new KeyValuePair<int, int>(6, 2);
                    break;
                case 14:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 3);
                    yield return new KeyValuePair<int, int>(6, 2);
                    yield return new KeyValuePair<int, int>(7, 1);
                    break;
                case 15:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 4);
                    yield return new KeyValuePair<int, int>(6, 3);
                    yield return new KeyValuePair<int, int>(7, 2);
                    break;
                case 16:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 4);
                    yield return new KeyValuePair<int, int>(6, 3);
                    yield return new KeyValuePair<int, int>(7, 2);
                    yield return new KeyValuePair<int, int>(8, 1);
                    break;
                case 17:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 4);
                    yield return new KeyValuePair<int, int>(6, 3);
                    yield return new KeyValuePair<int, int>(7, 3);
                    yield return new KeyValuePair<int, int>(8, 2);
                    break;
                case 18:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 4);
                    yield return new KeyValuePair<int, int>(6, 3);
                    yield return new KeyValuePair<int, int>(7, 3);
                    yield return new KeyValuePair<int, int>(8, 2);
                    yield return new KeyValuePair<int, int>(9, 1);
                    break;
                case 19:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 4);
                    yield return new KeyValuePair<int, int>(6, 3);
                    yield return new KeyValuePair<int, int>(7, 3);
                    yield return new KeyValuePair<int, int>(8, 3);
                    yield return new KeyValuePair<int, int>(9, 2);
                    break;
                default:
                    yield return new KeyValuePair<int, int>(0, 9);
                    yield return new KeyValuePair<int, int>(1, 5);
                    yield return new KeyValuePair<int, int>(2, 5);
                    yield return new KeyValuePair<int, int>(3, 4);
                    yield return new KeyValuePair<int, int>(4, 4);
                    yield return new KeyValuePair<int, int>(5, 4);
                    yield return new KeyValuePair<int, int>(6, 3);
                    yield return new KeyValuePair<int, int>(7, 3);
                    yield return new KeyValuePair<int, int>(8, 3);
                    yield return new KeyValuePair<int, int>(9, 3);
                    break;
            }
            yield break;
        }
        #endregion

        #region protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int level)
        protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int setIndex, int level)
        {
            switch (level)
            {
                case 0:
                    break;
                case 1:
                    yield return (0, 5);
                    yield return (1, 3);
                    break;
                case 2:
                    yield return (0, 6);
                    yield return (1, 4);
                    break;
                case 3:
                    yield return (0, 6);
                    yield return (1, 5);
                    break;
                case 4:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 3);
                    break;
                case 5:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 4);
                    break;
                case 6:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 5);
                    yield return (3, 3);
                    break;
                case 7:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 4);
                    break;
                case 8:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 5);
                    yield return (4, 3);
                    break;
                case 9:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 4);
                    break;
                case 10:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 5);
                    yield return (5, 3);
                    break;
                case 11:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 4);
                    break;
                case 12:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 5);
                    yield return (6, 3);
                    break;
                case 13:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 4);
                    break;
                case 14:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 5);
                    yield return (7, 3);
                    break;
                case 15:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 6);
                    yield return (7, 4);
                    break;
                case 16:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 6);
                    yield return (7, 5);
                    yield return (8, 3);
                    break;
                case 17:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 6);
                    yield return (7, 6);
                    yield return (8, 4);
                    break;
                case 18:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 6);
                    yield return (7, 6);
                    yield return (8, 5);
                    yield return (9, 3);
                    break;
                case 19:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 6);
                    yield return (7, 6);
                    yield return (8, 6);
                    yield return (9, 4);
                    break;
                default:
                    yield return (0, 6);
                    yield return (1, 6);
                    yield return (2, 6);
                    yield return (3, 6);
                    yield return (4, 6);
                    yield return (5, 6);
                    yield return (6, 6);
                    yield return (7, 6);
                    yield return (8, 6);
                    yield return (9, 6);
                    break;
            }
            yield break;
        }
        #endregion

        /// <summary>Charisma (CHA) for Sorcerer</summary>
        public override CastingAbilityBase SpellDifficultyAbility
            => Creature?.Abilities.Charisma;

        /// <summary>Charisma (CHA) for Sorcerer</summary>
        public override CastingAbilityBase BonusSpellAbility
            => Creature?.Abilities.Charisma;

        private IDeltable _SpellDiffBase;
        public override IDeltable SpellDifficultyBase => _SpellDiffBase;

        #region protected override void OnAdd()
        protected override void OnAdd()
        {
            base.OnAdd();
            _SpellDiffBase = new ConstDeltable(10);
            _SpellDiffBase.Deltas.Add(SpellDifficultyAbility.IModifier);
            _SpellDiffBase.Deltas.Add((IQualifyDelta)Creature.ExtraSpellDifficulty);
        }
        #endregion

        #region protected override void OnRemove()
        protected override void OnRemove()
        {
            _SpellDiffBase.Deltas.Remove(SpellDifficultyAbility.IModifier);
            _SpellDiffBase.Deltas.Remove((IQualifyDelta)Creature.ExtraSpellDifficulty);
            base.OnRemove();
        }
        #endregion

        #region public override IEnumerable<ClassSpell> UsableSpells { get; }
        public override IEnumerable<ClassSpell> UsableSpells
        {
            get
            {
                // sorcerer can cast any spell on the sorcerer's list
                return (from _csl in Campaign.SystemCampaign.SpellLists[GetType().FullName]
                        from _cSpell in _csl.Value
                        select _cSpell).ToList();
            }
        }
        #endregion

        public override bool MustRestToRecharge => true;
        public override MagicType MagicType => MagicType.Arcane;
        public override Type CasterClassType => typeof(Sorcerer);
        public override string ClassName => @"Sorcerer";
        public override string ClassIconKey => $@"{ClassName}_class";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.5d;
        public override bool HasGoodFortitude => false;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => true;

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(BluffSkill);
            yield return typeof(ConcentrationSkill);
            yield return typeof(SpellcraftSkill);
            yield return typeof(KnowledgeSkill<KnowArcana>);
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
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => (profType == WeaponProficiencyType.Simple) && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // all simple weapons (generally)
            return (!typeof(IExoticWeapon).IsAssignableFrom(type) && !typeof(IMartialWeapon).IsAssignableFrom(type)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        public string Description
            => @"All Simple Weapons";

        #endregion

        // TODO: familiar

        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            // base requirements
            foreach (var _req in BaseRequirements(level))
                yield return _req;

            // TODO: list replaceable spells with new spell (options=selectable spells)
            yield break;
        }

        #region public override IEnumerable<IFeature> Features(int level)
        public override IEnumerable<IFeature> Features(int level)
        {
            if (level == 1)
            {
                // familiar
                yield return new Feature(@"Summon Familiar", @"Ability to summon one permanent companion creature.");
            }

            // base features
            foreach (var _feature in BaseFeatures(level))
                yield return _feature;
            yield break;
        }
        #endregion
    }
}
