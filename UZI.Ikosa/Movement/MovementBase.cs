using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Senses;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;
using System.Diagnostics;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Time;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public abstract class MovementBase : Deltable, ISourcedObject, IActionProvider, ICore, IActionSource
    {
        #region Construction
        protected MovementBase(int speed, CoreObject coreObj, object source)
            : base(speed)
        {
            _CoreObj = coreObj;
            _Src = source;
            _ID = Guid.NewGuid();
        }
        #endregion

        #region Private/Protected Data
        protected CoreObject _CoreObj;
        private object _Src;
        private Guid _ID;
        #endregion

        public abstract string Name { get; }
        public object Source => _Src;
        public CoreObject CoreObject => _CoreObj;

        /// <summary>Indicates this movement can shift position</summary>
        public abstract bool CanShiftPosition { get; }

        /// <summary>Movement applies surface pressure</summary>
        public abstract bool SurfacePressure { get; }

        public abstract bool IsNativeMovement { get; }

        protected override void DoValueChanged()
        {
            DoPropertyChanged(nameof(Description));
            base.DoValueChanged();
        }

        #region public virtual string Description { get; }
        public virtual string Description
        {
            get
            {
                if (EffectiveValue != BaseValue)
                {
                    return $@"{Name} {EffectiveValue} ft (Base {BaseValue})";
                }
                else
                {
                    return $@"{Name} {EffectiveValue} ft.";
                }
            }
        }
        #endregion

        #region public override string ToString()
        public override string ToString()
        {
            try
            {
                return Description;
            }
            catch
            {
                return GetType().FullName;
            }
        }
        #endregion

        /// <summary>Returns true if requested movement direction is possible by the movement definition, and possibly other rule checks</summary>
        protected virtual bool OnCanMoveToTargetRegion(Locator locator, ICellLocation location, IGeometricRegion region, AnchorFace[] crossings)
        {
            return true;
        }

        /// <summary>True if this movement must be able to visualize movement target cells</summary>
        protected virtual bool MustVisualizeMovement => true;

        #region protected virtual bool OnBlocksHedralTransit(CoreTargetingProcess process, LocalMap map, ICellLocation cell, AnchorFace face, MovementBase lastMove)
        /// <summary>Checks whether outgoing or incoming cells (or stuff in the cell) block transit</summary>
        protected virtual bool OnBlocksHedralTransit(CoreTargetingProcess process, LocalMap map, ICellLocation cell, AnchorFace face,
            MovementBase lastMove, PlanarPresence planar, Dictionary<Guid, ICore> exclusions)
        {
            // default implementation is to block if cell blocks
            ref readonly var _cell = ref map[cell];
            var _nextLoc = CellLocation.GetAdjacentCellLocation(cell, face);
            ref readonly var _next = ref map[_nextLoc];
            var _currCtx = new MovementTacticalInfo
            {
                Movement = lastMove,
                TransitFaces = new AnchorFace[] { face },
                SourceCell = cell,
                TargetCell = _nextLoc
            };
            var _nextCtx = new MovementTacticalInfo
            {
                Movement = this,
                TransitFaces = new AnchorFace[] { face.ReverseFace() },
                SourceCell = _nextLoc,
                TargetCell = cell
            };

            // same cell object
            var _blocker
                = map.MapContext.AllInCell<IMoveAlterer>(cell, planar)
                .FirstOrDefault(_iti => !exclusions.ContainsKey(_iti.ID) && _iti.BlocksTransit(_currCtx));
            if (_blocker != null)
            {
                process?.SetFirstTarget<ValueTarget<IMoveAlterer>>(
                    new ValueTarget<IMoveAlterer>(MovementTargets.IMoveAlterer_BlocksTransit, _blocker));
                return true;
            }

            // physical terrain blockage
            if (_cell.BlockedAt(lastMove, new CellSnap(face.ToAnchorFaceList())))
            {
                return true;
            }

            // next cell object
            _blocker = map.MapContext.AllInCell<IMoveAlterer>(_nextLoc, planar)
                .FirstOrDefault(_iti => !exclusions.ContainsKey(_iti.ID) && _iti.BlocksTransit(_nextCtx));
            if (_blocker != null)
            {
                process?.SetFirstTarget<ValueTarget<IMoveAlterer>>(
                    new ValueTarget<IMoveAlterer>(MovementTargets.IMoveAlterer_BlocksTransit, _blocker));
                return true;
            }

            // physical terrain blockage
            if (_next.BlockedAt(this, new CellSnap(face.ReverseFace().ToAnchorFaceList())))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region protected virtual bool OnBlocksTransit(CoreTargetingProcess process, LocalMap map, ICellLocation cell, AnchorFace[] faces)
        /// <summary>Checks whether stuff in the outgoing or incoming cells block transit</summary>
        protected virtual bool OnBlocksTransit(CoreTargetingProcess process, LocalMap map, ICellLocation cell, AnchorFace[] faces,
            PlanarPresence planar, Dictionary<Guid, ICore> exclusions)
        {
            // default implementation is to block if cell blocks
            var _nextLoc = CellLocation.GetAdjacentCellLocation(cell, faces);
            var _currCtx = new MovementTacticalInfo
            {
                Movement = this,
                TransitFaces = faces,
                SourceCell = cell,
                TargetCell = _nextLoc
            };
            var _nextCtx = new MovementTacticalInfo
            {
                Movement = this,
                TransitFaces = faces.Select(_f => _f.ReverseFace()).ToArray(),
                SourceCell = _nextLoc,
                TargetCell = cell
            };

            // object blockage
            var _blocker
                = map.MapContext.AllInCell<IMoveAlterer>(cell, planar).FirstOrDefault(_iti => !exclusions.ContainsKey(_iti.ID) && _iti.BlocksTransit(_currCtx))
                ?? map.MapContext.AllInCell<IMoveAlterer>(_nextLoc, planar).FirstOrDefault(_iti => !exclusions.ContainsKey(_iti.ID) && _iti.BlocksTransit(_nextCtx));
            if (_blocker == null)
            {
                return false;
            }

            process?.SetFirstTarget<ValueTarget<IMoveAlterer>>(
                new ValueTarget<IMoveAlterer>(MovementTargets.IMoveAlterer_BlocksTransit, _blocker));
            return true;
        }
        #endregion

        public virtual IEnumerable<AimingMode> GetAimingModes(CoreActivity activity)
        {
            yield break;
        }

        /// <summary>create a region for the movement (if possible)</summary>
        protected abstract MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions);

        #region private bool? TransitFitness(AnchorFace[] faces, Cubic start, LocalMap map, MovementBase lastMove)
        /// <summary>Get fitness of transit (for each crossing face)</summary>
        /// <returns>true (fit), false (unfit), null (use if other tests are no better)</returns>
        protected bool? TransitFitness(CoreTargetingProcess process, AnchorFace[] faces, IGeometricRegion start, LocalMap map, MovementBase lastMove,
             PlanarPresence planar, Dictionary<Guid, ICore> exclusions)
        {
            // start with no expectation of having to squeeze to move
            var _squeezeAcross = false;

            var _nextCount = 0;
            var _blockCount = 0;

            #region each distinct transit face
            // do adjacent terrain cells prevent movement across all faces?
            foreach (var _face in faces)
            {
                // TODO: parallel
                // gets just the specified edge of the region
                _nextCount = 0;
                _blockCount = 0;
                foreach (var _cLoc in start.AllCellLocations().Where(_cl => start.IsCellAtSurface(_cl, _face)))
                {
                    _nextCount++;
                    if (OnBlocksHedralTransit(process, map, _cLoc, _face, lastMove ?? this, planar, exclusions))
                    {
                        // TODO: can moving thing squeeze at all?
                        // haven't flagged that already squeezing a dimension to move
                        if (!_squeezeAcross)
                        {
                            _blockCount++;
                        }
                        else
                        {
                            // cannot transit squeeze across two dimensions
                            return false;
                        }
                    }
                }

                // hit block threshold for viability
                if ((_blockCount * 2) >= _nextCount)
                {
                    return false;
                }

                // any blockage, must start squeezin
                if (_blockCount > 0)
                {
                    _squeezeAcross = true;
                }
            }
            #endregion

            // direct movement towards when multiple transit faces present
            if (faces.Length == 2)
            {
                #region edge-directed
                // if 2 faces, this will get the shared edge
                if (start.AllCellLocations()
                    .Where(_cLoc => start.IsCellAtSurface(_cLoc, faces[0]) && start.IsCellAtSurface(_cLoc, faces[1]))
                    .Any(_cLoc => OnBlocksTransit(process, map, _cLoc, faces, planar, exclusions)))
                {
                    return false;
                }
                #endregion
            }
            else if (faces.Length == 3)
            {
                #region corner and edges
                // need corner cell
                if (start.AllCellLocations()
                    .Where(_cLoc => start.IsCellAtSurface(_cLoc, faces[0])
                        && start.IsCellAtSurface(_cLoc, faces[1])
                        && start.IsCellAtSurface(_cLoc, faces[2]))
                    .Any(_cLoc => OnBlocksTransit(process, map, _cLoc, faces, planar, exclusions)))
                {
                    return false;
                }

                // NOTE: this may not all be relevant
                var _2faces = new AnchorFace[] { faces[0], faces[1] };
                if (start.AllCellLocations()
                    .Where(_cloc => start.IsCellAtSurface(_cloc, faces[0]) && start.IsCellAtSurface(_cloc, faces[1]))
                    .Any(_cLoc => OnBlocksTransit(process, map, _cLoc, _2faces, planar, exclusions)))
                {
                    return false;
                }

                _2faces = new AnchorFace[] { faces[0], faces[2] };
                if (start.AllCellLocations()
                    .Where(_cloc => start.IsCellAtSurface(_cloc, faces[0]) && start.IsCellAtSurface(_cloc, faces[2]))
                    .Any(_cLoc => OnBlocksTransit(process, map, _cLoc, _2faces, planar, exclusions)))
                {
                    return false;
                }

                _2faces = new AnchorFace[] { faces[1], faces[2] };
                if (start.AllCellLocations()
                    .Where(_cloc => start.IsCellAtSurface(_cloc, faces[1]) && start.IsCellAtSurface(_cloc, faces[2]))
                    .Any(_cLoc => OnBlocksTransit(process, map, _cLoc, _2faces, planar, exclusions)))
                {
                    return false;
                }
                #endregion
            }

            // squeezed movement is ambivalent
            if (_squeezeAcross)
            {
                return null;
            }

            // all transit faces were OK
            return true;
        }
        #endregion

        #region protected CellLocation TargetLeadCell(CoreTargettingProcess activity)
        protected CellLocation TargetLeadCell(CoreTargetingProcess process)
        {
            var _vol = process.GetFirstTarget<GeometricRegionTarget>(MovementTargets.LeadCell);
            if (_vol != null)
            {
                return new CellLocation(_vol.Region.AllCellLocations().FirstOrDefault());
            }
            return null;
        }
        #endregion

        #region private CellLocation StartCell(AnchorFace[] crossings, Locator locator, AnchorFace gravity, CoreAction action)
        protected CellLocation StartCell(AnchorFace[] crossings, Locator locator, AnchorFace baseFace, CoreAction action)
        {
            var _maxUp = locator.ICoreAs<ICorePhysical>().Max(_icp => _icp.Height) / ((action is HopUp) ? 2 : 3);
            var _locRgn = locator.GeometricRegion;
            var _planar = locator.PlanarPresence;

            // if already explictly dealing with gravity, simply use the crossings, 
            // otherwise, marriage of gravity
            var _crossings = (crossings.Any(_f => _f.GetAxis() == baseFace.GetAxis()))
                ? crossings
                : crossings.Union(baseFace.ToEnumerable());

            // TODO: if a single dimension is extended, use the cell with the majority of the locator
            int _getOrd(Axis axis, int low, int high, double offset)
            {
                var _bump =
                    (offset < 0) ? (((0 - offset) > _maxUp) ? -1 : 0) :
                    (offset > 0) ? ((offset > _maxUp) ? 1 : 0) :
                    0;
                if (_crossings.Contains(axis.GetLowFace()))
                {
                    return low + _bump;
                }
                else if (_crossings.Contains(axis.GetHighFace()))
                {
                    return high + _bump;
                }

                return (low + high) / 2;
            };

            var _cLoc = new CellLocation(
                _getOrd(Axis.Z, _locRgn.LowerZ, _locRgn.UpperZ, locator.IntraModelOffset.Z),
                _getOrd(Axis.Y, _locRgn.LowerY, _locRgn.UpperY, locator.IntraModelOffset.Y),
                _getOrd(Axis.X, _locRgn.LowerX, _locRgn.UpperX, locator.IntraModelOffset.X));

            // if the location is outside the region, walk from location toward the region centeruntil we find a good cell
            if (!_locRgn.ContainsCell(_cLoc))
            {
                // destination: centroid
                var _pt2 = new Point3D(
                    (_locRgn.LowerX + (double)_locRgn.UpperX + 1) * 2.5,
                    (_locRgn.LowerY + (double)_locRgn.UpperY + 1) * 2.5,
                    (_locRgn.LowerZ + (double)_locRgn.UpperZ + 1) * 2.5);

                // build a line set (using a permissive factory)
                var _lSet = locator.Map.SegmentCells(_cLoc.GetPoint3D(), _pt2,
                    new SegmentSetFactory(locator.Map, _cLoc, _locRgn,
                    ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Geometry), _planar);
                var _sRef = _lSet.All().FirstOrDefault(_cl => _locRgn.ContainsCell(_cl));
                if (_sRef.IsActual)
                {
                    // construct a new pure cell location
                    _cLoc = new CellLocation(_sRef);
                }
            }
            return _cLoc;
        }
        #endregion

        public virtual bool IsSqueezingAllowed => true;
        protected virtual double OnMoveCostFactor(CoreActivity activity) => 1;
        public virtual bool FailsAboveMaxLoad => false;
        public Guid ID => _ID;
        public IVolatileValue ActionClassLevel => new Deltable(1);

        #region public bool IsValidActivity(CoreActivity activity)
        /// <summary>
        /// True if the movement is valid.
        /// Side effect: calculates and adds the lead cell, target cube and locator to the activity
        /// </summary>
        public bool IsValidActivity(CoreTargetingProcess process, CoreObject mover)
        {
            var _dest = process.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = process.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
            if (_dest != null)
            {
                process.SetFirstTarget(new ValueTarget<CoreObject>(@"Mover", mover));
                var _allResults = GetLocatorsData.GetLocators(mover, this, _dest, _idx.Value).ToList();
                var _allICore = _allResults.Select(_l => _l.Locator.ICore).ToDictionary(_ic => _ic.ID);
                foreach (var _rslt in _allResults)
                {
                    var _locIdx = _allResults.IndexOf(_rslt);

                    if (_rslt.MoveCost > 1)
                    {
                        // locators might add extra cost
                        process.SetFirstTarget(
                            new ValueTarget<double>($@"Cost.Locator.{_rslt.Locator.ICore.ID}", _rslt.MoveCost));
                    }

                    #region try to get a lead cell
                    CellLocation _leadCell = null;
                    // TODO: this may only apply to actor locator...
                    var _leadTarget = TargetLeadCell(process);
                    var _baseFace = _rslt.Locator.BaseFace;
                    var _crossings = _dest.CrossingFaces(_baseFace, _idx?.Value ?? 0);
                    if ((_leadTarget == null) || (_locIdx != 0))
                    {
                        // didn't have one, or not first locator; so make a good guess
                        _leadCell = StartCell(_crossings, _rslt.Locator, _baseFace, (process as CoreActivity)?.Action);
                        if (_locIdx == 0)
                        {
                            // and update the activity targets (for first locator)
                            process.SetFirstTarget<GeometricRegionTarget>(
                                new GeometricRegionTarget(MovementTargets.LeadCell, new CellLocation(_leadCell), _rslt.Locator.MapContext));
                        }
                    }
                    else
                    {
                        _leadCell = new CellLocation(_leadTarget);
                    }
                    #endregion

                    if (_leadCell != null)
                    {
                        // immobile?
                        if ((_rslt.Locator.ICore as IAdjunctable)?.HasActiveAdjunct<Immobile>() ?? false)
                        {
                            // had an immobile object in a locator
                            process.SetFirstTarget(new ValueTarget<bool>(MovementTargets.Immobilized, true));
                        }
                        else
                        {
                            var _locator = _rslt.Locator;
                            if (_locator.Map.CanOccupy(_locator.ICore as ICoreObject, _leadCell, this, _allICore, _locator.PlanarPresence))
                            {
                                var _moveLoc = GetNextGeometry(process, _locator, _leadCell, _allICore);
                                if (_moveLoc?.TargetRegion != null)
                                {
                                    var _rgn = _moveLoc.TargetRegion;
                                    if ((from _l in _locator.MapContext.LocatorsInRegion(_rgn, _locator.PlanarPresence)
                                         where !_allICore.ContainsKey(_l.ICore.ID)
                                         from _ima in _l.ICoreAs<IMoveAlterer>()
                                         select _ima).Any(_i => _i.HindersTransit(this, _rgn)))
                                    {
                                        process.SetFirstTarget(new ValueTarget<double>(@"Cost.Obstacle", 2));
                                    }

                                    process.Targets.Add(
                                        new ValueTarget<MovementLocatorTarget>(MovementTargets.MoveData, _moveLoc));
                                }
                                else
                                {
                                    // at least one locator couldn't get next geometry
                                    process.SetFirstTarget(new ValueTarget<bool>(MovementTargets.Immobilized, true));
                                }
                            }
                            else
                            {
                                // NOTE: there isn't a contra-positive case here! indicating true-ish
                                return false;
                            }
                        }
                    }
                    else
                    {
                        // could not get a valid lead cell
                        return false;
                    }
                }

                // only if all linked locators can move...
                return true;
            }
            return false;
        }
        #endregion

        /// <summary>Overridden step should usually be followed by MoveCostCheckStep.</summary>
        public virtual CoreStep CostFactorStep(CoreActivity activity) => new MoveCostCheckStep(activity);

        /// <summary>Calculate cost to perform this move based on movement/target specific criteria</summary>
        protected virtual double BaseMoveCost(CoreActivity activity, double expected) => expected;

        #region public double MoveCost(CoreActivity activity, double baseCost)
        /// <summary>IsValidActivity must be called before MoveCost</summary>
        public double MoveCost(CoreActivity activity, double baseCost)
        {
            var _maxCost = 1d;
            var _avTarget = activity.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _visualizer = (activity.Actor as Creature).GetTerrainVisualizer();
            var _any = false;
            foreach (var _target in activity.Targets.OfType<ValueTarget<MovementLocatorTarget>>())
            {
                // indicate presence of targets
                _any = true;
                var _loc = _target.Value.Locator;
                var _region = _target.Value.TargetRegion;
                var _cost = BaseMoveCost(activity, baseCost);

                if ((_loc != null) && (_avTarget != null) && (_region != null))
                {
                    _cost = CostCalcPerLocator(activity, _visualizer, _loc, _region, _cost);
                }

                // highest cost from all locators
                if (_cost > _maxCost)
                {
                    _maxCost = _cost;
                }
            }

            if (!_any)
            {
                // no locator targets, so get locator of actor
                var _loc = activity.Actor.GetLocated()?.Locator;
                if (_loc != null)
                {
                    var _cost = BaseMoveCost(activity, baseCost);
                    _cost = CostCalcPerLocator(activity, _visualizer, _loc, _loc.GeometricRegion, _cost);

                    // highest cost from all locators
                    if (_cost > _maxCost)
                    {
                        _maxCost = _cost;
                    }
                }
            }

            return _maxCost;
        }

        private double CostCalcPerLocator(CoreActivity activity, TerrainVisualizer visualizer, Locator locator,
            IGeometricRegion region, double cost)
        {
            // make sure shifting into target cube is not blocked
            var _map = locator.Map;

            // terrain face squeezing (except incorporeal movers)
            if (MustVisualizeMovement)
            {
                // sensory cost in passable spaces
                var _cellLocs = region.AllCellLocations();
                if (_cellLocs.Select(_cell => _map.GetVisualEffect(locator.GeometricRegion, _cell, visualizer))
                    .Any(_eff => _eff == VisualEffect.Unseen || _eff == VisualEffect.Skip))
                {
                    if ((CoreObject as Creature)?.Feats.Contains(typeof(BlindFight)) ?? false)
                    {
                        cost *= 1.5;
                    }
                    else
                    {
                        cost *= 2;
                    }
                }
            }

            if ((CoreObject?.HasActiveAdjunct<Squeezing>() ?? false)
                || locator.SqueezesIntoGeometry(region)
                || (from _l in locator.MapContext.LocatorsInRegion(region, locator.PlanarPresence)
                    from _ma in _l.ICoreAs<IMoveAlterer>()
                    where _ma.IsCostlyMovement(this, region)
                    select _ma).Any())
            {
                // treat squeezing as an obstacle
                if (activity.GetFirstTarget<ValueTarget<double>>(@"Cost.Obstacle") == null)
                {
                    cost *= 2;
                }
            }

            foreach (var _costTarget in activity.Targets
                .OfType<ValueTarget<double>>().Where(_t => _t.Key.StartsWith(@"Cost.")))
            {
                // additional cost targets
                cost *= _costTarget.Value;
            }

            // other movement-specific costs (tumble, balance, swim, etc.)
            cost *= OnMoveCostFactor(activity);

            // TODO: cost factors from zonal effects (blocking incorporeal also), and terrain (water)
            return cost;
        }
        #endregion

        /// <summary>True if the movement mode can currently be used</summary>
        public abstract bool IsUsable { get; }

        /// <summary>True if the movement can be used in the specified cell material</summary>
        public abstract bool CanMoveThrough(CellMaterial material);

        /// <summary>True if the movement can be used in the specified material</summary>
        public abstract bool CanMoveThrough(Uzi.Ikosa.Items.Materials.Material material);

        public virtual void OnEndTurn()
        {
            // TODO: ending turn in an invalid space...
        }

        public virtual void OnResetBudget() { }

        /// <summary>Triggered when this movement ceases to be the active movement of a Locator</summary>
        public virtual void OnEndActiveMovement()
        {
            GetCurrentSound(CoreObject)?.Eject();
        }

        protected virtual string GetMovementSoundDescription()
            => @"moving";

        protected virtual SoundRef GetSoundRef(Guid id, ICoreObject movingObject, DeltaCalcInfo baseDifficulty, ulong serial)
        {
            var _sizer = movingObject as ISizable;
            var _maxRange = 120;
            var _order = _sizer?.Sizer.Size.Order ?? 0;
            switch (_order)
            {
                // -15 feet for each size step down from medium
                case -4: _maxRange = 60; break;
                case -3: _maxRange = 75; break;
                case -2: _maxRange = 90; break;
                case -1: _maxRange = 105; break;

                // +30 feet for each size step up from medium
                case 1: _maxRange = 150; break;
                case 2: _maxRange = 180; break;
                case 3: _maxRange = 210; break;
                case 4: _maxRange = 240; break;
            }
            var _mvDescript = GetMovementSoundDescription();
            var _sizeDescript = _order != 0 ? $@"{_sizer.Sizer.Size.Name} " : string.Empty;
            var _material = (movingObject as Creature)?.GetMovementSound()
                ?? (movingObject as IObjectBase)?.ObjectMaterial.SoundQuality;
            // TODO: make object sound from object method call

            var _exceed = new List<(int exceed, string description)>
            {
                (0, $@"something {_mvDescript}"),
                (5, $@"something {_sizeDescript}{_mvDescript}"),
                (10, $@"something {_sizeDescript}{_material} {_mvDescript}")
            };
            return new SoundRef(new Audible(id, movingObject.ID, $@"{Name} Sound: {movingObject.Name}", _exceed.ToArray()),
                baseDifficulty, _maxRange, serial);
        }

        public SoundParticipant GetCurrentSound(ICoreObject coreObject)
            => coreObject?.Adjuncts.OfType<SoundParticipant>()
            .FirstOrDefault(_sp => (_sp.Source as Type) == GetType() && ((_sp.SoundGroup.Source as CoreID)?.ID == ID));

        public virtual void OnPreRelocated(CoreProcess process, Locator locator)
        {
            if (locator.ICore is ICoreObject _movingObject)
            {
                // setting stuff
                var _map = (_movingObject.Setting as LocalMap);
                var _serial = _map?.MapContext?.SerialState ?? 0;
                var _time = _map?.CurrentTime ?? 0;

                // calculate difficulty
                var _stealthCalc = new DeltaCalcInfo(_movingObject.ID, @"move sound");
                var _delta = -10;
                var _mode = MovementRangeBudget.Stealth.None;
                if (process is CoreActivity _activity
                    && _activity?.Actor is Creature _critter)
                {
                    // movement generated by creature movement, so check creature's move silently status
                    var _budget = _critter.GetLocalActionBudget();
                    var _range = MovementRangeBudget.GetBudget(_budget);
                    if (_range != null)
                    {
                        _mode = _range.CurrentStealth;
                        switch (_mode)
                        {
                            case MovementRangeBudget.Stealth.Hasty:
                                _delta = -5;
                                break;

                            case MovementRangeBudget.Stealth.High:
                                _delta = 0;
                                break;
                        }
                    }

                    // update stealth-roll (or take 10) as needed
                    if ((_range.StealthCheckExpire ?? _time) <= _time)
                    {
                        if (_critter.IsTake10InEffect(typeof(SilentStealthSkill)))
                        {
                            _range.SetStealthCheck(_time + Round.UnitFactor, 10);
                        }
                        else
                        {
                            _range.SetStealthCheck(_time + Round.UnitFactor,
                                DieRoller.RollDie(_critter.ID, 20, @"Silent Stealth", @"Movement"));
                        }
                    }

                    // current stealth-roll (used to fill out _stealthCalc)
                    _critter.Skills.Skill<SilentStealthSkill>().CheckValue(
                        new Qualifier(_critter, _activity, _movingObject), _range.StealthRoll, _stealthCalc);
                    if (_critter == _movingObject)
                    {
                        // TODO: notify if check is for creature, and different than last creature-check
                        // TODO: must store check for creature in budget
                    }
                }
                else
                {
                    // no actor, movement is based on 0 dexterity = -5 silent stealth
                    _stealthCalc.SetBaseResult(10);
                    _stealthCalc.AddDelta(@"no object dexterity", -5);
                    _stealthCalc.Result -= 5;

                    // TODO: notify calculation for object if "different" than last calculation...???
                }

                if (_delta != 0)
                {
                    // add any stealth-mode delta
                    _stealthCalc.AddDelta($@"stealth mode: {_mode}", _delta);
                    _stealthCalc.Result += _delta;
                }

                // already participating
                var _participant = GetCurrentSound(_movingObject);
                var _id = _participant?.SoundGroup?.ID ?? Guid.NewGuid();

                // get sound that should be used...
                var _soundRef = GetSoundRef(_id, _movingObject, _stealthCalc, _serial);

                if (_participant == null)
                {
                    // start a new sound-group
                    var _group = new SoundGroup(new CoreID(ID), _soundRef);
                    _participant = new SoundParticipant(GetType(), _group);
                    _movingObject.AddAdjunct(_participant);
                }
                else
                {
                    // update existing sound-group
                    _participant.SoundGroup.SetSoundRef(_soundRef);
                }

                // set sound to auto-eject in one round
                _participant.SetExpire(_time + Round.UnitFactor);
            }
        }

        /// <summary>Triggered when this movement is used to relocate a locator</summary>
        public virtual void OnRelocated(CoreProcess process, Locator locator)
        {
            // TODO: sound participant...
        }

        /// <summary>Triggered on the second range increment of a total movement action</summary>
        public virtual void OnSecondIncrementOfTotal(MovementAction action) { }

        protected virtual IEnumerable<MovementAction> GetStartMoveActions(LocalActionBudget budget)
        {
            yield return new StartMove(this);
            if (budget.IsSingleAction)
            {
                yield return new StartWithdraw(this, new ActionTime(TimeType.Regular));
                yield return new StartCharge(this, new ActionTime(TimeType.Regular));
            }
            yield break;
        }

        protected virtual IEnumerable<MovementAction> GetTotalMoveActions(bool isInitiative)
        {
            yield return new StartRun(this);
            if (isInitiative)
            {
                yield return new StartWithdraw(this, new ActionTime(TimeType.Total));
                yield return new StartCharge(this, new ActionTime(TimeType.Total));
            }
            yield break;
        }

        protected virtual IEnumerable<MovementAction> GetContinueMoveActions()
        {
            yield return new ContinueMove(this);
            yield break;
        }

        #region IActionProvider Members
        public Guid PresenterID => ID;

        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (IsUsable
                && (budget is LocalActionBudget _budget) 
                && (_budget.TurnTick != null))
            {
                // get overloaded indicator
                var _ovrLd = (CoreObject as Creature)?.EncumberanceCheck.AtlasMustShrug ?? false;
                var _initiative = _budget.IsInitiative;
                var _mvBudget = MovementBudget.GetBudget(budget);
                if (!(_budget.TopActivity?.Action is MovementAction))
                {
                    if (_mvBudget.CanStillMove)
                    {
                        if (_budget.CanPerformBrief && !_ovrLd)
                        {
                            foreach (var _act in GetStartMoveActions(_budget))
                            {
                                yield return _act;
                            }
                        }
                        if (_budget.CanPerformRegular && !_ovrLd)
                        {
                            yield return new Overrun(this, new ActionTime(TimeType.Brief), false);
                        }

                        // only if no movement has already been performed
                        if (!_mvBudget.HasMoved)
                        {
                            if (CanShiftPosition && !_ovrLd)
                            {
                                yield return new ShiftPosition(this, false);
                            }
                            if (_budget.CanPerformTotal)
                            {
                                if (!_ovrLd)
                                {
                                    foreach (var _act in GetTotalMoveActions(_initiative))
                                    {
                                        yield return _act;
                                    }
                                }

                                if (!_ovrLd || !FailsAboveMaxLoad)
                                {
                                    // TODO: full-round move 1 square
                                    // TODO: if fails, should begin plummet (flight) or sink (swim)
                                }
                            }
                        }
                    }
                }
                else if (_budget.TopActivity?.Action is Overrun)
                {
                    if (!_ovrLd)
                    {
                        foreach (var _act in GetContinueMoveActions())
                        {
                            yield return _act;
                        }
                    }
                }
                else if (_budget.TopActivity?.Action is StartRun)
                {
                    if (!_ovrLd)
                    {
                        // TODO: continue linear move
                    }
                }
                else if (_budget.TopActivity?.Action is StartCharge)
                {
                    if (!_ovrLd)
                    {
                        // TODO: continue linear move
                        // TODO: attack action
                    }
                }
                else
                {
                    if (!_ovrLd)
                    {
                        foreach (var _act in GetContinueMoveActions())
                        {
                            yield return _act;
                        }

                        if (_budget.CanPerformRegular && !(_budget.TopActivity?.Action is StartWithdraw))
                        {
                            yield return new Overrun(this, new ActionTime(TimeType.SubAction), false);
                        }
                    }
                }
            }
            yield break;
        }

        public virtual Info GetProviderInfo(CoreActionBudget budget)
        {
            return ToMovementInfo<MovementInfo>();
        }
        #endregion

        protected MInfo ToMovementInfo<MInfo>()
            where MInfo : MovementInfo, new()
        {
            return new MInfo
            {
                Message = Name,
                ID = ID,
                CanShiftPosition = CanShiftPosition,
                Description = Description,
                Value = EffectiveValue,
                IsUsable = IsUsable
            };
        }

        public abstract MovementBase Clone(Creature forCreature, object source);

        public bool CanMoveInteract(ICore other)
        {
            if (other is ICoreObject _other)
            {
                bool _canInteract(ICoreObject target, ICoreObject candidate, bool? firstResult)
                {
                    var _actor = CoreObject as CoreActor;
                    var _interact = new Interaction(_actor, this, target,
                        new CanMoveInteract(_actor, candidate, firstResult));
                    target.HandleInteraction(_interact);
                    return _interact.Feedback.OfType<ValueFeedback<bool>>().FirstOrDefault()?.Value ?? false;
                }

                // if first fails, won't double check...
                var _first = _canInteract(CoreObject, _other, null);
                return _canInteract(_other, CoreObject, _first);
            }
            return false;
        }

        public bool IsLegalPosition(Locator locator)
        {
            var _atkSize = (CoreObject as Creature)?.Sizer.Size.Order ?? Size.Medium.Order;
            var _overlap = (from _l in locator.OverlappedLocators(PlanarPresence.Both)
                            where CanMoveInteract(_l.ICore)
                            let _oChief = _l.Chief as Creature
                            where _oChief != null
                            select new
                            {
                                Chief = _oChief,
                                Size = _oChief.Sizer.Size.Order
                            }).ToList();
            if (_overlap.Any(_o => _o.Chief.IsImpassible()))
            {
                return false;
            }
            else if (_atkSize <= Size.Tiny.Order)
            {
                return true;
            }
            else if ((from _o in _overlap
                      where (_o.Size < (_atkSize + 3))  // not too big
                      && (_o.Size > (_atkSize - 3))     // not too small
                      select _o.Chief).Any())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}