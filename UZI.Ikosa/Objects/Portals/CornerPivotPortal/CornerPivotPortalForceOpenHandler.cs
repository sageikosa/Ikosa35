using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Visualize;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class CornerPivotPortalForceOpenHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(ForceOpenData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet?.InteractData is ForceOpenData _forceOpen)
                && (workSet.Target is CornerPivotPortal _corner)
                && (workSet.Actor is Creature _critter))
            {
                // check if strength is successful...
                var _check = Deltable.GetCheckNotify(_critter.ID, @"Force Open", _corner.ID, @"Difficulty");
                if (_critter.Abilities.Strength.CheckValue(workSet, _forceOpen.GetStrengthRoll(), _check.CheckInfo)
                    >= _corner.ForceOpenDifficulty.QualifiedValue(workSet, _check.OpposedInfo))
                {
                    var _fType = _forceOpen.Activity.GetFirstTarget<OptionTarget>(@"Force.Type");
                    var _actLoc = _critter.GetLocated()?.Locator;
                    if (_actLoc != null)
                    {
                        // check if force-type is appropriate (inside<==>pull; outside<==>push)
                        if ((_corner.IsSideAccessible(true, _actLoc.GeometricRegion) && (_fType.Option.Key == @"Pull"))
                            || (_corner.IsSideAccessible(false, _actLoc.GeometricRegion) && (_fType.Option.Key == @"Push")))
                        {
                            // stuck!
                            var _stuck = _corner.Adjuncts.OfType<StuckAdjunct>().Where(_s => _s.IsActive).ToList();
                            if (_stuck.Any())
                            {
                                // unstick
                                foreach (var _s in _stuck)
                                {
                                    _s.Eject();
                                }
                            }

                            // blocked!
                            var _blocked = _corner.Adjuncts.OfType<OpenBlocked>().Where(_b => _b.IsActive).ToList();
                            if (_blocked.Any())
                            {
                                foreach (var _b in _blocked)
                                {
                                    // unblock
                                    _b.ForcedOpenTarget?.DoForcedOpen();
                                    _b.Eject();
                                }
                                // TODO: possibly disable/remove portal...
                            }

                            // open with valid check
                            workSet.Feedback.Add(new ValueFeedback<object>(this, _forceOpen));
                            return;
                        }
                    }
                }

                // open
                workSet.Feedback.Add(new ValueFeedback<object>(this, this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
