using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class EarthSubType : CreatureElementalSubType
    {
        public EarthSubType(object source) : base(source) { }
        public override string Name { get { return @"Earth"; } }

        public override CreatureSubType Clone(object source)
        {
            return new EarthSubType(source);
        }
    }
}
