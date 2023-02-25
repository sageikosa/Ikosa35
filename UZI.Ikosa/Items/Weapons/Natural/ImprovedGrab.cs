using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    /// <summary>
    /// Improved Grab as a natural weapon secondary attack result
    /// </summary>
    [Serializable]
    public class ImprovedGrab : WeaponSecondaryAttackResult
    {
        /// <summary>
        /// Improved Grab as a natural weapon secondary attack result
        /// </summary>
        public ImprovedGrab(object source)
            : base(source, 0, 0)
        {
        }

        public override IEnumerable<Info> IdentificationInfos
            => new Info { Message = @"Improved Grab" }.ToEnumerable();

        public override object Clone()
            => new ImprovedGrab(Source);

        public override bool IsDamageSufficient(StepInteraction final)
        {
            // even if damage is reduced to nothing, grab can still be made
            return true;
        }

        #region private IEnumerable<OptionAimOption> DecideToGrab()
        private IEnumerable<OptionAimOption> DecideToGrab()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Attempt imroved grab",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Do not attempt imroved grab",
                Name = @"No",
                Value = false
            };
            yield break;
        }
        #endregion

        public override IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        {
            if (workSet != null)
            {
                var _feedback = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                if ((_feedback != null) && _feedback.Success)
                {
                    // actor must make decision to counter-trip [serial]
                    yield return new ChoicePrerequisite(this, workSet.Actor, this, workSet.Target,
                        @"ImprovedGrab.Attempt", @"Perform improved grab", DecideToGrab(), true);
                }
            }
            yield break;
        }

        public override void AttackResult(StepInteraction deliverDamageInteraction)
        {
            if (deliverDamageInteraction != null)
            {
                var _choice = deliverDamageInteraction.Step
                    .AllPrerequisites<ChoicePrerequisite>(@"ImprovedGrab.Attempt").FirstOrDefault();
                if (_choice != null)
                {
                    if ((_choice.Selected is OptionAimValue<bool> _willGrab) && _willGrab.Value)
                    {
                        // set-up grapple checks
                        // TODO:
                        //deliverDamageInteraction.Step.AppendFollowing(
                        //    new TripChecks(deliverDamageInteraction.Step,
                        //        _choice.Qualification.Actor as Creature,
                        //        _choice.Qualification.Target as Creature));
                    }
                }
            }
        }
    }
}