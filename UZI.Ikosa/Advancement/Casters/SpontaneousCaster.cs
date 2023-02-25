using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Magic;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class SpontaneousCaster : SlottedCasterClass<SpellSlot>, IActionProvider, ISpontaneousCaster
    {
        #region Construction
        protected SpontaneousCaster(byte powerDieSize)
            : base(powerDieSize)
        {
            _Known = new KnownSpellSet();
        }

        protected SpontaneousCaster(byte powerDieSize, PowerDieCalcMethod calcMethod) :
            base(powerDieSize, calcMethod)
        {
            _Known = new KnownSpellSet();
        }
        #endregion

        private KnownSpellSet _Known;

        /// <summary>
        /// Returns the number of knowns spells per level for a given class level.  
        /// Key is spell level, value is number.
        /// </summary>
        public abstract IEnumerable<KeyValuePair<int, int>> KnownSpellsPerLevel(int level);

        public IEnumerable<KeyValuePair<int, int>> KnownSpellNumbers
            => KnownSpellsPerLevel(EffectiveLevel.QualifiedValue(PowerLevelCheck.LevelCheck(Creature.AdvancementLog.NumberPowerDice)));

        public KnownSpellSet KnownSpells => _Known;

        public override IEnumerable<ClassSpell> SlottableSpells(int setIndex)
            => from _spell in KnownSpells.AllKnown
               select new ClassSpell(_spell.SlotLevel, _spell.SpellDef);

        #region protected override void OnAdd()
        protected override void OnAdd()
        {
            Creature.Actions.Providers.Add(this, (IActionProvider)this);
            Creature.AddAdjunct(new SpontaneousCasterReactions(new ReactorStepCompleteGroup(this), this));
            base.OnAdd();
        }
        #endregion

        #region protected override void OnRemove()
        protected override void OnRemove()
        {
            base.OnRemove();
            Creature.Adjuncts
                .OfType<SpontaneousCasterReactions>()
                .FirstOrDefault(_r => _r.SpontaneousCaster == this)?
                .Eject();
            Creature.Actions.Providers.Remove(this);
        }
        #endregion

        #region protected IEnumerable<AdvancementRequirement> BaseRequirements(int level)
        protected IEnumerable<AdvancementRequirement> BaseRequirements(int level)
        {
            if (LockedLevel < level)
            {
                if (level == 1)
                {
                    // all known spells at level 1 are requirements
                    var _idx = 0;
                    foreach (var _kvp in KnownSpellsPerLevel(1))
                        for (var _lx = 0; _lx < _kvp.Value; _lx++)
                        {
                            yield return new AdvancementRequirement(
                                new LearnedSpellRequirementKey(_idx.ToString(), level, _kvp.Key, _idx),
                                $@"{ _kvp.Key} Level Spell", @"Spell automatically learnt at level",
                                SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                            {
                                CurrentValue = KnownSpells.LearnedSpell(level, _idx)
                            };
                            _idx++;
                        }
                }
                else
                {
                    // collect previous level's known spells
                    var _previous = new Dictionary<int, int>();
                    foreach (var _old in KnownSpellsPerLevel(level - 1))
                        _previous.Add(_old.Key, _old.Value);

                    // compare and yield differences
                    var _idx = 0;
                    foreach (var _current in KnownSpellsPerLevel(level))
                    {
                        // default to all known spells per spell level
                        var _diff = _current.Value;
                        if (_previous.Keys.Contains(_current.Key))
                        {
                            // if there were already some from the previous class level, get difference
                            _diff -= _previous[_current.Key];
                        }
                        for (var _lx = 0; _lx < _diff; _lx++)
                        {
                            // one requirement for each different known spell unit...
                            yield return new AdvancementRequirement(
                                new LearnedSpellRequirementKey(_idx.ToString(), level, _current.Key, _idx),
                                $@"{_current.Key} Level Spell", @"Spell automatically learnt at level",
                                SpellGainAtLevel, SetSpellGainAtLevel, CheckSpellGainAtLevel)
                            {
                                CurrentValue = KnownSpells.LearnedSpell(level, _idx)
                            };
                            _idx++;
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region private IEnumerable<IAdvancementOption> SpellGainAtLevel(IResolveRequirement target, RequirementKey key)
        private IEnumerable<IAdvancementOption> SpellGainAtLevel(IResolveRequirement target, RequirementKey key)
        {
            var _key = key as LearnedSpellRequirementKey;
            var _knownTypes = KnownSpells.KnownForSpellLevel(_key.SpellLevel).Select(_k => _k.SpellDef.GetType()).ToList();
            foreach (var _spell in from _cs in UsableSpells
                                   where (_cs.Level == _key.SpellLevel)
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
            var _key = key as LearnedSpellRequirementKey;
            var _classSpell = (advOption as AdvancementParameter<ClassSpell>).ParameterValue;

            // add
            KnownSpells.Add(new KnownSpell(_classSpell.SpellDef, _key.SpellLevel, _key.Level, _key.SpellIndex));
            return true;
        }
        #endregion

        #region private bool CheckSpellGainAtLevel(RequirementKey key)
        private bool CheckSpellGainAtLevel(RequirementKey key)
        {
            if (key is LearnedSpellRequirementKey _key)
            {
                return KnownSpells.LearnedSpell(_key.Level, _key.SpellIndex) != null;
            }
            return false;
        }
        #endregion

        #region protected IEnumerable<IFeature> BaseFeatures(int level)
        protected IEnumerable<IFeature> BaseFeatures(int level)
        {
            // each auto-known spell is a feature
            foreach (var _known in KnownSpells.AllKnown.Where(_k => _k.LearnedLevel == level))
                yield return _known;
            yield break;
        }
        #endregion

        #region public SpellSlot GetAvailableSlot(int level)
        public SpellSlot GetAvailableSlot(int level)
        {
            // climb spell slot levels across all sets looking for charged slots
            var _maxMax = _SpellSlots.Max(_set => _set.MaximumSlotLevel);
            for (var _lvl = level; _lvl < _maxMax; _lvl++)
            {
                var _rslt = (from _set in _SpellSlots
                             from _slot in _set.AvailableSlotsForSpellLevel(level)
                             where _slot != null
                             select _slot).FirstOrDefault();

                // return the first found
                if (_rslt != null)
                    return _rslt;
            }
            return null;
        }
        #endregion

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsPowerClassActive)
            {
                if (budget is LocalActionBudget _budget)
                {
                    // ready slots for spells by spell level
                    var _readySlots = new Dictionary<int, SpellSlot>();
                    SpellSlot _getSlot(int level)
                    {
                        if (!_readySlots.ContainsKey(level))
                            _readySlots.Add(level, GetAvailableSlot(level));
                        return _readySlots[level];
                    }

                    // each known spell that has a charged slot...
                    foreach (var _spell in KnownSpells.AllKnown)
                    {
                        // only create casting actions if we have time to perform
                        var _def = _spell.SpellDef;
                        var _actTime = _def.ActionTime;
                        if (_budget.HasTime(_actTime))
                        {
                            // find lowest slot-level that can cast this spell
                            var _slot = _getSlot(_spell.SlotLevel);
                            if (_slot != null)
                            {
                                // some spells have multiple modes, so include them all
                                foreach (var _spellMode in _def.SpellModes)
                                    yield return new CastSpell(
                                        new SpellSource(this, _spell.SlotLevel, _slot.SlotLevel, true, _def),
                                        _spellMode, _actTime, _slot, _def.DisplayName);
                            }
                        }
                    }
                }

                // TODO: metamagic casting?  or is that in the metamagic-feats...

                var _time = Creature?.GetCurrentTime() ?? 0d;
                if (!MustRestToRecharge)
                {
                    // rechargeable slots available...
                    if (AllSpellSlots.Any(_s => _s.CanRecharge(_time)))
                    {
                        yield return new RechargeSlots(Creature, this, new ActionTime(Minute.UnitFactor * 15, Contracts.TimeType.TimelineScheduling));
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToPowerClassInfo();
        #endregion
    }
}
