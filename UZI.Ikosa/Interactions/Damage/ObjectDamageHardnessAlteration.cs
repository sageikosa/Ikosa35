using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ObjectDamageHardnessAlteration : InteractionAlteration
    {
        public ObjectDamageHardnessAlteration(InteractData data, object source, decimal factor, int delta, params EnergyType?[] energyTypes)
            : base(data, source)
        {
            _Factor = factor;
            _Delta = delta;
            _EnergyTypes = energyTypes.ToList();
        }

        #region data
        private decimal _Factor;
        private int _Delta;
        private List<EnergyType?> _EnergyTypes;
        #endregion

        public decimal Factor => _Factor;
        public int Delta => _Delta;

        /// <summary>
        /// Use null for non-energy type damage.  Returns null if no affect.
        /// </summary>
        /// <param name="energyType"></param>
        /// <param name="hardness"></param>
        /// <returns></returns>
        public int? GetHardness(int hardness, EnergyType? energyType)
        {
            // no explicit list or energy type in explicit list
            if (!_EnergyTypes.Any() || _EnergyTypes.Contains(energyType))
            {
                if (_Factor == 0)
                    return 0;
                else if (_Factor != 1)
                    return (int)(hardness * _Factor);
                return hardness + _Delta;
            }
            return null;
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                if (_Factor == 0)
                    yield return new Info { Message = @"Hardness Bypass" };
                else if (_Factor != 1)
                    yield return new Info { Message = $@"Hardness Factor {_Factor}" };
                else
                    yield return new Info { Message = $@"Hardness Delta {_Delta}" };
                yield break;
            }
        }

    }
}
