using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class EnergyDamageData : DamageData
    {
        public EnergyDamageData(int amount, EnergyType energy, string extra, int minGroup)
            : base(amount, false, extra, minGroup)
        {
            Energy = energy;
        }

        public EnergyType Energy { get; set; }

        public override DamageInfo ToDamageInfo()
        {
            var _energy = ToBaseDamageInfo<EnergyDamageInfo>();
            _energy.Energy = Energy;
            return _energy;
        }

        public override object Clone()
            => new EnergyDamageData(Amount, Energy, Extra, MinGroup);
    }
}
