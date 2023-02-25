using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class SpeciesEntry : EncounterEntry
    {
        private Species _Species;
        private int? _PowerDice;
        private Roller _Roller;
        private int? _Number;

        public SpeciesEntry(Description description, int lowVal, int highVal, Roller roller, Species species, int? powerDice)
            : base(description, lowVal, highVal)
        {
            _Roller = roller;
            _Species = species;
            _PowerDice = powerDice;
        }

        public Roller Roller { get => _Roller; set => _Roller = value; }
        public Species Species { get => _Species; set => _Species = value; }
        public int? PowerDice { get => _PowerDice; set => _PowerDice = value; }
        public int? Number { get => _Number; set => _Number = value; }

        public override IEnumerable<Creature> GenerateCreatures(params string[] teamNames)
        {
            _Number ??= Roller.RollValue(null, $@"Encounter: {Description.Message}", @"Number of creatures");
            for (var _nx = 0; _nx < _Number; _nx++)
            {
                // TODO: generate creature
                // TODO: bind to team(s)
                // TODO: yield
            }
            yield break;
        }
    }
}
