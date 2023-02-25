using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class EvilSubType : CreatureAlignmentSubType
    {
        public EvilSubType(object source) : base(source) { }
        public override string Name { get { return @"Evil"; } }

        public override CreatureSubType Clone(object source)
        {
            return new EvilSubType(source);
        }
    }
}
