using System;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class Charisma : CastingAbilityBase
    {
        public Charisma(int seedValue)
            : base(seedValue, MnemonicCode.Cha)
        {
        }

        /// <summary>
        /// Non-Ability Constructor
        /// </summary>
        public Charisma()
            : base(MnemonicCode.Cha)
        {
        }
    }
}
