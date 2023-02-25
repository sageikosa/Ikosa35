using System;

namespace Uzi.Core
{
    /// <summary>
    /// Typically used as a reactive step to hold a process after start
    /// </summary>
    [Serializable]
    public class HoldProcessStep : CoreStep
    {
        public HoldProcessStep(CoreProcess process)
            : base(process)
        {
        }

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }
        protected override bool OnDoStep()
        {
            Process.IsHeld = true;
            return true;
        }
    }
}
