using System;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class AttackStep : CoreStep
    {
        public AttackStep(CoreTargetingProcess process, IAttackSource attackSource)
            : base(process)
        {
            _Source = attackSource;
        }

        private IAttackSource _Source;

        public IAttackSource AttackSource => _Source;

        public CoreTargetingProcess TargetProcess
            => Process as CoreTargetingProcess;

        public CoreActor Actor
            => (Process as CoreActivity)?.Actor;

        public Guid ActorID
            => Actor?.ID ?? Guid.Empty;

        protected override StepPrerequisite OnNextPrerequisite()
            => null;

        public override bool IsDispensingPrerequisites
            => false;

        protected override bool OnDoStep()
        {
            // attack step gets attack information from target process
            if (TargetProcess?.Targets.OfType<AttackTarget>().FirstOrDefault() is AttackTarget _target)
            {
                // shunt some status to the actor (before completing attack)
                NotifyStep _atkResult = null;
                if (TargetProcess is CoreActivity _activity)
                {
                    _atkResult = _activity.GetActivityResultNotifyStep();
                    AppendFollowing(_atkResult);
                }

                // use a step interaction to provide process context
                var _atkInteract = new StepInteraction(this, Actor, AttackSource, _target.Target, _target.Attack);

                // first, perform an attack against the target (if one is defined)
                if (_target.Target != null)
                {
                    _target.Target.HandleInteraction(_atkInteract);
                }
                else
                {
                    // otherwise, just transit the attack
                    var _handler = new TransitAttackHandler();
                    _handler.HandleInteraction(_atkInteract);
                }

                // altertions added to info
                var _atkFB = _atkInteract.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                _atkResult.SysNotify.Infos.Insert(0, new Info
                {
                    Message = (_atkFB?.Hit ?? false)
                            ? (_atkFB.CriticalHit ? @"Critical Hit" : @"Hit")
                            : @"No Hit"
                });
                _atkResult.SysNotify.Infos.AddRange(_atkInteract.InteractData.Alterations
                            .OfType<AttackRollAlteration>()
                            .Where(_alt => _alt.InformAttacker)
                            .SelectMany(_alt => _alt.Information));

                // push it on to the attack result step
                AppendFollowing(new AttackResultStep(TargetProcess, null, _atkInteract, AttackSource));
            }

            // done
            return true;
        }
    }
}
