using System;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Uzi.Ikosa.Senses;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SwimMovement : MovementBase, IMonitorChange<DeltaValue>
    {
        public SwimMovement(int speed, Creature creature, object source, bool shiftable, LandMovement landMovement)
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

        public override string Name
            => IsNaturalSwimmer ? @"Swim" : @"Swim (skill-based)";

        #region data
        private bool _Shift;
        private LandMovement _Land = null;
        #endregion

        public override bool CanShiftPosition => _Shift;
        public override bool FailsAboveMaxLoad => true;
        protected override bool MustVisualizeMovement => false;
        public override bool SurfacePressure => false;

        public Creature Creature
            => _Land.CoreObject as Creature;

        /// <summary>returns true if the swim movement is not sourced by the swim skill</summary>
        public bool IsNaturalSwimmer
            => _Land == null;

        public override bool IsNativeMovement
            => IsNaturalSwimmer;

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

        #region protected override IGeometricRegion GetNextGeometry(CoreTargettingProcess process)
        protected override MovementLocatorTarget GetNextGeometry(CoreTargetingProcess process,
            Locator locator, CellLocation leadCell, Dictionary<Guid, ICore> exclusions)
        {
            var _dest = process.GetFirstTarget<StepDestinationTarget>(MovementTargets.Direction);
            var _idx = process.GetFirstTarget<ValueTarget<int>>(MovementTargets.StepIndex);
            if ((locator != null) && (leadCell != null) && (_dest != null))
            {
                var _locCore = locator.ICore as ICoreObject;
                var _gravity = locator.GetGravityFace();
                var _crossings = _dest.CrossingFaces(_gravity, _idx?.Value ?? 0);
                var _planar = locator.PlanarPresence;

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

                var _difficulty = locator.Map.SwimDifficulty(leadCell, _next, this);
                if (_difficulty == null)
                {
                    // difficulty is incalculable.. therefore, cannot swim
                    return null;
                }
                else
                {
                    // set difficulty
                    process.Targets.Add(new ValueTarget<int?>(@"Difficulty", _difficulty));
                }

                if (locator.NormalSize.XLength == 1)
                {
                    // provide a cell list
                    var _cubic = new Cubic(_next, 1, 1, 1);
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d);
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

                    // TODO: extension? (from gravity...)

                    // finalize
                    var _moveVol = new MovementVolume(_locCore, this, _crossings, _gravity, locator.Map, _planar, exclusions);
                    var _final = _moveVol.SqueezeCells(_cubic, locator.ZFit, locator.YFit, locator.XFit, 0.5d);
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

        #region public override CoreStep CostFactorStep(CoreActivity activity)
        public override CoreStep CostFactorStep(CoreActivity activity)
        {
            var _diffTarget = activity.GetFirstTarget<ValueTarget<int?>>(@"Difficulty");
            if (_diffTarget != null)
            {
                int? _getCurrent()
                    => Creature?.Skills.Skill<SwimSkill>().CurrentSwim;

                // climbing already or force a check
                var _difficulty = new Deltable(_diffTarget.Value ?? 0);
                var _current = _getCurrent();
                if ((null == _current) || (_current < _difficulty.EffectiveValue))
                {
                    return new SwimCheckStep(activity, Creature, _difficulty);
                }
            }

            // should be MoveCostCheckStep
            return base.CostFactorStep(activity);
        }
        #endregion

        #region public override void OnEndTurn()
        public override void OnEndTurn()
        {
            // expire swimming for movement
            foreach (var _swimming in CoreObject.Adjuncts.OfType<Swimming>())
            {
                _swimming.IsCheckExpired = true;
            }
            base.OnEndTurn();
        }
        #endregion

        #region public override void OnResetBudget()
        public override void OnResetBudget()
        {
            // expire swimming for movement
            foreach (var _swimming in CoreObject.Adjuncts.OfType<Swimming>())
            {
                _swimming.IsCheckExpired = true;
            }
            base.OnResetBudget();
        }
        #endregion

        #region public override void OnEndActiveMovement()
        public override void OnEndActiveMovement()
        {
            // clean up any Swimming
            foreach (var _swimming in CoreObject.Adjuncts.OfType<Swimming>().ToList())
            {
                _swimming.Eject();
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
                foreach (var _swimming in CoreObject.Adjuncts.OfType<Swimming>())
                {
                    _swimming.IsCheckExpired = true;
                }
            }
            base.OnSecondIncrementOfTotal(action);
        }
        #endregion

        public override bool CanMoveThrough(Items.Materials.Material material)
            => false;

        public override bool CanMoveThrough(CellMaterial material)
            => material is LiquidCellMaterial;

        public override MovementBase Clone(Creature forCreature, object source)
        {
            var _land = forCreature.Movements.AllMovements.OfType<LandMovement>().FirstOrDefault();
            return new SwimMovement(BaseValue, forCreature, source, CanShiftPosition, _land);
        }

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
