using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Deltas;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Represents a loss of dexterity bonus to AR</summary>
    [Serializable]
    public class MaxDexterityToARAlteration : AttackRollAlteration
    {
        /// <summary>
        /// If the delta is positive, the attacker gets a bonus equal to it.  Otherwise, the defender is already at zero-gain or a loss.
        /// </summary>
        public MaxDexterityToARAlteration(AttackData attackData, IDelta maxDex, object reason) :
            base(attackData, typeof(DexterityOverride), (maxDex.Value > 0 ? maxDex.Value : 0), false)
        {
            _Reason = reason;
        }

        private object _Reason;
        public object Reason => _Reason;

        /// <summary>Indicates loss is due to unawareness (blind-fight may remove this)</summary>
        public bool UnawarenessLoss
            => Reason is TargetUnawareAlteration;

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = $@"Max Dex to AR: {Modifier}" };
                yield break;
            }
        }

    }
}
