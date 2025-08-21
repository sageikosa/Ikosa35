using System;
using Uzi.Ikosa.Deltas;
using System.Linq;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class Constitution : AbilityBase
    {
        public Constitution(int seedValue)
            : base(seedValue, MnemonicCode.Con)
        {
        }

        /// <summary>
        /// Non-Ability Constructor
        /// </summary>
        public Constitution()
            : base(MnemonicCode.Con)
        {
        }

        /// <summary>
        /// Ignores enhancement bonus for calculating "real" modifier
        /// </summary>
        public int AdvancementHealthPointModifier(int powerLevel)
        {
            if (IsNonAbility)
            {
                return 0;
            }

            int _eff = this.ValueAtPowerLevel(powerLevel, null);
            _eff -= this.Deltas.Where(_del => ((_del.Source as Type) == typeof(Enhancement)) && _del.Enabled).Sum(_del => _del.Value);
            return (int)Math.Floor(((decimal)_eff - 10.0M) / 2.0M);
        }
    }
}
