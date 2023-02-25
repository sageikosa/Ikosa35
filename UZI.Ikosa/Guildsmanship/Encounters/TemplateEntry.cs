using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class TemplateEntry : EncounterEntry
    {
        private Guid _TemplateID;
        private Roller _Roller;
        private int? _Number;

        public TemplateEntry(Guid creatureID, Description description, int lowVal, int highVal, Roller roller) 
            : base(description, lowVal, highVal)
        {
            _TemplateID = creatureID;
            _Roller = roller;
        }

        //public CreatureElement Template => Portfolio.GetElement<CreatureElement>(_TemplateID);
        public Roller Roller { get => _Roller; set => _Roller = value; }
        public int? Number { get => _Number; set => _Number = value; }

        public override IEnumerable<Creature> GenerateCreatures(params string[] teamNames)
        {
            _Number ??= Roller.RollValue(null, $@"Encounter: {Description.Message}", @"Number of creatures");
            for (var _nx = 0; _nx < _Number; _nx++)
            {
                // NOTE: no attempt to avoid name collision if template entry used more than once
                //yield return Template.Creature.TemplateClone($@"{Template.Creature.Name} ({Description.Message} [{_nx+1}])");
            }
            yield break;
        }
    }
}
