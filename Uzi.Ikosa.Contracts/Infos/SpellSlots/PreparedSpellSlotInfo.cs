using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class PreparedSpellSlotInfo : SpellSlotInfo
    {
        public PreparedSpellSlotInfo() 
            : base()
        {
        }

        protected PreparedSpellSlotInfo(PreparedSpellSlotInfo copyFrom)
            :base(copyFrom)
        {
            SpellSource = copyFrom.SpellSource?.Clone() as SpellSourceInfo;
        }

        [DataMember]
        public SpellSourceInfo SpellSource { get; set; }

        public override object Clone()
            => new PreparedSpellSlotInfo(this);
    }
}
