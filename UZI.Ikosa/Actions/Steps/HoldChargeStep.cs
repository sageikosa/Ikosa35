using Uzi.Core.Contracts;
using System;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>
    /// Groups up a held charge for a melee range touch spell
    /// </summary>
    [Serializable]
    public class HoldChargeStep : CoreStep
    {
        public HoldChargeStep(CoreStep original)
            : base(original)
        {
            _OriginalStep = original;
        }

        private CoreStep _OriginalStep;

        #region public void DoStep()
        /// <summary>
        /// Creates the held charge, and attempts to place it in a holding slot
        /// </summary>
        /// <returns></returns>
        protected override bool OnDoStep()
        {
            if (Process is CoreActivity _activity)
            {
                if (_activity.Actor is Creature _critter)
                {
                    // slot the held charge into an empty hand...
                    var _slot = _critter.Body.ItemSlots[ItemSlot.HoldingSlot, true];
                    if (_slot != null)
                    {
                        var _heldCharge = new HeldCharge(@"Held Charge", ItemSlot.HoldingSlot, _OriginalStep.Process as CoreActivity)
                        {
                            Possessor = _critter
                        };
                        _heldCharge.SetItemSlot(_slot);
                        AppendFollowing(_activity.GetActivityResultNotifyStep(@"Holding the charge."));
                    }
                    else
                    {
                        AppendFollowing(_activity.GetActivityResultNotifyStep(@"No empty holding slot."));
                    }
                }
            }

            return true;
        }
        #endregion

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }
    }
}
