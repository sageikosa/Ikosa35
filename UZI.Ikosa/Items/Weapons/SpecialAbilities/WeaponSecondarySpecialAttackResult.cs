using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons
{
    [Serializable]
    public class WeaponSecondarySpecialAttackResult : WeaponSecondaryAttackResult
    {
        public WeaponSecondarySpecialAttackResult(ISpecialAttack specialAttack,
            bool optional, bool requiresDamage)
            : base(specialAttack, 0, 0)
        {
            _Optional = optional;
            _RequiresDamage = requiresDamage;
        }

        #region data
        private bool _Optional;
        private bool _RequiresDamage;
        #endregion

        public bool IsOptional => _Optional;
        public bool RequiresDamage => _RequiresDamage;

        public ISpecialAttack SpecialAttack => Source as ISpecialAttack;

        #region private IEnumerable<OptionAimOption> DecideToUse()
        private IEnumerable<OptionAimOption> DecideToUse()
        {
            yield return new OptionAimValue<bool>
            {
                Key = @"True",
                Description = $@"Attempt {SpecialAttack.DisplayName}",
                Name = @"Yes",
                Value = true
            };
            yield return new OptionAimValue<bool>
            {
                Key = @"False",
                Description = $@"Do not attempt {SpecialAttack.DisplayName}",
                Name = @"No",
                Value = false
            };
            yield break;
        }
        #endregion

        #region public override IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        public override IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        {
            if (workSet != null)
            {
                if (_Optional)
                {
                    var _feedback = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                    if ((_feedback != null) && _feedback.Success)
                    {
                        // actor must make decision to counter-trip [serial]
                        yield return new ChoicePrerequisite(this, workSet.Actor, this, workSet.Target,
                            $@"{SpecialAttack.Key}.Attempt",
                            $@"Attempt {SpecialAttack.DisplayName}",
                            DecideToUse(), true);
                    }
                }
            }
            yield break;
        }
        #endregion

        public override void AttackResult(StepInteraction deliverDamageInteraction)
        {
            if (deliverDamageInteraction != null)
            {
                if (_Optional)
                {
                    var _choice = deliverDamageInteraction.Step
                        .AllPrerequisites<ChoicePrerequisite>($@"{SpecialAttack.Key}.Attempt").FirstOrDefault();
                    if (_choice != null)
                    {
                        if (!((_choice.Selected as OptionAimValue<bool>)?.Value ?? false))
                            return;
                    }
                }
                SpecialAttack.ApplySpecialAttack(deliverDamageInteraction);
            }
        }

        public override bool IsDamageSufficient(StepInteraction final)
            => !RequiresDamage || (((final?.InteractData as IDeliverDamage)?.GetTotal() ?? 0) > 0);

        public override IEnumerable<Info> IdentificationInfos
            => SpecialAttack.IdentificationInfos;

        public override object Clone()
            => new WeaponSecondarySpecialAttackResult(SpecialAttack, IsOptional, RequiresDamage);
    }
}
