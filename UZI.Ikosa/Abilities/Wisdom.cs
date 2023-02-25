using System;

// NOTE: in future, consider Wisdom as an adjunct to control awareness

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class Wisdom : CastingAbilityBase
    {
        public Wisdom(int seedValue)
            : base(seedValue, MnemonicCode.Wis)
        {
        }

        /// <summary>
        /// Non-Ability Constructor
        /// </summary>
        public Wisdom()
            : base(MnemonicCode.Wis)
        {
        }
    }
}
