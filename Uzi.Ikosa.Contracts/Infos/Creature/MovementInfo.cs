using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class MovementInfo : Info
    {
        public MovementInfo()
        {
        }

        public MovementInfo(MovementInfo copySource)
        {
            Message = copySource.Message;
            ID = copySource.ID;
            CanShiftPosition = copySource.CanShiftPosition;
            Description = copySource.Description;
            Value = copySource.Value;
            IsUsable = copySource.IsUsable;
        }

        [DataMember]
        public bool IsUsable { get; set; }
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public bool CanShiftPosition { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Value { get; set; }

        public override object Clone()
        {
            return new MovementInfo(this);
        }
    }
}
