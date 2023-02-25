using System;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class PlantType : CreatureType
    {
        public PlantType()
        {
        }

        public override string Name => @"Plant";
    }
}