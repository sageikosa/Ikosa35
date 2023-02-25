using System;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class AbilityDamageValue
    {
        public string Mnemonic { get; set; }
        public int Value { get; set; }
        public object Source { get; set; }
    }
}
