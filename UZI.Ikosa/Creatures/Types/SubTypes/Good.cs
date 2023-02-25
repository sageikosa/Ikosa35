using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class GoodSubType : CreatureAlignmentSubType
    {
        public GoodSubType(object source) : base(source) { }
        public override string Name { get { return @"Good"; } }

        public override CreatureSubType Clone(object source)
        {
            return new GoodSubType(source);
        }
    }
}
