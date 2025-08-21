using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Abilities;
using Uzi.Core;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Wizard", 4, 0.5d, 2, false, false, true)]
    [Serializable]
    public class Wizard : PreparedCaster, IWeaponProficiency
    {
        #region construction
        public Wizard()
            : this(PowerDieCalcMethod.Average)
        {
        }

        public Wizard(PowerDieCalcMethod calcMethod) :
            base(4, calcMethod)
        {
            _Known = new KnownSpellSet();
            _Prohibition = new Dictionary<int, MagicStyle>(2);
        }
        #endregion

        #region private data
        private IDeltable _SpellDiffBase;
        private KnownSpellSet _Known;
        private SpellBook _DefBook;
        private MagicStyle _Specialization;
        private Dictionary<int, MagicStyle> _Prohibition;
        #endregion

        /// <summary>Intelligence (INT) for Wizard</summary>
        public override CastingAbilityBase SpellDifficultyAbility
            => Creature?.Abilities.Intelligence;

        /// <summary>Intelligence (INT) for Wizard</summary>
        public override CastingAbilityBase BonusSpellAbility
            => Creature?.Abilities.Intelligence;

        public override IDeltable SpellDifficultyBase => _SpellDiffBase;

        /// <summary>Wizards learn spells, but still need them in spell books to prepare them...</summary>
        public KnownSpellSet KnownSpells => _Known;

        /// <summary>NULL or the magic style of the specialized school</summary>
        public MagicStyle Specialization => _Specialization;

        protected SpellSlotSet<PreparedSpellSlot> GetSpecialistSet()
            => _SpellSlots.FirstOrDefault(_set => _set.Source is MagicStyle);

        public IFeature SpecialistFeature
            => (Specialization != null)
            ? new Feature(Specialization.SpecialistName, @"1 Bonus Spell per Slot Level in Magic style")
            : null;

        /// <summary>Magic styles prohibited to the caster class</summary>
        public IEnumerable<MagicStyle> ProhibitedStyles
            => _Prohibition.OrderBy(_kvp => _kvp.Key).Select(_kvp => _kvp.Value);

        #region public ScribeScrollFeat ScribeScrollFeat
        public ScribeScrollFeat ScribeScrollFeat
        {
            get
            {
                var _powerLevel = Creature.AdvancementLog[this, 1].PowerDie.Level;
                return Creature.Feats.OfType<ScribeScrollFeat>()
                    .Where(_f => (_f.PowerLevel == _powerLevel) && _f.IsBonusFeat(this))
                    .FirstOrDefault();
            }
        }
        #endregion

        #region public FeatBase BonusFeat(int level)
        public FeatBase BonusFeat(int level)
        {
            if (IsBonusFeatLevel(level))
            {
                var _powerLevel = Creature.AdvancementLog[this, level].PowerDie.Level;
                return Creature.Feats
                    .Where(_f => (_f.PowerLevel == _powerLevel) && _f.IsBonusFeat(this))
                    .FirstOrDefault();
            }
            return null;
        }
        #endregion

        #region private IFeature BonusFeature(int level)
        private IFeature BonusFeature(int level)
        {
            if (IsBonusFeatLevel(level))
            {
                var _feat = BonusFeat(level);
                if (_feat != null)
                {
                    return new Feature(_feat.Name, _feat.Benefit);
                }
            }
            return null;
        }
        #endregion

        #region protected override void OnAdd()
        protected override void OnAdd()
        {
            base.OnAdd();
            _SpellDiffBase = new ConstDeltable(10);
            _SpellDiffBase.Deltas.Add(SpellDifficultyAbility.IModifier);
            _SpellDiffBase.Deltas.Add(Creature.ExtraSpellDifficulty);

            if (Creature.IsInSystemEditMode)
            {
                // create book with all 0-level spells
                _DefBook = new SpellBook(string.Format(@"Book of {0}", Creature.Name))
                {
                    Possessor = Creature
                };
                foreach (var _spell in Campaign.SystemCampaign.SpellLists[GetType().FullName][0])
                {
                    _DefBook.Add(new BookSpell(0, _spell.SpellDef, OwnerID));
                }
            }

            // scribe scroll (bonus feat)
            var _powerLevel = Creature.AdvancementLog[this, 1].PowerDie.Level;
            var _scribe = new ScribeScrollFeat(this, _powerLevel);
            _scribe.BindTo(Creature);
        }
        #endregion

        #region protected override void OnRemove()
        protected override void OnRemove()
        {
            _SpellDiffBase.Deltas.Remove(SpellDifficultyAbility.IModifier);
            _SpellDiffBase.Deltas.Remove(Creature.ExtraSpellDifficulty);

            // remove book
            if (Creature.IsInSystemEditMode)
            {
                _DefBook.Possessor = null;
            }

            // remove bonus scribe scroll
            var _scribe = ScribeScrollFeat;
            if (_scribe != null)
            {
                _scribe.UnbindFromCreature();
            }

            base.OnRemove();
        }
        #endregion

        #region protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int slotIndex, int level)
        protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int slotIndex, int level)
        {
            if (slotIndex == 0)
            {
                switch (level)
                {
                    case 0:
                        break;
                    case 1:
                        yield return (0, 3);
                        yield return (1, 1);
                        break;
                    case 2:
                        yield return (0, 4);
                        yield return (1, 2);
                        break;
                    case 3:
                        yield return (0, 4);
                        yield return (1, 2);
                        yield return (2, 1);
                        break;
                    case 4:
                        yield return (0, 4);
                        yield return (1, 3);
                        yield return (2, 2);
                        break;
                    case 5:
                        yield return (0, 4);
                        yield return (1, 3);
                        yield return (2, 2);
                        yield return (3, 1);
                        break;
                    case 6:
                        yield return (0, 4);
                        yield return (1, 3);
                        yield return (2, 3);
                        yield return (3, 2);
                        break;
                    case 7:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 3);
                        yield return (3, 2);
                        yield return (4, 1);
                        break;
                    case 8:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 3);
                        yield return (3, 3);
                        yield return (4, 2);
                        break;
                    case 9:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 3);
                        yield return (4, 2);
                        yield return (5, 1);
                        break;
                    case 10:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 3);
                        yield return (4, 3);
                        yield return (5, 2);
                        break;
                    case 11:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 3);
                        yield return (5, 2);
                        yield return (6, 1);
                        break;
                    case 12:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 3);
                        yield return (5, 3);
                        yield return (6, 2);
                        break;
                    case 13:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 3);
                        yield return (6, 2);
                        yield return (7, 1);
                        break;
                    case 14:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 3);
                        yield return (6, 3);
                        yield return (7, 2);
                        break;
                    case 15:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 3);
                        yield return (7, 2);
                        yield return (8, 1);
                        break;
                    case 16:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 3);
                        yield return (7, 3);
                        yield return (8, 2);
                        break;
                    case 17:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 4);
                        yield return (7, 3);
                        yield return (8, 2);
                        yield return (9, 1);
                        break;
                    case 18:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 4);
                        yield return (7, 3);
                        yield return (8, 3);
                        yield return (9, 2);
                        break;
                    case 19:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 4);
                        yield return (7, 4);
                        yield return (8, 3);
                        yield return (9, 3);
                        break;
                    default:
                        yield return (0, 4);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 4);
                        yield return (7, 4);
                        yield return (8, 4);
                        yield return (9, 4);
                        break;
                }
            }
            else if (Specialization != null)
            {
                switch (level)
                {
                    case 0:
                        break;
                    case 1:
                    case 2:
                        yield return (1, 1);
                        break;
                    case 3:
                    case 4:
                        yield return (1, 1);
                        yield return (2, 1);
                        break;
                    case 5:
                    case 6:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        break;
                    case 7:
                    case 8:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        break;
                    case 9:
                    case 10:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        yield return (5, 1);
                        break;
                    case 11:
                    case 12:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        yield return (5, 1);
                        yield return (6, 1);
                        break;
                    case 13:
                    case 14:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        yield return (5, 1);
                        yield return (6, 1);
                        yield return (7, 1);
                        break;
                    case 15:
                    case 16:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        yield return (5, 1);
                        yield return (6, 1);
                        yield return (7, 1);
                        yield return (8, 1);
                        break;
                    case 17:
                    case 18:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        yield return (5, 1);
                        yield return (6, 1);
                        yield return (7, 1);
                        yield return (8, 1);
                        yield return (9, 1);
                        break;
                    case 19:
                    default:
                        yield return (1, 1);
                        yield return (2, 1);
                        yield return (3, 1);
                        yield return (4, 1);
                        yield return (5, 1);
                        yield return (6, 1);
                        yield return (7, 1);
                        yield return (8, 1);
                        yield return (9, 1);
                        break;
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<ClassSpell> UsableSpells { get; }
        public override IEnumerable<ClassSpell> UsableSpells
        {
            get
            {
                var _prohibited = ProhibitedStyles.ToList();
                return (from _csl in Campaign.SystemCampaign.SpellLists[GetType().FullName]
                        from _cSpell in _csl.Value
                        where !_prohibited.Any(_ms => _cSpell.SpellDef.MagicStyle.GetType().Equals(_ms.GetType()))
                        select _cSpell).ToList();
            }
        }
        #endregion

        public override bool MustRestToRecharge => true;
        public override MagicType MagicType => MagicType.Arcane;
        public override Type CasterClassType => typeof(Wizard);
        public override string ClassIconKey => @"wizard_class";
        public override string ClassName => @"Wizard";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.5d;
        public override bool HasGoodFortitude => false;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => true;

        public bool IsBonusFeatLevel(int level)
            => ((level % 5) == 0);

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            yield return typeof(ConcentrationSkill);
            yield return typeof(DecipherScriptSkill);
            yield return typeof(SpellcraftSkill);
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
            {
                yield return _skillType;
            }
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<KnowledgeFocus>(typeof(KnowledgeSkill<>)))
            {
                yield return _skillType;
            }
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<ProfessionFocus>(typeof(ProfessionSkill<>)))
            {
                yield return _skillType;
            }
            yield break;
        }
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => false;

        public bool IsProficientWithWeapon(Type type, int powerLevel)
        {
            // Club, dagger, heavy crossbow, light crossbow, and quarterstaff
            return ((
                typeof(Club).IsAssignableFrom(type) || typeof(Dagger).IsAssignableFrom(type) ||
                typeof(HeavyCrossbow).IsAssignableFrom(type) || typeof(LightCrossbow).IsAssignableFrom(type) ||
                typeof(Quarterstaff).IsAssignableFrom(type))
                && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level));
        }

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWithWeapon(weapon.GetType(), powerLevel);

        public string Description
            => @"Club, dagger, heavy crossbow, light crossbow, and quarterstaff.";

        #endregion

        #region public override bool CanLockLevel(int level)
        public override bool CanLockLevel(int level)
        {
            var _feat = BonusFeat(level);
            if (Requirements(level).Any(_req => !_req.IsSet))
            {
                return false;
            }

            return base.CanLockLevel(level);
        }
        #endregion

        #region public override IEnumerable<IFeature> Features(int level)
        public override IEnumerable<IFeature> Features(int level)
        {
            if (level == 1)
            {
                // familiar
                yield return new Feature(@"Summon Familiar", @"Ability to summon one permanent companion creature.");

                // scribe scroll (first level)
                var _scribe = ScribeScrollFeat;
                if (_scribe != null)
                {
                    yield return new Feature(_scribe.Name, _scribe.Benefit);
                }

                // specialization
                if (Specialization != null)
                {
                    yield return SpecialistFeature;
                    foreach (var _kvp in _Prohibition)
                    {
                        yield return new Feature(string.Format(@"Prohibited: {0}", _kvp.Value.GetType().Name), @"Unable to access magic of this style");
                    }
                }
            }

            // bonus feats (5, 10, 15, 20)
            if (IsBonusFeatLevel(level))
            {
                var _powerLevel = Creature.AdvancementLog[this, level].PowerDie.Level;
                var _feat = Creature.Feats
                    .Where(_f => (_f.PowerLevel == _powerLevel) && _f.IsBonusFeat(this))
                    .FirstOrDefault();
                if (_feat != null)
                {
                    yield return new Feature(_feat.Name, _feat.Benefit);
                }
            }

            // each auto-known spell is a feature
            foreach (var _known in from _k in KnownSpells.AllKnown
                                   where (_k.LearnedLevel == level) && (_k.LearnedIndex >= 0)
                                   orderby _k.LearnedIndex
                                   select _k)
            {
                yield return _known;
            }

            yield break;
        }
        #endregion

        #region public override IEnumerable<AdvancementRequirement> Requirements(int level)
        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            if ((level == 1) && (LockedLevel < level))
            {
                #region specialization and prohibitions
                // specialization (at level 1)
                yield return new AdvancementRequirement(new RequirementKey(@"Specialization"), @"Specialization",
                    @"Select magic style for additional spells per day and learn spell bonus", MagicStyleSpecialization,
                    SetSpecialistMagicStyle, CheckSpecialistMagicStyle)
                { CurrentValue = SpecialistFeature };

                // and prohibitions
                if (_Specialization != null)
                {
                    Feature _prohib(int _idx) => _Prohibition.ContainsKey(_idx)
                        ? new Feature(string.Format(@"Prohibited: {0}", _Prohibition[_idx].GetType().Name), @"Unable to access magic of this style")
                        : null;
                    yield return new AdvancementRequirement(new RequirementValueKey<int>(@"1", 1), @"Prohibited Style",
                        @"Magic style blocked from learning and use", MagicStyleProhibition, SetProhibitedMagicStyle,
                        CheckProhibitedMagicStyle)
                    {
                        CurrentValue = _prohib(1)
                    };
                    if (!(_Specialization is Divination))
                    {
                        yield return new AdvancementRequirement(new RequirementValueKey<int>(@"2", 2), @"Prohibited Style",
                            @"Magic style blocked from learning and use", MagicStyleProhibition, SetProhibitedMagicStyle,
                            CheckProhibitedMagicStyle)
                        {
                            CurrentValue = _prohib(2)
                        };
                    }
                }
                #endregion
            }

            // learnt feature
            if (LockedLevel < level)
            {
                if (level > 1)
                {
                    #region learn 2 new spells automatically
                    yield return new AdvancementRequirement(new LevelRequirementKey(@"0", level), @"Learn Spell",
                        @"Spell automatically learnt at level", SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                    {
                        CurrentValue = KnownSpells.LearnedSpell(level, 0)
                    };
                    yield return new AdvancementRequirement(new LevelRequirementKey(@"1", level), @"Learn Spell",
                        @"Spell automatically learnt at level", SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                    {
                        CurrentValue = KnownSpells.LearnedSpell(level, 1)
                    };
                    #endregion
                }
                else
                {
                    #region learnt spells at first level
                    // first level
                    yield return new AdvancementRequirement(new LevelRequirementKey(@"0", level), @"Learn Spell",
                        @"Spell automatically learnt at level", SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                    {
                        CurrentValue = KnownSpells.LearnedSpell(level, 0)
                    };
                    yield return new AdvancementRequirement(new LevelRequirementKey(@"1", level), @"Learn Spell",
                        @"Spell automatically learnt at level", SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                    {
                        CurrentValue = KnownSpells.LearnedSpell(level, 1)
                    };
                    yield return new AdvancementRequirement(new LevelRequirementKey(@"2", level), @"Learn Spell",
                        @"Spell automatically learnt at level", SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                    {
                        CurrentValue = KnownSpells.LearnedSpell(level, 2)
                    };
                    for (var _ix = 1; _ix <= Creature.Abilities.Intelligence.DeltaValue; _ix++)
                    {
                        yield return new AdvancementRequirement(new LevelRequirementKey((2 + _ix).ToString(), level), @"Learn Spell",
                            @"Spell automatically learnt at level", SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                        {
                            CurrentValue = KnownSpells.LearnedSpell(level, _ix + 2)
                        };
                    }
                    #endregion
                }
            }

            // bonus feats (5, 10, 15, 20)
            if (IsBonusFeatLevel(level) && (LockedLevel < level))
            {
                yield return new AdvancementRequirement(
                    new LevelRequirementKey(@"Wizard.BonusFeat", level), @"Bonus Feat",
                    @"Wizard receives magic related bonus feats", WizardBonusFeats, SetBonusFeat, CheckBonusFeat)
                {
                    CurrentValue = BonusFeature(level)
                };
            }

            yield break;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> WizardBonusFeats(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> WizardBonusFeats(IResolveRequirement target, RequirementKey key)
        {
            var _levelKey = key as LevelRequirementKey;
            var _powerLevel = Creature.AdvancementLog[this, _levelKey.Level].PowerDie.Level;

            // comparison
            // TODO: spell mastery
            bool _isBonus(Type _type)
                => typeof(MetamagicFeatBase).IsAssignableFrom(_type)
                || typeof(ItemCreationFeatBase).IsAssignableFrom(_type);

            foreach (var _item in from _available in FeatLister.AvailableFeats(Creature, this, _powerLevel)
                                  orderby _available.Name
                                  select _available)
            {
                if ((_item.Feat != null) && _isBonus(_item.Feat.GetType()))
                {
                    // GenericAdvancementOption<> carries to the selection UI as an IAdvancementOption, ...
                    // ...but back to SetBonusFeat as a GenericAdvancementOption<>
                    yield return new AdvancementParameter<FeatBase>(target, _item.Name, _item.Benefit, _item.Feat);
                }
                else
                {
                    // ParameterizedAdvancementOption carries to the selection UI as an IParameterizedAdvancementOption 
                    // (with option values of GenericAdvancementOption<Type>)
                    if ((_item is ParameterizedFeatListItem _pItem) && _isBonus(_pItem.GenericType))
                    {
                        // convert feat type list to generic advancement option list
                        yield return new GenericTypeAdvancementOption(target, _pItem.Name, _pItem.Benefit, _pItem.GenericType,
                            from p in _pItem.AvailableParameters
                            select (IAdvancementOption)(new AdvancementParameter<Type>(p.Name, p.Description, p.Type)));
                    }
                }
            }
            yield break;
        }
        #endregion

        #region private bool DoBindBonusFeat(FeatBase bonusFeat, int classLevel)
        private bool DoBindBonusFeat(FeatBase bonusFeat, int classLevel)
        {
            if (LockedLevel < classLevel)
            {
                var _feat = BonusFeat(classLevel);
                if (_feat != null)
                {
                    // tries to remove an existing bonus feat for this level
                    if (_feat.CanRemove())
                    {
                        // even if the new feat cannot be added, the old will be removed
                        _feat.UnbindFromCreature();
                    }
                    else
                    {
                        return false;
                    }
                }

                if (bonusFeat.CanAdd(Creature))
                {
                    bonusFeat.BindTo(Creature);
                    return true;
                }
                else
                {
                    // indicate this feat cannot be added
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region private bool SetBonusFeat(RequirementKey key, IAdvancementOption advOption)
        private bool SetBonusFeat(RequirementKey key, IAdvancementOption advOption)
        {
            // must be a simple feat
            var _levelKey = key as LevelRequirementKey;
            if (advOption is AdvancementParameter<FeatBase> _featOption)
            {
                var _newFeat = _featOption.ParameterValue;
                return DoBindBonusFeat(_newFeat, _levelKey.Level);
            }
            else
            {
                if (advOption is AdvancementParameter<Type> _typeOption)
                {
                    // must have been a parameterized feat, get the option, and its target
                    var _powerLevel = Creature.AdvancementLog[this, _levelKey.Level].PowerDie.Level;
                    var _newFeat = (FeatBase)Activator.CreateInstance(_typeOption.ParameterValue, this, _powerLevel);
                    return DoBindBonusFeat(_newFeat, _levelKey.Level);
                }

                // !!! sometimes a GenericTypeAdvanementOption worms its way into here when it has no options and is clicked
                return false;
            }
        }
        #endregion

        #region private bool CheckBonusFeat(RequirementKey key)
        private bool CheckBonusFeat(RequirementKey key)
        {
            var _levelKey = key as LevelRequirementKey;
            if (IsBonusFeatLevel(_levelKey.Level))
            {
                var _feat = BonusFeat(_levelKey.Level);
                if (_feat != null)
                {
                    return _feat.MeetsRequirementsAtPowerLevel;
                }
            }
            return false;
        }
        #endregion

        #region protected override void OnDecreaseLevel()
        /// <summary>Removes any wizard bonus feat for the level</summary>
        protected override void OnDecreaseLevel()
        {
            if (IsBonusFeatLevel(CurrentLevel + 1))
            {
                var _feat = BonusFeat(CurrentLevel + 1);
                if (_feat != null)
                {
                    _feat.UnbindFromCreature();
                }
            }
            foreach (var _known in KnownSpells.LearnedAtPowerLevel(CurrentLevel + 1).ToList())
            {
                KnownSpells.Remove(_known);
                if (Creature.IsInSystemEditMode)
                {
                    // remove from all books if editing in system-mode
                    var _bookSpots = (from _bk in Creature.Possessions.OfType<SpellBook>()
                                      from _bs in _bk.Spells
                                      where _bs.Owner.Equals(OwnerID) && (_bs.SpellDef.GetType().Equals(_known.SpellDef.GetType()))
                                      select new
                                      {
                                          Book = _bk,
                                          Spell = _bs
                                      }).ToList();
                    foreach (var _removal in _bookSpots)
                    {
                        _removal.Book.Remove(_removal.Spell);
                    }
                }
            }
            // NOTE: specialization and prohibition are not unbound automatically
            base.OnDecreaseLevel();
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> MagicStyleSpecialization(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> MagicStyleSpecialization(IResolveRequirement target, RequirementKey key)
        {
            yield return new AdvancementParameter<MagicStyle>(target, @"-Unspecialized-", @"-Unspecialized-", null);
            yield return new AdvancementParameter<MagicStyle>(target, @"Evocation", @"Evocation", new Evocation());
            yield return new AdvancementParameter<MagicStyle>(target, @"Conjuration", @"Conjuration", new Conjuration(Conjuration.SubConjure.Summoning));
            yield return new AdvancementParameter<MagicStyle>(target, @"Enchantment", @"Enchantment", new Enchantment(Enchantment.SubEnchantment.Charm));
            yield return new AdvancementParameter<MagicStyle>(target, @"Illusion", @"Illusion", new Illusion(Illusion.SubIllusion.Figment));
            yield return new AdvancementParameter<MagicStyle>(target, @"Necromancy", @"Necromancy", new Necromancy());
            yield return new AdvancementParameter<MagicStyle>(target, @"Divination", @"Divination", new Divination(Divination.SubDivination.Lore));
            yield return new AdvancementParameter<MagicStyle>(target, @"Transformation", @"Transformation", new Transformation());
            yield return new AdvancementParameter<MagicStyle>(target, @"Abjuration", @"Abjuration", new Abjuration());
            yield break;
        }
        #endregion

        #region private bool SetSpecialistMagicStyle(RequirementKey key, IAdvancementOption advOption)
        private bool SetSpecialistMagicStyle(RequirementKey key, IAdvancementOption advOption)
        {
            var _msOption = advOption as AdvancementParameter<MagicStyle>;
            if (_msOption.ParameterValue == null)
            {
                var _set = GetSpecialistSet();
                if (_set != null)
                {
                    // remove specialist spell slots (if defined)
                    _SpellSlots.Remove(_set);
                }
                _Prohibition.Clear();
            }
            else
            {
                var _set = GetSpecialistSet();
                if (_set != null)
                {
                    // remove any old specialist spell slots (if defined)
                    _SpellSlots.Remove(_set);
                }

                // add specialist spell slots (if missing)
                _SpellSlots.Add(new SpellSlotSet<PreparedSpellSlot>(Specialization));
                if ((_msOption.ParameterValue is Divination) && _Prohibition.ContainsKey(2))
                {
                    // only need one prohibition if using divination
                    _Prohibition.Remove(2);
                }
            }
            _Specialization = _msOption.ParameterValue;
            return true;
        }
        #endregion

        #region private bool CheckSpecialistMagicStyle(RequirementKey key)
        private bool CheckSpecialistMagicStyle(RequirementKey key)
        {
            // always true, since this is an optional
            return true;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> MagicStyleProhibition(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> MagicStyleProhibition(IResolveRequirement target, RequirementKey key)
        {
            bool _exist(Type _t)
                => ProhibitedStyles.Any(_ps => _t.IsAssignableFrom(_ps.GetType()));

            if (!(_Specialization is Evocation) && !_exist(typeof(Evocation)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Evocation", @"Evocation", new Evocation());
            }

            if (!(_Specialization is Conjuration) && !_exist(typeof(Conjuration)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Conjuration", @"Conjuration", new Conjuration(Conjuration.SubConjure.Summoning));
            }

            if (!(_Specialization is Enchantment) && !_exist(typeof(Enchantment)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Enchantment", @"Enchantment", new Enchantment(Enchantment.SubEnchantment.Charm));
            }

            if (!(_Specialization is Illusion) && !_exist(typeof(Illusion)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Illusion", @"Illusion", new Illusion(Illusion.SubIllusion.Figment));
            }

            if (!(_Specialization is Necromancy) && !_exist(typeof(Necromancy)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Necromancy", @"Necromancy", new Necromancy());
            }

            if (!(_Specialization is Divination) && !_exist(typeof(Divination)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Divination", @"Divination", new Divination(Divination.SubDivination.Lore));
            }

            if (!(_Specialization is Transformation) && !_exist(typeof(Transformation)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Transformation", @"Transformation", new Transformation());
            }

            if (!(_Specialization is Abjuration) && !_exist(typeof(Abjuration)))
            {
                yield return new AdvancementParameter<MagicStyle>(target, @"Abjuration", @"Abjuration", new Abjuration());
            }

            yield break;
        }
        #endregion

        #region private bool SetProhibitedMagicStyle(RequirementKey key, IAdvancementOption advOption)
        private bool SetProhibitedMagicStyle(RequirementKey key, IAdvancementOption advOption)
        {
            var _reqKey = key as RequirementValueKey<int>;
            var _index = _reqKey.Key;
            var _msOption = advOption as AdvancementParameter<MagicStyle>;
            switch (_index)
            {
                case 1:
                    if (_Prohibition.ContainsKey(2) && _Prohibition[2].GetType().Equals(_msOption.ParameterValue.GetType()))
                    {
                        return false;
                    }

                    break;
                case 2:
                    if (_Prohibition.ContainsKey(1) && _Prohibition[1].GetType().Equals(_msOption.ParameterValue.GetType()))
                    {
                        return false;
                    }

                    break;
            }
            _Prohibition[_index] = _msOption.ParameterValue;
            return true;
        }
        #endregion

        #region private bool CheckProhibitedMagicStyle(RequirementKey key)
        private bool CheckProhibitedMagicStyle(RequirementKey key)
        {
            if (_Specialization != null)
            {
                if (_Specialization is Divination)
                {
                    // NEED 1
                    return _Prohibition.Count == 1;
                }
                else
                {
                    // NEED 2
                    return _Prohibition.Count == 2;
                }
            }
            return true;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> SpellGainAtLevel(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> SpellGainAtLevel(IResolveRequirement target, RequirementKey key)
        {
            var _key = key as LevelRequirementKey;
            var _max = MaximumSpellLevelAtLevel(_key.Level);
            var _knownTypes = KnownSpells.AllKnown.Select(_k => _k.SpellDef.GetType()).ToList();
            foreach (var _spell in from _cs in UsableSpells
                                   where (_cs.Level <= _max) && (_cs.Level > 0)
                                   && !_knownTypes.Any(_kt => _kt.Equals(_cs.SpellDef.GetType()))
                                   select _cs)
            {
                yield return new AdvancementParameter<ClassSpell>(target, string.Format(@"{0} ({1})", _spell.SpellDef.DisplayName, _spell.Level),
                    _spell.SpellDef.Description, _spell);
            }
            yield break;
        }
        #endregion

        #region private bool SetSpellGainAtLevel(RequirementKey key, IAdvancementOption advOption)
        private bool SetSpellGainAtLevel(RequirementKey key, IAdvancementOption advOption)
        {
            var _key = key as LevelRequirementKey;
            var _idx = System.Convert.ToInt32(_key.Name);
            var _classSpell = (advOption as AdvancementParameter<ClassSpell>).ParameterValue;

            // remove from any books it was written in (that we still own), 
            // NOTE: if a book has been depossessed, it still has the spell 
            // NOTE: potential abuse here during character creation, or recovery after level loss
            var _exist = KnownSpells.LearnedSpell(_key.Level, _idx);
            if (_exist != null)
            {
                // find book(s) it may have been written in (multiple in case of rewriting)
                var _bookSpots = (from _bk in Creature.Possessions.OfType<SpellBook>()
                                  from _bs in _bk.Spells
                                  where _bs.Owner.Equals(OwnerID) && (_bs.SpellDef.GetType().Equals(_classSpell.SpellDef.GetType()))
                                  select new
                                  {
                                      Book = _bk,
                                      Spell = _bs
                                  }).ToList();
                foreach (var _removal in _bookSpots)
                {
                    _removal.Book.Remove(_removal.Spell);
                }
            }

            // add (and to book if available)
            KnownSpells.Add(new KnownSpell(_classSpell.SpellDef, _classSpell.Level, _key.Level, _idx));

            // look for available book in load
            var _book = (from _bk in Creature.ObjectLoad.OfType<SpellBook>()
                         where _bk.Possessor.Equals(Creature)
                         && _bk.CanHoldSpell(_classSpell.Level)
                         select _bk).FirstOrDefault();
            _book ??= (from _bk in Creature.Possessions.OfType<SpellBook>()
                         where _bk.CanHoldSpell(_classSpell.Level)
                         select _bk).FirstOrDefault();
            if (_book != null)
            {
                _book.Add(new BookSpell(_classSpell.Level, _classSpell.SpellDef, OwnerID));
            }
            return true;
        }
        #endregion

        #region private bool CheckSpellGainAtLevel(RequirementKey key)
        private bool CheckSpellGainAtLevel(RequirementKey key)
        {
            if (key is LevelRequirementKey _level)
            {
                var _idx = System.Convert.ToInt32(_level.Name);
                return KnownSpells.LearnedSpell(_level.Level, _idx) != null;
            }
            return false;
        }
        #endregion

        public IEnumerable<SpellBook> AvailableSpellBooks
            => from _bk in Creature.ObjectLoad.AllLoadedObjects().OfType<SpellBook>()
               select _bk;

        #region public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
        public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
        {
            if (setIndex == 0)
            {
                // always read magic...
                yield return new BookSpell(0, new ReadMagic(), OwnerID);

                // any spell in a personal spell-book
                var _prohibited = ProhibitedStyles.ToList();
                foreach (var _spell in from _book in AvailableSpellBooks
                                       from _bookSpell in _book.DecipheredSpells(OwnerID)
                                       where !_prohibited.Any(_ms => _bookSpell.SpellDef.MagicStyle.GetType().Equals(_ms.GetType()))
                                       select _bookSpell)
                {
                    // exclude read magic, since this was already yielded
                    if (!(_spell.SpellDef is ReadMagic))
                    {
                        yield return _spell;
                    }
                }
            }
            else if (Specialization != null)
            {
                // yield specialization spells
                var _specType = Specialization.GetType();
                foreach (var _spell in from _book in AvailableSpellBooks
                                       from _bookSpell in _book.DecipheredSpells(OwnerID)
                                       where _bookSpell.SpellDef.MagicStyle.GetType().Equals(_specType)
                                       select _bookSpell)
                {
                    yield return _spell;
                }
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<BookSpell> UndecipheredBookSpells { get; }
        public IEnumerable<BookSpell> UndecipheredBookSpells
        {
            get
            {
                // any spell in a personal spell-book
                var _prohibited = ProhibitedStyles.ToList();
                foreach (var _spell in from _book in AvailableSpellBooks
                                       from _bookSpell in _book.Spells
                                       where !_bookSpell.HasDecipheredOrScribed(this)
                                       && !_prohibited.Any(_ms => _bookSpell.SpellDef.MagicStyle.GetType().Equals(_ms.GetType()))
                                       select _bookSpell)
                {
                    // exclude read magic, since this was already yielded
                    if (!(_spell.SpellDef is ReadMagic))
                    {
                        yield return _spell;
                    }
                }
                yield break;
            }
        }
        #endregion

        public override string SpellSlotsName(int setIndex)
            => ((setIndex > 0) && (Specialization != null))
            ? $@"{Specialization.SpecialistName} Spells"
            : base.SpellSlotsName(setIndex);

        /// <summary>setIndex==0: standard spells; setIndex==1: specialization spells</summary>
        public override IEnumerable<(int SlotLevel, int SpellsPerDay)> SpellsPerDayAtLevel(int setIndex, int level)
        {
            // influence spells do not get bonus slots, so just the levels
            if ((setIndex > 0) && (Specialization != null))
            {
                return BaseSpellsPerDayAtLevel(setIndex, level);
            }

            // regular spells
            return base.SpellsPerDayAtLevel(setIndex, level);
        }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsPowerClassActive)
            {
                if (budget is LocalActionBudget _budget)
                {
                    // TODO: prepare foreign spell action also...
                    // TODO: specialization prepare

                    // base actions
                    foreach (var _act in base.BaseActions(budget))
                    {
                        yield return _act;
                    }
                }
            }
            yield break;
        }

        public static ItemCaster CreateItemCaster(int level, Alignment alignment = null)
            => new ItemCaster(MagicType.Arcane, level, alignment ?? Alignment.TrueNeutral,
                10 + level + (level / 2), Guid.Empty, typeof(Wizard));
    }
}
