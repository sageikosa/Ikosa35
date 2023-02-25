using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PrepareSpellSlotsTarget : AimTarget
    {
        public PrepareSpellSlotsTarget(string key, IInteract target) 
            : base(key, target)
        {
            _Sets = new List<SpellSlotSetInfo>();
        }

        private List<SpellSlotSetInfo> _Sets;

        public List<SpellSlotSetInfo> SlotSets => _Sets;

        public override AimTargetInfo GetTargetInfo()
        {
            return base.GetTargetInfo();
        }
    }
}
