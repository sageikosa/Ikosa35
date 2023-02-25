using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class ColdSubType : CreatureElementalSubType
    {
        public ColdSubType(object source) : base(source) { }
        public override string Name { get { return @"Cold"; } }

        public override CreatureSubType Clone(object source)
        {
            return new ColdSubType(source);
        }
    }
}
