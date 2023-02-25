using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class CoreActionBudgetInfo
    {
        public CoreActionBudgetInfo()
        {
            ActivityStack = new ActivityInfo[] { };
            BudgetItems = new BudgetItemInfo[] { };
        }

        [DataMember]
        public Guid ActorID { get; set; }
        [DataMember]
        public ActivityInfo[] ActivityStack { get; set; }
        [DataMember]
        public BudgetItemInfo[] BudgetItems { get; set; }
    }
}
