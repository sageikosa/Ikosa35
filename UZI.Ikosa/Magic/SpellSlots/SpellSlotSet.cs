using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class SpellSlotSet<SlotClass> : IActionProvider, ISourcedObject
        where SlotClass : SpellSlot, new()
    {
        public SpellSlotSet(object source)
        {
            _Source = source;
            _Levels = [];
        }

        #region data
        private object _Source;
        internal Dictionary<int, SpellSlotLevel<SlotClass>> _Levels;
        private Guid _ID = Guid.NewGuid();
        #endregion

        // ISourcedObject
        public object Source => _Source;

        public Guid ID => _ID;
        public Guid PresenterID => _ID;

        /// <summary>Maximum slot level currently in the set</summary>
        public int MaximumSlotLevel
            => _Levels.Any() ? _Levels.Max(_kvp => _kvp.Key) : -1;

        /// <summary>Minimum slot level currently in the set</summary>
        public int MinimumSlotLevel
            => _Levels.Any() ? _Levels.Min(_kvp => _kvp.Key) : -1;

        /// <summary>All slots (by slot level enumeration)</summary>
        public IEnumerable<SpellSlotLevel<SlotClass>> AllLevels
            => _Levels.Select(_kvp => _kvp.Value);

        public SpellSlotLevel<SlotClass> this[int level]
            => _Levels.ContainsKey(level) ? _Levels[level] : null;

        /// <summary>All slots of a particular level</summary>
        public IEnumerable<SlotClass> SlotsForSpellLevel(int level)
            => _Levels.ContainsKey(level) ? _Levels[level].Slots.Select(_s => _s) : new SlotClass[] { };

        /// <summary>All charged slots</summary>
        public IEnumerable<SlotClass> AvailableSlotsForSpellLevel(int level)
            => SlotsForSpellLevel(level).Where(_s => _s.IsCharged);

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            // TODO: budget...

            // TODO: meditate to recover spells
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            // TODO: different info? (caster class?)
            return new Info { Message = @"Spell Slot Set" };
        }
        #endregion

        public SpellSlotSetInfo ToSpellSlotSetInfo(ISlottedCasterBaseClass casterClass, int setIndex, string slotSetName, double currentTime)
            => new SpellSlotSetInfo
            {
                ID = ID,
                SetIndex = setIndex,
                SetName = slotSetName,
                AvailableSpells = (from _spell in casterClass.SlottableSpells(setIndex)
                                   select _spell.ToClassSpellInfo()).ToList(),
                SlotLevels = (from _level in AllLevels
                              select new SpellSlotLevelInfo
                              {
                                  SlotLevel = _level.Level,
                                  Message = string.Empty,
                                  SpellSlots = (from _slot in _level.Slots
                                                select _slot.ToSpellSlotInfo(_level.IndexOf(_slot), currentTime)).ToList()
                              }).ToList()
            };
    }
}
