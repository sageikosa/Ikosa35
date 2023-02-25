using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core
{
    [Serializable]
    public class ReactorQueueStep : CoreStep
    {
        /// <summary>Process next potential reactor</summary>
        public ReactorQueueStep(CoreStep predecessor, CoreStep reactSource, Queue<ICanReactToStepComplete> canReactToSteps)
            : base(predecessor)
        {
            _Step = reactSource;
            _Reactors = canReactToSteps;
        }

        /// <summary>Head of a new process to step through potential reactors</summary>
        public ReactorQueueStep(CoreStep reactSource, Queue<ICanReactToStepComplete> canReactToSteps)
            : base((CoreProcess)null)
        {
            _Step = reactSource;
            _Reactors = canReactToSteps;
        }

        #region state
        private CoreStep _Step;
        private Queue<ICanReactToStepComplete> _Reactors;
        #endregion

        public CoreStep ReactSource => _Step;
        public Queue<ICanReactToStepComplete> Reactors => _Reactors;

        public override bool IsNewRoot => true;
        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsDispensingPrerequisites => false;

        protected override bool OnDoStep()
        {
            if (Reactors.Any())
            {
                // get first
                var _reactor = _Reactors.Dequeue();

                // if reaction cannot happen (conditions may have changed)
                while (!_reactor.CanReactToStepComplete(ReactSource))
                {
                    // if more to try
                    if (Reactors.Any())
                    {
                        // get the next
                        _reactor = Reactors.Dequeue();
                    }
                    else
                    {
                        // done
                        return true;
                    }
                }

                // since we can react, do react
                _reactor.ReactToStepComplete(ReactSource);
                if (Reactors.Any())
                {
                    // any left, enqueue a new step to this process
                    new ReactorQueueStep(this, ReactSource, Reactors);
                }
            }
            return true;
        }
    }
}
