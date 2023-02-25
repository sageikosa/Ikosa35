using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>Performs trip checks and processes results (no counter allowed)</summary>
    [Serializable]
    public class TripChecks : PreReqListStepBase
    {
        /// <summary>Performs trip checks and processes results (no counter allowed)</summary>
        public TripChecks(CoreStep predecessor, Creature tripper, Creature target)
            : base(predecessor)
        {
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, null, tripper, @"TripChecks.AttackerCheck", @"Strength to Trip",
                    new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, null, target, @"TripChecks.DefenderCheck", @"Resist Trip",
                    new DieRoller(20), false));
        }

        public RollPrerequisite AttackerCheck
            => AllPrerequisites<RollPrerequisite>(@"TripChecks.AttackerCheck").FirstOrDefault();

        public RollPrerequisite DefenderCheck
            => AllPrerequisites<RollPrerequisite>(@"TripChecks.DefenderCheck").FirstOrDefault();

        protected override bool OnDoStep()
        {
            // perform counter-trip
            var _atkRoll = AttackerCheck;
            var _defRoll = DefenderCheck;
            if ((_atkRoll != null) && (_defRoll != null))
            {
                Trip.DoTrip(this, _atkRoll, _defRoll, false, null);
            }
            return true;
        }
    }
}
