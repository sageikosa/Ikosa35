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
    /// <summary>Performs overrun checks and processes results (no counter allowed)</summary>
    [Serializable]
    public class OverrunChecks : PreReqListStepBase
    {
        /// <summary>Performs overrun checks and processes results (no counter allowed)</summary>
        public OverrunChecks(CoreStep predecessor, Creature counterer, Creature originalOffender) 
            : base(predecessor)
        {
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, null, counterer, @"Overrun.AttackerCheck", @"Strength to Overrun",
                    new DieRoller(20), false));
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(this, null, originalOffender, @"Overrun.DefenderCheck", @"Resist Overrun",
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
                Overrun.DoOverrun(this, _atkRoll, _defRoll, false);
            }
            return true;
        }
    }
}
