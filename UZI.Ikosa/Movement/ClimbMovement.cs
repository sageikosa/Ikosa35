using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Skills;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ClimbMovement : MovementBase, IAcceleratedMovement, IMonitorChange<DeltaValue>
    {
        public ClimbMovement(int speed, Creature creature, object source, bool shiftable, LandMovement landMovement)
            : base(speed, creature, source)
        {
            _Shift = shiftable;
            if (landMovement != null)
            {
                _Land = landMovement;
                _Land.AddChangeMonitor(this);
                BaseValue = _Land.EffectiveValue / 4;
            }
        }

        #region data
        private bool _Shift;
        private bool _Accelerated = false;
        private LandMovement _Land = null;
        #endregion

        /// <summary>True if a non-move shift position can be performed (will prevent further movement if used)</summary>
        public override bool CanShiftPosition => _Shift;

        /// <summary>Movement applies surface pressure</summary>
        public override bool SurfacePressure => true;

        /// <summary>True if movement cannot be used above maximum load (true for climbing)</summary>
        public override bool FailsAboveMaxLoad
            => true;

        /// <summary>returns true if the climb movement is not sourced by the Climb skill</summary>
        public bool IsNaturalClimber
            => (_Land == null);

        public override bool IsNativeMovement
            => IsNaturalClimber;

        public override string Name
            => (IsNaturalClimber)
            ? @"Climb"
            : @"Climb (skill-based)";

        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
            // NOTE: !!! validate all of this !!!
            var _dest = process.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = process.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
            if ((locator != null) && (leadCell != null) && (_dest != null) && (_idx != null))
            {
                var _gravity = locator.GetGravityFace();
                var _baseFace = locator.BaseFace;
                var _revBase = _baseFace.ReverseFace();
                var _crossings = _dest.CrossingFaces(locator.BaseFace, _idx?.Value ?? 0);
                var _moveTowards = AnchorFaceListHelper.Create(_crossings);
                var _activity = process as CoreActivity;
                var _maxUp = (_activity.Actor as Creature).Body.Height;
                var _locCore = locator.ICore as ICoreObject;
                var _planar = locator.PlanarPresence;

                // get faces that can be gripped during the move
                var _grippers = (from _move in _moveTowards.ToAnchorFaces()
                                 from _ortho in _move.GetOrthoFaces()
                                 group _ortho by _ortho into _multi
                                 where _multi.Count() == _moveTowards.Count()
                                 select _multi.Key).ToList();
                var _startSlopes = locator.Map.CellSlopes(_locCore, leadCell, this, _baseFace, exclusions, _planar)
                    .ToList();

                // calculate next lead cell and location
                var _next = leadCell.Move(_crossings.GetAnchorOffset()) as CellLocation;

                var _start = locator.Map.ClimbOutDifficulty(_locCore, locator.GeometricRegion, this,
                    _moveTowards, locator.MovementCrossings, _grippers, _gravity, _baseFace, _planar);
                if (_start != null)
                {
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

                    if (locator.NormalSize.XLength == 1)
                    {
                        #region validate/rectify occupancy of next cell or return null
                        bool _canMove()
                            => locator.Map.CanOccupy(_locCore, _next, this, exclusions, _planar)
                            && (TransitFitness(process, _crossings, leadCell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? true);
                        if (!_canMove())
                        {
                            // can only revbase offset if not already moving that way
                            if (_moveTowards.Contains(_revBase))
                            {
                                return null;
                            }

                            // make sure we can enter "above" lead cell
                            if (!TransitFitness(process, new[] { _revBase }, leadCell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? true)
                            {
                                return null;
                            }

                            // sufficient space to slide away from base surface (does not guarantee success)
                            // ... so set new lead cell, and new next cell
                            leadCell = leadCell.Move(_revBase.GetAnchorOffset()) as CellLocation;
                            _next = leadCell.Move(_crossings.GetAnchorOffset()) as CellLocation;
                            if (!_canMove())
                            {
                                // cannot move into the up-step cell
                                return null;
                            }
                        }
                        #endregion

                        var _offset = new Vector3D();
                        if (!_moveTowards.Contains(_revBase))
                        {
                            #region look for a suitable cell to land on
                            var _endSlopes = _next.CellElevation(_baseFace)
                                .ToEnumerable()
                                .Select(_e => new SlopeSegment { Low = _e, High = _e, Run = 5 }).ToList();
                            if (locator.Map.DirectionalBlockage(_locCore, _next, this, _baseFace, _gravity, exclusions, _planar) > 0.2)
                            {
                                // target cell was suitable
                                _endSlopes = locator.Map.CellSlopes(_locCore, _next, this, _baseFace, exclusions, _planar).ToList();
                            }
                            else
                            {
                                var _down = _next.Move(_baseFace.GetAnchorOffset()) as CellLocation;
                                if (locator.Map.DirectionalBlockage(_locCore, _down, this, _baseFace, _gravity, exclusions, _planar) > 0)
                                {
                                    // cell "underneath" appears suitable, can we step into it
                                    _endSlopes = locator.Map.CellSlopes(_locCore, _down, this, _baseFace, exclusions, _planar).ToList();
                                    _next = _down;
                                }
                                else
                                {
                                    // if we get to the end, run this before returning
                                    //_onSuccess = () =>
                                    //{
                                    //    if (CoreObject is Creature)
                                    //        process.AppendPreEmption(
                                    //            new SaveToCancel(process, CoreObject as Creature, Interactions.SaveType.Reflex, 20));
                                    //    // NOTE: normal relocation will happen in-between
                                    //    process.AppendCompletion(new FallingStartStep(process, locator, 500, 0, 0));
                                    //};
                                }
                            }
                            #endregion

                            #region elevation changes
                            var _baseElev = _next.CellElevation(_baseFace);
                            if (_endSlopes.Any())
                            {
                                var _endStep = _bestStep(_endSlopes);
                                if (_startSlopes.Any())
                                {
                                    var (slope, elev) = _bestStep(_startSlopes);
                                    if (elev < _endStep.elev)
                                    {
                                        // stepping up
                                        if ((_endStep.elev - elev) >= 4.5)
                                        {
                                            // steep slope up
                                            process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                        }
                                        if ((_endSlopes.Min(_e => _e.Low) - _startSlopes.Max(_s => _s.High)) > _maxUp)
                                        {
                                            // too high to step or hop-up
                                            return null;
                                        }
                                    }
                                    else if (elev > _endStep.elev)
                                    {
                                        // stepping down
                                        if ((_startSlopes.Min(_s => _s.Low) - _endSlopes.Max(_e => _e.High)) > _maxUp)
                                        {
                                            // will cause a small fall
                                            process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                        }
                                    }
                                }

                                // anything contributing to end slopes can be ignored for squeeze offsets
                                if ((_endStep.slope.Source != null) && !exclusions.ContainsKey(_endStep.slope.Source.ID))
                                {
                                    exclusions.Add(_endStep.slope.Source.ID, _endStep.slope.Source);
                                }

                                #region offset
                                var _best = _endStep; // TODO: improve
                                switch (_baseFace)
                                {
                                    case AnchorFace.ZLow:
                                        _offset = new Vector3D(0, 0, _best.elev - _baseElev);
                                        break;
                                    case AnchorFace.ZHigh:
                                        _offset = new Vector3D(0, 0, _baseElev - _best.elev);
                                        break;
                                    case AnchorFace.YLow:
                                        _offset = new Vector3D(0, _best.elev - _baseElev, 0);
                                        break;
                                    case AnchorFace.YHigh:
                                        _offset = new Vector3D(0, _baseElev - _best.elev, 0);
                                        break;
                                    case AnchorFace.XLow:
                                        _offset = new Vector3D(_best.elev - _baseElev, 0, 0);
                                        break;
                                    default:
                                        _offset = new Vector3D(_baseElev - _best.elev, 0, 0);
                                        break;
                                }
                                #endregion
                            }
                            #endregion
                        }
                        var _cubic = new Cubic(_next, 1, 1, 1);

                        #region extension?
                        // must extend?
                        if ((_offset.Length + locator.ICoreAs<ICorePhysical>().Max(_icp => _icp.Height)) > 5) // counts up to 5'
                        {
                            // extend the cubic
                            _cubic = _next.ContainingCube(_next.Move(_revBase.GetAnchorOffset()) as CellLocation);
                        }
                        #endregion

                        // provide a cell list
                        var _moveVol = new MovementVolume(_locCore, this, _crossings, _baseFace, locator.Map, _planar, exclusions);
                        var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d, _baseFace.GetAxis());
                        if (_final != null)
                        {
                            // ending difficulty (and geometry!)
                            var _end = locator.Map.ClimbInDifficulty(_locCore, locator.GeometricRegion, _final, this,
                                _moveTowards, locator.MovementCrossings, _grippers, _gravity, _baseFace, _planar);
                            if (_end.Difficulty != null)
                            {
                                // set difficulty
                                process.Targets.Add(new ValueTarget<int?>(@"Difficulty", Math.Max(_end.Difficulty ?? 0, _start ?? 0)));
                                return new MovementLocatorTarget
                                {
                                    Locator = locator,
                                    Offset = _offset,
                                    TargetRegion = _final,
                                    BaseFace = _end.GetBaseFace(_gravity, _baseFace)
                                };
                            }
                        }
                        else
                        {
                            // TODO: perhaps try with a different base face?
                        }
                    }
                    else
                    {
                        var _startOffset = locator.GeometricRegion.GetExitCubicOffset(leadCell, locator.NormalSize);
                        Cubic _getCubic(CellLocation startCell)
                        {
                            // might be called twice
                            var _cubeStart = startCell.Add(_startOffset);
                            return new Cubic(_cubeStart, locator.NormalSize);
                        }
                        var _cubic = _getCubic(_next);

                        #region validate/rectify occupancy of next cell or return null
                        bool _canMove()
                            => locator.Map.CanOccupy(_locCore, _cubic, this, exclusions, _planar)
                           && (TransitFitness(process, _crossings, leadCell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? true);

                        if (!_canMove())
                        {
                            // can only revbase offset if not already moving that way
                            if (_moveTowards.Contains(_revBase))
                            {
                                return null;
                            }

                            // make sure we can enter "above" lead cell
                            if (!TransitFitness(process, new[] { _revBase }, leadCell, locator.Map, locator.ActiveMovement, _planar, exclusions) ?? true)
                            {
                                return null;
                            }

                            // sufficient space to step up out of the cell (does not guarantee success)
                            // ... so set new lead cell, and new next cell
                            leadCell = leadCell.Move(_revBase.GetAnchorOffset()) as CellLocation;
                            _next = leadCell.Move(_crossings.GetAnchorOffset()) as CellLocation;

                            _cubic = _getCubic(_next);
                            if (!_canMove())
                            {
                                // cannot move into the up-step cell
                                return null;
                            }
                        }
                        #endregion

                        var _offset = new Vector3D();
                        if (!_moveTowards.Contains(_revBase))
                        {
                            // NOTE: with really small support levels, balance checks are needed?!?
                            #region look for a suitable cell to land on
                            var _endSlopes = _next.CellElevation(_baseFace)
                            .ToEnumerable()
                            .Select(_e => new SlopeSegment { Low = _e, High = _e, Run = 5 })
                            .ToList(); ;

                            // TODO: if body is large enough, may be able to "pass through" multiple cells (not just immediately under)
                            if (locator.Map.DirectionalBlockage(_locCore, _cubic, this, _baseFace, _gravity, exclusions, _planar) > 0)
                            {
                                // target cell was suitable
                                _endSlopes = locator.Map.CellSlopes(_locCore, _next, this, _baseFace, exclusions, _planar).ToList();
                            }
                            else
                            {
                                var _down = _next.Move(_baseFace.GetAnchorOffset()) as CellLocation;
                                if (locator.Map.DirectionalBlockage(_locCore, _down, this, _baseFace, _gravity, exclusions, _planar) > 0)
                                {
                                    // cell "underneath" appears suitable, can we step into it
                                    _endSlopes = locator.Map.CellSlopes(_locCore, _down, this, _baseFace, exclusions, _planar).ToList();
                                    _next = _down;
                                    _cubic = _getCubic(_down);
                                }
                                else
                                {
                                    // if we get to the end, run this before returning
                                    //_onSuccess = () =>
                                    //{
                                    //    if (CoreObject is Creature)
                                    //        process.AppendPreEmption(
                                    //            new SaveToCancel(process, CoreObject as Creature, Interactions.SaveType.Reflex, 20));
                                    //    // NOTE: normal relocation will happen in-between
                                    //    process.AppendCompletion(new FallingStartStep(process, locator, 500, 0, 0));
                                    //};
                                }
                            }
                            #endregion

                            #region elevation changes
                            var _baseElev = _next.CellElevation(_baseFace);
                            if (_endSlopes.Any())
                            {
                                var (slope, elev) = _bestStep(_endSlopes);
                                if (_startSlopes.Any())
                                {
                                    var _startStep = _bestStep(_startSlopes);
                                    if (_startStep.elev < elev)
                                    {
                                        // stepping up
                                        if ((elev - _startStep.elev) >= 4.5)
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
                                    else if (_startStep.elev > elev)
                                    {
                                        // stepping down
                                        if ((_startSlopes.Min(_s => _s.Low) - _endSlopes.Max(_e => _e.High)) > _maxUp)
                                        {
                                            // will cause a small fall
                                            process.SetFirstTarget(new ValueTarget<double>(@"Cost.Elevation", 2));
                                        }
                                    }
                                }

                                // anything contributing to end slopes can be ignored for squeeze offsets
                                if ((slope.Source != null) && !exclusions.ContainsKey(slope.Source.ID))
                                {
                                    exclusions.Add(slope.Source.ID, slope.Source);
                                }

                                #region offset
                                var _best = elev; // TODO: improve
                                switch (_baseFace)
                                {
                                    case AnchorFace.ZLow:
                                        _offset = new Vector3D(0, 0, _best - _baseElev);
                                        break;
                                    case AnchorFace.ZHigh:
                                        _offset = new Vector3D(0, 0, _baseElev - _best);
                                        break;
                                    case AnchorFace.YLow:
                                        _offset = new Vector3D(0, _best - _baseElev, 0);
                                        break;
                                    case AnchorFace.YHigh:
                                        _offset = new Vector3D(0, _baseElev - _best, 0);
                                        break;
                                    case AnchorFace.XLow:
                                        _offset = new Vector3D(_best - _baseElev, 0, 0);
                                        break;
                                    default:
                                        _offset = new Vector3D(_baseElev - _best, 0, 0);
                                        break;
                                }
                                #endregion
                            }
                            #endregion
                        }

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
                        var _moveVol = new MovementVolume(_locCore, this, _crossings, _baseFace, locator.Map, _planar, exclusions);
                        var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d, _baseFace.GetAxis());
                        if (_final != null)
                        {
                            // ending difficulty (and geometry!)
                            var _end = locator.Map.ClimbInDifficulty(_locCore, locator.GeometricRegion, _final, this,
                                _moveTowards, locator.MovementCrossings, _grippers, _gravity, _baseFace, _planar);
                            if (_end.Difficulty != null)
                            {
                                // set difficulty
                                process.Targets.Add(new ValueTarget<int?>(@"Difficulty", Math.Max(_end.Difficulty ?? 0, _start ?? 0)));
                                return new MovementLocatorTarget
                                {
                                    Locator = locator,
                                    Offset = _offset,
                                    TargetRegion = _final,
                                    BaseFace = _end.GetBaseFace(_gravity, _baseFace)
                                };
                            }
                        }
                        else
                        {
                            // TODO: perhaps try with a different base face?
                        }
                    }
                }
            }
            return null;
        }

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

                    // TODO: creature's locator must be adjacent to something solid
                    return true;
                }
                return false;
            }
        }

        #region public override CoreStep CostFactorStep(CoreActivity activity)
        public override CoreStep CostFactorStep(CoreActivity activity)
        {
            var _diffTarget = activity.GetFirstTarget<ValueTarget<int?>>(@"Difficulty");
            if (_diffTarget != null)
            {
                int? _getCurrent()
                    => (CoreObject as Creature)?.Skills.Skill<ClimbSkill>()?.CurrentClimb;

                // climbing already or force a check
                var _difficulty = new Deltable(_diffTarget.Value ?? 0);
                var _current = _getCurrent();
                if ((null == _current) || (_current < _difficulty.EffectiveValue))
                {
                    return new ClimbCheckStep(activity, (CoreObject as Creature), _difficulty, this);
                }
            }

            // should be MoveCostCheckStep
            return base.CostFactorStep(activity);
        }
        #endregion

        #region protected override double OnMoveCostFactor(CoreActivity activity)
        protected override double OnMoveCostFactor(CoreActivity activity)
        {
            // climbing and not accelerated?
            var _factor = base.OnMoveCostFactor(activity);
            if (IsAccelerated)
            {
                // lessen cost for accelerated climbing
                _factor *= 0.5;

                var _land = (CoreObject as Creature)?.Movements.AllMovements.OfType<LandMovement>().FirstOrDefault();
                if (_land != null)
                {
                    // when accelerated climbing exceeds max land speed, we need to reign in the factor
                    var _dblClimb = EffectiveValue * 2;
                    var _maxLand = _land.EffectiveValue;
                    if (_dblClimb > _maxLand)
                    {
                        // we lessened the factor above by multiplying a fraction
                        // so now we have to increase it by dividing a fraction
                        _factor /= (_maxLand / _dblClimb);
                    }
                }
            }
            return _factor;
        }
        #endregion

        #region public override void OnEndTurn()
        public override void OnEndTurn()
        {
            // expire climbing for movement
            foreach (var _climbing in CoreObject.Adjuncts.OfType<Climbing>())
            {
                _climbing.IsCheckExpired = true;
            }
            base.OnEndTurn();
        }
        #endregion

        #region public override void OnResetBudget()
        public override void OnResetBudget()
        {
            // expire climbing for movement
            foreach (var _climbing in CoreObject.Adjuncts.OfType<Climbing>())
            {
                _climbing.IsCheckExpired = true;
            }
            base.OnResetBudget();
        }
        #endregion

        #region public override void OnEndActiveMovement()
        public override void OnEndActiveMovement()
        {
            // clean up any climbing
            foreach (var _climbing in CoreObject.Adjuncts.OfType<Climbing>().ToList())
            {
                _climbing.Eject();
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
                foreach (var _climbing in CoreObject.Adjuncts.OfType<Climbing>())
                {
                    _climbing.IsCheckExpired = true;
                }
            }
            base.OnSecondIncrementOfTotal(action);
        }
        #endregion

        protected override int CalcEffectiveValue()
        {
            // TODO: calculate based on encumberance?
            return base.CalcEffectiveValue();
        }

        public override bool CanMoveThrough(Items.Materials.Material material)
            => false;

        public override bool CanMoveThrough(CellMaterial material)
            => (material is GasCellMaterial) || (material is VoidCellMaterial);

        #region public override MovementBase Clone(Creature forCreature, object source)
        public override MovementBase Clone(Creature forCreature, object source)
        {
            if (_Land != null)
            {
                // unnatural climber
                var _land = forCreature.Movements.AllMovements.OfType<LandMovement>().FirstOrDefault();
                return new ClimbMovement(BaseValue, forCreature, source, CanShiftPosition, _land);
            }
            else
            {
                // natural climber
                return new ClimbMovement(BaseValue, forCreature, source, CanShiftPosition, null);
            }
        }
        #endregion

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsUsable
                && (budget is LocalActionBudget _budget)
                && (_budget.TurnTick != null))
            {
                yield return new AcceleratedMovement<ClimbMovement>(this);
                if (IsNaturalClimber || !_budget.IsInitiative)
                {
                    // TODO: take 10 if a natural climber, or not in combat situation
                }
            }

            // all base actions
            foreach (var _act in base.GetActions(budget))
            {
                yield return _act;
            }

            yield break;
        }

        #region IAcceleratedMovement Members

        /// <summary>Taking a check penalty to use accelerated movement</summary>
        public virtual bool IsAccelerated
        {
            get { return _Accelerated; }
            set { _Accelerated = value; }
        }

        #endregion

        public virtual bool IsCheckRequired
            => true;

        #region IMonitorChange<DeltaValue> Members

        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
        }

        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
        }

        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if ((_Land != null) && (sender == _Land))
            {
                BaseValue = _Land.EffectiveValue / 4;
            }
            else
            {
                DeltableValueChanged();
            }
        }

        #endregion
    }
}
