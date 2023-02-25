using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AbilitySetInfo
    {
        [DataMember]
        public AbilityInfo Strength { get; set; }
        [DataMember]
        public AbilityInfo Dexterity { get; set; }
        [DataMember]
        public AbilityInfo Constitution { get; set; }
        [DataMember]
        public AbilityInfo Intelligence { get; set; }
        [DataMember]
        public AbilityInfo Wisdom { get; set; }
        [DataMember]
        public AbilityInfo Charisma { get; set; }
        [DataMember]
        public Take10Info Take10 { get; set; }

        public List<AbilityInfo> AllAbilities()
            => new List<AbilityInfo>
            {
                Strength,
                Dexterity,
                Constitution,
                Intelligence,
                Wisdom,
                Charisma
            };
    }
}
