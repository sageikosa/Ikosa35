using System;
using System.Linq;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public class CoreProcess
    {
        public CoreProcess(CoreStep rootStep, string name)
        {
            _RootStep = rootStep;
            if (rootStep != null)
            {
                rootStep.Process = this;
            }

            _IsActive = true;
            _Reactives = new Queue<CoreStep>();
            _Completions = new Queue<CoreStep>();
            _Finalizers = null;
            _IsHeld = false;
            _Manager = null;
            _Name = name;
        }

        #region state
        private readonly Queue<CoreStep> _Reactives;
        private readonly Queue<CoreStep> _Completions;
        private List<IFinalizeProcess> _Finalizers;
        private CoreProcessManager _Manager;
        protected CoreStep _RootStep;
        private bool _IsActive;
        private bool _IsHeld;
        private readonly string _Name;
        #endregion

        /// <summary>Typically, a triggered step that can terminate an action will enqueue a terminator step, which sets this flag to false.</summary>
        public bool IsActive { get => _IsActive; set => _IsActive = value; }

        /// <summary>A held process is not on the stack (currently)</summary>
        public bool IsHeld { get => _IsHeld; set => _IsHeld = value; }

        /// <summary>Append steps that are pre-emptive to the current processing state in the activity</summary>
        public void AppendPreEmption(params CoreStep[] steps)
        {
            foreach (var _step in steps.Where(_s => _s != null))
            {
                _step.Process = this;
                _Reactives.Enqueue(_step);
            }
        }

        /// <summary>First step as defined by the action</summary>
        public CoreStep RootStep
            => _RootStep;

        /// <summary>Append steps that are activity completing (but not when it is de-activated)</summary>
        public void AppendCompletion(params CoreStep[] steps)
        {
            foreach (var _step in steps.Where(_s => _s != null))
            {
                _step.Process = this;
                _Completions.Enqueue(_step);
            }
        }

        public CoreProcessManager ProcessManager
        {
            get => _Manager;
            internal set { _Manager = value; OnInitProcessManager(); }
        }

        public string Name
            => _Name;

        public virtual string Description
            => _Name;

        protected virtual void OnInitProcessManager() { }

        protected virtual void OnProcessInitiation() { }
        internal void StartProcess() { OnProcessInitiation(); }

        protected virtual void OnProcessCompletion(bool deactivated) { }
        internal void ProcessCompletion(bool deactivated)
        {
            OnProcessCompletion(deactivated);
            if (_Finalizers?.Any() ?? false)
            {
                foreach (var _final in _Finalizers)
                {
                    _final.FinalizeProcess(this, deactivated);
                }
            }
        }

        #region public CoreStep GetCurrentStep()
        /// <summary>Get the next step to be processed in the activity</summary>
        public CoreStep GetCurrentStep()
        {
            var _next = GetStepFromQueue(_Reactives);

            // if we picked up reactive step processing, continue with it
            if (_next != null)
            {
                return _next;
            }

            // otherwise, see what is current in the regular action-based steps
            _next = _RootStep?.GetCurrentStep();
            if (_next != null)
            {
                if (_next.IsNewRoot)
                {
                    _RootStep = _next;
                }

                return _next;
            }

            // finally look through completion steps (not injected or following steps...)
            return GetStepFromQueue(_Completions);
        }

        private static CoreStep GetStepFromQueue(Queue<CoreStep> queue)
        {
            while (queue.Count > 0)
            {
                // look at the topmost (next) following step, and find out which step it thinks is next (may be nested)
                var _next = queue.Peek().GetCurrentStep();

                // if it found something ...
                if (_next != null)
                {
                    // returning the next step
                    return _next;
                }

                // ... discarding the top step, don't want to save it, it's really done
                queue.Dequeue();
            }

            // nothing
            return null;
        }
        #endregion

        public void AddFinalizer(IFinalizeProcess finalizer)
        {
            _Finalizers ??= [];
            if (!_Finalizers.Contains(finalizer))
            {
                _Finalizers.Add(finalizer);
            }
        }
    }
}
