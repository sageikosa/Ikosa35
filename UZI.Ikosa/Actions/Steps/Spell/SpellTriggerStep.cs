using Uzi.Core.Contracts;
using System;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SpellTriggerStep : PreReqListStepBase
    {
        #region Construction
        public SpellTriggerStep(CoreActivity activity) : base(activity)
        {
            _UseCheck = null;

            // enqueue a prerequisite if needed
            if (TriggerSpell == null)
                return;

            var _critter = Activity.Actor as Creature;
            if (!TriggerSpell.SpellTrigger.UseDirectly(_critter))
            {
                // must use magic device to continue
                _UseCheck = new SuccessCheckPrerequisite(Activity, new Interaction(null, TriggerSpell.PowerActionSource, _critter, null), @"Skill.UseMagicDevice", @"Use Magic Device",
                    new SuccessCheck(_critter.Skills.Skill<UseMagicItemSkill>(), 20, TriggerSpell.PowerActionSource), true);
                _PendingPreRequisites.Enqueue(_UseCheck);
            }
        }
        #endregion

        #region Private Data
        private SuccessCheckPrerequisite _UseCheck;
        #endregion

        public CoreActivity Activity
            => Process as CoreActivity;

        public TriggerSpell TriggerSpell
            => Activity.Action as TriggerSpell;

        #region public void DoStep()
        protected override bool OnDoStep()
        {
            // use magic device needed and failed?
            if ((_UseCheck != null) && _UseCheck.FailsProcess)
            {
                AppendFollowing(Activity.GetActivityResultNotifyStep(@"Use Magic Device Failure"));
            }
            else
            {

                // sufficient charges (action shouldn't be offered if insufficient charges)
                if (TriggerSpell.SpellTrigger.PowerBattery.AvailableCharges < TriggerSpell.SpellTrigger.ChargePerUse)
                {
                    AppendFollowing(Activity.GetActivityResultNotifyStep(@"Not enough charges"));
                }
                else
                {
                    // suck down some battery
                    TriggerSpell.SpellTrigger.PowerBattery.UseCharges(TriggerSpell.SpellTrigger.ChargePerUse);
                    AppendFollowing(new PowerActivationStep<SpellSource>(Activity, TriggerSpell, Activity.Actor));
                }
            }

            // done
            return true;
        }
        #endregion
    }
}
