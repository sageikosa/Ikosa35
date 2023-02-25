using System;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SpellSourceInfo : MagicPowerSourceInfo
    {
        public SpellSourceInfo()
            : base()
        {
        }

        protected SpellSourceInfo(SpellSourceInfo copyFrom)
            : base(copyFrom)
        {
            SlotLevel = copyFrom.SlotLevel;
            IsSpontaneous = copyFrom.IsSpontaneous;
        }

        /// <summary>Slot-level used</summary>
        [DataMember]
        public int? SlotLevel { get; set; }

        [DataMember]
        public bool IsSpontaneous { get; set; }

        public SpellDefInfo SpellDef
        {
            get => MagicPowerDef as SpellDefInfo;
            set => MagicPowerDef = value;
        }

        public CasterClassInfo CasterClass
        {
            get => PowerClass as CasterClassInfo;
            set => PowerClass = value;
        }

        public override object Clone()
            => new SpellSourceInfo(this);
    }
}
