using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PowerDieInfo
    {
        [DataMember]
        public int PowerLevel { get; set; }
        [DataMember]
        public decimal AltFraction { get; set; }
        [DataMember]
        public bool IsFractional { get; set; }
        [DataMember]
        public bool IsSmallestFractional { get; set; }
        [DataMember]
        public bool IsLocked { get; set; }
        [DataMember]
        public bool IsLockable { get; set; }
        [DataMember]
        public int HealthPoints { get; set; }
        [DataMember]
        public int EffectiveHealthPoints { get; set; }
        [DataMember]
        public int SkillPointsLeft { get; set; }
        [DataMember]
        public List<SkillBuyInfo> SkillsAssigned { get; set; }
        [DataMember]
        public bool IsFeatPowerDie { get; set; }
        [DataMember]
        public FeatInfo Feat { get; set; }
        [DataMember]
        public bool IsAbilityBoostPowerDie { get; set; }
        [DataMember]
        public AbilityInfo BoostedAbility { get; set; }

        // ---------- non-serialized ----------
        public bool IsFeatMissing
            => IsFeatPowerDie && (Feat == null);

        public bool IsAbilityBoostMissing
            => IsAbilityBoostPowerDie && (BoostedAbility == null);

        public bool IsHealthPointCountLow
            => HealthPoints < 1;
    }
}
