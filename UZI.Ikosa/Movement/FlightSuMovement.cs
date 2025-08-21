using System;
using System.Linq;
using Uzi.Visualize;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using System.Collections.Generic;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Perfect=0, Good=1, Average=2, Poor=3, Clumsy=4.  Lower is better.  Higher is worse.</summary>
    [Serializable]
    public enum FlightManeuverability
    {
        /// <summary>Rating=0</summary>
        Perfect,
        /// <summary>Rating=1</summary>
        Good,
        /// <summary>Rating=2</summary>
        Average,
        /// <summary>Rating=3</summary>
        Poor,
        /// <summary>Rating=4</summary>
        Clumsy
    }

    /// <summary>Supernatural Flight (and Base For FlightExMovement)</summary>
    [Serializable]
    public class FlightSuMovement : MovementBase
    {
        /// <summary>Supernatural Flight (and Base For FlightExMovement)</summary>
        public FlightSuMovement(int speed, CoreObject coreObj, object source, FlightManeuverability flightManeuverability,
            bool armorEncumberance, bool heavyAsMedium)
            : base(speed, coreObj, source)
        {
            _ManeuverabilityRating = flightManeuverability;
            _ArmorEncumberance = armorEncumberance;
            _HeavyAsMedium = heavyAsMedium;
        }

        protected FlightManeuverability _ManeuverabilityRating;
        protected bool _ArmorEncumberance;
        protected bool _HeavyAsMedium;

        public override bool FailsAboveMaxLoad => true;
        public virtual string FlightType => @"Su";
        public override string Name => @"Fly";
        public override string Description => $@"{base.Description} ({FlightType}) {ManeuverabilityRating}";
        public override bool CanShiftPosition => ManeuverabilityRating <= FlightManeuverability.Good;
        public override bool IsSqueezingAllowed => ManeuverabilityRating == FlightManeuverability.Perfect;
        public override bool SurfacePressure => false;

        public override bool IsNativeMovement
            => (Source == CoreObject) || ((CoreObject as Creature)?.Body == Source);

        #region protected override int CalcEffectiveValue()
        protected override int CalcEffectiveValue()
        {
            // flying creature cannot fly if not lightly loaded
            var _val = base.CalcEffectiveValue();
            var _critter = CoreObject as Creature;
            var _moveLoad = ObjectGrabber.GetTotalMoveLoad(_critter, this);
            var _capacity = _critter.CarryingCapacity;

            void _penalize()
            {
                if (_val <= 20)
                {
                    _val -= 5;
                }
                else if (_val <= 40)
                {
                    _val -= 10;
                }
                else if (_val <= 50)
                {
                    _val -= 15;
                }
                else if (_val <= 70)
                {
                    _val -= 20;
                }
                else if (_val <= 80)
                {
                    _val -= 25;
                }
                else if (_val <= 100)
                {
                    _val -= 30;
                }
                else
                {
                    _val -= 35;
                }
            }

            if (_ArmorEncumberance)
            {
                // armor/load
                if (!_critter.EncumberanceCheck.Unencumbered
                    || (_moveLoad > _capacity.LightLoadLimit))
                {
                    _penalize();

                    // some magic flight powers treat heavy as medium for penalties
                    if (!_HeavyAsMedium
                        && (_critter.EncumberanceCheck.GreatlyEncumbered
                        || (_moveLoad > _capacity.MediumLoadLimit)))
                    {
                        // greatly encumbered will have only 5 feet of movement, hope its enough
                        _val = 5;
                    }

                    if (_critter.EncumberanceCheck.AtlasMustShrug
                        || (_moveLoad > _capacity.HeavyLoadLimit))
                    {
                        // cannot fly
                        _val = 0;
                    }
                }
            }
            else
            {
                // just load
                if (_critter.EncumberanceCheck.WeighedDown
                    || ((_moveLoad > _capacity.LightLoadLimit) && (_moveLoad <= _capacity.MediumLoadLimit)))
                {
                    _penalize();
                }
                else if (_critter.EncumberanceCheck.Straining
                    || ((_moveLoad > _capacity.MediumLoadLimit) && (_moveLoad <= _capacity.HeavyLoadLimit)))
                {
                    if (_HeavyAsMedium)
                    {
                        // some magic flight powers treat heavy as medium for penalties
                        _penalize();
                    }
                    else
                    {
                        // otherwise only 5'
                        _val = 5;
                    }
                }
                else if (_critter.EncumberanceCheck.AtlasMustShrug
                    || (_moveLoad > _capacity.HeavyLoadLimit))
                {
                    _val = 0;
                }
            }

            if (_val < 0)
            {
                _val = 0;
            }

            return _val;
        }
        #endregion

        public FlightManeuverability ManeuverabilityRating
        {
            get { return _ManeuverabilityRating; }
            set
            {
                _ManeuverabilityRating = value;
                DoPropertyChanged(@"ManeuverabilityRating");
            }
        }

        #region public FlightBudget GetFlightBudget(CoreActivity activity, int upDownAdjust)
        /// <summary>Returns a flight budget appropriate for this FlightMovement</summary>
        public FlightBudget GetFlightBudget(CoreActivity activity, UpDownAdjustment upDownAdjust)
        {
            if (activity.Action is MovementAction _action)
            {
                return FlightBudget.GetBudget(_action.Budget, upDownAdjust);
            }
            return null;
        }
        #endregion

        #region public MovementBudget GetMovementBudget(CoreActivity activity)
        /// <summary>Returns the movement budget appropriate for the action</summary>
        public MovementBudget GetMovementBudget(CoreActivity activity)
        {
            if (activity.Action is MovementAction _action)
            {
                return MovementBudget.GetBudget(_action.Budget);
            }
            return null;
        }
        #endregion

        #region protected override double BaseMoveCost(CoreActivity activity, double expected)
        protected override double BaseMoveCost(CoreActivity activity, double expected)
        {
            var _loc = activity.GetFirstTarget<ValueTarget<Locator>>(@"Locator");
            var _avTarget = activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = activity.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
            if ((_loc != null) && (_avTarget != null) && (_idx != null))
            {
                // half cost going down (with gravity)
                var _gravity = _loc.Value.GetGravityFace();
                var _upDown = _avTarget.GetUpDownAdjustment(_gravity, _idx.Value);
                if (_upDown < UpDownAdjustment.Level)
                {
                    expected /= 2;
                }
                else if (_upDown > UpDownAdjustment.Level)
                {
                    // (often) double cost going up
                    if (ManeuverabilityRating != FlightManeuverability.Perfect)
                    {
                        expected *= 2;
                    }
                }

                // cost for some turns based on maneuverability rating
                var _moveBudget = GetMovementBudget(activity);
                var _turnHeading = (8 + _avTarget.GetHeading(_gravity, _idx.Value) - _moveBudget.Heading) % 8;
                switch (ManeuverabilityRating)
                {
                    case FlightManeuverability.Good:
                        // greater than 90 degree turn, extra 5 feet
                        if ((_turnHeading > 2) && (_turnHeading < 6))
                        {
                            expected += 1;
                        }

                        break;

                    case FlightManeuverability.Average:
                        // greater than 45 degree turn, extra 5 feet
                        if ((_turnHeading > 1) && (_turnHeading < 7))
                        {
                            expected += 1;
                        }

                        break;
                }

                activity.AppendCompletion(new FlightBudgetUpdateStep(activity));
            }
            return base.BaseMoveCost(activity, expected);
        }
        #endregion

        #region protected override MovementLocatorTarget GetNextGeometry(...)
        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
            var _dest = process.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = process.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
            if ((locator != null) && (leadCell != null) && (_dest != null) && (_idx != null))
            {
                var _locCore = locator.ICore as ICoreObject;
                var _gravity = locator.GetGravityFace();
                var _planar = locator.PlanarPresence;

                // limit or adjust direction if necessary
                var _udAdjust = _dest.GetUpDownAdjustment(_gravity, _idx.Value);
                var _flightBudget = GetFlightBudget(process as CoreActivity, _udAdjust);
                var _moveBudget = GetMovementBudget(process as CoreActivity);
                var _heading = _dest.GetHeading(_gravity, _idx.Value);
                var _flightHeading = _moveBudget.Heading ?? _heading;

                // NOTE: if target heading wasn't set, this sets turn heading to 0 (no deviation)
                var _fixup = false;
                var _moveHeading = _heading;
                var _turnHeading = (8 + _moveHeading - _flightHeading) % 8;

                // NOTE: neither perfect nor good has any hard limits (but good has some additional costs)
                switch (ManeuverabilityRating)
                {
                    case FlightManeuverability.Average:
                        #region average fixups
                        // cannot ascend after descent unless travelled 5 feet level
                        if ((_udAdjust > UpDownAdjustment.Level) && (_flightBudget.DistanceSinceDown < 5))
                        {
                            _udAdjust = UpDownAdjustment.Level;
                            _fixup = true;
                        }

                        // already used one direct climb, must use a forward motion one next
                        if ((_flightBudget.UpwardsCrossings > 0)
                            && (_udAdjust > UpDownAdjustment.Upward))
                        {
                            // fixup with up/down adjust
                            _udAdjust = UpDownAdjustment.Upward;
                            _fixup = true;
                        }

                        #region adjust turning
                        // adjust turning
                        switch (_turnHeading)
                        {
                            case 4:
                                // cannot reverse course, so maintain current
                                _moveHeading = _flightHeading;
                                _fixup = true;
                                break;

                            case 3:
                                // maximum turn is 90 degrees (with extra budget) (±2)
                                _moveHeading = (_flightHeading + 2) % 8;
                                _fixup = true;
                                break;

                            case 5:
                                // maximum turn is 90 degrees (with extra budget) (±2)
                                _moveHeading = (_flightHeading + 6) % 8;
                                _fixup = true;
                                break;
                        }
                        #endregion
                        #endregion
                        break;

                    case FlightManeuverability.Poor:
                        #region poor fixups
                        // cannot ascend after descent unless travelled 10 feet level
                        if ((_udAdjust > UpDownAdjustment.Level) && (_flightBudget.DistanceSinceDown < 10))
                        {
                            _udAdjust = UpDownAdjustment.Level;
                            _fixup = true;
                        }

                        #region adjust up/down
                        // cannot go straight up or down
                        if (_udAdjust > UpDownAdjustment.Upward)
                        {
                            // fixup with up/down adjust
                            _udAdjust = UpDownAdjustment.Upward;
                            _fixup = true;
                        }
                        else if (_udAdjust < UpDownAdjustment.Downward)
                        {
                            // fixup with up/down adjust
                            _udAdjust = UpDownAdjustment.Downward;
                            _fixup = true;
                        }
                        #endregion

                        #region adjust turning
                        switch (_turnHeading)
                        {
                            case 4:
                                // cannot reverse course, so maintain current
                                _moveHeading = _flightHeading;
                                _fixup = true;
                                break;

                            case 2:
                            case 3:
                                // maximum turn is 45 degrees (±1)
                                _moveHeading = (_flightHeading + 1) % 8;
                                _fixup = true;
                                break;

                            case 5:
                            case 6:
                                // maximum turn is 45 degrees (±1)
                                _moveHeading = (_flightHeading + 7) % 8;
                                _fixup = true;
                                break;
                        }
                        #endregion
                        #endregion
                        break;

                    case FlightManeuverability.Clumsy:
                        #region clumsy fixups
                        // cannot ascend after descent unless travelled 20 feet level
                        if ((_udAdjust > UpDownAdjustment.Level) && (_flightBudget.DistanceSinceDown < 20))
                        {
                            _udAdjust = UpDownAdjustment.Level;
                            _fixup = true;
                        }

                        #region adjust up/down
                        // cannot go straight up or down
                        if (_udAdjust > UpDownAdjustment.Upward)
                        {
                            // fixup with up/down adjust
                            _udAdjust = UpDownAdjustment.Upward;
                            _fixup = true;
                        }
                        else if (_udAdjust < UpDownAdjustment.Downward)
                        {
                            // fixup with up/down adjust
                            _udAdjust = UpDownAdjustment.Downward;
                            _fixup = true;
                        }
                        #endregion

                        #region adjust turning
                        // must have travelled 5 feet since last turn
                        if (_flightBudget.DistanceSinceTurn >= 5)
                        {
                            switch (_turnHeading)
                            {
                                case 4:
                                    // cannot reverse course, so maintain current heading
                                    _moveHeading = _flightHeading;
                                    _fixup = true;
                                    break;

                                case 2:
                                case 3:
                                    // maximum turn is 45 degrees (±1)
                                    _moveHeading = (_flightHeading + 1) % 8;
                                    _fixup = true;
                                    break;

                                case 5:
                                case 6:
                                    // maximum turn is 45 degrees (±1)
                                    _moveHeading = (_flightHeading + 7) % 8;
                                    _fixup = true;
                                    break;
                            }
                        }
                        else
                        {
                            // must continue on last heading
                            _moveHeading = _flightHeading;
                            _fixup = true;
                        }
                        #endregion
                        #endregion
                        break;
                }

                if (_fixup)
                {
                    // fixup with heading and updown that were calculated to be required
                    _dest = new HeadingTarget(_dest.Key, _moveHeading, (int)_udAdjust);
                    process.SetFirstTarget(_dest);
                }
                var _crossings = _dest.CrossingFaces(_gravity, _idx.Value);

                // calculate next lead cell and location
                var _next = leadCell.Move(_crossings.GetAnchorOffset()) as CellLocation;

                // see if can occupy
                if (!locator.Map.CanOccupy(_locCore, _next, this, exclusions, _planar))
                {
                    return null;
                }

                // see if can transit
                if (!TransitFitness(process, _crossings, leadCell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? false)
                {
                    return null;
                }

                if (locator.NormalSize.XLength == 1)
                {
                    // provide a cell list
                    var _cubic = new Cubic(_next, 1, 1, 1);
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d);
                    if ((ManeuverabilityRating != FlightManeuverability.Perfect) && locator.SqueezesIntoGeometry(_final))
                    {
                        return null;
                    }
                    return new MovementLocatorTarget
                    {
                        Locator = locator,
                        TargetRegion = _final,
                        BaseFace = _gravity
                    };
                }
                else
                {
                    // cubic first
                    var _offset = locator.GeometricRegion.GetExitCubicOffset(leadCell, locator.NormalSize);
                    var _cubeStart = _next.Add(_offset);
                    var _cubic = new Cubic(_cubeStart, locator.NormalSize);

                    // finalize
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d);
                    if ((ManeuverabilityRating != FlightManeuverability.Perfect) && locator.SqueezesIntoGeometry(_final))
                    {
                        return null;
                    }
                    return new MovementLocatorTarget
                    {
                        Locator = locator,
                        TargetRegion = _final,
                        BaseFace = _gravity
                    };
                }
            }
            return null;
        }
        #endregion

        /// <summary>Creature has a flight mode that can hover freely</summary>
        public static FlightSuMovement FirstRecoverableFlight(Creature creature)
            => (creature?.Movements.AllMovements.OfType<FlightSuMovement>()
                .FirstOrDefault(_m => _m.ManeuverabilityRating <= FlightManeuverability.Good && _m.IsUsable));

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

                    // TODO: always useable (unless unconscious)
                    return true;
                }
                return false;
            }
        }

        public override bool CanMoveThrough(Items.Materials.Material material)
            => false;

        public override bool CanMoveThrough(CellMaterial material)
            => (material is GasCellMaterial) || (material is VoidCellMaterial);

        #region public override void OnRelocated(CoreProcess process, Locator locator)
        public override void OnRelocated(CoreProcess process, Locator locator)
        {
            // ensure an inflight adjunct is adjuncted
            var _locCore = locator.ICore as ICoreObject;
            var _inFlight = _locCore.Adjuncts.OfType<InFlight>().FirstOrDefault();
            if (_inFlight == null)
            {
                _inFlight = new InFlight();
                _locCore.AddAdjunct(_inFlight);
            }

            // set perched flag
            var _grav = locator.GetGravityFace();
            _inFlight.IsPerched = locator.IsGravityEffective
                && locator.Map.DirectionalBlockage(_locCore, locator.GeometricRegion, this, _grav, _grav, null, locator.PlanarPresence) > 0;

            // set next time for in-flight check...
            _inFlight.NextTime = (_locCore.Setting as ITacticalMap).CurrentTime + Round.UnitFactor;

            // base stuff
            base.OnRelocated(process, locator);
        }
        #endregion

        #region public override void OnEndActiveMovement()
        public override void OnEndActiveMovement()
        {
            // NOTE: when switching from one fly mode to another
            // 1) RelocationStep sets ActiveMovement on Locator
            // 2) Locator calls OnEndActiveMovement
            // 3) RelocationStep calls OnRelocated on the movement
            // therefore: InFlight will be put back-on even though it is taken off here
            var _inFlight = CoreObject.Adjuncts.OfType<InFlight>().FirstOrDefault();
            if (_inFlight != null)
            {
                _inFlight.Eject();
            }

            base.OnEndActiveMovement();
        }
        #endregion

        #region public override Info GetProviderInfo(CoreActionBudget budget)
        public override Info GetProviderInfo(CoreActionBudget budget)
        {
            var _info = ToMovementInfo<FlightMovementInfo>();
            _info.CanHover = false;
            _info.FreeYaw = 1;
            _info.MaxYaw = 1;
            _info.YawGap = 0;
            _info.MinSpeed = BaseDoubleValue / 2;

            switch (ManeuverabilityRating)
            {
                case FlightManeuverability.Perfect:
                case FlightManeuverability.Good:
                    _info.CanHover = true;
                    _info.MinSpeed = 0;
                    _info.FreeYaw = (ManeuverabilityRating == FlightManeuverability.Perfect ? 4 : 2);
                    _info.MaxYaw = 4;
                    _info.MaxUpAngle = 90;
                    _info.MaxDownAngle = 90;
                    break;

                case FlightManeuverability.Average:
                    _info.MaxYaw = 2;
                    _info.MaxUpAngle = 60;
                    _info.MaxDownAngle = 90;
                    _info.DownUpGap = 5;
                    break;

                case FlightManeuverability.Poor:
                    _info.MaxUpAngle = 45;
                    _info.MaxDownAngle = 45;
                    _info.DownUpGap = 10;
                    break;

                case FlightManeuverability.Clumsy:
                    _info.YawGap = 5;
                    _info.MaxUpAngle = 45;
                    _info.MaxDownAngle = 45;
                    _info.DownUpGap = 20;
                    break;
            }
            return _info;
        }
        #endregion

        public override MovementBase Clone(Creature forCreature, object source)
            => new FlightSuMovement(BaseValue, forCreature, source, ManeuverabilityRating, _ArmorEncumberance, _HeavyAsMedium);
    }
}
