using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SkillBuyInfo
    {
        [DataMember]
        public SkillInfo Skill { get; set; }
        [DataMember]
        public bool IsClassSkill { get; set; }  // at linked PowerDie level
        [DataMember]
        public int PointsUsed { get; set; }

        public double RanksAccumulated
            => (IsClassSkill)
            ? Convert.ToDouble(PointsUsed)
            : Convert.ToDouble(PointsUsed) / 2d;
    }
}
