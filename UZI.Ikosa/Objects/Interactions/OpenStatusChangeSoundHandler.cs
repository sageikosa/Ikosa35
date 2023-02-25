using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class OpenStatusChangeSoundHandler : IInteractHandler
    {
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is OpenStatusChangeSound _change
                && workSet?.Target is IAudibleOpenable _audObj
                && _audObj.GetLocated()?.Locator.MapContext is MapContext _ctx)
            {
                var _source = _change.Target.Source;
                var _serial = _ctx.SerialState;
                var _close = _change.Target.IsClosed;

                var _sRef = (_change.Blocked)
                    ? _audObj.GetBlockedSound(_change.IDFactory, _source, _serial)
                        : ((!_change.Initial)
                        ? (_close
                            ? _audObj.GetClosedSound(_change.IDFactory,_source, _serial)
                            : _audObj.GetOpenedSound(_change.IDFactory, _source, _serial))
                        : (_close
                            ? _audObj.GetClosingSound(_change.IDFactory,_source, _serial)
                            : _audObj.GetOpeningSound(_change.IDFactory, _source, _serial)));
                if (_sRef != null)
                {
                    workSet.Feedback.Add(new OpenStatusChangedSoundFeedback(this, _sRef));
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(OpenStatusChangeSound);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
