using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class CreatureFallStopHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(FallStop);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.Target is Creature _critter)
                && (workSet.InteractData is FallStop _stop)
                && (workSet is StepInteraction _stepSet))
            {
                var _fallMove = (_stepSet.Step as FallingStopStep)?.BaseFallMovement
                    ?? (_stepSet.Step as LiquidFallingStopStep)?.LiquidFallMovement?.FallMovement;

                if (_fallMove?.IsUncontrolled ?? false)
                {
                    // TODO: may also be controlled by jump/tumble check?
                    _critter.AddAdjunct(new ProneEffect(this));
                    if (_critter.HasActiveAdjunct<ProneEffect>())
                    {
                        _stop.Messages.Add(@"Landed Prone");
                    }

                    _stepSet.Step.EnqueueNotify(new RefreshNotify(true, false, false, false, false), _critter.ID);
                }
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
