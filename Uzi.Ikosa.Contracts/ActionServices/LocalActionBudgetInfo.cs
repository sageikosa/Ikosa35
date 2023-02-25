using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LocalActionBudgetInfo : CoreActionBudgetInfo
    {
        #region construction
        public LocalActionBudgetInfo()
            : base()
        {
        }
        #endregion

        [DataMember]
        public bool IsUsingTurn { get; set; }
        [DataMember]
        public bool IsTwitchAvailable { get; set; }
        [DataMember]
        public bool CanPerformBrief { get; set; }
        [DataMember]
        public bool CanPerformRegular { get; set; }
        [DataMember]
        public bool CanPerformTotal { get; set; }
        [DataMember]
        public bool CanPerformAttack { get; set; }
        [DataMember]
        public ActivityInfo HeldActivity { get; set; }
        [DataMember]
        public ActivityInfo NextActivity { get; set; }
        [DataMember]
        public double? HoldTimeRemaining { get; set; }
        [DataMember]
        public double CurrentTime { get; set; }
        [DataMember]
        public bool IsInitiative { get; set; }
        [DataMember]
        public bool IsFocusedBudget { get; set; }

        public MovementRangeBudgetInfo MovementRangeBudget
            => BudgetItems.OfType<MovementRangeBudgetInfo>().FirstOrDefault();

        public MovementBudgetInfo MovementBudget
            => BudgetItems.OfType<MovementBudgetInfo>().FirstOrDefault();

        public CreatureLoginInfo CreatureLoginInfo { get; set; }
    }
}
