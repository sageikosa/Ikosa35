using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Was free-falling, but then entered liquid</summary>
    [Serializable]
    public class LiquidFallMovement : BaseFallMovement
    {
        #region construction
        /// <summary>Was free-falling, but then entered liquid</summary>
        public LiquidFallMovement(CoreObject coreObj, FallMovement fallMove, int speed)
            : base(coreObj, fallMove, speed)
        {
            _Track = 0;
            var _distance = fallMove.FallDice * 10d;
            if (_distance < 10)
            {
                _Max = 1;
            }
            else if (_distance < 50)
            {
                _Max = 2;
            }
            else if (_distance < 100)
            {
                _Max = 3;
            }
            else if (_distance < 500)
            {
                _Max = 4;
            }
            else
            {
                _Max = 5;
            }
        }
        #endregion

        #region state
        private int _Track;
        private int _Max;
        #endregion

        public int FallTrack => _Track;
        public int MaxFall => _Max;

        public FallMovement FallMovement => Source as FallMovement;

        public void RemoveLiquidFalling()
        {
            // remove liquid falling adjunct
            CoreObject.Adjuncts.OfType<LiquidFalling>()
                .Where(_lf => _lf.LiquidFallMovement == this)
                .FirstOrDefault()?.Eject();

            GetCurrentSound(CoreObject)?.Eject();
        }

        #region public override void AddInterval(double amount)
        /// <summary>Track intervals fallen through liquid</summary>
        public override void AddInterval(double amount)
        {
            // add regular damage
            _Track += (int)(amount * 10d);
            if (_Track < 0)
            {
                // zero out damage
                _Track = 0;
            }
        }
        #endregion

        public override string Name => @"Liquid Fall Movement";

        #region public void GenerateImpactSound(bool splashdown)
        public void GenerateImpactSound(bool splashdown)
        {
            var _sound = new SoundGroup(this, GetImpactSound(splashdown));
            var _participant = new SoundParticipant(this, _sound);
            CoreObject.AddAdjunct(_participant);
            _participant.Eject();
        }

        protected SoundRef GetImpactSound(bool splashdown)
        {
            var _serial = CoreObject.IncreaseSerialState();
            var _sizer = (CoreObject as ISizable)?.Sizer;
            var _sizeDescript = _sizer?.Size.Order != 0 ? $@"{_sizer.Size.Name} " : string.Empty;
            var _material = (CoreObject as Creature)?.GetMovementSound()
                ?? (CoreObject as IObjectBase)?.ObjectMaterial.SoundQuality;
            var (_dCalc, _range) = GetSoundCarry(_sizer);
            var _impact = splashdown ? @"splashing" : @"thumped";
            return new SoundRef(new Audible(Guid.NewGuid(), CoreObject.ID, @"Liquid Fall Stop",
                (0, $@"something {_impact}"),
                (5, $@"something {_sizeDescript}{_impact}"),
                (10, $@"something {_sizeDescript}{_material} {_impact}")), _dCalc, _range, _serial);
        }

        protected (DeltaCalcInfo difficulty, double range) GetSoundCarry(Sizer sizer)
        {
            var _dCalc = new DeltaCalcInfo(CoreObject.ID, @"Liquid Fall Impact Sound");
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

            var _nonLethal = (int)Math.Floor(FallMovement.NonLethal);
            if (_nonLethal >= 1)
            {
                _dCalc.AddDelta(@"Non-Lethal Distance", 0 - _nonLethal);
                _dCalc.Result -= _nonLethal;
            }
            var _lethal = (int)Math.Floor(FallMovement.FallDice - FallMovement.MaxNonLethal) * 2;
            if (_lethal >= 1)
            {
                _dCalc.AddDelta(@"Lethal Distance", 0 - _lethal);
                _dCalc.Result -= _lethal;
                _range += _lethal * 2.5d;
            }
            return (_dCalc, _range);
        }
        #endregion

        public override bool CanMoveThrough(CellMaterial material)
            => material is LiquidCellMaterial;

        public override MovementBase Clone(Creature forCreature, object source)
            => new LiquidFallMovement(forCreature, source as FallMovement, BaseValue);
    }
}
