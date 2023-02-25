using System;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Actions
{
    public static class AttackTargetInfoHelper
    {
        /// <summary>Used to indicate the destination cell being attacked</summary>
        public static ICellLocation GetTargetCell(this AttackTargetInfo self, ISensorHost sensors, IInteract target)
        {
            if (self.TargetZ.HasValue && self.TargetY.HasValue && self.TargetX.HasValue)
                return new CellPosition(self.TargetZ.Value, self.TargetY.Value, self.TargetX.Value);
            return sensors.GetTargetCell(target: target);
        }

        public static Deltable GetAttackScoreDeltable(this AttackTargetInfo self, Guid notify)
        {
            // auto-roll if no attackScore provided
            return new Deltable(Math.Min(self.AttackScore ?? DieRoller.RollDie(notify, 20, self?.Key, @"Attack", notify), 20));
        }

        public static Deltable GetCriticalConfirmDeltable(this AttackTargetInfo self, Guid notify)
        {
            return new Deltable(Math.Min(self.CriticalConfirm ?? DieRoller.RollDie(notify, 20, self?.Key, @"Critical", notify), 20));
        }
    }
}
