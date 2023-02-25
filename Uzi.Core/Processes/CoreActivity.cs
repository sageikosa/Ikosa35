using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class CoreActivity : CoreTargetingProcess
    {
        public CoreActivity(CoreActor actor, CoreAction action, List<AimTarget> targets)
            : base(actor, action.DisplayName(actor), targets)
        {
            _Action = action;
        }

        #region data
        private CoreAction _Action = null;
        #endregion

        public override string Description
            => $@"{Name} (by {Actor.Name})";

        /// <summary>Who performs the action</summary>
        public CoreActor Actor
            => MainObject as CoreActor;

        /// <summary>What action they intend to perform</summary>
        public CoreAction Action
            => _Action;

        #region public ActivityResponse CanPerform()
        public ActivityResponse CanPerform()
        {
            if (!(Actor?.BaseActivityController.WillAbortChange(this, @"Start") ?? false) && (Action != null))
            {
                ActivityResponse _response = Action.DoCanPerformActivity(this);
                return _response;
            }
            else
            {
                return new ActivityResponse(false);
            }
        }
        #endregion

        protected override void OnProcessInitiation()
        {
            // get first step as defined by the action
            Actor.BaseActivityController.DoPreValueChanged(this, @"Start");
            _RootStep = Action.DoPerformActivity(this);
            Actor.BaseActivityController.DoValueChanged(this, @"Start");
        }

        protected override void OnProcessCompletion(bool deactivated)
        {
            Actor.BaseActivityController.DoPreValueChanged(this, @"Stop");
            Actor.BaseActivityController.DoValueChanged(this, @"Stop");
            Action.ActivityFinalization(this, deactivated);
        }

        protected override void OnInitProcessManager()
        {
            Action.ProcessManagerInitialized(this);
            base.OnInitProcessManager();
        }

        public ObservedActivityInfo GetActivityInfo(CoreActor actor = null)
            => Action.GetActivityInfo(this, actor ?? Actor);

        /// <summary>
        /// Appends a pre-emption to terminate the process
        /// </summary>
        public void Terminate(params Info[] infos)
        {
            AppendPreEmption(new TerminationStep(this, _CreateActivityResult(infos))
            {
                InfoReceivers = new Guid[] { Actor?.ID ?? Guid.Empty }
            });
        }

        /// <summary>
        /// Appends a pre-emption to terminate the process
        /// </summary>
        public void Terminate(string description)
        {
            AppendPreEmption(new TerminationStep(this, _CreateActivityResult(description))
            {
                InfoReceivers = new Guid[] { Actor?.ID ?? Guid.Empty }
            });
        }

        /// <summary>
        /// <para>Appends a pre-emption to register activity.  Call early to ensure budget is consumed.</para> 
        /// <para>If the actionTime is a span, the activity will be held before normal action processing continues.</para>
        /// <para>However, other pre-emptive steps before the hold process step will still be processed.</para>
        /// </summary>
        public void EnqueueRegisterPreEmptively(CoreActionBudget budget)
        {
            AppendPreEmption(new RegisterActivityStep(this, budget));
        }

        /// <summary>Creates an ActivityResultNotify</summary>
        private ActivityResultNotify _CreateActivityResult(string description)
            => _CreateActivityResult(new Info { Message = description });

        /// <summary>Creates an ActivityResultNotify</summary>
        private ActivityResultNotify _CreateActivityResult(params Info[] infos)
            => new ActivityResultNotify(GetActivityInfo(Actor), infos);

        public NotifyStep GetNotifyStep(SysNotify notify)
            => GetNotifyStep(notify, Actor);

        /// <summary>OnPerformActivity</summary>
        public NotifyStep GetActivityResultNotifyStep(params Info[] infos)
            => GetNotifyStep(_CreateActivityResult(infos));

        /// <summary>OnPerformActivity</summary>
        public NotifyStep GetActivityResultNotifyStep(string description)
            => GetNotifyStep(_CreateActivityResult(description));

        /// <summary>Appends a NotifyStep with an ActivityResultNotify to the given step for the actor</summary>
        public void EnqueueActivityResultOnStep(CoreStep step, params Info[] infos)
        {
            step.EnqueueNotify(_CreateActivityResult(infos), Actor?.ID ?? Guid.Empty);
        }

        /// <summary>Appends a NotifyStep with an ActivityResultNotify to the given step for the actor</summary>
        public void EnqueueActivityResultOnStep(CoreStep step, string description)
        {
            step.EnqueueNotify(_CreateActivityResult(description), Actor?.ID ?? Guid.Empty);
        }

        public void SendActivityResult(string description)
            => Actor.SendSysNotify(_CreateActivityResult(description));
    }
}