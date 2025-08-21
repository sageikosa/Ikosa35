using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures
{
    /// <summary>Removes holding-breath and drowning</summary>
    [Serializable]
    public class RecoveringBreath : Adjunct, ITrackTime
    {
        /// <summary>Removes holding-breath and drowning</summary>
        public RecoveringBreath()
            : base(typeof(RecoveringBreath))
        {
            // NOTE: this adjunct will evaporate after enough time
        }

        #region private data
        private BreathlessCounter _Counter;
        private double _NextCheck;
        #endregion

        public BreathlessCounter BreathlessCounter => _Counter;

        protected double CurrentTime
            => Anchor?.GetCurrentTime() ?? double.MaxValue;

        #region protected override bool OnPreActivate()
        protected override bool OnPreActivate(object source)
        {
            var _holding = Anchor.Adjuncts.OfType<HoldingBreath>().FirstOrDefault();
            if (_holding == null)
            {
                // didn't have a holding breath, possibly had a drowning
                var _drowning = Anchor.Adjuncts.OfType<Drowning>().FirstOrDefault();
                if (_drowning != null)
                {
                    _holding = _drowning.HoldingBreath;
                }
            }

            if (_holding != null)
            {
                // swipe the holding breath adjunct's counter
                _Counter = _holding.BreathlessCounter;
                _holding.Eject();
                return true;
            }

            // no breathless counter to get!
            return false;
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);

            // cannot be drowning if recovering
            Anchor?.Adjuncts.OfType<Drowning>().FirstOrDefault()?.Eject();

            var _time = CurrentTime;
            if (_time < (double.MaxValue - Round.UnitFactor))
            {
                _NextCheck = _time + Round.UnitFactor;
            }
            else
            {
                _NextCheck = _time;
            }

            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Add(new Condition(Condition.RecoveringBreath, this));
                _critter.SendSysNotify(new GoodNewsNotify(_critter.ID, @"Breathing",
                    new Description(@"Recovering Breath", $@"{BreathlessCounter.HeldCount} rounds")));
            }
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (Anchor is Creature _critter)
            {
                var _recover = _critter.Conditions[Condition.RecoveringBreath, this];
                _critter.Conditions.Remove(_recover);
            }
        }

        public override object Clone()
            => new RecoveringBreath();

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextCheck) && (direction == TimeValTransition.Entering))
            {
                if (_NextCheck < (double.MaxValue - Round.UnitFactor))
                {
                    _NextCheck += Round.UnitFactor;
                }

                // decrease counter until 0, eject when done...
                BreathlessCounter.HeldCount -= 2;
                BreathlessCounter.ContinueDifficulty--;
                if (BreathlessCounter.HeldCount <= 0)
                {
                    Eject();
                }

                if (Anchor is Creature _critter)
                {
                    _critter.SendSysNotify(new GoodNewsNotify(_critter.ID, @"Breathing",
                        new Description(@"Recovering Breath", $@"{BreathlessCounter.HeldCount} rounds")));
                }
            }
        }

        public double Resolution
        {
            get { return Round.UnitFactor; }
        }

        #endregion
    }
}
