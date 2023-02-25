using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class CustomSpellActivationCost :SpellActivationCost
    {
        public CustomSpellActivationCost(decimal cost)
        {
            _Cost = cost;
        }

        private decimal _Cost;

        public override decimal CalculatePrice(SpellActivation activation)
            => _Cost;
    }
}
