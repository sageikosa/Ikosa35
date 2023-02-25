using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Once deactivated, a fragile magic effect cannot be re-enabled.  
    /// Also, performs OnEndAnchor for DurableAnchorMode on disable.
    /// Suitable for Concentration (with or without max-capacity)
    /// </summary>
    [Serializable]
    public class FragileMagicEffect : DurableMagicEffect
    {
        /// <summary>
        /// Once deactivated, a fragile magic effect cannot be re-enabled.  
        /// Also, performs OnEndAnchor for DurableAnchorMode on disable.
        /// Suitable for Concentration (with or without max-capacity)
        /// </summary>
        public FragileMagicEffect(MagicPowerActionSource magicSource, ICapabilityRoot root, PowerAffectTracker tracker,
            double expirationTime, TimeValTransition direction, int subMode)
            : base(magicSource, root, tracker, expirationTime, direction, subMode)
        {
        }

        #region state
        private bool _Disabled = false;
        #endregion

        public bool IsDisabled => _Disabled;

        protected override void OnActivate(object source)
        {
            if (!IsDisabled)
            {
                base.OnActivate(source);
            }
        }

        protected override void OnDeactivate(object source)
        {
            if (!IsDisabled)
            {
                _Disabled = true;
                base.OnDeactivate(source);
                CapabilityRoot.GetCapability<IDurableAnchorCapable>()?.OnEndAnchor(this, Anchor);
            }
        }

        public override object Clone()
            => new FragileMagicEffect(MagicPowerActionSource, CapabilityRoot, PowerTracker, ExpirationTime, Direction, SubMode);
    }
}
