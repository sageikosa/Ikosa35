using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Senses;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class DropHandler : IInteractHandler
    {
        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is Drop _dropData) && (workSet.Target != null))
            {
                var _map = _dropData.Map;
                var _location = _dropData.Location;
                var _startFall = false;

                if (workSet.Target is ICoreObject _obj)
                {
                    // TODO: more reduce calculations

                    Locator _locator = null;
                    if (!(workSet.Target is IObjectBase) || !(workSet.Target as IObjectBase).IsLocatable)
                    {
                        var (_trove, _fallLoc, _ethereal) = Trove.GetTrove(_dropData, _map, _obj);

                        // add the object to the trove (before generating impact sound)
                        if (_trove != null)
                        {
                            _trove.Add(_obj);
                        }

                        if (_fallLoc != null)
                        {
                            _startFall = true;
                            _locator = _fallLoc;
                        }
                        else if (!_ethereal)    // if ethereal, don't make a sound
                        {
                            GenerateImpactSound(_obj);
                        }
                    }
                    else if ((_obj is IObjectBase _objBase) && (_map != null) && (_location != null))
                    {
                        #region new locator
                        // create a new locator for the object
                        var _size = _objBase.Sizer.Size.CubeSize();
                        _locator = new ObjectPresenter(_objBase, _map.MapContext, _size,
                                    new Cubic(_location, _size), _map.Resources);
                        var _ethereal = _obj.HasActiveAdjunct<EtherealState>();

                        if (_dropData.Surface is IObjectBase _surface)
                        {
                            // put the object on the surface
                            var _container = SurfaceGroup.GetSurfaceContainer(_surface);
                            _objBase.AddAdjunct(new OnSurface(_container.Surface));
                            if (!_ethereal)
                            {
                                GenerateImpactSound(_obj);
                            }
                        }
                        else
                        {
                            _startFall = !_ethereal;
                        }
                        #endregion
                    }

                    if (_startFall)
                    {
                        var _maxSpeed = 500;
                        var _reduce = (_dropData.DropGently ? 0.5 : 0);
                        if (workSet.Target is IObjectBase _ob)
                        {
                            _reduce += _ob.FallReduce;
                            _maxSpeed = _ob.MaxFallSpeed;
                        }
                        else if (workSet.Target is IItemBase _ib)
                        {
                            _reduce += _ib.FallReduce;
                            _maxSpeed = _ib.MaxFallSpeed;
                        }
                        FallingStartStep.StartFall(_locator, _maxSpeed, _maxSpeed, @"Dropped", _reduce);
                    }
                }
            }
        }
        #endregion

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Drop);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;

        #region public void GenerateImpactSound(ICoreObject coreObject)
        public void GenerateImpactSound(ICoreObject producer)
        {
            var _sound = new SoundGroup(this, GetImpactSound(producer));
            var _participant = new SoundParticipant(this, _sound);
            producer.AddAdjunct(_participant);
            producer.IncreaseSerialState();
            _participant.Eject();
        }

        protected SoundRef GetImpactSound(ICoreObject producer)
        {
            var _serial = producer.IncreaseSerialState();
            var _sizer = (producer as ISizable)?.Sizer;
            var _sizeDescript = _sizer?.Size.Order != 0 ? $@"{_sizer.Size.Name} " : string.Empty;
            var _material = (producer as Creature)?.GetMovementSound()
                ?? (producer as IObjectBase)?.ObjectMaterial.SoundQuality;
            // TODO: material difficulty deltas
            var (_dCalc, _range) = GetSoundCarry(producer, _sizer);
            return new SoundRef(new Audible(Guid.NewGuid(), producer.ID, @"Trove Add",
                (0, @"thump"),
                (5, $@"{_sizeDescript}thump"),
                (10, $@"{_sizeDescript}{_material} thump")), _dCalc, _range, _serial);
        }

        protected (DeltaCalcInfo difficulty, double range) GetSoundCarry(ICoreObject coreObject, Sizer sizer)
        {
            var _dCalc = new DeltaCalcInfo(coreObject.ID, @"Trove Add Sound");
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
    }
}
