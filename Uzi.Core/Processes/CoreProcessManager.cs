using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreProcessManager
    {
        #region ctor()
        protected CoreProcessManager()
        {
            _ProcessStack = new Stack<CoreProcess>();
            _Reactors = new ReactorSet();
        }
        #endregion

        #region data
        private readonly Stack<CoreProcess> _ProcessStack;
        private readonly ReactorSet _Reactors;
        private CoreStep _Current;
        #endregion

        #region public CoreStep CurrentStep { get; private set; }
        public CoreStep CurrentStep
        {
            get => _Current;
            private set
            {
                var _new = value != _Current;
                _Current = value;
                if (_new)
                {
                    CurrentCoreStep(this, new StepEventArgs(value));

                    if (_Current != null)
                    {
                        // current step entered focus with prerequisites, need to notify
                        if (_Current.DispensedPrerequisites.Any()
                            && _Current.DispensedPrerequisites.Any(_pre => !_pre.IsReady))
                        {
                            // gather all actors
                            var _actors = new List<Guid>();
                            foreach (var _pre in _Current.DispensedPrerequisites)
                            {
                                if (_pre.Fulfiller != null)
                                {
                                    if (!_actors.Contains(_pre.Fulfiller.ID))
                                        _actors.Add(_pre.Fulfiller.ID);
                                }
                                else
                                {
                                    if (!_actors.Contains(Guid.Empty))
                                        _actors.Add(Guid.Empty);
                                }
                            }

                            if (_actors.Count > 0)
                                NewPrerequisiteActors?.Invoke(this, new PrerequisiteActorsEventArgs(_actors));
                            CurrentPrerequisities?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }
        }
        #endregion

        public ReactorSet ContextReactors => _Reactors;

        public IEnumerable<CoreProcess> AllProcesses
            => _ProcessStack.ToList();

        public IEnumerable<CoreActivity> AllActivities
            => _ProcessStack.OfType<CoreActivity>().ToList();

        #region public bool StartProcess(CoreProcess process)
        /// <summary>Informs the environment that a process has started.</summary>
        /// <param name="process">CoreProcess that will start</param>
        /// <remarks>
        /// The environment checks whether there are reactors to the process.  
        /// If there are, they are given the information about the started process serially.
        /// </remarks>
        public void StartProcess(CoreProcess process)
        {
            if (_ProcessStack.Contains(process))
                return;

            // let reactors know what's going on
            var _activity = process as CoreActivity;
            process.ProcessManager = this;

            #region actor suppressors
            if (_activity != null)
            {
                foreach (var _reactor in _activity.Actor.Adjuncts.OfType<ICanReactBySuppress>().ToList())
                {
                    if (process.IsActive)
                    {
                        if (_reactor.IsFunctional)
                            _reactor.ReactToProcessBySuppress(process);
                    }
                    else
                        break;
                }
            }
            #endregion

            #region setting suppressors
            if (process.IsActive)
                foreach (var _reactor in GetReactors().OfType<ICanReactBySuppress>().ToList())
                {
                    if (process.IsActive)
                    {
                        if (_reactor.IsFunctional)
                            _reactor.ReactToProcessBySuppress(process);
                    }
                    else
                        break;
                }
            #endregion

            #region actor side effects
            if (process.IsActive)
                if (_activity != null)
                {
                    foreach (var _reactor in _activity.Actor.Adjuncts.OfType<ICanReactBySideEffect>().ToList())
                    {
                        if (process.IsActive)
                        {
                            if (_reactor.IsFunctional)
                                _reactor.ReactToProcessBySideEffect(process);
                        }
                        else
                            break;
                    }
                }
            #endregion

            #region setting side effects
            if (process.IsActive)
                foreach (var _reactor in GetReactors().OfType<ICanReactBySideEffect>().ToList())
                {
                    if (process.IsActive)
                    {
                        if (_reactor.IsFunctional)
                            _reactor.ReactToProcessBySideEffect(process);
                    }
                    else
                        break;
                }
            #endregion

            #region actor step reactions
            if (process.IsActive)
                if (_activity != null)
                {
                    foreach (var _reactor in _activity.Actor.Adjuncts.OfType<ICanReactWithNewProcess>().ToList())
                    {
                        if (process.IsActive)
                        {
                            if (_reactor.IsFunctional)
                                _reactor.ReactToProcessByStep(process);
                        }
                        else
                            break;
                    }
                }
            #endregion

            #region setting step reactions
            if (process.IsActive)
                foreach (var _reactor in GetReactors().OfType<ICanReactWithNewProcess>().ToList())
                {
                    if (process.IsActive)
                    {
                        _reactor.ReactToProcessByStep(process);
                    }
                    else
                        break;
                }
            #endregion

            // stack the activity on top
            if (process.IsActive)
            {
                UnholdProcess(process);
                process.StartProcess();
            }
        }
        #endregion

        #region public void UnholdProcess(CoreProcess process)
        /// <summary>If process is active but not in the stack, this puts it in the stack and unholds the process</summary>
        public void UnholdProcess(CoreProcess process)
        {
            if (process.IsActive && !_ProcessStack.Contains(process))
            {
                if (process.IsHeld)
                    process.IsHeld = false;
                _ProcessStack.Push(process);
                SignalCurrentProcess();
            }
        }
        #endregion

        #region protected void SignalCurrentProcess()
        protected void SignalCurrentProcess()
        {
            CurrentCoreProcess?.Invoke(this, _ProcessStack.Count > 0
                ? new ProcessEventArgs(_ProcessStack.Peek())
                : new ProcessEventArgs(null));
        }
        #endregion

        /// <summary>Get the reactors</summary>
        protected virtual IEnumerable<ICanReact> GetReactors()
            => _Reactors.AsEnumerable();

        #region private CoreStep GetCurrentStep()
        /// <summary>
        /// Provides the next step to process.  
        /// Caller should enumerate and populate prerequisites, call DoStep, and check Status afterwards.
        /// if (!Status.Success) process.IsActive = false;
        /// </summary>
        private CoreStep GetCurrentStep()
        {
            while (_ProcessStack.Count > 0)
            {
                // look at the topmost process
                var _proc = _ProcessStack.Peek();

                // is it active?
                if (_proc.IsHeld || !_proc.IsActive)
                {
                    // if held, remove from stack 
                    // NOTE: assumes something else is holding onto it and will unhold when ready)
                    _ProcessStack.Pop();
                    if (!_proc.IsActive)
                    {
                        // stop the process if it isn't active
                        _proc.ProcessCompletion(true);
                    }
                }
                else
                {
                    var _next = _proc.GetCurrentStep();
                    if (_next != null)
                    {
                        // return the next step from the topmost activity
                        CurrentStep = _next;
                        return _next;
                    }

                    // this activity didn't yield a step, so it is naturally complete
                    _ProcessStack.Pop();
                    _proc.ProcessCompletion(false);
                }
                SignalCurrentProcess();
            }

            // nothing in the stack returned a step, so no step to return
            CurrentStep = null;
            return null;
        }
        #endregion

        protected abstract void OnPreDoStep(CoreStep step);

        protected abstract IEnumerable<ICanReactToStepComplete> OrderStepCompleteReactors(
            IEnumerable<ICanReactToStepComplete> canReactToSteps);

        #region public bool DoProcess()
        public bool DoProcess()
        {
            // get current processible step
            var _step = GetCurrentStep();
            if (_step != null)
            {
                if (_step.Process.IsActive)
                {
                    // can the step be processed?
                    if (_step.CanDoStep)
                    {
                        // step
                        OnPreDoStep(_step);
                        _step.DoStep();
                        if (_step.IsComplete)
                        {
                            var _reactors = new Queue<ICanReactToStepComplete>(
                                OrderStepCompleteReactors(
                                    GetReactors()
                                    .OfType<ICanReactToStepComplete>()
                                    .Where(_r => _r.IsFunctional && _r.CanReactToStepComplete(_step))));
                            if (_reactors.Any())
                            {
                                StartProcess(
                                    new CoreProcess(new ReactorQueueStep(_step, _reactors), @"React"));
                            }
                        }
                        return true;
                    }
                    else if (CurrentStep.IsDispensingPrerequisites)
                    {
                        // still dispensing prerequisites, so see what the last one is
                        var _last = CurrentStep.DispensedPrerequisites.LastOrDefault();

                        // had no last, or the last is ready (therefore, need to get the next?)
                        if (_last?.IsReady ?? true)
                        {
                            var _actors = new List<Guid>();
                            do
                            {
                                _last = CurrentStep.NextPrerequisite();

                                // collect actors to notify of prerequisites
                                if (_last != null)
                                {
                                    if (!_actors.Contains(_last.Fulfiller?.ID ?? Guid.Empty))
                                    {
                                        _actors.Add(_last.Fulfiller?.ID ?? Guid.Empty);
                                    }
                                }
                            }
                            // still getting prerequisites, that are not serial, and step hasn't changed
                            while ((_last != null) && !_last.IsSerial && (_step == GetCurrentStep()));

                            // only send if need to
                            if (_actors.Count > 0)
                            {
                                // always send to master
                                if (!_actors.Contains(Guid.Empty))
                                    _actors.Add(Guid.Empty);

                                // service callback processing
                                NewPrerequisiteActors?.Invoke(this, new PrerequisiteActorsEventArgs(_actors));
                            }

                            // intra-appDomain callback
                            CurrentPrerequisities?.Invoke(this, new EventArgs());
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        public bool DoProcessAll()
        {
            var _processed = false;
            while (DoProcess())
                _processed = true;
            OnDoProcessAllEnd();
            return _processed;
        }

        protected abstract void OnDoProcessAllEnd();

        public void SendSysStatus(SysNotify status, params Guid[] receivers)
        {
            if (status != null)
            {
                SysStatusAvailable?.Invoke(this, new SysStatusEventArgs(status, receivers));
            }
        }

        [field:NonSerialized, JsonIgnore]
        public event EventHandler<ProcessEventArgs> CurrentCoreProcess;

        [field:NonSerialized, JsonIgnore]
        public event EventHandler<PrerequisiteActorsEventArgs> NewPrerequisiteActors;

        [field:NonSerialized, JsonIgnore]
        public event EventHandler<StepEventArgs> CurrentCoreStep;

        [field:NonSerialized, JsonIgnore]
        public event EventHandler CurrentPrerequisities;

        [field:NonSerialized, JsonIgnore]
        public event EventHandler<SysStatusEventArgs> SysStatusAvailable;
    }
}
