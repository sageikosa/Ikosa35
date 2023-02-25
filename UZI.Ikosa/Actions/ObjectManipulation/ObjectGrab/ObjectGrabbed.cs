using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Universal;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{

    /// <summary>GroupMasterAdjunct representing the grabbed object</summary>
    [Serializable]
    public class ObjectGrabbed : GroupMasterAdjunct, IActionProvider, IProcessFeedback
    {
        /// <summary>GroupMasterAdjunct representing the grabbed object</summary>
        public ObjectGrabbed(ObjectGrabGroup group)
            : base(group, group)
        {
        }

        public ObjectGrabGroup ObjectGrabGroup
            => Group as ObjectGrabGroup;

        public ICoreObject CoreObject
            => Anchor as ICoreObject;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is ICoreObject) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new ObjectGrabbed(ObjectGrabGroup);

        protected override void OnActivate(object source)
        {
            CoreObject?.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            CoreObject?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
            if (CoreObject is Furnishing _furnish
                && !_furnish.IsUprightAllowed
                && ((_furnish.Orientation.Upright == Verticality.Upright)
                    || (_furnish.Orientation.Upright == Verticality.Inverted)))
            {
                // TODO: if furnishing left in an invalid orientation when released, must adjust accordingly
                // TODO: leaning...?
                _furnish.Orientation.SetOrientation(Verticality.OnSideBottomOut, null, null);
            }
        }

        #region IProcessFeedback
        public void ProcessFeedback(Interaction workSet)
        {
            // successfully added this locator to the gathering set?
            if (workSet.InteractData is GetLocatorsData _gld
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value))
            {
                // if any were retrieved, then add the object also...
                var _move = workSet.Source as MovementBase;
                Parallel.ForEach(
                    ObjectGrabGroup?.ObjectGrabbers.Select(_og => _og.Creature),
                    (_crt => _gld.AddObject(_crt, _move)));

                // movement sourced from something else increases cost
                if (_move.CoreObject != CoreObject)
                {
                    var _gravity = CoreObject?.GetLocated()?.Locator?.GetGravityFace() ?? Visualize.AnchorFace.ZLow;
                    var _faces = AnchorFaceListHelper.Create(_gld.StepDestinationTarget.CrossingFaces(_gravity, _gld.StepIndex));
                    _gld.SetCost(CoreObject, ObjectGrabbedCostData.GetCost(CoreObject, _move, _faces));
                }
            }
        }

        public void HandleInteraction(Interaction workSet)
        {
            // FEEDBACK ONLY!
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetLocatorsData);
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (interactType == typeof(GetLocatorsData))
            && (existingHandler is GetLocatorsHandler);

        #endregion

        #region IActionProvider
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget.Actor is Creature _critter)
            {
                yield return new ReleaseObject(CoreObject, @"202");
                // TODO: pick up and put in holding slot...
                // TODO: drop (if carrying)

                var _budget = budget as LocalActionBudget;
                if (CoreObject is FlatPanel _flatPanel)
                {
                    var _flexPanel = _flatPanel as FlexibleFlatPanel;
                    if (_budget?.CanPerformBrief ?? false)
                    {
                        if (_flatPanel.CanFlip)
                        {
                            yield return new FlatObjectManipulation(_flatPanel,
                                new ActionTime(Contracts.TimeType.Brief), @"202", @"Flip", _flexPanel != null);
                        }
                        if (_flexPanel != null)
                        {
                            switch (_flexPanel.FlexState)
                            {
                                case FlexibleFlatState.Flat:
                                    yield return new FlatObjectManipulation(_flatPanel,
                                        new ActionTime(Contracts.TimeType.Brief), @"205", @"Fold", true);
                                    yield return new FlatObjectManipulation(_flatPanel,
                                        new ActionTime(Contracts.TimeType.Brief), @"207", @"Roll", true);
                                    yield return new FlatObjectManipulation(_flatPanel,
                                        new ActionTime(Contracts.TimeType.Brief), @"209", @"Ball", true);
                                    break;
                                case FlexibleFlatState.Balled:
                                    yield return new FlatObjectManipulation(_flatPanel,
                                        new ActionTime(Contracts.TimeType.Brief), @"210", @"Unball", true);
                                    break;
                                case FlexibleFlatState.Rolled:
                                    yield return new FlatObjectManipulation(_flatPanel,
                                        new ActionTime(Contracts.TimeType.Brief), @"208", @"Unroll", true);
                                    break;
                                case FlexibleFlatState.Folded:
                                    yield return new FlatObjectManipulation(_flatPanel,
                                        new ActionTime(Contracts.TimeType.Brief), @"206", @"Unfold", true);
                                    break;
                            }
                        }
                    }
                }

                if (CoreObject is Furnishing _furnishing)
                {
                    if ((_budget?.TopActivity?.Action is PivotFurniture _pivot)
                        && (_pivot.TimeCost.ActionTimeType == Contracts.TimeType.Brief)
                        && (_pivot.TurnsLeft > 0))
                    {
                        // yield out a continuation pivot if possible
                        yield return new PivotFurniture(_furnishing, new ActionTime(Contracts.TimeType.SubAction));
                    }
                    else if (_budget?.CanPerformBrief ?? false)
                    {
                        // otherwise a brief pivot
                        yield return new PivotFurniture(_furnishing, new ActionTime(Contracts.TimeType.Brief));
                    }
                    if (_budget?.CanPerformBrief ?? false)
                    {
                        // tilt if effort is possible
                        yield return new TiltFurniture(_furnishing);
                        // TODO: lift
                    }
                }
                else if (CoreObject is Conveyance _conveyance)
                {
                    // not sure what the cost of these actions will be, so don't lock into brief
                    if ((_budget?.TopActivity?.Action is PivotConveyance _pivot)
                        && (_pivot.TimeCost.ActionTimeType == Contracts.TimeType.Brief)
                        && (_pivot.TurnsLeft > 0))
                    {
                        // yield out a continuation pivot if possible
                        yield return new PivotConveyance(_conveyance, new ActionTime(Contracts.TimeType.SubAction));
                    }
                    else if (_budget?.CanPerformBrief ?? false)
                    {
                        // otherwise a brief pivot
                        yield return new PivotConveyance(_conveyance, new ActionTime(Contracts.TimeType.Brief));
                    }

                    foreach (var _act in _conveyance.GetGrabbedActions(budget))
                    {
                        yield return _act;
                    }
                }
            }

            // TODO: MOVE WITH
            // TODO: check support against gravity after one of these MOVE or DROP operations
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return null;
        }
        #endregion
    }
}
