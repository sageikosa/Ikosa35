using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Advancement.NPCClasses
{
    [ClassInfo(@"Adept", 8, 0.5d, 2, false, false, true)]
    [Serializable]
    public class Adept : PreparedCaster, IWeaponProficiency
    {
        #region construction
        public Adept()
            : this(PowerDieCalcMethod.Average)
        {
        }

        public Adept(PowerDieCalcMethod calcMethod) :
            base(8, calcMethod)
        {
            _SpellDifficultyBase = new ConstDeltable(10);
        }
        #endregion

        private IDeltable _SpellDifficultyBase;

        public override bool MustRestToRecharge => false;
        public override MagicType MagicType => MagicType.Divine;
        public override Type CasterClassType => GetType();
        public override string ClassName => @"Adept";
        public override string ClassIconKey => $@"{ClassName}_class";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.5d;
        public override bool HasGoodFortitude => false;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => true;
        public override IDeltable SpellDifficultyBase => _SpellDifficultyBase;

        #region public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
        public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
        {
            // standard spells
            return (from _csl in Campaign.SystemCampaign.SpellLists[GetType().FullName]
                    from _cSpell in _csl.Value
                        // TODO: filter by alignment and descriptor compatibility?
                    select _cSpell)
                    .ToList();
        }
        #endregion

        #region protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int slotIndex, int level)
        protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int slotIndex, int level)
        {
            switch (level)
            {
                case 0:
                    break;
                case 1:
                case 2:
                    yield return (0, 3);
                    yield return (1, 1);
                    break;
                case 3:
                    yield return (0, 3);
                    yield return (1, 2);
                    break;
                case 4:
                    yield return (0, 3);
                    yield return (1, 2);
                    yield return (2, 0);
                    break;
                case 5:
                case 6:
                    yield return (0, 3);
                    yield return (1, 2);
                    yield return (2, 1);
                    break;
                case 7:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 2);
                    break;
                case 8:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 2);
                    yield return (3, 0);
                    break;
                case 9:
                case 10:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 2);
                    yield return (3, 1);
                    break;
                case 11:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 2);
                    break;
                case 12:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 2);
                    yield return (4, 0);
                    break;
                case 13:
                case 14:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 2);
                    yield return (4, 1);
                    break;
                case 15:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 3);
                    yield return (4, 2);
                    break;
                case 16:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 3);
                    yield return (4, 2);
                    yield return (5, 0);
                    break;
                case 17:
                case 18:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 3);
                    yield return (4, 2);
                    yield return (5, 1);
                    break;
                case 19:
                default:
                    yield return (0, 3);
                    yield return (1, 3);
                    yield return (2, 3);
                    yield return (3, 3);
                    yield return (4, 3);
                    yield return (5, 2);
                    break;
            }
            yield break;
        }
        #endregion

        /// <summary>Wisdom (WIS) for Adept</summary>
        public override CastingAbilityBase SpellDifficultyAbility
            => Creature?.Abilities.Wisdom;

        /// <summary>Wisdom (WIS) for Adept</summary>
        public override CastingAbilityBase BonusSpellAbility
            => Creature?.Abilities.Wisdom;

        #region public override IEnumerable<ClassSpell> UsableSpells { get; }
        /// <summary>Usable spells includes all preparable spells </summary>
        public override IEnumerable<ClassSpell> UsableSpells
        {
            get
            {
                return PreparableSpells(0);
            }
        }
        #endregion

        // TODO: other class specific and caster specific tie-ins
        // TODO: campaign spell list
        // TODO: class skills

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => (profType == WeaponProficiencyType.Simple)
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // all simple weapons (generally)
            return !typeof(IExoticWeapon).IsAssignableFrom(type)
                && !typeof(IMartialWeapon).IsAssignableFrom(type)
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        public string Description
            => @"All Simple Weapons";
        #endregion
    }
}
