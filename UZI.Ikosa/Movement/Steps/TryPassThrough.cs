using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class TryPassThrough : CoreStep
    {
        public TryPassThrough(CoreActivity activity)
            : base(activity)
        {
        }

        public CoreActivity Activity
            => Process as CoreActivity;

        public MovementAction MovementAction
            => Activity?.Action as MovementAction;

        public override bool IsDispensingPrerequisites
            => false;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        protected override bool OnDoStep()
        {
            // NOTE: tacked onto the end, so that the movement happens first, 
            var _critter = Activity?.Actor as Creature;
            var _loc = _critter?.GetLocated()?.Locator;
            if (_loc != null)
            {
                var _overlap = (from _l in _loc.OverlappedLocators(PlanarPresence.Both)
                                let _cChief = _l.Chief as Creature
                                where MovementAction.Movement.CanMoveInteract(_cChief)
                                select new
                                {
                                    Chief = _cChief,
                                    Size = _cChief.Sizer.Size.Order
                                }).ToList();
                if (_overlap.Any(_o => _o.Chief.IsImpassible()))
                {
                    // at least one impassible creature
                    AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                }
                else
                {
                    var _atkSize = _critter.Sizer.Size.Order;
                    if (_atkSize > Size.Tiny.Order)
                    {
                        var _occupiers = (from _o in _overlap
                                          where (_o.Size < (_atkSize + 3))  // not too big
                                          && (_o.Size > (_atkSize - 3))     // not too small
                                          select new
                                          {
                                              Chief = _o.Chief,
                                              Friendly = !_o.Chief.IsUnfriendly(_critter.ID),
                                              // TODO: pass by pre-disposition?
                                              ChiefAware = _o.Chief.Awarenesses[_critter.ID] >= Senses.AwarenessLevel.Aware,
                                              ActorAware = _critter.Awarenesses[_o.Chief.ID] >= Senses.AwarenessLevel.Aware
                                          }).ToList();

                        if (_occupiers.Any(_o => _o.ChiefAware && !_o.Friendly /* pass-by predisposition */))
                        {
                            // an unfriendly chief will definitely block (except if we have pass-by predisposition)
                            // NOTE: whether or not movement must be undone is handled 
                            //       by legal position checks in relocationStep (already handled)
                            // NOTE: actual undo is in movement action.PopStack
                            AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                        }
                        else if (_occupiers.All(_o => _o.Friendly && _o.ChiefAware))
                        {
                            // all occupiers are friendly chiefs that are aware of the actor so will let him pass
                        }
                        else if (_occupiers.Any(_u => !_u.ActorAware))
                        {
                            // actor is unaware of something, so clumsily bounces
                            AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                        }
                    }
                }
            }
            return true;
        }
    }
}
