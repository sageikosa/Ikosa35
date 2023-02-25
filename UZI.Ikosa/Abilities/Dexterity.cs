using System;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class Dexterity : AbilityBase
    {
        public Dexterity(int seedValue)
            : base(seedValue, MnemonicCode.Dex)
        {
        }

        /// <summary>
        /// Non-Ability Constructor
        /// </summary>
        public Dexterity()
            : base(MnemonicCode.Dex)
        {
        }
    }
}
