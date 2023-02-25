using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ArmorInfo : ProtectorInfo
    {
        #region construction
        public ArmorInfo()
            : base()
        {
        }

        protected ArmorInfo(ArmorInfo copySource)
            : base(copySource)
        {
            MaxDexterityBonus = copySource.MaxDexterityBonus.Clone() as DeltableInfo;
            ProficiencyType = copySource.ProficiencyType;
            BodyType = copySource.BodyType;
        }
        #endregion

        [DataMember]
        public DeltableInfo MaxDexterityBonus { get; set; }
        [DataMember]
        public ArmorProficiencyType ProficiencyType { get; set; }
        [DataMember]
        public string BodyType { get; set; }

        public override object Clone()
        {
            return new ArmorInfo(this);
        }
    }
}
