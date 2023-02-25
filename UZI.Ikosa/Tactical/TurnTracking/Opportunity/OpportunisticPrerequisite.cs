using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Tactical
{

    /// <summary>[Serial]</summary>
    [Serializable]
    public class OpportunisticPrerequisite : StepPrerequisite
    {
        /// <summary>[Serial]</summary>
        public OpportunisticPrerequisite(
            CoreActivity activity,
            CoreActor actor,
            IEnumerable<(IActionProvider, OpportunisticAttack)> attacks
            )
            : base(activity, actor, null, activity.Actor, @"Opportunity.WillAct", @"Opportunistic Attack?")
        {
            _Attacks = attacks.ToList();
        }

        #region state
        private List<(IActionProvider provider, OpportunisticAttack action)> _Attacks;
        private bool _Finished;
        #endregion

        /// <summary>Opportunistic attacks that can be made</summary>
        public List<(IActionProvider provider, OpportunisticAttack action)> Attacks => _Attacks;

        /// <summary>Activity that provided the opportunity to attack</summary>
        public CoreActivity Activity => Source as CoreActivity;

        /// <summary>Once any decisions to pass (or activities after taking the opportunity) are performed, this must be set to true.</summary>
        public bool Finished { get => _Finished; set => _Finished = value; }

        public override CoreActor Fulfiller
            => Qualification.Actor;

        public override bool IsReady => Finished;

        // NOTE: the attack itself doesn't fail the action that caused the attack
        // NOTE: however, if the critter being attacked loses the ability to perform the action, the activity may cease
        public override bool FailsProcess => false;

        public override bool IsSerial => true;

        #region public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<OpportunisticPrerequisiteInfo>(step);

            // get info for each attack
            _info.AttackActions = (from _atk in Attacks
                                   let _rslt = new ActionResult
                                   {
                                       Provider = _atk.provider,
                                       Action = _atk.action,
                                       IsExternal = false
                                   }
                                   select _rslt.ToActionInfo(Fulfiller)).ToArray();

            // activity info
            _info.ActivityInfo = Activity.Action.GetActivityInfo(Activity, Qualification.Actor);

            return _info;
        }
        #endregion

        #region public override void MergeFrom(PrerequisiteInfo info)
        public override void MergeFrom(PrerequisiteInfo info)
        {
            if (info is OpportunisticPrerequisiteInfo _oppty)
            {
                if (_oppty.OpportunisticActivity != null)
                {
                    var _activity = _oppty.OpportunisticActivity.CreateOpportunisticActivity(this);
                    if (_activity?.Action != null)
                    {
                        if (_activity.Actor.ProcessManager is IkosaProcessManager _manager)
                        {
                            (_manager.LocalTurnTracker.GetBudget(_activity.Actor.ID))
                                ?.DoAction(_manager, _activity);
                        }
                    }
                }
                Finished = true;
            }
        }
        #endregion
    }
}
