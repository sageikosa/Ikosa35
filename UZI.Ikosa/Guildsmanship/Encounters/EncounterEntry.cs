using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public abstract class EncounterEntry : ModuleElement
    {
        private readonly Mutabilities _Mutabilities;
        private int _LowVal;
        private int _HighVal;

        protected EncounterEntry(Description description, int lowVal, int highVal)
            : base(description)
        {
            _Mutabilities = new Mutabilities();
            _LowVal = lowVal;
            _HighVal = highVal;
        }

        public Mutabilities Mutabilities => _Mutabilities;
        public int LowValue { get => _LowVal; set => _LowVal = value; }
        public int HighValue { get => _HighVal; set => _HighVal = value; }

        public abstract IEnumerable<Creature> GenerateCreatures(params string[] teamNames);
    }
}
