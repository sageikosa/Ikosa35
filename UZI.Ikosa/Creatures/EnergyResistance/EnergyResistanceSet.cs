using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    [Serializable]
    public class EnergyResistanceSet : ICreatureBound
    {
        public EnergyResistanceSet(Creature creature)
        {
            Creature = creature;
            _Resistances = new Dictionary<EnergyType, EnergyResistance>
            {
                { EnergyType.Acid, new EnergyResistance(EnergyType.Acid) },
                { EnergyType.Cold, new EnergyResistance(EnergyType.Cold) },
                { EnergyType.Electric, new EnergyResistance(EnergyType.Electric) },
                { EnergyType.Fire, new EnergyResistance(EnergyType.Fire) },
                { EnergyType.Sonic, new EnergyResistance(EnergyType.Sonic) },
                { EnergyType.Force, new EnergyResistance(EnergyType.Force) },
                { EnergyType.Positive, new EnergyResistance(EnergyType.Positive) },
                { EnergyType.Negative, new EnergyResistance(EnergyType.Negative) }
            };
        }

        private Dictionary<EnergyType, EnergyResistance> _Resistances;

        public IEnumerable<EnergyResistance> EffectiveResistances
        {
            get
            {
                foreach (KeyValuePair<EnergyType, EnergyResistance> _kvp in _Resistances)
                {
                    if (_kvp.Value.EffectiveValue > 0)
                    {
                        yield return _kvp.Value;
                    }
                }
                yield break;
            }
        }

        public EnergyResistance this[EnergyType energyType] => _Resistances[energyType];
        public Creature Creature { get; private set; }
    }
}
