using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Abilities;
using Uzi.Core;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.TypeListers;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Advancement.CharacterClasses
{
    [ClassInfo(@"Cleric", 8, 0.75d, 2, true, false, true)]
    [Serializable]
    public class Cleric : PreparedCaster, IWeaponProficiency, IArmorProficiency, IShieldProficiency,
        IPrimaryInfluenceClass, IProcessFeedback
    {
        #region construction
        public Cleric()
            : this(PowerDieCalcMethod.Average)
        {
        }

        public Cleric(PowerDieCalcMethod calcMethod) :
            base(8, calcMethod)
        {
            _SpellDifficultyBase = new ConstDeltable(10);
            _ChannelChoice = true;

            // influence spells (from devotion)
            _SpellSlots.Add(new SpellSlotSet<PreparedSpellSlot>(typeof(Devotion)));
        }
        #endregion

        #region private data
        private IDeltable _SpellDifficultyBase;
        private bool _ChannelChoice;
        #endregion

        public override bool MustRestToRecharge => false;
        public override MagicType MagicType => MagicType.Divine;
        public override IDeltable SpellDifficultyBase => _SpellDifficultyBase;
        public override Type CasterClassType => typeof(Cleric);

        #region public override IEnumerable<Type> ClassSkills()
        public override IEnumerable<Type> ClassSkills()
        {
            // TODO: additional skills provided by influences...
            yield return typeof(ConcentrationSkill);
            yield return typeof(DiplomacySkill);
            yield return typeof(HealSkill);
            yield return typeof(KnowledgeSkill<KnowArcana>);
            yield return typeof(KnowledgeSkill<KnowReligion>);
            yield return typeof(KnowledgeSkill<KnowPlanes>);
            yield return typeof(SpellcraftSkill);
            foreach (Type _skillType in SubSkillLister.SubSkillTypes<CraftFocus>(typeof(CraftSkill<>)))
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

        public override CastingAbilityBase SpellDifficultyAbility
            => Creature?.Abilities.Wisdom;

        public override CastingAbilityBase BonusSpellAbility
            => Creature?.Abilities.Wisdom;

        #region public bool? IsNaturallyPositive { get; }
        /// <summary>Indicates that the cleric is naturally positive or negative (or undefined)</summary>
        public bool? IsNaturallyPositive
        {
            get
            {
                if (Creature != null)
                {
                    if ((Creature.Alignment.Ethicality == GoodEvilAxis.Good)
                        || ((Creature.Alignment.Ethicality == GoodEvilAxis.Neutral) &&
                        (Creature.Devotion.Alignment.Ethicality == GoodEvilAxis.Good)))
                    {
                        return true;
                    }
                    else if ((Creature.Alignment.Ethicality == GoodEvilAxis.Evil)
                        || ((Creature.Alignment.Ethicality == GoodEvilAxis.Neutral) &&
                        (Creature.Devotion.Alignment.Ethicality == GoodEvilAxis.Evil)))
                    {
                        return false;
                    }
                }
                return null;
            }
        }
        #endregion

        /// <summary>Indicates that the cleric is channeling energy positively or negatively</summary>
        public bool IsPositiveChannel
            => IsNaturallyPositive ?? _ChannelChoice;

        public override bool CanUseDescriptor(Descriptor descriptor)
        {
            if (descriptor is Chaotic)
            {
                return Creature?.Alignment.Orderliness != LawChaosAxis.Lawful;
            }
            else if (descriptor is Lawful)
            {
                return Creature?.Alignment.Orderliness != LawChaosAxis.Chaotic;
            }
            else if (descriptor is Good)
            {
                return Creature?.Alignment.Ethicality != GoodEvilAxis.Evil;
            }
            else if (descriptor is Evil)
            {
                return Creature?.Alignment.Ethicality != GoodEvilAxis.Good;
            }
            return true;
        }

        #region protected override void OnAdd()
        protected override void OnAdd()
        {
            base.OnAdd();
            _SpellDifficultyBase.Deltas.Add(SpellDifficultyAbility.IModifier);
            _SpellDifficultyBase.Deltas.Add((IQualifyDelta)Creature.ExtraSpellDifficulty);

            DriveUndeadAdjunct.AddSource(Creature, this, IsPositiveChannel);
            Creature.AddIInteractHandler(this);
        }
        #endregion

        #region protected override void OnRemove()
        protected override void OnRemove()
        {
            // remove deltas provided by creature, so they do not continue to anchor this class
            _SpellDifficultyBase.Deltas.Remove(SpellDifficultyAbility.IModifier);
            _SpellDifficultyBase.Deltas.Remove((IQualifyDelta)Creature.ExtraSpellDifficulty);

            DriveUndeadAdjunct.RemoveSource(Creature, this);
            Creature.RemoveIInteractHandler(this);

            // remove all influences associated with the cleric
            foreach (var _inf in Influences.ToList())
            {
                _inf.Eject();
            }

            base.OnRemove();
        }
        #endregion

        #region public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // cleric must have access to powers
            if (IsPowerClassActive)
            {
                // TODO: influence prepare...

                // base actions
                foreach (var _act in BaseActions(budget))
                {
                    yield return _act;
                }

                if (budget is LocalActionBudget _budget)
                {
                    // all channellings are regular actions
                    if (_budget.CanPerformRegular)
                    {
                        CastSpell _cast(int spellLevel, SpellDef spell, PreparedSpellSlot slot)
                        {
                            var _source = new SpellSource(this, spellLevel, slot.SlotLevel, true, spell);
                            return new CastSpell(_source, spell.SpellModes.FirstOrDefault(), spell.ActionTime, slot, spell.DisplayName);
                        }

                        // must have made a choice on channelling
                        if (IsPositiveChannel)
                        {
                            // only cleric slots can be channelled
                            foreach (var (_, _, _slotSet) in AllSpellSlotSets.Where(_ss => _ss.SlotSet.Source == this))
                            {
                                foreach (var _level in _slotSet.AllLevels)
                                {
                                    foreach (var _slot in _level.Slots.Where(_s => _s.PreparedSpell != null))
                                    {
                                        switch (_level.Level)
                                        {
                                            case 8: // 8: critical, mass
                                                goto case 7;
                                            case 7: // 7: serious, mass
                                                goto case 6;
                                            case 6: // 6: moderate, mass
                                                goto case 5;
                                            case 5: // 5: light, mass
                                                goto case 4;
                                            case 4: // 4: critical
                                                yield return _cast(4, new CureCriticalWounds(), _slot);
                                                goto case 3;
                                            case 3: // 3: serious
                                                yield return _cast(3, new CureSeriousWounds(), _slot);
                                                goto case 2;
                                            case 2: // 2: moderate
                                                yield return _cast(2, new CureModerateWounds(), _slot);
                                                goto case 1;
                                            case 1: // 1: light
                                                yield return _cast(1, new CureLightWounds(), _slot);
                                                goto case 0;
                                            case 0: // 0: minor
                                                yield return _cast(0, new CureMinorWounds(), _slot);
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (!IsPositiveChannel)
                        {
                            // only cleric slots can be channelled
                            foreach (var (_, _, _slotSet) in AllSpellSlotSets.Where(_ss => _ss.SlotSet.Source == this))
                            {
                                foreach (var _level in _slotSet.AllLevels)
                                {
                                    foreach (var _slot in _level.Slots.Where(_s => _s.PreparedSpell != null))
                                    {
                                        switch (_level.Level)
                                        {
                                            case 8: // 8: critical, mass
                                                goto case 7;
                                            case 7: // 7: serious, mass
                                                goto case 6;
                                            case 6: // 6: moderate, mass
                                                goto case 5;
                                            case 5: // 5: light, mass
                                                goto case 4;
                                            case 4: // 4: critical
                                                yield return _cast(4, new InflictCriticalWounds(), _slot);
                                                goto case 3;
                                            case 3: // 3: serious
                                                yield return _cast(3, new InflictSeriousWounds(), _slot);
                                                goto case 2;
                                            case 2: // 2: moderate
                                                yield return _cast(2, new InflictModerateWounds(), _slot);
                                                goto case 1;
                                            case 1: // 1: light
                                                yield return _cast(1, new InflictLightWounds(), _slot);
                                                goto case 0;
                                            case 0: // 0: minor
                                                yield return _cast(0, new InflictMinorWounds(), _slot);
                                                break;

                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        /// <summary>Usable spells includes castable spells and influence spells</summary>
        public override IEnumerable<ClassSpell> UsableSpells
            => PreparableSpells(0).Union(PreparableSpells(1));

        public override string ClassName => @"Cleric";
        public override string ClassIconKey => @"cleric_class";
        public override int SkillPointsPerLevel => 2;
        public override double BABProgression => 0.75d;
        public override bool HasGoodFortitude => true;
        public override bool HasGoodReflex => false;
        public override bool HasGoodWill => true;

        #region public override bool CanLockLevel(int level)
        public override bool CanLockLevel(int level)
        {
            if (level == 1)
            {
                if (Influences.Count() < 2)
                {
                    return false;
                }
            }
            return base.CanLockLevel(level);
        }
        #endregion

        #region public override IEnumerable<IFeature> Features(int level)
        public override IEnumerable<IFeature> Features(int level)
        {
            if (level == 1)
            {
                if (IsPositiveChannel)
                {
                    yield return new Feature(@"Repulse Undead", @"Repulse or destroy undead");
                    yield return new Feature(@"Cure Spell Transmutation", @"Convert spells to cure spells of equal power level");
                }
                else
                {
                    yield return new Feature(@"Overwhelm Undead", @"Overwhelm, reinforce or dispel repulsion of undead");
                    yield return new Feature(@"Inflict Spell Transmutation", @"Convert spells to inflict spells of equal power level");
                }

                foreach (var _inf in Influences)
                {
                    if (_inf.AdjunctKey.StartsWith(@"ClassInfluence."))
                    {
                        yield return new Feature(_inf.Name, _inf.Description);
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<AdvancementRequirement> Requirements(int level)
        public override IEnumerable<AdvancementRequirement> Requirements(int level)
        {
            if ((level == 1) && (LockedLevel < 1))
            {
                if (IsNaturallyPositive == null)
                {
                    yield return new AdvancementRequirement(new RequirementKey(@"Channel"), @"Channel Bias",
                        @"Channel positive or channel negative", ChannelSupplier, ChannelSetter,
                        ChannelChecker)
                    {
                        CurrentValue = IsPositiveChannel
                            ? new Feature(@"Positive Channel", @"Repulse undead and cure spells")
                            : new Feature(@"Negative Channel", @"Overwhelm undead and inflict spells")
                    };
                }
                yield return new AdvancementRequirement(new RequirementKey(@"ClassInfluence.1"), @"Influence",
                    @"Select one influence from devotion:", InfluenceSupplier, InfluenceSetter,
                    InfluenceChecker)
                { CurrentValue = InfluenceFeature(@"ClassInfluence.1") };
                yield return new AdvancementRequirement(new RequirementKey(@"ClassInfluence.2"), @"Influence",
                    @"Select another influence from devotion:", InfluenceSupplier, InfluenceSetter,
                    InfluenceChecker)
                { CurrentValue = InfluenceFeature(@"ClassInfluence.2") };
            }
        }
        #endregion

        #region channel positive requirements
        private IEnumerable<IAdvancementOption> ChannelSupplier(IResolveRequirement target, RequirementKey key)
        {
            yield return new AdvancementParameter<bool>(target, @"Positive", @"Channel Positive Energy", true);
            yield return new AdvancementParameter<bool>(target, @"Negative", @"Channel Negative Energy", false);
            yield break;
        }

        private bool ChannelSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if (advOption is AdvancementParameter<bool> _bool)
            {
                // cleanup old adjunct uses
                DriveUndeadAdjunct.RemoveSource(Creature, this);

                // use new 
                _ChannelChoice = _bool.ParameterValue;
                DriveUndeadAdjunct.AddSource(Creature, this, IsPositiveChannel);
                return true;
            }
            return false;
        }

        private bool ChannelChecker(RequirementKey key)
        {
            return true;
        }
        #endregion

        #region influence requirements
        private IEnumerable<IAdvancementOption> InfluenceSupplier(IResolveRequirement target, RequirementKey key)
        {
            // supply influence from creature's devotion
            if (Creature.Devotion != null)
            {
                // exclude existing influences
                var _exist = Influences.Select(_e => _e.Name).ToList();
                foreach (var _inf in Creature.Devotion.Influences(this).Where(_i => !_exist.Contains(_i.Name)))
                {
                    yield return new AdvancementParameter<Influence>(target, _inf.Name, _inf.Description, _inf);
                }
            }
            yield break;
        }

        private bool InfluenceSetter(RequirementKey key, IAdvancementOption advOption)
        {
            if ((advOption is AdvancementParameter<Influence> _inf) && (Creature != null))
            {
                // remove existing adjunct if replacing
                var _exist = Influences.FirstOrDefault(_i => _i.AdjunctKey.Equals(key.Name));
                if (_exist != null)
                {
                    _exist.Eject();
                }

                // add new as adjunct
                _inf.ParameterValue.AdjunctKey = key.Name;
                _inf.ParameterValue.PowerLevel = Creature.AdvancementLog[this, 1].PowerDie.Level;
                Creature.AddAdjunct(_inf.ParameterValue);
                return true;
            }
            return false;
        }

        private IFeature InfluenceFeature(string keyName)
        {
            var _inf = Influences.FirstOrDefault(_i => _i.AdjunctKey.Equals(keyName));
            if (_inf != null)
            {
                return new Feature(_inf.Name, _inf.Description);
            }
            return null;
        }

        private bool InfluenceChecker(RequirementKey key)
        {
            return Influences.Any(_inf => _inf.AdjunctKey.Equals(key.Name));
        }
        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => (profType == WeaponProficiencyType.Simple)
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => IsProficientWithWeapon(typeof(WpnType), powerLevel);

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => IsProficientWith(weapon.ProficiencyType, powerLevel);

        /// <summary>everything but exotic weapons (generally)</summary>
        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => (!typeof(IMartialWeapon).IsAssignableFrom(type) && !typeof(IExoticWeapon).IsAssignableFrom(type))
            && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        string IWeaponProficiency.Description => @"All simple weapons";

        #endregion

        #region IArmorProficiency Members
        /// <summary>proficient with all armor</summary>
        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
            => (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        /// <summary>proficient with all armor</summary>
        public bool IsProficientWith(ArmorBase armor, int powerLevel)
            => (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        string IArmorProficiency.Description => @"All Armor";
        #endregion

        #region IShieldProficiency Members
        public bool IsProficientWithShield(bool tower, int powerLevel)
            => !tower && (powerLevel >= Creature.AdvancementLog[this, 1].PowerDie.Level);

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
            => IsProficientWithShield(shield.Tower, powerLevel);

        string IShieldProficiency.Description => @"Normal Shields";
        #endregion

        #region IPrimaryInfluenceClass Members

        public IEnumerable<Influence> Influences
        {
            get
            {
                if (Creature != null)
                {
                    foreach (var _inf in Creature.Adjuncts.OfType<Influence>().Where(_i => _i.InfluenceClass == this))
                    {
                        yield return _inf;
                    }
                }

                yield break;
            }
        }

        #endregion

        #region IProcessFeedback Members

        void IProcessFeedback.ProcessFeedback(Interaction workSet)
        {
            // successfully added or removed devotion, influence or alignment
            if (((workSet?.InteractData is AddAdjunctData _add)
                && ((_add.Adjunct is Devotion) || (_add.Adjunct is AlignedCreature) || (_add.Adjunct is Influence)))
                ||
                ((workSet?.InteractData is RemoveAdjunctData _rmv)
                && ((_rmv.Adjunct is Devotion) || (_rmv.Adjunct is AlignedCreature) || (_rmv.Adjunct is Influence)))
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_b => _b.Value))
            {
                var _critterAlign = Creature.Alignment;
                var _devotionAlign = Creature.Devotion.Alignment;
                if (Creature != null)
                {
                    if (_critterAlign.IsNeutral)
                    {
                        // creature can be neutral, only if the devotion is neutral
                        if (!_devotionAlign.IsNeutral)
                        {
                            if (!Creature.Adjuncts.OfType<PowerClassSuppress>().Any(_pcs => _pcs.Source.Equals(this)
                                && _pcs.PowerClass.Equals(this)))
                            {
                                Creature.AddAdjunct(new PowerClassSuppress(this, this));
                            }

                            return;
                        }
                    }
                    else
                    {
                        var _step = 0;
                        // creature's alignment must be within a step of its devotion
                        switch (_critterAlign.Ethicality)
                        {
                            case GoodEvilAxis.Good:
                                _step += (_devotionAlign.Ethicality == GoodEvilAxis.Good ? 0 : (_devotionAlign.Ethicality == GoodEvilAxis.Neutral ? 1 : 2));
                                break;
                            case GoodEvilAxis.Neutral:
                                _step += (_devotionAlign.Ethicality == GoodEvilAxis.Neutral ? 0 : 1);
                                break;
                            case GoodEvilAxis.Evil:
                                _step += (_devotionAlign.Ethicality == GoodEvilAxis.Evil ? 0 : (_devotionAlign.Ethicality == GoodEvilAxis.Neutral ? 1 : 2));
                                break;
                        }
                        if (_step > 1)
                        {
                            // too far away already
                            if (!Creature.Adjuncts.OfType<PowerClassSuppress>().Any(_pcs => _pcs.Source.Equals(this)
                                && _pcs.PowerClass.Equals(this)))
                            {
                                Creature.AddAdjunct(new PowerClassSuppress(this, this));
                            }

                            return;
                        }
                        switch (_critterAlign.Orderliness)
                        {
                            case LawChaosAxis.Lawful:
                                _step += (_devotionAlign.Orderliness == LawChaosAxis.Lawful ? 0 : (_devotionAlign.Orderliness == LawChaosAxis.Neutral ? 1 : 2));
                                break;
                            case LawChaosAxis.Neutral:
                                _step += (_devotionAlign.Orderliness == LawChaosAxis.Neutral ? 0 : 1);
                                break;
                            case LawChaosAxis.Chaotic:
                                _step += (_devotionAlign.Orderliness == LawChaosAxis.Chaotic ? 0 : (_devotionAlign.Orderliness == LawChaosAxis.Neutral ? 1 : 2));
                                break;
                        }
                        if (_step > 1)
                        {
                            // too far away
                            if (!Creature.Adjuncts.OfType<PowerClassSuppress>().Any(_pcs => _pcs.Source.Equals(this)
                                && _pcs.PowerClass.Equals(this)))
                            {
                                Creature.AddAdjunct(new PowerClassSuppress(this, this));
                            }

                            return;
                        }
                    }

                    // every influence must be for this devotion
                    foreach (var _inf in Influences)
                    {
                        if (!_inf.Devotion.Name.Equals(Creature.Devotion.Name))
                        {
                            if (!Creature.Adjuncts.OfType<PowerClassSuppress>().Any(_pcs => _pcs.Source.Equals(this)
                                && _pcs.PowerClass.Equals(this)))
                            {
                                Creature.AddAdjunct(new PowerClassSuppress(this, this));
                            }

                            return;
                        }
                    }

                    // passed all tests
                    foreach (var _pcs in Creature.Adjuncts.OfType<PowerClassSuppress>().Where(_pcs => _pcs.Source.Equals(this)
                        && _pcs.PowerClass.Equals(this)).ToList())
                    {
                        _pcs.Eject();
                    }
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        void IInteractHandler.HandleInteraction(Interaction workSet)
        {
        }

        IEnumerable<Type> IInteractHandler.GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData); // feedback processing
            yield return typeof(RemoveAdjunctData); // feedback processing
            yield break;
        }

        bool IInteractHandler.LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor
            if (typeof(AddAdjunctData).Equals(interactType))
            {
                return true;
            }

            // last feedback processor
            if (typeof(RemoveAdjunctData).Equals(interactType))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
        /// <summary>setIndex==0: standard spells; setIndex==1: influence spells</summary>
        public override IEnumerable<ClassSpell> PreparableSpells(int setIndex)
        {
            if (setIndex == 0)
            {
                // standard spells
                return (from _csl in Campaign.SystemCampaign.SpellLists[GetType().FullName]
                        from _cSpell in _csl.Value
                            // TODO: filter by alignment and descriptor compatibility
                        select _cSpell)
                        .ToList();
            }
            else
            {
                // influence spells
                return Influences.SelectMany(_inf => _inf.Spells).ToList();
            }
        }
        #endregion

        #region protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int setIndex, int level)
        /// <summary>setIndex==0: standard spells; setIndex==1: influence spells</summary>
        protected override IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int setIndex, int level)
        {
            if (setIndex == 0)
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
                        yield return (0, 5);
                        yield return (1, 3);
                        yield return (2, 2);
                        break;
                    case 5:
                        yield return (0, 5);
                        yield return (1, 3);
                        yield return (2, 2);
                        yield return (3, 1);
                        break;
                    case 6:
                        yield return (0, 5);
                        yield return (1, 3);
                        yield return (2, 3);
                        yield return (3, 2);
                        break;
                    case 7:
                        yield return (0, 6);
                        yield return (1, 4);
                        yield return (2, 3);
                        yield return (3, 2);
                        yield return (4, 1);
                        break;
                    case 8:
                        yield return (0, 6);
                        yield return (1, 4);
                        yield return (2, 3);
                        yield return (3, 3);
                        yield return (4, 2);
                        break;
                    case 9:
                        yield return (0, 6);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 3);
                        yield return (4, 2);
                        yield return (5, 1);
                        break;
                    case 10:
                        yield return (0, 6);
                        yield return (1, 4);
                        yield return (2, 4);
                        yield return (3, 3);
                        yield return (4, 3);
                        yield return (5, 2);
                        break;
                    case 11:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 3);
                        yield return (5, 2);
                        yield return (6, 1);
                        break;
                    case 12:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 4);
                        yield return (3, 4);
                        yield return (4, 3);
                        yield return (5, 3);
                        yield return (6, 2);
                        break;
                    case 13:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 3);
                        yield return (6, 2);
                        yield return (7, 1);
                        break;
                    case 14:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 4);
                        yield return (4, 4);
                        yield return (5, 3);
                        yield return (6, 3);
                        yield return (7, 2);
                        break;
                    case 15:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 5);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 3);
                        yield return (7, 2);
                        yield return (8, 1);
                        break;
                    case 16:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 5);
                        yield return (4, 4);
                        yield return (5, 4);
                        yield return (6, 3);
                        yield return (7, 3);
                        yield return (8, 2);
                        break;
                    case 17:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 5);
                        yield return (4, 5);
                        yield return (5, 4);
                        yield return (6, 4);
                        yield return (7, 3);
                        yield return (8, 2);
                        yield return (9, 1);
                        break;
                    case 18:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 5);
                        yield return (4, 5);
                        yield return (5, 4);
                        yield return (6, 4);
                        yield return (7, 3);
                        yield return (8, 3);
                        yield return (9, 2);
                        break;
                    case 19:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 5);
                        yield return (4, 5);
                        yield return (5, 5);
                        yield return (6, 4);
                        yield return (7, 4);
                        yield return (8, 3);
                        yield return (9, 3);
                        break;
                    default:
                        yield return (0, 6);
                        yield return (1, 5);
                        yield return (2, 5);
                        yield return (3, 5);
                        yield return (4, 5);
                        yield return (5, 5);
                        yield return (6, 4);
                        yield return (7, 4);
                        yield return (8, 4);
                        yield return (9, 4);
                        break;
                }
            }
            else
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

        /// <summary>setIndex==0: standard spells; setIndex==1: influence spells</summary>
        public override IEnumerable<(int SlotLevel, int SpellsPerDay)> SpellsPerDayAtLevel(int setIndex, int level)
        {
            // influence spells do not get bonus slots, so just the levels
            if (setIndex > 0)
            {
                return BaseSpellsPerDayAtLevel(setIndex, level);
            }

            // regular spells
            return base.SpellsPerDayAtLevel(setIndex, level);
        }

        public override string SpellSlotsName(int setIndex)
            => (setIndex == 0)
            ? base.SpellSlotsName(setIndex)
            : @"Influence Spells";

        public static ItemCaster CreateItemCaster(int level, Alignment alignment = null)
            => new ItemCaster(MagicType.Divine, level, alignment ?? Alignment.TrueNeutral, 10 + level + (level / 2), Guid.Empty, typeof(Cleric));
    }
}