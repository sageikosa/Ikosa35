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
    [Serializable]
    public class SinkingMovement : BaseFallMovement
    {
        #region construction
        public SinkingMovement(int speed, CoreObject coreObj, object source)
            : base(coreObj, source, speed)
        {
            _Distance = 0;
        }
        #endregion

        private int _Distance;

        /// <summary>Indication of distance travelled this round</summary>
        public int Distance { get => _Distance; set => _Distance = value; }

        public void RemoveSinking()
        {
            // remove sinking adjunct
            CoreObject.Adjuncts.OfType<Sinking>()
                .Where(_s => _s.SinkingMovement == this)
                .FirstOrDefault()?.Eject();

            GetCurrentSound(CoreObject)?.Eject();
        }

        public override string Name => @"Sinking";

        #region public override bool IsUsable { get; }
        public override bool IsUsable
        {
            get
            {
                var _locator = CoreObject.GetLocated()?.Locator;
                if (_locator != null)
                {
                    if (!_locator.PlanarPresence.HasMaterialPresence())
                    {
                        return false;
                    }

                    var _map = _locator.Map;
                    return (from _cell in _locator.GeometricRegion.AllCellLocations()
                            select _map[_cell].ValidSpace(this)).Any();
                }
                return false;
            }
        }
        #endregion

        public override MovementBase Clone(Creature forCreature, object source)
            => new SinkingMovement(BaseValue, forCreature, source);

        #region public void GenerateImpactSound()
        public void GenerateImpactSound()
        {
            var _sound = new SoundGroup(this, GetImpactSound());
            var _participant = new SoundParticipant(this, _sound);
            CoreObject.AddAdjunct(_participant);
            _participant.Eject();
        }

        protected SoundRef GetImpactSound()
        {
            var _serial = CoreObject.IncreaseSerialState();
            var _sizer = (CoreObject as ISizable)?.Sizer;
            var _sizeDescript = _sizer?.Size.Order != 0 ? $@"{_sizer.Size.Name} " : string.Empty;
            var _material = (CoreObject as Creature)?.GetMovementSound()
                ?? (CoreObject as IObjectBase)?.ObjectMaterial.SoundQuality;
            var (_dCalc, _range) = GetSoundCarry(_sizer);
            var _impact = @"bumped";
            return new SoundRef(new Audible(Guid.NewGuid(), CoreObject.ID, @"Sinking Stop",
                (0, $@"something {_impact}"),
                (5, $@"something {_sizeDescript}{_impact}"),
                (10, $@"something {_sizeDescript}{_material} {_impact}")), _dCalc, _range, _serial);
        }

        protected (DeltaCalcInfo difficulty, double range) GetSoundCarry(Sizer sizer)
        {
            var _dCalc = new DeltaCalcInfo(CoreObject.ID, @"Sinking Stop Sound");
            var _range = 90d;
            switch (sizer.Size.Order)
            {
                case -4:
                    _dCalc.SetBaseResult(12);
                    _range = 30d;
                    break;
                case -3:
                    _dCalc.SetBaseResult(8);
                    _range = 45d;
                    break;
                case -2:
                    _dCalc.SetBaseResult(4);
                    _range = 60d;
                    break;
                case -1:
                    _dCalc.SetBaseResult(2);
                    _range = 75d;
                    break;
                case 1:
                    _dCalc.SetBaseResult(-2);
                    _range = 105d;
                    break;
                case 2:
                    _dCalc.SetBaseResult(-4);
                    _range = 120d;
                    break;
                case 3:
                    _dCalc.SetBaseResult(-8);
                    _range = 135d;
                    break;
                case 4:
                    _dCalc.SetBaseResult(-12);
                    _range = 150d;
                    break;

                case 0:
                default:
                    _dCalc.SetBaseResult(0);
                    break;
            }
            return (_dCalc, _range);
        }
        #endregion

        public override bool CanMoveThrough(CellMaterial material)
            => material is LiquidCellMaterial;
    }
}
