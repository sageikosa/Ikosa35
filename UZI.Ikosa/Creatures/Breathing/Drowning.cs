using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures
{
    /// <summary>Drowning: unconscious at 0hp; then -1 and dying; then dead</summary>
    [Serializable]
    public class Drowning : Adjunct, ITrackTime
    {
        /// <summary>Drowning: unconscious at 0 hp; then -1 hp and dying; then dead</summary>
        public Drowning(HoldingBreath holdingBreath)
            : base(holdingBreath)
        {
            // NOTE: recovering breath removes this adjunct
            _Count = 0;
            _Unconscious = new UnconsciousEffect(this, double.MaxValue, Round.UnitFactor);
            _Dying = new Dying(this);
            _Dead = null;
        }

        #region private data
        private double _NextCheck;
        private int _Count;
        private UnconsciousEffect _Unconscious;
        private Dying _Dying;
        private DeadEffect _Dead;
        #endregion

        public Creature Creature
            => Anchor as Creature;

        public HoldingBreath HoldingBreath
            => Source as HoldingBreath;

        protected double CurrentTime
            => Creature?.GetCurrentTime() ?? double.MaxValue;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Creature;

            // no longer holding breath
            if (HoldingBreath != null)
            {
                HoldingBreath.Eject();
            }

            // 0 health points
            if (_critter?.HealthPoints.CurrentValue > 0)
            {
                _critter.HealthPoints.CurrentValue = 0;
            }

            var _time = CurrentTime;
            if (_time < (double.MaxValue - Round.UnitFactor))
            {
                _NextCheck = _time + Round.UnitFactor;
            }
            else
            {
                _NextCheck = _time;
            }

            // unconscious
            _critter?.AddAdjunct(_Unconscious);
            _critter?.Conditions.Add(new Condition(Condition.Drowning, this));
            _critter?.SendSysNotify(new BadNewsNotify(_critter?.ID ?? Guid.Empty, @"Breathing", new Description(@"Drowning", @"Started")));
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // cleanup temporary effects as needed
            _Unconscious.Eject();
            _Dying.Eject();
            var _drown = Creature.Conditions[Condition.Drowning, this];
            Creature.Conditions.Remove(_drown);
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new Drowning(HoldingBreath);

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextCheck) && (direction == TimeValTransition.Entering))
            {
                if (_NextCheck < (double.MaxValue - Round.UnitFactor))
                {
                    _NextCheck += Round.UnitFactor;
                }

                var _critter = Creature;
                _Count++;
                _critter?.SendSysNotify(new BadNewsNotify(_critter?.ID ?? Guid.Empty, @"Breathing", new Description(@"Drowning", $@"{_Count} rounds")));
                if (_Count == 1)
                {
                    // round 2: -1 hp and dying
                    if (_critter?.HealthPoints.CurrentValue == 0)
                    {
                        _critter.HealthPoints.CurrentValue = -1;
                    }

                    _critter?.AddAdjunct(_Dying);
                }
                else
                {
                    // round 3: dead
                    _Dead = _critter?.Adjuncts.OfType<DeadEffect>().FirstOrDefault();
                    if ((_Dead == null) && (_critter != null))
                    {
                        // too bad, this one stays
                        _Dead = new DeadEffect(this, _critter?.GetCurrentTime() ?? 0d);
                        Creature.AddAdjunct(_Dead);
                    }

                    // no need to drown a dead creature
                    Eject();
                }
            }
        }

        public double Resolution
            => Round.UnitFactor;

        #endregion
    }
}
