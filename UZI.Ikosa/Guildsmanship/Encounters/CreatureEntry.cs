using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class CreatureEntry : EncounterEntry
    {
        private Guid _CreatureID;

        public CreatureEntry(Guid creatureID, Description description, int lowVal, int highVal)
            : base(description, lowVal, highVal)
        {
            _CreatureID = creatureID;
        }

        //public CreatureElement CreatureElement => Portfolio.GetElement<CreatureElement>(_CreatureID);

        public override IEnumerable<Creature> GenerateCreatures(params string[] teamNames)
        {
            //yield return CreatureElement.Creature;
            yield break;
        }
    }
}