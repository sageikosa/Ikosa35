using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SpellActivationCost
    {
        public virtual decimal CalculatePrice(SpellActivation activation)
        {
            if (activation.PowerCapacity is IRegeneratingBattery)
            {
                // charges per day
                return activation.MagicLevelPrice * (activation.PowerCapacity as IRegeneratingBattery).MaximumCharges.EffectiveValue / 5m;
            }
            else if (activation.PowerCapacity is IPowerBattery)
            {
                // non-recharging
                return activation.MagicLevelPrice * (activation.PowerCapacity as IPowerBattery).AvailableCharges;
            }
            else
            {
                // single or on-demand
                return activation.MagicLevelPrice;
            }
        }
    }
}
