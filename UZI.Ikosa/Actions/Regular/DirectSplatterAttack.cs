using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Dice;
using Uzi.Visualize;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Splatter weapon attack targetting a creature [ActionBase (Regular)]</summary>
    [Serializable]
    public class DirectSplatterAttack : ActionBase, IBurstCaptureCapable, IRangedSourceProvider
    {
        /// <summary>Splatter weapon attack targetting a creature [ActionBase (Regular)]</summary>
        public DirectSplatterAttack(ISplatterWeapon splatterer, string orderKey)
            : base(splatterer, new ActionTime(TimeType.Regular), true, false, orderKey)
        {
            _Splat = splatterer;
        }

        #region data
        protected ISplatterWeapon _Splat;
        #endregion

        public override bool CombatList
            => true;

        public override string Key => @"Splatter.Direct";
        public override string DisplayName(CoreActor actor) => $@"Throw {_Splat.GetKnownName(actor)} to Hit Target";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Ranged Strike", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(_Splat as CoreObject, observer);
            return _obs;
        }

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if (activity.Targets.Where(_t => _t.Key.Equals(@"Target")) is AttackTarget _target)
            {
                var _interactor = _target.Target;
                var _atkInteract = new Interaction(activity.Actor, _Splat, _interactor, _target.Attack);
                var _aLoc = activity.Actor.GetLocated().Locator;
                var _map = _aLoc.Map;
                var _tLoc = Locator.FindFirstLocator(_interactor); // NOTE: this will get spatial targets as well

                var _cRef = _target.Attack.TargetCell;

                // first, perform a ranged touch attack against the target
                var _hit = false;
                if (_interactor != null)
                {
                    // try the direct approach
                    _interactor.HandleInteraction(_atkInteract);
                    _hit = _atkInteract.Feedback.OfType<AttackFeedback>().FirstOrDefault()?.Hit ?? false;
                }
                else
                {
                    // otherwise, targetting an intersection
                    _hit = (_target.Attack.IsHit
                        ?? _target.Attack.AttackScore.QualifiedValue(_atkInteract, Deltable.GetDeltaCalcNotify(activity.Actor.ID, @"Splatter").DeltaCalc) >= 5);
                }

                // expected termination cell
                if (_hit)
                {
                    return SplatterOnTarget(activity, _interactor, _aLoc, _cRef);
                }
                else
                {
                    // or just splatter somwhere nearby if it missed (Z+/-1dRInc, Y+/-1dRIncr, X+/-1dRIncr) (all!=0)
                    var _rAtk = _target.Attack as RangedAttackData;
                    var _rIncr = (Convert.ToInt32(_tLoc.GeometricRegion.NearDistance(_rAtk.AttackPoint)) / _rAtk.RangedSource.RangeIncrement);
                    var _rule = new ComplexDiceRoller(string.Format(@"1d{0}-{1}", _rIncr * 2 + 1, _rIncr + 1));

                    var _newZ = _cRef.Z;
                    var _newY = _cRef.Y;
                    var _newX = _cRef.X;
                    var _loop = 0;

                    // probablistically, this could loop forever, but that's not very likely, we'll let it go 5 times just in case
                    while ((_newZ == _cRef.Z) && (_newY == _cRef.Y) && (_newX == _cRef.X) && (_loop < 5))
                    {
                        // randomly pick new potential location...
                        _newZ = _cRef.Z + _rule.RollValue(activity.Actor.ID, Key, @"OffZ");
                        _newY = _cRef.Y + _rule.RollValue(activity.Actor.ID, Key, @"OffY");
                        _newX = _cRef.X + _rule.RollValue(activity.Actor.ID, Key, @"OffX");
                        _loop++;
                    }

                    // create new plan line
                    var _missPt = new Intersection(_newZ, _newY, _newX);
                    var _factory = new SegmentSetFactory(_map, _aLoc.GeometricRegion, _rAtk.TargetCell?.ToCellPosition(),
                        ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Effect);
                    var _newLine = _map.SegmentCells(_rAtk.AttackPoint, _missPt.Point3D(), _factory, _aLoc.PlanarPresence);
                    if (_newLine.BlockedCell.IsActual)
                    {
                        return new MultiNextStep(activity, SplatterAtEnd(activity, _newLine), null);
                    }
                    else
                    {
                        // otherwise, if there is nothing solid at the target point when it gets there, drop it until it hits something
                        // ... but only let it drop so far, otherwise its simply gone...
                        var _dropLine = _map.SegmentCells(_missPt.Point3D(), _missPt.Point3D() + _map.GetGravityDropVector3D(_missPt),
                            _factory, _aLoc.PlanarPresence);
                        if (_dropLine.BlockedCell.IsActual)
                        {
                            return new MultiNextStep(activity, SplatterAtEnd(activity, _dropLine), null);
                        }
                        else
                        {
                            _Splat.DoneUseItem();
                        }
                    }
                }
            }

            // no valid target and return
            return null;
        }
        #endregion

        #region protected virtual CoreStep SplatterOnTarget(CoreActivity activity, IInteract interactor, LineSet planLine, LinearCellRef targetCell)
        /// <summary>
        /// Hits a target directly, and splatters everything around it.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="planLine"></param>
        /// <returns></returns>
        protected virtual CoreStep SplatterOnTarget(CoreActivity activity, IInteract interactor, Locator locator, ICellLocation targetCell)
        {
            // then, step to direct and splatter application...
            var _step = new ApplySplatterStep(activity, _Splat, true, interactor);

            // get splattered targets
            var _cLoc = new CellLocation(targetCell.Z - 1, targetCell.Y - 1, targetCell.X - 1);
            var _cube = new Geometry(new CubicBuilder(new GeometricSize(3, 3, 3), new CellLocation(1, 1, 1)), _cLoc, true);

            // return everything (except indirect on the direct target)
            var _burst = new BurstCapture(locator.MapContext, activity, _cube, targetCell.AllIntersections().First(), this,
                locator.PlanarPresence);
            foreach (var _follow in _burst.DoBurst().OfType<ApplySplatterStep>().Where(_as => _as.Target != interactor))
                _step.AppendFollowing(_follow);

            // return step
            _Splat.DoneUseItem();
            return _step;
        }
        #endregion

        #region protected virtual IEnumerable<CoreStep> SplatterAtEnd(CoreProcess process, LineSet planLine)
        /// <summary>
        /// Splatters in a 2x2x2 burst around the last unblocked cell of the plan line.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="planLine"></param>
        /// <returns></returns>
        protected virtual IEnumerable<CoreStep> SplatterAtEnd(CoreProcess process, SegmentSet planLine)
        {
            // if it is blocked before it gets there, indirect splatter from last unblocked cell's exit
            var _indirLoc = new CellLocation(planLine.UnblockedCell.Z - 1, planLine.UnblockedCell.Y - 1, planLine.UnblockedCell.X - 1);
            var _indirCube = new Geometry(new CubicBuilder(new GeometricSize(2, 2, 2)), _indirLoc, true); // TODO: need offset?

            // splatter everything nearby
            var _burst = new BurstCapture(planLine.Map.MapContext, process, _indirCube, planLine.UnblockedCell.NearestExit, this,
                planLine.PlanarPresence);
            foreach (var _step in _burst.DoBurst())
            {
                yield return _step;
            }

            // done
            _Splat.DoneUseItem();
            yield break;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new TouchAim(@"Target", @"Target", _Splat.Lethality, ImprovedCriticalRangedTouchFeat.CriticalThreatStart(activity.Actor),
                this, FixedRange.One, FixedRange.One, new FixedRange(_Splat.MaxRange), new CreatureTargetType(), new ObjectTargetType())
            {
                // NOTE: regular ranged touch, but requesting a target cell as well
                UseCellForIndirect = true
            };
            yield break;
        }
        #endregion

        #region IBurstCapture Members

        public void PostInitialize(BurstCapture burst) { }

        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // TODO: check line of effect from burst origin to locator...
            // if the chief is not a creature, splatter the object
            if (!typeof(Creature).IsAssignableFrom(locator.Chief.GetType()))
            {
                yield return new ApplySplatterStep(burst.Source as CoreActivity, _Splat, false, locator.Chief);
            }

            // splattering all creatures...
            foreach (var _obj in locator.AllConnectedOf<Creature>())
            {
                yield return new ApplySplatterStep(burst.Source as CoreActivity, _Splat, false, _obj);
            }
            yield break;
        }

        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
            => selection;

        #endregion

        public IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target)
            => _Splat;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
