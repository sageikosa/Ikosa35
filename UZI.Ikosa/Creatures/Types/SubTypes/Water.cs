using System;

namespace Uzi.Ikosa.Creatures.SubTypes
{
    [Serializable]
    public class WaterSubType : CreatureElementalSubType
    {
        public WaterSubType(object source) : base(source) { }
        public override string Name { get { return @"Water"; } }

        public override CreatureSubType Clone(object source)
        {
            return new WaterSubType(source);
        }
    }
}
