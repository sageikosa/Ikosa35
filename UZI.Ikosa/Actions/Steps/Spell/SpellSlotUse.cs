using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SpellSlotUse : CoreStep
    {
        public SpellSlotUse(CoreActivity activity, SpellSlot slot, double currentTime)
            : base(activity)
        {
            _Slot = slot;
            _Time = currentTime;
        }

        #region private data
        private SpellSlot _Slot;
        private double _Time;
        #endregion

        public SpellSlot SpellSlot => _Slot;

        public override string Name => @"Use Spell Slot";

        public CoreActivity Activity
            => Process as CoreActivity;

        public override bool IsDispensingPrerequisites
            => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            SpellSlot?.UseSlot(_Time);
            return true;
        }
    }
}
