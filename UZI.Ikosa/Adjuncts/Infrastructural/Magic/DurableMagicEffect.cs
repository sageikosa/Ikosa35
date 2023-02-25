using System;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Provides expiration time for a magic power effect [MagicPowerEffect, IAdjunctTracker]</summary>
    [Serializable]
    public class DurableMagicEffect : MagicPowerEffect, ITrackTime
    {
        /// <summary>Provides expiration time for a magic power effect [MagicPowerEffect, IAdjunctTracker]</summary>
        public DurableMagicEffect(MagicPowerActionSource magicSource, ICapabilityRoot root, PowerAffectTracker tracker,
            double expirationTime, TimeValTransition direction, int subMode)
            : base(magicSource, root, tracker, subMode)
        {
            ExpirationTime = expirationTime;
            Direction = direction;
        }

        #region data
        protected double _ExpirationTime;
        private TimeValTransition _Direction;
        #endregion

        public double ExpirationTime { get => _ExpirationTime; set => _ExpirationTime = value; }
        public TimeValTransition Direction { get => _Direction; set => _Direction = value; }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= ExpirationTime) && (direction == Direction))
            {
                // Deactivate when expires
                Anchor.RemoveAdjunct(this);
            }
        }

        public double Resolution
            => CapabilityRoot.GetCapability<IDurableCapable>()?.DurationRule(SubMode).Resolution ?? Round.UnitFactor;

        #endregion

        public override object Clone()
            => new DurableMagicEffect(MagicPowerActionSource, CapabilityRoot, PowerTracker, ExpirationTime, Direction, SubMode);

        public static MagicPowerEffect GetDurableMagicEffect(DurationRule durationRule)
            => durationRule.DurationType switch
            {
                DurationType.Concentration => null,
                DurationType.ConcentrationPlusSpan => null,
                _ => null
            };
    }
}
