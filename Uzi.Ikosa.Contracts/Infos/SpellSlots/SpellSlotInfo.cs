using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class SpellSlotInfo : Info
    {
        public SpellSlotInfo()
        {
        }

        protected SpellSlotInfo(SpellSlotInfo copyFrom)
        {
            Message = copyFrom.Message;
            SlotIndex = copyFrom.SlotIndex;
            SlotLevel = copyFrom.SlotLevel;
            CanRecharge = copyFrom.CanRecharge;
            IsCharged = copyFrom.IsCharged;
        }

        [DataMember]
        public int SlotIndex { get; set; }

        [DataMember]
        public int SlotLevel { get; set; }

        /// <summary>Slots cannot be recharged or abandonned if used in past 8 hours</summary>
        [DataMember]
        public bool CanRecharge { get; set; }

        [DataMember]
        public bool IsCharged { get; set; }

        public override object Clone()
            => new SpellSlotInfo(this);
    }
}