using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class ProneEffect : Adjunct, IActionProvider, IActionFilter, IActionSource, IProcessFeedback
    {
        public ProneEffect(object source)
            : base(source)
        {
        }

        #region public override bool CanAnchor(IAdjunctable newAnchor)
        /// <summary>
        /// prone can only be added under certain movement modes
        /// </summary>
        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            if (newAnchor is Creature _critter)
            {
                var _loc = Locator.FindFirstLocator(_critter);
                if (_loc != null)
                {
                    // these are all ok to apply prone
                    if (_loc.ActiveMovement is LandMovement)
                    {
                        return true;
                    }

                    if (_loc.ActiveMovement is FallMovement)
                    {
                        return true;
                    }

                    if (_loc.ActiveMovement is JumpMovement)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Add(new Condition(Condition.Prone, this));
                _critter.Actions.Providers.Add(this, this);
                _critter.Actions.Filters.Add(this, this);
                _critter.AddIInteractHandler(this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Remove(_critter.Conditions[Condition.Prone, this]);
                _critter.Actions.Providers.Remove(this);
                _critter.Actions.Filters.Remove(this);
                _critter.RemoveIInteractHandler(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget as LocalActionBudget)?.CanPerformBrief ?? false)
            {
                yield return new StandUp(this as IActionSource, @"101");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Prone", ID);
        #endregion

        #region IActionFilter Members
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // crawl not suppressed
            if (action is Crawl)
            {
                return false;
            }

            if (action is ISupplyAttackAction _supply)
            {
                switch (_supply.Attack)
                {
                    case BowStrike _bs:
                        return true;

                    case SlingStrike _ss:
                        return true;

                    case ThrowStrike _ts:
                        return true;
                }
            }

            if (action is DirectSplatterAttack)
            {
                return true;
            }

            // movements suppressed, all else OK
            return (action.Source is MovementBase);
        }
        #endregion

        public override object Clone()
            => new ProneEffect(Source);

        #region IProcessFeedback
        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield return typeof(VisualPresentationData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor
            if (typeof(AddAdjunctData).Equals(interactType))
            {
                return true;
            }

            if (typeof(VisualPresentationData).Equals(interactType))
            {
                return true;
            }

            return false;
        }

        public void ProcessFeedback(Interaction workSet)
        {
            if ((workSet?.InteractData is AddAdjunctData _addAdjunct)
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_b => _b.Value))
            {
                // these adjuncts indicate movement that ends prone
                if (_addAdjunct.Adjunct is Climbing)
                {
                    Eject();
                }
                else if (_addAdjunct.Adjunct is InFlight)
                {
                    Eject();
                }
                else if (_addAdjunct.Adjunct is Burrowing)
                {
                    Eject();
                }
                else if (_addAdjunct.Adjunct is Swimming)
                {
                    Eject();
                }
                else if (_addAdjunct.Adjunct is SmallClimbing)
                {
                    Eject();
                }
                else if (_addAdjunct.Adjunct is Tumbling)
                {
                    Eject();
                }
            }
            else if (workSet?.InteractData is VisualPresentationData _visData)
            {
                // modify visualization
                var _visBack = workSet.Feedback.OfType<VisualModelFeedback>().FirstOrDefault();
                if ((_visBack != null) && (Anchor is Creature _critter))
                {
                    // get gravity, and set base face of model based on heading and prone-ness
                    var _loc = Locator.FindFirstLocator(_critter);
                    var _gravity = _loc.GetGravityFace();

                    var _lOffSet = _critter.Length / 2;
                    var _hOffSet = _critter.Height / 2;

                    _visBack.ModelPresentation.Tilt = -90d;
                    _visBack.ModelPresentation.IntraModelOffset =
                        _visBack.ModelPresentation.IntraModelOffset + (_gravity.ReverseFace().GetNormalVector() * _lOffSet);
                    var _back = _gravity.BackFace((_critter.Heading / 2) * 2);
                    if (_back.HasValue)
                    {
                        _visBack.ModelPresentation.IntraModelOffset =
                            _visBack.ModelPresentation.IntraModelOffset + (_back.Value.GetNormalVector() * _hOffSet);
                    }
                }
            }
        }
        #endregion

        // IActionSource
        public IVolatileValue ActionClassLevel
            => (Anchor as Creature)?.ActionClassLevel ?? new Deltable(1);
    }
}
