using System;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using System.Collections.Generic;
using Uzi.Ikosa.Senses;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Movement
{
    // TODO: recover from stall action...
    /// <summary>Falling is a movement that arises during a particular instance of falling.</summary>
    [Serializable]
    public class FallMovement : BaseFallMovement
    {
        #region construction
        // NOTE: stalled flying creature falling is 150', feather fall is 60', others may fall faster (~500')
        public FallMovement(CoreObject coreObj, object source, int speed, double initialLethalDice, double maxNonLethalDice)
            : base(coreObj, source, speed)
        {
            _FallTrack = initialLethalDice;
            _FallReduce = 0;
            _Distance = 0;
            _Total = 0;
            _Softer = false;
            _ContinueSpeed = speed;
            _MaxNonLethal = maxNonLethalDice;
        }
        #endregion

        #region state
        private double _FallTrack;
        private double _Total;
        private double _Distance;
        private double _FallReduce;
        private double _MaxNonLethal;
        private int _ContinueSpeed;
        private bool _Softer;
        private bool _Stall;
        #endregion

        public override void RemoveFalling()
        {
            // remove falling adjunct
            CoreObject.Adjuncts.OfType<Falling>()
                .Where(_f => _f.FallMovement == this)
                .FirstOrDefault()?.Eject();

            GetCurrentSound(CoreObject)?.Eject();
        }

        /// <summary>True if the movement represents an actual fall (rather than one that started, but couldn't continue)</summary>
        public override bool IsUncontrolled
            => (_Total > 5) || !(CoreObject is Creature);

        /// <summary>Fall dice based on calculated distance of fall divided by 10 feet (maximum 20)</summary>
        public double FallDice
            => Math.Min((_FallTrack >= FallReduce ? _FallTrack : FallReduce) - FallReduce, 20d);

        // TODO: make this a deltable...
        /// <summary>Amount (in 10ft increments) by which to reduce actual fall distance when determining FallDice (may still max at 20)</summary>
        public double FallReduce { get => _FallReduce; set => _FallReduce = value; }

        /// <summary>Speed used after ContinueFall is activated</summary>
        public int ContinueSpeed { get => _ContinueSpeed; set => _ContinueSpeed = value; }

        /// <summary>True if this falling is due to a flight stall, leading to a reflex save and consuming the next round of budget</summary>
        public bool FlightStall { get => _Stall; set => _Stall = value; }

        /// <summary>Total Non-lethal damage dice accumulated</summary>
        public double NonLethal
            => MaxNonLethal > FallDice ? FallDice : MaxNonLethal;

        /// <summary>Maximum non-lethal damage dice counter, decrease while increasing nonlethal</summary>
        public double MaxNonLethal { get => _MaxNonLethal; set => _MaxNonLethal = value; }

        /// <summary>If true, uses 1d3 for non-lethal rollers</summary>
        public bool IsSofterNonLethal { get => _Softer; set => _Softer = value; }

        #region public DiceRoller NonLethalRoller { get; }
        /// <summary>Returns non lethal damage roller</summary>
        public DiceRoller NonLethalRoller
        {
            get
            {
                var _non = NonLethal;

                // only provide non lethal roller if something is left
                if (_non > 0)
                {
                    return new DiceRoller(Convert.ToInt32(Math.Round(_non, MidpointRounding.AwayFromZero)), IsSofterNonLethal ? (byte)3 : (byte)6);
                }

                return null;
            }
        }
        #endregion

        #region public DiceRoller DamageRoller { get; }
        /// <summary>Returns damage roller (after accounting for fall reduction and non lethal damage)</summary>
        public DiceRoller DamageRoller
        {
            get
            {
                var _dmg = FallDice - MaxNonLethal;
                if (_dmg > 0)
                {
                    // if reduction totally eliminates damage, start taking from nonlethal
                    return new DiceRoller(Convert.ToInt32(Math.Round(_dmg, MidpointRounding.AwayFromZero)), 6);
                }
                return null;
            }
        }
        #endregion

        #region public void AddInterval(double amount)
        /// <summary>Add 10 foot interval</summary>
        public override void AddInterval(double amount)
        {
            // add regular damage
            if (EffectiveValue >= 80)
            {
                _FallTrack += amount;
                if (_FallTrack < 0)
                {
                    // zero out damage
                    _FallTrack = 0;
                }
            }
            else
            {
                // falling too slowly to pick up any damage
                _FallTrack = 0;
            }
            if (amount > 0)
            {
                // still track distance
                _Distance += 10 * amount;
                _Total += 10 * amount;
            }
        }
        #endregion

        // TODO: alterations to falling might go here 
        // TODO: e.g., attempts to be caught, to catch self, tumble attempt, jump down...
        // TODO: solid fog reduces damage by 1d6

        public override string Name => @"Fall Movement";

        #region public void GenerateImpactSound()
        public void GenerateImpactSound(CoreObject impactor)
        {
            var _sound = new SoundGroup(this, GetImpactSound(impactor));
            var _participant = new SoundParticipant(this, _sound);
            impactor.AddAdjunct(_participant);
            _participant.Eject();
        }

        protected SoundRef GetImpactSound(CoreObject impactor)
        {
            var _serial = impactor.IncreaseSerialState();
            var _sizer = (impactor as ISizable)?.Sizer;
            var _sizeDescript = _sizer?.Size.Order != 0 ? $@"{_sizer.Size.Name} " : string.Empty;
            var _material = (impactor as Creature)?.GetMovementSound()
                ?? (impactor as IObjectBase)?.ObjectMaterial.SoundQuality;
            var (_dCalc, _range) = GetSoundCarry(_sizer, impactor);
            return new SoundRef(new Audible(Guid.NewGuid(), impactor.ID, @"Fall Stop",
                (0, @"something crashing"),
                (5, $@"something {_sizeDescript}crashing"),
                (10, $@"something {_sizeDescript}{_material} crashing")), _dCalc, _range, _serial);
        }

        protected (DeltaCalcInfo difficulty, double range) GetSoundCarry(Sizer sizer, CoreObject impactor)
        {
            var _dCalc = new DeltaCalcInfo(impactor.ID, @"Fall Impact Sound");
            var _range = 120d;
            switch (sizer.Size.Order)
            {
                case -4:
                    _dCalc.SetBaseResult(8);
                    _range = 60d;
                    break;
                case -3:
                    _dCalc.SetBaseResult(4);
                    _range = 75d;
                    break;
                case -2:
                    _dCalc.SetBaseResult(0);
                    _range = 90d;
                    break;
                case -1:
                    _dCalc.SetBaseResult(-2);
                    _range = 105d;
                    break;
                case 1:
                    _dCalc.SetBaseResult(-6);
                    _range = 135d;
                    break;
                case 2:
                    _dCalc.SetBaseResult(-8);
                    _range = 150d;
                    break;
                case 3:
                    _dCalc.SetBaseResult(-12);
                    _range = 165d;
                    break;
                case 4:
                    _dCalc.SetBaseResult(-16);
                    _range = 180d;
                    break;

                case 0:
                default:
                    _dCalc.SetBaseResult(-4);
                    break;
            }

            var _nonLethal = (int)Math.Floor(NonLethal);
            if (_nonLethal >= 1)
            {
                _dCalc.AddDelta(@"Non-Lethal Distance", 0 - _nonLethal);
                _dCalc.Result -= _nonLethal;
            }
            var _lethal = (int)Math.Floor(FallDice - MaxNonLethal) * 2;
            if (_lethal >= 1)
            {
                _dCalc.AddDelta(@"Lethal Distance", 0 - _lethal);
                _dCalc.Result -= _lethal;
                _range += _lethal * 2.5d;
            }
            return (_dCalc, _range);
        }
        #endregion

        public override MovementBase Clone(Creature forCreature, object source)
            => new FallMovement(forCreature, source, BaseValue, _FallTrack, MaxNonLethal);

        public override void ProcessNoRegion(CoreStep step, Locator locator)
        {
            // hypothetical liquid fall movement to check
            var _liquidFall = new LiquidFallMovement(CoreObject, this, EffectiveValue);
            var _next = _liquidFall.NextRegion();
            if (_next != null)
            {
                // if we can enter the drink, we should enter the drink
                step.EnqueueNotify(new BadNewsNotify(CoreObject.ID, @"Movement", new Description(@"Falling", @"fell into liquid")),
                    CoreObject.ID);
                new LiquidFallingStartStep(step, locator, this);
            }
            else
            {
                // otherwise, stop (the hard way)
                new FallingStopStep(step, locator, this);
            }
        }
    }
}
