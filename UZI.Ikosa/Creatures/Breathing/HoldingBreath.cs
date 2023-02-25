using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures
{
    /// <summary>Removes recovering breath, might go right to drowning if unconscious</summary>
    [Serializable]
    public class HoldingBreath : Adjunct, ITrackTime
    {
        /// <summary>Removes recovering breath, might go right to drowning if unconscious</summary>
        public HoldingBreath()
            : base(typeof(HoldingBreath))
        {
        }

        #region private data
        private BreathlessCounter _Counter;
        private double _NextCheck;
        #endregion

        public BreathlessCounter BreathlessCounter => _Counter;

        protected double CurrentTime
            => Anchor?.GetCurrentTime() ?? double.MaxValue;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (IsUnconscious)
            {
                // proceed right to drowning
                Anchor.AddAdjunct(new Drowning(this));
            }
            else
            {
                var _time = CurrentTime;
                if (_time < (double.MaxValue - Round.UnitFactor))
                    _NextCheck = _time + Round.UnitFactor;
                else
                    _NextCheck = _time;

                if (Anchor is Creature _critter)
                {
                    _critter.Conditions.Add(new Condition(Condition.HoldingBreath, this));
                    _critter.SendSysNotify(new BadNewsNotify(_critter.ID, @"Breathing",
                        new Description(@"Holding Breath", $@"{BreathlessCounter.HeldCount} rounds")));
                }
            }
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (Anchor is Creature _critter)
            {
                var _holdBreath = _critter.Conditions[Condition.HoldingBreath, this];
                _critter.Conditions.Remove(_holdBreath);
            }
        }

        #region OnAnchorSet()
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (Anchor != null)
            {
                // set anchor
                var _recovery = Anchor.Adjuncts.OfType<RecoveringBreath>().FirstOrDefault();
                if (_recovery != null)
                {
                    // swipe the recovery adjunct's counter
                    _Counter = _recovery.BreathlessCounter;
                    _recovery.Eject();
                }
                else
                {
                    // otherwise we make our own
                    _Counter = new BreathlessCounter(Anchor as Creature);
                }
            }
        }
        #endregion

        /// <summary>Cannot hold breath if unconscious</summary>
        private bool IsUnconscious
            => (Anchor as Creature)?.Conditions.Contains(Condition.Unconscious) ?? false;

        public override object Clone()
            => new HoldingBreath();

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextCheck) && (direction == TimeValTransition.Entering))
            {
                if (_NextCheck < (double.MaxValue - Round.UnitFactor))
                    _NextCheck += Round.UnitFactor;

                var _critter = Anchor as Creature;
                if (IsUnconscious)
                {
                    // drowning (could have monitored conditions, but this works just as well)
                    _critter?.AddAdjunct(new Drowning(this));
                }
                else
                {
                    BreathlessCounter.HeldCount++;
                    _critter?.SendSysNotify(new BadNewsNotify(_critter?.ID ?? Guid.Empty, @"Breathing",
                        new Description(@"Holding Breath", $@"{BreathlessCounter.HeldCount} rounds")));
                    if (BreathlessCounter.MustCheck)
                    {
                        _critter?.StartNewProcess(new ContinueHoldingBreath(_critter, this),
                            @"Check to Continue Holding Breath");
                    }
                }
            }
        }

        public double Resolution { get { return Round.UnitFactor; } }

        #endregion
    }
}
