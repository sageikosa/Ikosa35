using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class OverrunBlock : PreReqListStepBase
    {
        public OverrunBlock(CoreActivity activity, Creature target)
            : base(activity)
        {
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, null, activity.Actor, @"Overrun.AttackerCheck", @"Strength to Overrun",
                    new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, null, target, @"Overrun.DefenderCheck", @"Resist Overrun",
                    new DieRoller(20), false));
        }

        public RollPrerequisite AttackerCheck
            => AllPrerequisites<RollPrerequisite>(@"Overrun.AttackerCheck").FirstOrDefault();

        public RollPrerequisite DefenderCheck
            => AllPrerequisites<RollPrerequisite>(@"Overrun.DefenderCheck").FirstOrDefault();

        protected override bool OnDoStep()
        {
            var _atkRoll = AttackerCheck;
            var _defRoll = DefenderCheck;
            if ((_atkRoll != null) && (_defRoll != null))
            {
                // consume regular budget due to blocking
                ((Process as CoreActivity)?.Action as MovementAction)?
                    .Budget?.ConsumeBudget(Contracts.TimeType.Regular);

                // then resolve the attempt
                Overrun.DoOverrun(this, _atkRoll, _defRoll, true);
            }
            return true;
        }
    }
}
