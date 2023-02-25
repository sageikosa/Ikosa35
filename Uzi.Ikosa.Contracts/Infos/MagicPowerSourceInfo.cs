using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class MagicPowerSourceInfo : CoreInfo
    {
        public MagicPowerSourceInfo()
        {
        }

        protected MagicPowerSourceInfo(MagicPowerSourceInfo copyFrom)
        {
            Message = copyFrom.Message;
            ID = copyFrom.ID;
            PowerLevel = copyFrom.PowerLevel;
            CasterLevel = copyFrom.CasterLevel;
            AuraStrength = copyFrom.AuraStrength;
            PowerClass = copyFrom.PowerClass.Clone() as PowerClassInfo;
            MagicPowerDef = copyFrom.MagicPowerDef.Clone() as MagicPowerDefInfo;
        }

        [DataMember]
        public string Description { get; set; }

        /// <summary>Power-level (slot-level needed for known spell)</summary>
        [DataMember]
        public int PowerLevel { get; set; }

        [DataMember]
        public int CasterLevel { get; set; }
        [DataMember]
        public AuraStrength AuraStrength { get; set; }
        [DataMember]
        public PowerClassInfo PowerClass { get; set; }
        [DataMember]
        public MagicPowerDefInfo MagicPowerDef { get; set; }

        public override object Clone()
            => new MagicPowerSourceInfo(this);
    }
}