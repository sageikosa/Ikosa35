using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class StartOpenCloseStep : CoreStep
    {
        public StartOpenCloseStep(CoreProcess process, IOpenable openable, CoreActor actor, object source, double value)
            : base(process)
        {
            _Openable = openable;
            _Actor = actor;
            _Source = source;
            _Value = value;
        }

        #region state
        private IOpenable _Openable;
        private CoreActor _Actor;
        private object _Source;
        private double _Value;
        #endregion

        public IOpenable Openable => _Openable;
        public CoreActor Actor => _Actor;
        public object Source => _Source;
        public double Value => _Value;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            if (Actor is Creature _critter && !ManipulateTouch.CanManipulateTouch(_critter, Openable))
            {
                (Process as CoreActivity)?.Terminate(@"Cannot touch");
            }
            else
            {
                // start open-close
                var (_nextStatus, _tryChange, _sound) = Openable.StartOpenClose(Actor, Source, Value);
                if (_sound != null)
                {
                    // eject sound on process complete (after increasing serial state)
                    Openable?.AddAdjunct(new EjectOnFinalize(_sound, Process, true));
                }

                // following step is normally complete open close
                AppendFollowing(
                    new CompleteOpenCloseStep(Process, Openable,
                        _nextStatus, _tryChange, _sound));
            }
            return true;
        }
    }
}
