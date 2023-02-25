using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Allows a magic power effect to be connected to concentration
    /// </summary>
    [Serializable]
    public class ConcentrationPlusSpanMagicTarget : ConcentrationMagicEffect
    {
        /// <summary>
        /// Allows a magic power effect to be connected to concentration
        /// </summary>
        public ConcentrationPlusSpanMagicTarget(MagicPowerEffect magicPowerEffect,
            ConcentrationMagicControl control, Duration duration)
            : base(magicPowerEffect, control)
        {
            _Duration = duration;
        }

        #region state
        private Duration _Duration;
        #endregion

        public Duration Duration => _Duration;

        public override object Clone()
            => new ConcentrationPlusSpanMagicTarget(MagicPowerEffect, Control, Duration);

        protected override void OnDeactivate(object source)
        {
            // NOTE: this could be deactivated from the magic power effect, or the concentration group
            // once deactivated, it will be cleared from the magic power effect
            if (MagicPowerEffect is DurableMagicEffect _durable
                && Anchor.Setting is LocalMap _map)
            {
                if (_durable.AnchoredAdjunctObject == this)
                {
                    // durable will be set to final expiration time
                    // NOTE: if being unanchored, this won't really matter
                    _durable.ExpirationTime = _map.CurrentTime + Duration.SpanLength;
                    MagicPowerEffect.ClearAnchoredAdjunctObject();
                }
            }
            base.OnDeactivate(source);
        }
    }
}
