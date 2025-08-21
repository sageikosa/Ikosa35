using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.SpellLists;

namespace Uzi.Ikosa.Advancement
{
    [Serializable]
    public abstract class SlottedCasterClass<SlotClass> : CasterClass, ISlottedCasterClass<SlotClass>
        where SlotClass : SpellSlot, new()
    {
        #region construction
        protected SlottedCasterClass(byte powerDie)
            : this(powerDie, PowerDieCalcMethod.Average)
        {
        }

        protected SlottedCasterClass(byte powerDie, PowerDieCalcMethod calcMethod)
            : base(powerDie, calcMethod)
        {
            _SpellSlots =
            [
                new SpellSlotSet<SlotClass>(this)
            ];
        }
        #endregion

        #region private data
        protected List<SpellSlotSet<SlotClass>> _SpellSlots;
        #endregion

        /// <summary>Derived classes override to yield spells per day table</summary>
        protected abstract IEnumerable<(int SlotLevel, int SpellsPerDay)> BaseSpellsPerDayAtLevel(int setIndex, int level);

        #region private void FixupSpellsPerDay()
        private void FixupSpellsPerDay()
        {
            var _effLevel = EffectiveLevel.QualifiedValue(PowerLevelCheck.LevelCheck(Creature.AdvancementLog.NumberPowerDice));
            for (var _index = 0; _index < SlotSetCount; _index++)
            {
                var _set = GetSpellSlotSets(_index);
                foreach (var _pair in SpellsPerDayAtLevel(_index, _effLevel))
                {
                    if (!_set._Levels.ContainsKey(_pair.SlotLevel))
                    {
                        // must create the slot level
                        _set._Levels.Add(_pair.SlotLevel, new SpellSlotLevel<SlotClass>(_pair.SlotLevel));
                    }
                    var _levelSlots = _set._Levels[_pair.SlotLevel];
                    var _levelCount = _levelSlots.SlotCount;
                    if (_levelCount < _pair.SpellsPerDay)
                    {
                        // add new slots
                        while (_levelSlots.SlotCount < _pair.SpellsPerDay)
                        {
                            _levelSlots._Slots.Add(new SlotClass { SlotLevel = _pair.SlotLevel });
                        }
                    }
                    else if (_levelCount > _pair.SpellsPerDay)
                    {
                        // remove slots
                        while (_levelSlots.SlotCount > _pair.SpellsPerDay)
                        {
                            _levelSlots._Slots.RemoveAt(_levelSlots.SlotCount - 1);
                        }
                    }
                }

                // now...make sure we don't still have levels in the slots that are not available
                var _maxSlotLevel = MaximumSpellLevelAtLevel(_effLevel);
                var _maxCurrentSlot = _set.MaximumSlotLevel;
                while (_maxCurrentSlot > _maxSlotLevel)
                {
                    _set._Levels.Remove(_maxCurrentSlot);
                    _maxCurrentSlot--;
                }
            }
        }
        #endregion

        public abstract bool MustRestToRecharge { get; }

        public abstract IEnumerable<ClassSpell> SlottableSpells(int setIndex);

        public int SlotSetCount
            => _SpellSlots.Count;

        #region public virtual IEnumerable<(int slotLevel, int spellsPerDay)> SpellsPerDayAtLevel(int setIndex, int level)
        /// <summary>Spell slot counts for default set (including bonuses for high ability scores)</summary>
        public virtual IEnumerable<(int SlotLevel, int SpellsPerDay)> SpellsPerDayAtLevel(int setIndex, int level)
        {
            if (setIndex == 0)
            {
                foreach (var _pair in BaseSpellsPerDayAtLevel(setIndex, level))
                {
                    yield return (_pair.SlotLevel, _pair.SpellsPerDay + BonusSpellAbility.BonusSpellsForLevel(_pair.SlotLevel));
                }
            }
            yield break;
        }
        #endregion

        /// <summary>spells per day per level fro a given SpellSlot set</summary>
        public IEnumerable<(int SlotLevel, int SpellsPerDay)> SpellsPerDay(int setIndex)
            => SpellsPerDayAtLevel(setIndex, EffectiveLevel.QualifiedValue(null));

        /// <summary>Maximum spell level from base set</summary>
        public int MaximumSpellLevelAtLevel(int level)
            => SpellsPerDayAtLevel(0, level).Max(_pair => _pair.SlotLevel);

        public SpellSlotSet<SlotClass> GetSpellSlotSets(int setIndex)
            => ((setIndex < _SpellSlots.Count) && (setIndex > 0))
            ? _SpellSlots[setIndex]
            : _SpellSlots[0];

        public virtual string SpellSlotsName(int setIndex)
            => $@"{ClassName} Spells";

        /// <summary>item1==setIndex; item2==setName; item3=spellSlotSet</summary>
        public IEnumerable<(int SetIndex, string SetName, SpellSlotSet<SlotClass> SlotSet)> AllSpellSlotSets
            => Enumerable.Range(0, SlotSetCount)
            .Select(_sx => (_sx, SpellSlotsName(_sx), GetSpellSlotSets(_sx)));

        public IEnumerable<SlotClass> AllSpellSlots
            => (from _sx in Enumerable.Range(0, SlotSetCount)
                from _level in GetSpellSlotSets(_sx).AllLevels
                from _slot in _level.Slots.Select(_s => _s)
                select _slot);

        #region lock/unlock level
        protected override void OnLockOneLevel()
        {
            base.OnLockOneLevel();
            FixupSpellsPerDay();
        }

        protected override void OnUnlockOneLevel()
        {
            FixupSpellsPerDay();
            base.OnUnlockOneLevel();
        }
        #endregion
    }
}
