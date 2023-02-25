using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ClassSpellInfo : Info
    {
        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public SpellDefInfo SpellDef { get; set; }

        public override object Clone()
            => new ClassSpellInfo
            {
                Level = Level,
                Message = Message,
                SpellDef = SpellDef.Clone() as SpellDefInfo
            };
    }
}
