using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class SpellSlotLevelTargeting : ViewModelBase
    {
        #region ctor()
        public SpellSlotLevelTargeting(SpellSlotSetTargeting spellSlotSet, SpellSlotLevelInfo spellSlotLevel)
        {
            _SlotSet = spellSlotSet;
            _Level = spellSlotLevel;
            _SpellSlots = spellSlotLevel.SpellSlots.OfType<PreparedSpellSlotInfo>().Select(_prep => new PreparedSpellSlotTargeting(this, _prep)).ToList();
        }

        static SpellSlotLevelTargeting()
        {
            Unslotted = new ClassSpellInfo
            {
                Level = 0,
                Message = @"-- Empty --",
                SpellDef = new SpellDefInfo
                {
                    ArcaneCharisma = false,
                    Description = @"No spell selected",
                    Descriptors = new string[] { },
                    Key = null,
                    MagicStyle = string.Empty,
                    Message = @"-- Empty --",
                    MetaMagics = new List<MetaMagicInfo>()
                }
            };
        }
        #endregion

        #region data
        private readonly SpellSlotSetTargeting _SlotSet;
        private readonly SpellSlotLevelInfo _Level;
        private readonly List<PreparedSpellSlotTargeting> _SpellSlots;
        #endregion

        public static readonly ClassSpellInfo Unslotted;

        public SpellSlotSetTargeting SpellSlotSet => _SlotSet;
        public SpellSlotLevelInfo SpellSlotLevelInfo => _Level;
        public List<PreparedSpellSlotTargeting> SpellSlots => _SpellSlots;

        #region public IEnumerable<ClassSpellInfo> AvailableClassSpells { get; }
        public IEnumerable<ClassSpellInfo> AvailableClassSpells
        {
            get
            {
                var _maxLevel = SpellSlotLevelInfo.SlotLevel;
                yield return Unslotted;
                foreach (var _spell in SpellSlotSet.SpellSlotSetInfo.AvailableSpells
                    .Where(_avail => _avail.Level <= _maxLevel)
                    .OrderByDescending(_avail => _avail.Level)
                    .ThenBy(_avail => _avail.SpellDef.Message))
                {
                    yield return _spell;
                }
                yield break;
            }
        }
        #endregion

        public IEnumerable<MetaMagicInfo> AvailableMetaMagics
            => from _am in SpellSlotSet.PrepareSpellSlots.PrepareSpellSlotsAimInfo.AvailableMetaMagics
               where ((_am.SlotAdjustment + SpellSlotLevelInfo.SlotLevel) <= SpellSlotLevelInfo.SlotLevel)
               orderby _am.MetaTag
               select _am;
    }
}
