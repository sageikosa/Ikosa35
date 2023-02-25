using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public class DamageReductionSet: WatchableSet<IDamageReduction>, ICreatureBound
    {
        public DamageReductionSet(Creature creature)
        {
            Creature = creature;
        }

        public Creature Creature { get; private set; }
    }
}
