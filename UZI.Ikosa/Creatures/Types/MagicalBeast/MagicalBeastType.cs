using System;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class MagicalBeastType : CreatureType
    {
        public override string Name { get { return @"Magical Beast"; } }
        public override bool IsLiving { get { return true; } }
    }
}
