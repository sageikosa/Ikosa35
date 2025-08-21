using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Skills;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class LandMovement : MovementBase
    {
        public LandMovement(int speed, Creature creature, object source)
            : base(speed, creature, source)
        {
        }

        public LandMovement(CoreObject coreObject, object source)
            : base(0, coreObject, source)
        {
        }

        public override string Name => @"Overland";
        public override bool CanShiftPosition => true;
        public override bool SurfacePressure => true;

        public override bool IsNativeMovement
            => (Source == CoreObject) || ((CoreObject as Creature)?.Body == Source);

        /// <summary>Dwarven speed flag</summary>
        public bool NoEncumberancePenalty = false;

        // TODO: barbarian --> +10' with light/medium or load/armor adds to base

        #region public override bool IsUsable { get; }
        public override bool IsUsable
        {
            get
            {
                var _locator = CoreObject.GetLocated()?.Locator;
                if (_locator != null)
                {
                    if (_locator.PlanarPresence == PlanarPresence.Ethereal)
                    {
                        return false;
                    }

                    var _grav = _locator.GetGravityFace();
                    return _locator.Map.DirectionalBlockage(
                        CoreObject, _locator.GeometricRegion, this, _grav, _grav, null, _locator.PlanarPresence) > 0;
                }
                return false;
            }
        }
        #endregion

        #region public override CoreStep CostFactorStep(CoreActivity activity)
        public override CoreStep CostFactorStep(CoreActivity activity)
        {
            var _allLocators = activity.Targets.OfType<ValueTarget<MovementLocatorTarget>>()
                .Select(_vt => _vt.Value)
                .ToList();
            if (_allLocators.Any())
            {
                var _allICore = _allLocators
                    .Select(_mlt => _mlt?.Locator.ICore)
                    .Where(_i => _i != null)
                    .ToDictionary(_i => _i.ID);

                // average support for all locators
                var _support = _allLocators
                    .Sum(_al =>
                    {
                        var _grav = _al.Locator.GetGravityFace();
                        return _al.Locator.Map.DirectionalBlockage(
                            _al.Locator.ICore as ICoreObject, _al.TargetRegion, this, _grav, _grav, _allICore,
                            _al.Locator.PlanarPresence);
                    }) / _allLocators.Count;

                if (_support > 0)
                {
                    int? _getCurrent()
                        => (CoreObject as Creature)?.Skills.Skill<BalanceSkill>().CurrentBalance;

                    // narrow ledge?
                    if (_support <= 0.2)
                    {
                        #region narrow difficulty
                        // MUST balance
                        var _difficulty = new Deltable(0);
                        if (_support < 0.033333) // about 2 inches of 5 feet
                        {
                            _difficulty.BaseValue = 20;
                        }
                        else if (_support < 0.116667) // about 7 inches of 5 feet
                        {
                            _difficulty.BaseValue = 15;
                        }
                        else if (_support <= 0.2) // about 1 of 5 feet
                        {
                            _difficulty.BaseValue = 10;
                        }
                        #endregion

                        #region other balance factors
                        // add all distinct non-zero balance adjustments
                        foreach (var _landProp in (from _al in _allLocators
                                                   from _mz in _al.Locator.MapContext.MovementZones
                                                   where _mz.AffectsMovement<LandMovement>()
                                                   && _mz.ContainsGeometricRegion(_al.TargetRegion, _al.Locator.ICore as ICoreObject, _al.Locator.PlanarPresence)
                                                   select _mz)
                                                   .OfType<LandMovementProperties>()
                                                   .Where(_lp => _lp.Balance.Value != 0)
                                                   .Distinct())
                        {
                            _difficulty.Deltas.Add(_landProp.Balance);
                        }
                        // TODO: sloped: +2
                        #endregion

                        // balancing already, or force a check
                        var _current = _getCurrent();
                        if ((_current == null) || _current < _difficulty.EffectiveValue)
                        {
                            // TODO: if only a single creature locator this is fine
                            // ... otherwise we need a balance check per locator with a creature
                            return new BalanceCheckStep(activity, CoreObject as Creature, _difficulty, false);
                        }
                    }
                    else if ((activity.Action is StartCharge)
                        || (activity.Action is StartRun)
                        || (activity.Action is ContinueLinearMove))
                    {
                        // run or charge through difficult terrain
                        var _zoneProps = (from _al in _allLocators
                                          from _mz in _al.Locator.MapContext.MovementZones
                                          where _mz.AffectsMovement<LandMovement>()
                                          && _mz.ContainsGeometricRegion(_al.TargetRegion, _al.Locator.ICore as ICoreObject, _al.Locator.PlanarPresence)
                                          select _mz)
                                          .OfType<LandMovementProperties>()
                                          .Distinct()
                                          .ToList();
                        if (_zoneProps.Any(_lp => _lp.MustCheckToRunCharge))
                        {
                            // add all non-zero balance adjustments
                            var _difficulty = new Deltable(10);
                            foreach (var _landProp in _zoneProps.Where(_lp => _lp.Balance.Value != 0))
                            {
                                _difficulty.Deltas.Add(_landProp.Balance);
                            }
                            // TODO: sloped: +2

                            // balancing already, or force a check
                            var _current = _getCurrent();
                            if ((_current == null) || (_current < _difficulty.EffectiveValue))
                            {
                                // TODO: if only a single creature locator this is fine
                                // ... otherwise we need a balance check per locator with a creature
                                return new BalanceCheckStep(activity, CoreObject as Creature, _difficulty, true);
                            }
                        }
                    }
                }
            }

            // no need to balance
            var _balancing = CoreObject.Adjuncts.OfType<Balancing>().FirstOrDefault();
            if (_balancing != null)
            {
                CoreObject.RemoveAdjunct(_balancing);
            }

            // should be MoveCostCheckStep
            return base.CostFactorStep(activity);
        }
        #endregion

        #region protected override double OnMoveCostFactor(CoreActivity activity)
        protected override double OnMoveCostFactor(CoreActivity activity)
        {
            // balancing and not accelerated?
            var _factor = base.OnMoveCostFactor(activity);
            if (!((CoreObject as Creature)?.Skills.Skill<BalanceSkill>().IsSpeedNormal ?? true))
            {
                _factor *= 2;
            }

            if (activity.Action is HopUp)
            {
                _factor *= 2;
            }
            // TODO: tumble factor
            return _factor;
        }
        #endregion

        #region public override void OnEndTurn()
        public override void OnEndTurn()
        {
            // expire balancing for movement
            foreach (var _balancing in CoreObject.Adjuncts.OfType<Balancing>())
            {
                _balancing.IsCheckExpired = true;
            }
            base.OnEndTurn();
        }
        #endregion

        #region public override void OnResetBudget()
        public override void OnResetBudget()
        {
            // expire balancing for movement
            foreach (var _balancing in CoreObject.Adjuncts.OfType<Balancing>())
            {
                _balancing.IsCheckExpired = true;
            }
            base.OnResetBudget();
        }
        #endregion

        #region public override void OnEndActiveMovement()
        public override void OnEndActiveMovement()
        {
            // clean-up balancing
            foreach (var _balancing in CoreObject.Adjuncts.OfType<Balancing>().ToList())
            {
                _balancing.Eject();
            }
            base.OnEndActiveMovement();
        }
        #endregion

        #region public override void OnSecondIncrementOfTotal(MovementAction action)
        public override void OnSecondIncrementOfTotal(MovementAction action)
        {
            if ((action is ContinueMove) || (action is ContinueLinearMove))
            {
                // force a new balance check if we continue to move after this
                foreach (var _balancing in CoreObject.Adjuncts.OfType<Balancing>())
                {
                    _balancing.IsCheckExpired = true;
                }
            }
            base.OnSecondIncrementOfTotal(action);
        }
        #endregion

        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
            var _dest = process.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = process.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);

            if ((locator != null) && (leadCell != null) && (_dest != null) && (process is CoreActivity _activity))
            {
                var _locCore = locator.ICore as ICoreObject;
                Action _onSuccess = () => { };
                var _gravity = locator.GetGravityFace();
                var _crossings = _dest.CrossingFaces(_gravity, _idx?.Value ?? 0);
                var _revGrav = _gravity.ReverseFace();
                var _planar = locator.PlanarPresence;

                // -------------------- vertical change intent and capacity --------------------
                // deliberately ascend/descend to indicate intention to step up/down (cost for failure?)
                var _rising = false;
                var _dropping = false;
                var _hopUp = (_activity.Action is HopUp);
                var _tryRise = _hopUp || _crossings.Contains(_revGrav);
                var _tryDrop = (_activity.Action is JumpDown) || _crossings.Contains(_gravity);
                if (_tryRise || _tryDrop)
                {
                    // strip out crossings that signal rise/drop intent
                    _crossings = _crossings.Where(_f => (_f != _revGrav) && (_f != _gravity)).ToArray();
                }
                if (!_crossings.Any())
                {
                    // cannot simply climb or drop via land movement
                    return null;
                }

                // get maximum step size and elevation of cell (must use climb or jump to exceed the max step up)
                // TODO: make step thresholds body-dependent
                var _maxUp = ((_activity.Actor as Creature).Body.Height / (_hopUp ? 2 : 3));
                var _maxStepDown = ((_activity.Actor as Creature).Body.Height / 2);

                // -------------------- elevation cell --------------------
                // base elevation of locator
                var _elev = locator.Elevation;

                // lead cell elevation
                var _fromCell = leadCell;
                var _fromElev = _fromCell.CellElevation(_gravity);

                // if lead is higher than locator, then holding cell is underneath
                var _holdCell = (_fromElev > _elev)
                    ? _fromCell.Move(_gravity.GetAnchorOffset()) as CellLocation
                    : _fromCell;

                // -------------------- intended target of lead cell --------------------
                // next from lead cell
                var _next = _fromCell.Move(_crossings.GetAnchorOffset()) as CellLocation;

                // result variables
                CellList _final = null;
                var _offset = new Vector3D();

                // validate transit fitness
                bool _canTransit(CellLocation cell, params AnchorFace[] faces)
                    => (TransitFitness(process, faces, cell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? true);

                #region (SlopeSegment slope, double elev) _bestStep(List<SlopeSegment> slopes)
                // find best step...
                (SlopeSegment slope, double elev) _bestStep(List<SlopeSegment> slopes)
                {
                    // more lenient heuristic?
                    var _max = slopes.Max(_s => _s.Run);
                    var _median = (from _slope in slopes
                                   where _slope.Run == _max
                                   select _slope.Middle()).Median();
                    return slopes
                        .Where(_s => _s.Run == _max && _s.Middle() == _median)
                        .Select(_s => (_s, _median))
                        .First();
                };
                #endregion

                #region (SlopeSegment slope, double elev) _bestEndStep(List<SlopeSegment> slopes)
                (SlopeSegment slope, double elev) _bestEndStep(List<SlopeSegment> slopes)
                {
                    if (_tryRise)
                    {
                        var _candidates = slopes.Where(_s => _s.Low > _elev || _s.High > _elev).ToList();
                        if (_candidates.Any())
                        {
                            // more lenient heuristic?
                            return _bestStep(_candidates);
                        }
                    }
                    else if (_tryDrop)
                    {
                        var _candidates = slopes.Where(_s => _s.Low < _elev || _s.High < _elev).ToList();
                        if (_candidates.Any())
                        {
                            return _bestStep(_candidates);
                        }
                    }
                    var _big = slopes.Where(_s => _s.Run >= 1).ToList();
                    if (_big.Count == 2)
                    {
                        // choose from best two
                        return (from _s in _big
                                orderby Math.Min(Math.Abs(_elev - _s.Low), Math.Min(Math.Abs(_elev - _s.High), Math.Abs(_elev - _s.Middle())))
                                select (_s, _s.Middle())).First();
                    }
                    return _bestStep(slopes);
                }
                #endregion

                #region void _saveToCancelFall()
                void _saveToCancelFall()
                {
                    if (CoreObject is Creature _critter)
                    {
                        var _delta = new DeltaCalcInfo(_critter.ID, @"Save to Cancel Fall") { Result = 20, BaseValue = 20 };
                        process.AppendPreEmption(
                            new SaveToCancel(process, _critter, Interactions.SaveType.Reflex, _delta));
                    }
                    // NOTE: normal relocation will happen in-between
                    process.AppendCompletion(new FallingStartStep(process, locator, 500, 0, 0));
                }
                #endregion

                if (locator.NormalSize.XLength == 1)
                {
                    // get maximum step size and elevation of holding cell (must use climb or jump to exceed the max step up)
                    var _startSlopes = locator.Map.CellSlopes(_locCore, _holdCell, this, _gravity, exclusions, _planar)
                        .Where(_s => _s.Incline() <= 30)
                        .ToList();

                    #region validate/rectify occupancy of next cell or return null
                    bool _canOccupy()
                        => locator.Map.CanOccupy(_locCore, _next, this, exclusions, _planar);
                    bool _canMove()
                        => _canOccupy() && _canTransit(_fromCell, _crossings);
                    // FURNISH: (1) transit fitness rejected direct move into, so we tried elevation

                    if (!_canMove())
                    {
                        if (_fromCell != _holdCell)
                        {
                            // already trying from an elevated cell, no going up any more
                            return null;
                        }

                        // make sure we can enter "above" lead cell
                        if (!_canTransit(_fromCell, _revGrav))
                        {
                            return null;
                        }

                        // sufficient space to step up out of the cell (does not guarantee success)
                        // ... so set new lead cell, and new next cell
                        _fromCell = _fromCell.Move(_revGrav.GetAnchorOffset()) as CellLocation;
                        _next = _fromCell.Move(_crossings.GetAnchorOffset()) as CellLocation;
                        _rising = true;
                        if (!_canMove())
                        {
                            // cannot move from up cell to up-step cell
                            return null;
                        }
                    }
                    #endregion

                    // NOTE: with really small support levels, balance checks are needed?!?
                    #region look for a suitable cell to land on
                    var _endSlopes = _next.CellElevation(_gravity)
                        .ToEnumerable()
                        .Select(_e => new SlopeSegment { Low = _e, High = _e, Run = 5 }).ToList();

                    // rising needs to be confirmed that actor can reach the base of the above cell via stepping
                    if (_rising)
                    {
                        var _highest = _elev;
                        foreach (var _upSlope in _startSlopes.Where(_s => _s.Low > _elev || _s.High > _elev)
                                                    .Union(_endSlopes)
                                                    .OrderBy(_s => _s.Low).ThenBy(_s => _s.High)
                                                    .ToList())
                        {
                            if (_upSlope.Low > _highest)
                            {
                                if ((_upSlope.Low - _highest) > _maxUp)
                                {
                                    // reached as high as we can without exhausting the list
                                    return null;
                                }
                                _highest = _upSlope.Low;
                            }
                            if (_upSlope.High > _highest)
                            {
                                _highest = _upSlope.High;
                            }
                        }
                        // NOTE: passed if we haven't exited yet...
                    }

                    if (locator.Map.DirectionalBlockage(_locCore, _next, this, _gravity, _gravity, exclusions, _planar) > 0.2)
                    {
                        // target cell was suitable
                        _endSlopes = locator.Map.CellSlopes(_locCore, _next, this, _gravity, exclusions, _planar).ToList();
                    }
                    else
                    {
                        // FURNISH: (2) we dropped down due to lack of direct support
                        _dropping = true;
                        var _down = _next.Move(_gravity.GetAnchorOffset()) as CellLocation;
                        // TODO: some of the below coverage may be inaccessible due to above cell stacking...
                        if (locator.Map.DirectionalBlockage(_locCore, _down, this, _gravity, _gravity, exclusions, _planar) > 0)
                        {
                            // FURNISH: (3) this now includes the cell base due to coverages

                            // cell "underneath" appears suitable, can we step into it
                            _endSlopes = locator.Map.CellSlopes(_locCore, _down, this, _gravity, exclusions, _planar).ToList();
                            _next = _down;
                        }
                        else
                        {
                            // was holding cell under the lead cell?
                            if (_fromElev > _elev)
                            {
                                // get to try going down one more time...
                                _down = _down.Move(_gravity.GetAnchorOffset()) as CellLocation;
                                if (locator.Map.DirectionalBlockage(_locCore, _down, this, _gravity, _gravity, exclusions, _planar) > 0)
                                {
                                    // FURNISH: (3) this now includes the cell base due to coverages

                                    // cell "underneath" appears suitable, can we step into it
                                    _endSlopes = locator.Map.CellSlopes(_locCore, _down, this, _gravity, exclusions, _planar).ToList();
                                    _next = _down;
                                }
                                else
                                {
                                    // if we get to the end, run this before returning
                                    _onSuccess = () => _saveToCancelFall();
                                }
                            }
                            else
                            {
                                // if we get to the end, run this before returning
                                _onSuccess = () => _saveToCancelFall();
                            }
                        }
                    }
                    #endregion

                    #region elevation changes
                    var _baseElev = _next.CellElevation(_gravity);
                    _offset = new Vector3D();
                    if (_endSlopes.Any())
                    {
                        var _endStep = _bestEndStep(_endSlopes);
                        if (_startSlopes.Any())
                        {
                            // FURNISH: (4) now we're stepping directly across
                            var (_bestSlope, _bestElev) = _bestStep(_startSlopes);
                            if (_bestElev < _endStep.elev)
                            {
                                // stepping up
                                if ((_endStep.elev - _bestElev) >= 4.5)
                                {
                                    // steep slope up
                                    process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                }

                                // TODO: this doesn't account for _startSlopes having gaps that are too big to cover
                                // NOTE: uprising steps
                                if ((_endSlopes.Min(_e => _e.Low) - _startSlopes.Max(_s => _s.High)) > _maxUp)
                                {
                                    // too high to step or hop-up
                                    return null;
                                }
                            }
                            else if (_bestElev > _endStep.elev)
                            {
                                // stepping down
                                // TODO: this doesn't account for _startSlopes having gaps that are too big to cover
                                // NOTE: down moving steps
                                if ((_startSlopes.Min(_s => _s.Low) - _endSlopes.Max(_e => _e.High)) > _maxStepDown)
                                {
                                    // will cause a small fall
                                    process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                }
                            }
                            else if (_rising)
                            {
                                // FURNISH: (5) theoretically this should be tripped

                                // started rising, but finished not stepping-up; must've gone over some barrier
                                process.SetFirstTarget(new ValueTarget<double>(@"Cost.Obstacle", 2));
                            }
                        }

                        // anything contributing to end slopes can be ignored for squeeze offsets
                        if ((_endStep.slope.Source != null) && !exclusions.ContainsKey(_endStep.slope.Source.ID))
                        {
                            exclusions.Add(_endStep.slope.Source.ID, _endStep.slope.Source);
                        }

                        #region offset
                        var (_endSlope, _endElev) = _endStep; // TODO: improve
                        switch (_gravity)
                        {
                            case AnchorFace.ZLow:
                                _offset = new Vector3D(0, 0, _endElev - _baseElev);
                                break;
                            case AnchorFace.ZHigh:
                                _offset = new Vector3D(0, 0, _baseElev - _endElev);
                                break;
                            case AnchorFace.YLow:
                                _offset = new Vector3D(0, _endElev - _baseElev, 0);
                                break;
                            case AnchorFace.YHigh:
                                _offset = new Vector3D(0, _baseElev - _endElev, 0);
                                break;
                            case AnchorFace.XLow:
                                _offset = new Vector3D(_endElev - _baseElev, 0, 0);
                                break;
                            default:
                                _offset = new Vector3D(_baseElev - _endElev, 0, 0);
                                break;
                        }
                        #endregion
                    }
                    #endregion

                    var _cubic = new Cubic(_next, 1, 1, 1);

                    #region extension?
                    // must extend?
                    if ((_offset.Length + locator.ICoreAs<ICorePhysical>().Max(_icp => _icp.Height)) > 5) // counts up to 5'
                    {
                        // extend the cubic
                        _cubic = _next.ContainingCube(_next.Move(_revGrav.GetAnchorOffset()) as CellLocation);
                    }
                    #endregion

                    // provide a cell list
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d, _gravity.GetAxis());
                }
                else
                {
                    // TODO: start elevations "map" for each "column" in current locator?
                    var _startSlopes = locator.Map.CellSlopes(_locCore, _holdCell, this, _gravity, exclusions, _planar).ToList();

                    // cubic first
                    var _startOffset = locator.GeometricRegion.GetExitCubicOffset(_fromCell, locator.NormalSize);
                    Cubic _getCubic(CellLocation startCell)
                    {
                        // might be called twice
                        var _cubeStart = startCell.Add(_startOffset);
                        return new Cubic(_cubeStart, locator.NormalSize);
                    };
                    var _cubic = _getCubic(_next);

                    #region validate/rectify occupancy of next cell or return null
                    bool _canOccupy()
                       => locator.Map.CanOccupy(_locCore, _cubic, this, exclusions, _planar);
                    bool _canMove()
                        => _canOccupy() && _canTransit(_fromCell, _crossings);

                    if (!_canMove())
                    {
                        if (_fromCell != _holdCell)
                        {
                            // already trying from an elevated cell, no going up any more
                            return null;
                        }

                        // make sure we can enter "above" lead cell
                        if (!_canTransit(_fromCell, _revGrav))
                        {
                            return null;
                        }

                        // sufficient space to step up out of the cell (does not guarantee success)
                        // ... so set new lead cell, and new next cell
                        _fromCell = _fromCell.Move(_revGrav.GetAnchorOffset()) as CellLocation;
                        _next = _fromCell.Move(_crossings.GetAnchorOffset()) as CellLocation;

                        _cubic = _getCubic(_next);
                        _rising = true;
                        if (!_canMove())
                        {
                            // cannot move into the up-step cell
                            return null;
                        }
                    }
                    #endregion

                    // NOTE: with really small support levels, balance checks are needed?!?
                    #region look for a suitable cell to land on
                    var _endSlopes = _next.CellElevation(_gravity)
                        .ToEnumerable()
                        .Select(_e => new SlopeSegment { Low = _e, High = _e, Run = 5 })
                        .ToList();

                    // TODO: if body is large enough, may be able to "pass through" multiple cells (not just immediately under)
                    if (locator.Map.DirectionalBlockage(_locCore, _cubic, this, _gravity, _gravity, exclusions, _planar) > 0)
                    {
                        // target cell was suitable
                        _endSlopes = locator.Map.CellSlopes(_locCore, _next, this, _gravity, exclusions, _planar).ToList();
                    }
                    else
                    {
                        _dropping = true;
                        var _down = _next.Move(_gravity.GetAnchorOffset()) as CellLocation;
                        // TODO: some of the below coverage may be inaccessible due to above cell stacking...
                        if (locator.Map.DirectionalBlockage(_locCore, _down, this, _gravity, _gravity, exclusions, _planar) > 0)
                        {
                            // cell "underneath" appears suitable, can we step into it
                            _endSlopes = locator.Map.CellSlopes(_locCore, _down, this, _gravity, exclusions, _planar).ToList();
                            _next = _down;
                            _cubic = _getCubic(_down);
                        }
                        else
                        {
                            // if we get to the end, run this before returning
                            _onSuccess = () => _saveToCancelFall();
                        }
                    }
                    #endregion

                    #region elevation changes
                    var _baseElev = _next.CellElevation(_gravity);
                    _offset = new Vector3D();
                    if (_endSlopes.Any())
                    {
                        var _endStep = _bestEndStep(_endSlopes);
                        if (_startSlopes.Any())
                        {
                            var (_bestSlope, _bestElev) = _bestStep(_startSlopes);
                            if (_bestElev < _endStep.elev)
                            {
                                // stepping up
                                if ((_endStep.elev - _bestElev) >= 4.5)
                                {
                                    // steep slope up
                                    process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                }
                                if ((_endSlopes.Min(_e => _e.Low) - _startSlopes.Max(_s => _s.High)) > _maxUp)
                                {
                                    // too high to step (must climb or jump)
                                    return null;
                                }
                            }
                            else if (_bestElev > _endStep.elev)
                            {
                                // stepping down
                                if ((_startSlopes.Min(_s => _s.Low) - _endSlopes.Max(_e => _e.High)) > _maxStepDown)
                                {
                                    // will cause a small fall
                                    process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                }
                            }
                            else if (_rising)
                            {
                                // started rising, but finished not stepping-up; must've gone over some barrier
                                process.SetFirstTarget(new ValueTarget<double>(@"Cost.Obstacle", 2));
                            }
                        }

                        // anything contributing to end slopes can be ignored for squeeze offsets
                        if ((_endStep.slope.Source != null) && !exclusions.ContainsKey(_endStep.slope.Source.ID))
                        {
                            exclusions.Add(_endStep.slope.Source.ID, _endStep.slope.Source);
                        }

                        #region offset
                        var (_endSlope, _endElev) = _endStep; // TODO: improve
                        switch (_gravity)
                        {
                            case AnchorFace.ZLow:
                                _offset = new Vector3D(0, 0, _endElev - _baseElev);
                                break;
                            case AnchorFace.ZHigh:
                                _offset = new Vector3D(0, 0, _baseElev - _endElev);
                                break;
                            case AnchorFace.YLow:
                                _offset = new Vector3D(0, _endElev - _baseElev, 0);
                                break;
                            case AnchorFace.YHigh:
                                _offset = new Vector3D(0, _baseElev - _endElev, 0);
                                break;
                            case AnchorFace.XLow:
                                _offset = new Vector3D(_endElev - _baseElev, 0, 0);
                                break;
                            default:
                                _offset = new Vector3D(_baseElev - _endElev, 0, 0);
                                break;
                        }
                        #endregion
                    }
                    #endregion

                    #region extension?
                    // must extend?
                    // TODO: bad extension for large locator!!!!!!!!!!!!!!!!!!! (will never fit!)
                    //var _space = _loc.Map[_next];
                    //if (!_space.ValidSpace(this)) // counts up to 5'
                    //{
                    //    // NOTE: this doesn't account for 6'+ tall medium creature
                    //    var _open = _space.OpensTowards(this).Where(_o => _o.Face == _revGrav).ToList();
                    //    if (_open.Count > 0)
                    //    {
                    //        if (_open.Average(_o => _o.Amount) < _loc.AllOf<ICorePhysical>().Max(_icp => _icp.Height))
                    //        {
                    //            // extend the cubic
                    //            _cubic = _next.ContainingCube(_next.Move(_revGrav.GetAnchorOffset()) as CellLocation);
                    //        }
                    //    }
                    //}
                    #endregion

                    // provide a cell list
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d, _gravity.GetAxis());
                }

                #region check and apply final fall status
                // deflection may have put us in a spot without support...
                if (_final != null)
                {
                    if (locator.Map.DirectionalBlockage(_locCore, _final, this, _gravity, _gravity, exclusions, _planar) <= 0.2)
                    {
                        // if not already dropping, check under cell support to see if we promote to dropping
                        if (!_dropping)
                        {
                            var _down = _final.Move(_gravity.GetAnchorOffset());
                            if (locator.Map.DirectionalBlockage(_locCore, _down, this, _gravity, _gravity, exclusions, _planar) > 0)
                            {
                                if (locator.IsGravityEffective)
                                {
                                    FallingStartStep.StartFall(locator, 500, 500, @"Down step sideways", 1);
                                }
                            }
                            else
                            {
                                // no support in under cell is a drop
                                _dropping = true;
                            }
                        }

                        // truly dropping without support, save to cancel
                        if (_dropping)
                        {
                            // if we get to the end, run this before returning
                            _onSuccess = () => _saveToCancelFall();
                        }
                    }

                    // most likely save to cancel
                    _onSuccess();
                }
                #endregion

                return new MovementLocatorTarget
                {
                    Locator = locator,
                    Offset = _offset,
                    TargetRegion = _final,
                    BaseFace = _gravity
                };
            }
            return null;
        }

        #region protected override int CalcEffectiveValue()
        protected override int CalcEffectiveValue()
        {
            var _val = base.CalcEffectiveValue();
            if (!NoEncumberancePenalty)
            {
                if (!((CoreObject as Creature)?.EncumberanceCheck.Unencumbered ?? true))
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

                    // grab weight and load over max weight, only 5' step possible
                    if (CoreObject is Creature _critter)
                    {
                        var _totalLoad = ObjectGrabber.GetTotalMoveLoad(_critter, this);
                        if (_totalLoad > _critter.CarryingCapacity.LoadPushDrag)
                        {
                            // exceeding load push/drag capacity
                            return 0;
                        }
                        else if (_totalLoad > _critter.CarryingCapacity.HeavyLoadLimit)
                        {
                            // exceeded heavy load
                            _val = 5;
                        }
                    }

                    // minimum speed
                    if (_val < 5)
                    {
                        _val = 5;
                    }
                }
            }
            return _val;
        }
        #endregion

        public override bool CanMoveThrough(Items.Materials.Material material) => false;

        public override bool CanMoveThrough(CellMaterial material)
            => (material is GasCellMaterial) || (material is VoidCellMaterial);

        public override MovementBase Clone(Creature forCreature, object source)
            => new LandMovement(BaseValue, forCreature, source);

        protected override IEnumerable<MovementAction> GetStartMoveActions(LocalActionBudget budget)
        {
            foreach (var _act in base.GetStartMoveActions(budget))
            {
                yield return _act;
            }

            yield return new HopUp(this, new ActionTime(Contracts.TimeType.Brief));
            yield return new JumpDown(this, new ActionTime(Contracts.TimeType.Brief));
            yield break;
        }

        protected override IEnumerable<MovementAction> GetContinueMoveActions()
        {
            foreach (var _act in base.GetContinueMoveActions())
            {
                yield return _act;
            }

            yield return new HopUp(this, new ActionTime(Contracts.TimeType.SubAction));
            yield return new JumpDown(this, new ActionTime(Contracts.TimeType.SubAction));
            yield break;
        }

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsUsable
                && (budget is LocalActionBudget _budget) 
                && (_budget.TurnTick != null))
            {
                // these actions can only happen if not overloaded
                if (!((CoreObject as Creature)?.EncumberanceCheck.AtlasMustShrug ?? false))
                {
                    var _mvBudget = MovementBudget.GetBudget(budget);
                    var _prone = CoreObject.Adjuncts.OfType<ProneEffect>().Any(_p => _p.IsActive);
                    if (!_prone)
                    {
                        // if not prone, then can drop prone
                        yield return new DropProne(this, @"301");
                    }
                    else
                    {
                        // otherwise already prone, so see if crawling is possible
                        // ...not doing anything, or not moving; but can still move and perform at least a brief action
                        if (!(_budget.TopActivity?.Action is MovementAction)
                            && _mvBudget.CanStillMove && _budget.CanPerformBrief)
                        {
                            yield return new Crawl(this);
                        }
                    }
                }
            }

            // and all the base actions
            foreach (var _act in base.GetActions(budget))
            {
                yield return _act;
            }

            yield break;
        }
    }
}
