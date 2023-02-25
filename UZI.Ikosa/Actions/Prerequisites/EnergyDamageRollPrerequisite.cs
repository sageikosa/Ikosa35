using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core.Dice;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Divides damage evenly among a set of energy types (or a single type)
    /// </summary>
    [Serializable]
    public class EnergyDamageRollPrerequisite : DamageRollPrerequisite
    {
        public EnergyDamageRollPrerequisite(object source, Interaction workSet, string key, string name, Roller roller,
            bool serial, string extra, int minGroup, params EnergyType[] energy)
            : base(source, workSet, key, name, roller, serial, false, extra, minGroup)
        {
            _Energy = energy;
        }

        private EnergyType[] _Energy;
        public EnergyType[] Energy { get { return _Energy; } }

        public override string ToString()
        {
            var _builder = new StringBuilder(Roller.ToString());
            _builder.Append(@"(");
            var _first = true;
            foreach (var _e in _Energy)
            {
                _builder.AppendFormat(@"{0}{1}", (!_first ? @"+" : string.Empty), _e.ToString());
                _first = false;
            }
            _builder.Append(@")");
            return _builder.ToString();
        }

        public override IEnumerable<DamageData> GetDamageData()
        {
            return from _energy in _Energy
                   select new EnergyDamageData(RollValue / _Energy.Length, _energy, Extra, MinGroup);
        }
    }
}
