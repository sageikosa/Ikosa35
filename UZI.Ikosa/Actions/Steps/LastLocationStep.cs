using System;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>
    /// Inject into step sequence to undo the movement and put a locator in its last legal position
    /// </summary>
    [Serializable]
    public class LastLocationStep : CoreStep
    {
        public LastLocationStep(CoreStep predecessor, Locator locator)
            : base(predecessor)
        {
            _Locator = locator;
        }

        private Locator _Locator;
        public Locator Locator { get { return _Locator; } }

        protected override bool OnDoStep()
        {
            // TODO: implement...

            // done
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() { return null; }
        public override bool IsDispensingPrerequisites { get { return false; } }
    }
}
