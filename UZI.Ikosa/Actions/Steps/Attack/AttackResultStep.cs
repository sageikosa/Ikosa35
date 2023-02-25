using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>The interaction should contain AttackData and AttackFeedback</summary>
    [Serializable]
    public class AttackResultStep : InteractionPreReqStep
    {
        /// <summary>The interaction should contain AttackData and AttackFeedback</summary>
        public AttackResultStep(CoreProcess process, IEnumerable<StepPrerequisite> preRequisites, Interaction attack, IAttackSource attackSource)
            : base(process, attack)
        {
            #region additional prerequisites
            if (preRequisites != null)
            {
                foreach (var _addPre in preRequisites)
                {
                    // if the prerequisite assumes a unique key, check for existing
                    if (_addPre.UniqueKey)
                    {
                        // if there are no matching bindKeys, add the prerequisite
                        if (_PendingPreRequisites.FirstOrDefault(_p => _p.BindKey.Equals(_addPre.BindKey)) == null)
                        {
                            _PendingPreRequisites.Enqueue(_addPre);
                        }
                    }
                    else
                    {
                        // add it!
                        _PendingPreRequisites.Enqueue(_addPre);
                    }
                }
            }
            #endregion

            foreach (var _pre in attackSource.AttackResultPrerequisites(attack))
                _PendingPreRequisites.Enqueue(_pre);

            _Source = attackSource;
        }

        private IAttackSource _Source;

        public IAttackSource AttackSource
            => _Source;

        /// <summary>May be null if no action provided</summary>
        public ISupplyAttackAction AttackAction
            => (Process as CoreActivity)?.Action as ISupplyAttackAction;

        /// <summary>May be null if nto a targetting process</summary>
        public CoreTargetingProcess TargetingProcess
            => Process as CoreTargetingProcess;

        public AttackData AttackData
            => _Interaction.InteractData as AttackData;

        public bool IsMeleeAttack
            => (_Interaction?.InteractData is MeleeAttackData)
            || (_Interaction?.InteractData.GetType() == typeof(ReachAttackData));

        public bool IsRangedAttack
            => (_Interaction?.InteractData is RangedAttackData);

        protected override bool OnDoStep()
        {
            // see if any prerequisite fails the activity
            var _fail = FailingPrerequisite;
            if (_fail != null)
            {
                // TODO: status for target...
                if (TargetingProcess is CoreActivity _activity)
                {
                    AppendFollowing(_activity.GetActivityResultNotifyStep($@"{_fail.Name} Failed"));
                }
                else
                {
                    if (AttackData.Attacker != null)
                    {
                        EnqueueNotify(new BadNewsNotify(AttackData.Attacker.ID, $@"{_fail.Name} Failed"));
                    }
                }
            }
            else
            {
                // attackee
                if (_Interaction.Target is CoreActor _attackee)
                {
                    if (TargetingProcess is CoreActivity _activity)
                    {
                        EnqueueNotify(new AttackedNotify(_attackee.ID, @"Attacked", _activity.GetActivityInfo(_attackee)), _attackee.ID);
                    }
                    else
                    {
                        EnqueueNotify(new BadNewsNotify(_attackee.ID, @"Attacked"));
                    }
                }

                AttackSource.AttackResult(this, _Interaction);

                // attacker
                EnqueueNotify(new RefreshNotify(true, false, true, false, false), AttackData.Attacker.ID);

                // attackee
                if (_Interaction.Target is CoreActor _target)
                {
                    EnqueueNotify(new RefreshNotify(true, false, true, false, false), _target.ID);
                }
            }

            // done
            return true;
        }
    }
}
