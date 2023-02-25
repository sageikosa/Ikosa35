using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Uzi.Core.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class DiceRangeInfo : RangeInfo
    {
        [DataMember]
        public string RollerString { get; set; }
        [DataMember]
        public string OffsetRoll { get; set; }
        [DataMember]
        public string PerStep { get; set; }
        [DataMember]
        public int StepSize { get; set; }
        [DataMember]
        public int GroundStep { get; set; }
        [DataMember]
        public int MaxStepDice { get; set; }
    }
}
