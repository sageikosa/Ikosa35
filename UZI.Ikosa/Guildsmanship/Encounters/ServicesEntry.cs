using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class ServicesEntry : EncounterEntry
    {
        // TODO: portfolio element references...

        private List<NonPlayerService> _Services;

        public ServicesEntry(Description description, int lowVal, int highVal)
            : base(description, lowVal, highVal)
        {
        }


        // --- what the game master can offer
        public List<NonPlayerService> NonPlayerServices => _Services;

        public override IEnumerable<Creature> GenerateCreatures(params string[] teamNames)
        {
            yield break;
        }
    }
}
