using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreStep
    {
        #region ctor()
        /// <summary>associates the activity, and enqueues the step within the predecessor's following steps queue</summary>
        protected CoreStep(CoreStep predecessor)
            : this(predecessor.Process)
        {
            predecessor.AppendFollowing(this);
        }

        /// <summary>
        /// associates the process, typically used for root steps, reactive steps and completion steps
        /// process can be null, allowing the CoreProcess to set the root step internally
        /// </summary>
        protected CoreStep(CoreProcess process)
        {
            _Process = process;
            _Followers = new Queue<CoreStep>();
            _Complete = false;
            _Dispensed = [];
            _ID = Guid.NewGuid();
        }
        #endregion

        #region data
        private CoreProcess _Process;
        private Queue<CoreStep> _Followers;
        private bool _Complete;
        private Collection<StepPrerequisite> _Dispensed;
        private Guid _ID;
        #endregion

        #region public CoreProcess Process { get; internal set; }
        public CoreProcess Process
        {
            get { return _Process; }
            internal set
            {
                if (_Process == null)
                {
                    _Process = value;
                    foreach (var _step in _Followers)
                    {
                        _step.Process = _Process;
                    }
                }
            }
        }
        #endregion

        public bool IsComplete { get { return _Complete; } protected set { _Complete = value; } }

        public void PreEmptStatus(SysNotify notify, params Guid[] receivers)
        {
            Process?.AppendPreEmption(new NotifyStep(Process, notify)
            {
                InfoReceivers = receivers
            });
        }

        public void PreEmptStatus(Guid receiver, SysNotify notify)
        {
            Process?.AppendPreEmption(new NotifyStep(Process, notify)
            {
                InfoReceivers = new Guid[] { receiver }
            });
        }

        #region public void EnqueueNotify(SysNotify notify, params Guid[] receivers)
        /// <summary>Creates a NotifyStep for multiple receivers, and a single SysNotify</summary>
        public void EnqueueNotify(SysNotify notify, params Guid[] receivers)
        {
            AppendFollowing(new NotifyStep(Process, notify)
            {
                InfoReceivers = receivers
            });
        }
        #endregion

        /// <summary>
        /// True if this step is intended to replace the current root step.
        /// Used by tracker processes to truncate the follower chain.
        /// </summary>
        public virtual bool IsNewRoot => false;

        public virtual string Name => GetType().FullName;
        public Guid ID => _ID;

        public void StartNewProcess(CoreStep step, string name)
            => Process.ProcessManager.StartProcess(new CoreProcess(step, name));

        /// <summary>Append steps that directly follow this step</summary>
        public void AppendFollowing(params CoreStep[] steps)
        {
            foreach (var _step in steps.Where(_s => _s != null))
            {
                _step.Process = Process;

                // prevent step from being added twice
                if (!_Followers.Contains(_step))
                {
                    _Followers.Enqueue(_step);
                }
            }
        }

        /// <summary>Steps that directly follow this step</summary>
        public IEnumerable<CoreStep> FollowingSteps => _Followers.Select(_f => _f);

        #region public CoreStep GetCurrentStep()
        /// <summary>Gets the next step that is not complete.  If this returns null, this entire step and followers are done.</summary>
        public CoreStep GetCurrentStep()
        {
            if (!IsComplete)
            {
                // this is the current step if it is not complete
                return this;
            }

            // look through pending steps
            while (_Followers.Count > 0)
            {
                // look at the topmost (next) following step, and find out which step it thinks is next (may be nested)
                CoreStep _next = _Followers.Peek().GetCurrentStep();

                // if it found something...
                if (_next != null)
                {
                    // return next current step
                    return _next;
                }

                // ... discarding the top step, don't want to save it, its really done
                _Followers.Dequeue();
            }

            // no pending steps
            return null;
        }
        #endregion

        #region public StepPrerequisite NextPrerequisite()
        /// <summary>After calling this, communication manager should see if CoreProcessManager current step has changed</summary>
        public StepPrerequisite NextPrerequisite()
        {
            var _preReq = OnNextPrerequisite();
            if (_preReq != null)
            {
                // target prerequisite reactors
                if (_preReq.Fulfiller != null)
                {
                    if (_preReq.Fulfiller is IAdjunctable _fulfiller)
                    {
                        foreach (var _reactor in _fulfiller.Adjuncts.OfType<IReactToPrerequisite>().Where(_r => _r.IsFunctional))
                        {
                            _reactor.ReactToPrerequisite(this, _preReq);
                        }
                    }
                }

                // setting reactors
                if (Process.ProcessManager != null)
                {
                    foreach (var _reactor in Process.ProcessManager.ContextReactors.OfType<IReactToPrerequisite>())
                    {
                        _reactor.ReactToPrerequisite(this, _preReq);
                    }
                }
                _Dispensed.Add(_preReq);
            }
            return _preReq;
        }
        #endregion

        /// <summary>Dispense prerequisite that can be presented.</summary>
        protected abstract StepPrerequisite OnNextPrerequisite();

        /// <summary>True if prerequisites have not been all been dispensed.</summary>
        public abstract bool IsDispensingPrerequisites { get; }

        // TODO: disjuncted prerequisites

        #region Prerequisite Inquiry
        /// <summary>Get first dispensed prerequisite that matches the specified type.</summary>
        public PreType GetPrerequisite<PreType>() where PreType : StepPrerequisite
            => (AllPrerequisites<PreType>()).FirstOrDefault() as PreType;

        public IEnumerable<StepPrerequisite> DispensedPrerequisites => _Dispensed.AsEnumerable();

        /// <summary>Enumerate all dispensed prerequisites that match the specified type.</summary>
        public IEnumerable<PreType> AllPrerequisites<PreType>() where PreType : StepPrerequisite
            => _Dispensed.OfType<PreType>();

        /// <summary>Enumerate all dispensed prerequisites that match the specified type.</summary>
        public IEnumerable<PreType> AllPrerequisites<PreType>(string bindKey) where PreType : StepPrerequisite
            => (from _pre in _Dispensed.OfType<PreType>()
                where _pre.BindKey.Equals(bindKey)
                select _pre as PreType);

        /// <summary>Enumerate all dispensed prerequisites that match the specified type.</summary>
        public IEnumerable<PreType> AllPrerequisites<PreType>(Func<string, bool> keyTest) where PreType : StepPrerequisite
            => _Dispensed.OfType<PreType>().Where(_pre => keyTest(_pre.BindKey));

        /// <summary>Returns first prerequisite that has indicated it fails the process (or null if none do)</summary>
        public StepPrerequisite FailingPrerequisite => _Dispensed.FirstOrDefault(_pre => _pre.FailsProcess);
        #endregion

        /// <summary>True if done dispensing, and either no dispensed or all are ready</summary>
        public bool CanDoStep
            => !IsDispensingPrerequisites
            && ((_Dispensed.Count == 0) || _Dispensed.All(_pre => _pre.IsReady));

        /// <summary>
        /// Typically, a step enqueues following steps at the end of processing.  
        /// Triggers may enqueue as reactions and will typically precede normal followers.
        /// </summary>
        internal void DoStep()
        {
            if (CanDoStep)
            {
                _Complete = OnDoStep();
            }
        }

        /// <summary>Override to perform step</summary>
        protected abstract bool OnDoStep();

        public virtual Info GetStepInfo(CoreActor actor)
            => new Info
            {
                Message = $@"Step: {Name}"
            };
    }
}
