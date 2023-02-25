using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons.Natural
{
    [Serializable]
    public class NaturalTrip : WeaponSecondaryAttackResult, IQualifyDelta
    {
        public NaturalTrip(object source)
            : base(source, 0, 0)
        {
        }

        #region private IEnumerable<OptionAimOption> DecideToTrip()
        private IEnumerable<OptionAimOption> DecideToTrip()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = @"Attempt trip",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = @"Do not attempt trip",
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
                        @"NaturalTrip.Attempt", @"Attempt a trip", DecideToTrip(), true);
                }
            }
            yield break;
        }

        public override void AttackResult(StepInteraction deliverDamageInteraction)
        {
            if (deliverDamageInteraction != null)
            {
                var _choice = deliverDamageInteraction.Step
                    .AllPrerequisites<ChoicePrerequisite>(@"NaturalTrip.Attempt").FirstOrDefault();
                if (_choice != null)
                {
                    if ((_choice.Selected is OptionAimValue<bool> _willTrip) && _willTrip.Value)
                    {
                        // set-up trip checks
                        deliverDamageInteraction.Step.AppendFollowing(
                            new TripChecks(deliverDamageInteraction.Step,
                                _choice.Qualification.Actor as Creature,
                                _choice.Qualification.Target as Creature));
                    }
                }
            }
            return;
        }

        public override bool IsDamageSufficient(StepInteraction final)
        {
            // even if damage is reduced to nothing, a trip can still be made
            return true;
        }

        public override IEnumerable<Info> IdentificationInfos
            => new Info { Message = @"Trip" }.ToEnumerable();

        public override object Clone()
            => new NaturalTrip(Source);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                // TODO: add +1 to trip attack with a natural weapon
            }
        }

        protected override void OnDeactivate(object source)
        {
            // TODO: remove +1 to trip attack with natural weapon
            base.OnDeactivate(source);
        }

        #region IQualifyDelta
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // TODO:
            yield break;
        }
        #endregion

        #region Terminating
        private TerminateController _TCtrl;
        private TerminateController Terminator
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
        {
            Terminator.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => Terminator.TerminateSubscriberCount;
        #endregion
    }
}
