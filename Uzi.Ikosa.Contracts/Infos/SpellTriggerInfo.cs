using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SpellTriggerInfo : Info
    {
        public SpellTriggerInfo()
        {
        }

        protected SpellTriggerInfo(SpellTriggerInfo copySource)
        {
            Message = copySource.Message;
            SpellSource = copySource.SpellSource.Clone() as SpellSourceInfo;
            ChargesPerUse = copySource.ChargesPerUse;
            ChargesRemaining = copySource.ChargesRemaining;
        }

        [DataMember]
        public SpellSourceInfo SpellSource { get; set; }
        [DataMember]
        public int ChargesPerUse { get; set; }
        [DataMember]
        public int ChargesRemaining { get; set; }

        public override object Clone()
            => new SpellTriggerInfo(this);
    }
}
