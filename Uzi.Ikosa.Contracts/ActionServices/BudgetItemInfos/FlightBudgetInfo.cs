using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class FlightBudgetInfo : BudgetItemInfo
    {
        [DataMember]
        public int UpwardsCrossings { get; set; }

        [DataMember]
        public double DistanceCovered { get; set; }

        [DataMember]
        public double DistanceSinceTurn { get; set; }
    }
}
