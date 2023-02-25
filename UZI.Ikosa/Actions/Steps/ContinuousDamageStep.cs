using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class ContinuousDamageStep : PreReqListStepBase
    {
        public ContinuousDamageStep(ContinuousDamageSource damageSource)
            : base((CoreProcess)null)
        {
            _Source = damageSource;
            foreach (var _pre in GetDamagePrerequisites())
                _PendingPreRequisites.Enqueue(_pre);
        }

        #region data
        private ContinuousDamageSource _Source;
        #endregion

        public ContinuousDamageSource ContinuousDamageSource => _Source;

        #region private IEnumerable<StepPrerequisite> GetDamagePrerequisites()
        private IEnumerable<StepPrerequisite> GetDamagePrerequisites()
        {
            foreach (var _rule in ContinuousDamageSource.DamageRules)
            {
                var _dice = _rule.Range as DiceRange;
                if (_rule is EnergyDamageRule _energyRule)
                {
                    if (_dice != null)
                    {
                        var _energyPre =
                            new EnergyDamageRollPrerequisite(ContinuousDamageSource, null, _energyRule.Key, _energyRule.Name,
                                _dice.EffectiveRoller(Guid.Empty, ContinuousDamageSource.PowerLevel),
                                false, @"Continuous", 0, _energyRule.EnergyType);
                        yield return _energyPre;
                    }
                    else
                    {
                        var _energyPre =
                            new EnergyDamageRollPrerequisite(ContinuousDamageSource, null, _energyRule.Key, _energyRule.Name,
                                new ConstantRoller(Convert.ToInt32(_rule.Range.EffectiveRange(null, ContinuousDamageSource.PowerLevel))),
                                false, @"Continuous", 0, _energyRule.EnergyType);
                        yield return _energyPre;
                    }
                }
                else
                {
                    if (_dice != null)
                    {
                        var _rollPre =
                            new DamageRollPrerequisite(ContinuousDamageSource, null, _rule.Key, _rule.Name,
                                _dice.EffectiveRoller(Guid.Empty, ContinuousDamageSource.PowerLevel),
                                false, _rule.NonLethal, @"Continuous", 0);
                        yield return _rollPre;
                    }
                    else
                    {
                        var _rollPre =
                            new DamageRollPrerequisite(ContinuousDamageSource, null, _rule.Key, _rule.Name,
                                new ConstantRoller(Convert.ToInt32(_rule.Range.EffectiveRange(null, ContinuousDamageSource.PowerLevel))),
                                false, _rule.NonLethal, @"Continuous", 0);
                        yield return _rollPre;
                    }
                }
            }
            yield break;
        }
        #endregion

        protected override bool OnDoStep()
        {
            if (!IsComplete)
            {
                // unsaveable damage (includes energy damages)
                var _damages = (from _dmgRoll in AllPrerequisites<DamageRollPrerequisite>()
                                from _getDmg in _dmgRoll.GetDamageData()
                                select _getDmg).ToList();
                var _deliver = new DeliverDamageData(null, _damages, true, false);

                // interaction with retry
                var _target = ContinuousDamageSource.Anchor as IInteract;
                var _dmgInteract = new StepInteraction(this, null, ContinuousDamageSource, _target, _deliver);
                _target.HandleInteraction(_dmgInteract);
                if (_dmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
                {
                    new RetryInteractionStep(this, @"Retry", _dmgInteract);
                }
            }
            return true;
        }
    }
}
