using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Default handler for GetLocatorsData
    /// </summary>
    [Serializable]
    public class GetLocatorsHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetLocatorsData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is GetLocatorsData _gld)
            {
                var _loc = (workSet?.Target as IAdjunctable)?.GetLocated()?.Locator;
                if (_loc != null)
                {
                    workSet?.Feedback.Add(new ValueFeedback<bool>(this,
                        _gld.Add(new GetLocatorsResult
                        {
                            Locator = _loc,
                            MoveCost = 1d
                        })));
                }
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
    }
}
