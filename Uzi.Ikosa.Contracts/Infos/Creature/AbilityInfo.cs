using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AbilityInfo : DeltableInfo
    {
        #region construction
        public AbilityInfo()
        {
        }
        #endregion

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Mnemonic { get; set; }
        [DataMember]
        public string DisplayValue { get; set; }
        [DataMember]
        public int DeltaValue { get; set; }
        [DataMember]
        public int Damage { get; set; }
        [DataMember]
        public bool IsNonAbility { get; set; }
        [DataMember]
        public Collection<int> Boosts { get; set; }
        [DataMember]
        public Take10Info Take10 { get; set; }
    }
}
