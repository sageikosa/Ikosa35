using System;
using Uzi.Core;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public class SpeciesDock : LinkableDock<Species>, ICreatureBound
    {
        public SpeciesDock(Creature creature)
            : base("Species")
        {
            Creature = creature;
        }

        public Creature Creature { get; private set; }
        public Species Species { get { return this.Link; } }
    }
}
