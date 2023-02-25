using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PrepareSpellSlotsTargeting : AimTargeting<PrepareSpellSlotsAimInfo, PrepareSpellSlotsTargetInfo>
    {
        public PrepareSpellSlotsTargeting(ActivityInfoBuilder builder, PrepareSpellSlotsAimInfo aimingMode)
            : base(builder, aimingMode)
        {
            _PrepareSpellSlotsAim = aimingMode;
            _SlotSets = aimingMode.SlotSets.Select(_ss => new SpellSlotSetTargeting(this, _ss)).ToList();
            _Selectors = new List<ViewModelBase>();

            // flattened list of view models
            foreach (var _set in _SlotSets)
            {
                _Selectors.Add(_set);
                foreach (var _level in _set.SlotLevels)
                {
                    _Selectors.Add(_level);
                    foreach (var _slot in _level.SpellSlots)
                    {
                        _Selectors.Add(_slot);
                    }
                }
            }

            Targets.Add(new PrepareSpellSlotsTargetInfo
            {
                Key = aimingMode.Key,
                TargetID = null,
                SlotSets = new List<SpellSlotSetInfo>(aimingMode.SlotSets.Select(_ss => _ss.Clone()).Cast<SpellSlotSetInfo>())
            });
        }

        #region data
        private readonly PrepareSpellSlotsAimInfo _PrepareSpellSlotsAim;
        private readonly List<SpellSlotSetTargeting> _SlotSets;
        private readonly List<ViewModelBase> _Selectors;
        #endregion

        public PrepareSpellSlotsAimInfo PrepareSpellSlotsAimInfo => _PrepareSpellSlotsAim;
        public List<SpellSlotSetTargeting> SpellSlotSets => _SlotSets;

        public List<ViewModelBase> PrepareSelectors => _Selectors;

        public override bool IsReady => true;

        protected override void SyncAimMode(PrepareSpellSlotsAimInfo aimMode)
        {
            // TODO: what could cause a change?
        }

        protected override void SetAimTargets(List<PrepareSpellSlotsTargetInfo> targets)
        {
            // NOP
        }
    }
}
