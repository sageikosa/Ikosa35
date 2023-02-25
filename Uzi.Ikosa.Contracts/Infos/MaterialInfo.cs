using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class MaterialInfo : ICloneable
    {
        public MaterialInfo()
        {
        }

        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int StructurePerInch { get; set; }
        [DataMember]
        public int Hardness { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            return new MaterialInfo
            {
                Name = Name,
                StructurePerInch = StructurePerInch,
                Hardness = Hardness
            };
        }

        #endregion
    }
}
