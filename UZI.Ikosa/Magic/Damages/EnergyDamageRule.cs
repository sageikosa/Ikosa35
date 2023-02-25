using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class EnergyDamageRule : DamageRule
    {
        public EnergyDamageRule(string key, Range range, string name, params EnergyType [] energyType)
            : base(key, range, false, name)
        {
            _Type = energyType;
        }

        private EnergyType [] _Type;
        public EnergyType [] EnergyType { get { return _Type; } }
    }
}
