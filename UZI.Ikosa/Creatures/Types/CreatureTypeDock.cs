using System;
using Uzi.Core;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class CreatureTypeDock : LinkableDock<CreatureType>, ICreatureBound
    {
        public CreatureTypeDock(Creature creature)
            : base("CreatureType")
        {
            Creature = creature;
        }

        public Creature Creature { get; private set; }
        public CreatureType CreatureType { get { return Link; } }
    }
}
