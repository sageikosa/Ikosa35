using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class CompleteOpenCloseStep : CoreStep
    {
        public CompleteOpenCloseStep(CoreProcess process, IOpenable openable, OpenStatus targetState, bool tryChange, SoundParticipant sound)
            : base(process)
        {
            _Openable = openable;
            _State = targetState;
            _TryChange = tryChange;
            _Participant = sound;
        }

        #region state
        private IOpenable _Openable;
        private OpenStatus _State;
        private bool _TryChange;
        private SoundParticipant _Participant;
        #endregion

        public IOpenable Openable => _Openable;
        public OpenStatus TargetState => _State;
        public bool TryChange => _TryChange;
        public SoundParticipant SoundParticipant => _Participant;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            // if we get this far, try to change the open/close state (may still fail)
            _Openable.CompleteOpenClose((_State, _TryChange, _Participant));
            return true;
        }
    }
}
