using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    public static class IkosaOpenableHelper
    {
        /// <summary>gets openState and starts sound.  If not piping into CompleteOpenClose, make sure sound lifecycle is managed</summary>
        public static (OpenStatus nextStatus, bool tryChange, SoundParticipant sound) StartOpenClose(this IOpenable self, CoreActor actor, object source, double value)
        {
            if (value != self.OpenState.Value)
            {
                // try to get an open status for the value
                var _proposed = self.GetOpenStatus(actor, source, value);

                // blocked if the proposed value isn't what we wanted
                var _blocked = (_proposed.Value != value) || !self.CanChangeOpenState(_proposed);
                Guid _idFactory() => Guid.NewGuid();

                // ask object what sound it should make (if any)
                var _interact = new Interaction(null, source, self,
                    new OpenStatusChangeSound(actor, _idFactory, self.OpenState, _proposed, true, _blocked));
                self?.HandleInteraction(_interact);
                if ((_interact.Feedback.OfType<OpenStatusChangedSoundFeedback>().FirstOrDefault()
                    is OpenStatusChangedSoundFeedback _feedback)
                    && (_feedback.SoundRef != null))
                {
                    // successful
                    var _participant = new SoundParticipant(source, new SoundGroup(source, _feedback.SoundRef));
                    self.AddAdjunct(_participant);

                    // we did try to change
                    return (_proposed, true, _participant);
                }

                // we did try to change
                return (_proposed, true, null);
            }

            // no change attempted
            return (self.OpenState, false, null);
        }

        /// <summary>Finishes opening (possibly creating or changing sound) then terminates sound</summary>
        public static void CompleteOpenClose(this IOpenable self, (OpenStatus nextStatus, bool tryChange, SoundParticipant sound) starter)
        {
            // had to really try to change
            if ((self != null) && starter.tryChange)
            {
                // save old
                var _old = self.OpenState;

                // blocked if we're not actually changing (must've been blocked at start)
                var _blocked = _old.Value == starter.nextStatus.Value;

                // try set value anyway...
                self.OpenState = starter.nextStatus;

                // blocked if we were blocked to start, or if the new value didn't take
                _blocked = _blocked || (starter.nextStatus.Value != self.OpenState.Value);

                // existing sound
                var _participant = starter.sound;
                Guid _idFactory() => _participant?.SoundGroup.ID ?? Guid.NewGuid();

                // ask object what sound it should make (if any)
                var _interact = new Interaction(null, starter.nextStatus.Source, self,
                    new OpenStatusChangeSound(null, _idFactory, _old, starter.nextStatus, false, _blocked));
                self.IncreaseSerialState();
                self.HandleInteraction(_interact);
                if ((_interact.Feedback.OfType<OpenStatusChangedSoundFeedback>().FirstOrDefault()
                    is OpenStatusChangedSoundFeedback _feedback)
                    && (_feedback.SoundRef != null))
                {
                    // successful
                    if (_participant != null)
                    {
                        // alter sound sourced by the change
                        _participant.SoundGroup.SetSoundRef(_feedback.SoundRef);
                    }
                    else
                    {
                        // no current sound sourced by this change, so make new one
                        // NOTE: no need to increase serial state, since this is the first real sound
                        _participant = new SoundParticipant(starter.nextStatus.Source,
                            new SoundGroup(starter.nextStatus.Source, _feedback.SoundRef));
                        self.AddAdjunct(_participant);
                    }
                }

                if (_participant != null)
                {
                    // eject sound
                    self.IncreaseSerialState();
                    _participant?.Eject();
                }
            }
        }
    }
}
