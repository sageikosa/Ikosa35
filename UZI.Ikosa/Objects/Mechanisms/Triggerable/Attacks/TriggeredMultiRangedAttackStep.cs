using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class TriggeredMultiRangedAttackStep : PreReqListStepBase
    {
        #region ctor()
        public TriggeredMultiRangedAttackStep(RangedTriggerable triggerable)
            : base((CoreProcess)null)
        {
            _Triggerable = triggerable;
            switch (triggerable.AttackCount)
            {
                case RangedTriggerableAttackCount.D2:
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack.Count", $@"{triggerable.Name} Trap Attack Count", new DieRoller(2), false));
                    break;
                case RangedTriggerableAttackCount.D3:
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack.Count", $@"{triggerable.Name} Trap Attack Count", new DieRoller(3), false));
                    break;
                case RangedTriggerableAttackCount.D4:
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack.Count", $@"{triggerable.Name} Trap Attack Count", new DieRoller(4), false));
                    break;
                case RangedTriggerableAttackCount.D6:
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack.Count", $@"{triggerable.Name} Trap Attack Count", new DieRoller(6), false));
                    break;
                case RangedTriggerableAttackCount.D8:
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack.Count", $@"{triggerable.Name} Trap Attack Count", new DieRoller(8), false));
                    break;

                case RangedTriggerableAttackCount.One:
                default:
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Trap.Attack.Count", $@"{triggerable.Name} Trap Attack Count", new ConstantRoller(1), false));
                    break;
            }
        }
        #endregion

        #region data
        private RangedTriggerable _Triggerable;
        #endregion

        public CoreTargetingProcess TargetingProcess
            => Process as CoreTargetingProcess;

        public RollPrerequisite AttackCount
            => AllPrerequisites<RollPrerequisite>(@"Trap.Attack.Count").FirstOrDefault();

        public RangedTriggerable RangedTriggerable => _Triggerable;

        protected override bool OnDoStep()
        {
            if ((TargetingProcess is CoreTargetingProcess _proc)
               && (RangedTriggerable.Setting is LocalMap _map))
            {
                var _atkCount = AttackCount.RollValue;
                for (var _ax = 0; _ax < _atkCount; _ax++)
                {
                    // enqueue multiple attacks
                    AppendFollowing(new TriggeredRangedAttackStep(RangedTriggerable, _ax + 1, _atkCount));
                }
            }
            return true;
        }
    }
}
