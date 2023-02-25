using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class AirSubType : CreatureElementalSubType
    {
        public AirSubType(object source) : base(source) { }
        public override string Name { get { return @"Air"; } }

        public override CreatureSubType Clone(object source)
        {
            return new AirSubType(source);
        }
    }
}
