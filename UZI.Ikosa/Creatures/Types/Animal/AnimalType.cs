using System;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class AnimalType : CreatureType
    {
        public override string Name { get { return @"Animal"; } }
        public override bool IsLiving { get { return true; } }
    }
}
