using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AdvancementLogInfo
    {
        [DataMember]
        public ClassInfo Class { get; set; }
        [DataMember]
        public int ClassLevelLow { get; set; }
        [DataMember]
        public int ClassLevelHigh { get; set; }
        [DataMember]
        public bool IsLocked { get; set; }
        [DataMember]
        public bool IsLockable { get; set; }
        [DataMember]
        public List<PowerDieInfo> PowerDice { get; set; }
        [DataMember]
        public List<FeatureInfo> Features { get; set; }
        [DataMember]
        public List<AdvancementRequirementInfo> Requirements { get; set; }

        public bool IsMinPowerDie(PowerDieInfo powerDie)
            => (powerDie?.PowerLevel ?? int.MinValue) <= (PowerDice?.Min(_pd => _pd.PowerLevel) ?? int.MaxValue);

        public bool IsMaxPowerDie(PowerDieInfo powerDie)
            => (powerDie?.PowerLevel ?? int.MaxValue) >= (PowerDice?.Max(_pd => _pd.PowerLevel) ?? int.MinValue);
    }
}
