using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class TickInfo
    {
        [DataMember]
        public double Time { get; set; }

        [DataMember]
        public int? InitiativeScore { get; set; }

        [DataMember]
        public List<LocalActionBudgetInfo> Budgets { get; set; }

        [DataMember]
        public bool IsRoundMarker { get; set; }

        public LocalActionBudgetInfo FirstBudget => Budgets?.FirstOrDefault();
    }
}
