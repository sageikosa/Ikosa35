using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class TryOverrun : CoreStep
    {
        public TryOverrun(CoreActivity activity, OverrunningBudget overrunning)
            : base(activity)
        {
            _Overrunning = overrunning;
        }

        #region data
        private OverrunningBudget _Overrunning;
        #endregion

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
                var _atkSize = _critter.Sizer.Size.Order;
                var _overlappingChiefs = (from _l in _loc.OverlappedLocators(PlanarPresence.Both)
                                          let _cChief = _l.Chief as Creature
                                          where MovementAction.Movement.CanMoveInteract(_cChief)
                                          select _cChief).ToList();
                if (_overlappingChiefs.Any(_c => _c.IsImpassible()))
                {
                    // at least one impassible creature
                    AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                }
                else
                {
                    var _unfriendlies = (from _c in _overlappingChiefs
                                         where _c.IsUnfriendly(_critter.ID)
                                         select new
                                         {
                                             Chief = _c,
                                             Size = _c.Sizer.Size.Order,
                                             // TODO: pass by pre-disposition?
                                             ChiefAware = _c.Awarenesses[_critter.ID] >= Senses.AwarenessLevel.Aware,
                                             ActorAware = _critter.Awarenesses[_c.ID] >= Senses.AwarenessLevel.Aware
                                         }).ToList();

                    // any blockers? all done
                    if (_unfriendlies.Any(_u => _u.ChiefAware && (_u.Size == _atkSize + 2) /* pass-by predisposition */))
                    {
                        // no more movement
                        // NOTE: whether or not movement must be undone is handled 
                        //       by legal position checks in relocationStep (already handled)
                        // NOTE: actual undo is in movement action.PopStack
                        AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                    }
                    else
                    {
                        // those we can overrun
                        var _candidates = _unfriendlies.Where(_u => _u.Size <= _atkSize + 1).ToList();
                        if (_candidates.Any())
                        {
                            if (_candidates.Count > 1)
                            {
                                // whittle down by removing the ones we can ignore
                                var _whittle = _candidates.Where(_c => _c.Size > _atkSize - 3).ToList();
                                if (_whittle.Any())
                                {
                                    if (_whittle.Count > 1)
                                    {
                                        // no more movement
                                        // NOTE: whether or not movement must be undone is handled 
                                        //       by legal position checks in relocationStep (already handled)
                                        AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                                    }
                                    else
                                    {
                                        // whittled to one, so pick that one, rest can be ignored
                                        var _target = _whittle.First();

                                        // already overrunning...
                                        if (_Overrunning != null)
                                        {
                                            // someone else?
                                            if (_Overrunning.Target != _target.Chief)
                                            {
                                                AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                                            }
                                            // NOTE: otherwise already registered, and choice was made...
                                        }
                                        else
                                        {
                                            AppendFollowing(new RegisterOverrunning(Activity, _target.Chief));
                                            AppendFollowing(new OverrunAvoid(Activity, _target.Chief, _target.ChiefAware));
                                        }
                                    }
                                }
                                else
                                {
                                    // whittled to none, so 'randomly' pick a candidate?
                                    var _target = _candidates.First();

                                    // already overrunning...
                                    if (_Overrunning != null)
                                    {
                                        // someone else?
                                        if (_Overrunning.Target != _target.Chief)
                                        {
                                            AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                                        }
                                        // NOTE: otherwise already registered, and choice was made...
                                    }
                                    else
                                    {
                                        AppendFollowing(new RegisterOverrunning(Activity, _target.Chief));
                                        AppendFollowing(new OverrunAvoid(Activity, _target.Chief, _target.ChiefAware));
                                    }
                                }
                            }
                            else
                            {
                                // only one candidate
                                var _target = _candidates.First();

                                // already overrunning...
                                if (_Overrunning != null)
                                {
                                    // someone else?
                                    if (_Overrunning.Target != _target.Chief)
                                    {
                                        AppendFollowing(new CanStillMoveStep(Activity, MovementAction?.MovementBudget));
                                    }
                                    // NOTE: otherwise already registered, and choice was made...
                                }
                                else
                                {
                                    AppendFollowing(new RegisterOverrunning(Activity, _target.Chief));
                                    AppendFollowing(new OverrunAvoid(Activity, _target.Chief, _target.ChiefAware));
                                }
                            }
                        }
                        else
                        {
                            // NOTE: no target candidates
                            //       ergo, no registration of overrunning (no target), no choices by targets
                            //       nor is there any blocking
                        }
                    }
                }
            }
            return true;
        }
    }
}
