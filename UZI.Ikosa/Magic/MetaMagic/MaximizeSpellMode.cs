using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class MaximizeSpellMode : MetaMagicSpellMode, IDamageCapable, IDurableCapable, IGeneralSubMode
    {
        public MaximizeSpellMode(ISpellMode wrapped)
            : base(wrapped)
        {
        }

        #region public override IMode GetMode<IMode>()
        public override IMode GetCapability<IMode>()
        {
            // ensure base supports the interface
            var _baseMode = base.GetCapability<IMode>();
            if (_baseMode != null)
            {
                // if so, replace if it matches one of our supported modes
                switch (_baseMode)
                {
                    case IDamageCapable _:
                        return this as IMode;

                    case IDurableCapable _:
                        return this as IMode;

                    case IGeneralSubMode _:
                        return this as IMode;
                }
            }
            return _baseMode;
        }
        #endregion

        private Roller MaximizeRoller(Roller original)
            => new ConstantRoller(original.MaxRoll);

        #region IDamageMode Members

        public IEnumerable<int> DamageSubModes
            => base.GetCapability<IDamageCapable>().DamageSubModes;

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            var _iDmg = Wrapped.GetCapability<IDamageCapable>();
            foreach (var _rule in _iDmg.GetDamageRules(subMode, isCriticalHit))
            {
                if (_rule.Range is DiceRange _dRange)
                {
                    // maximize rollers
                    _dRange.OffSet = MaximizeRoller(_dRange.OffSet);
                    _dRange.PerStep = MaximizeRoller(_dRange.OffSet);
                }
                yield return _rule;
            }
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
            => base.GetCapability<IDamageCapable>().DamageSaveKey(workSet, subMode);

        public bool CriticalFailDamagesItems(int subMode)
            => base.GetCapability<IDamageCapable>().CriticalFailDamagesItems(subMode);

        #endregion

        #region IDurableMode Members

        public IEnumerable<int> DurableSubModes
            => base.GetCapability<IDurableCapable>().DurableSubModes;

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
            => base.GetCapability<IDurableCapable>().Activate(source, target, subMode, activateSource);

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => base.GetCapability<IDurableCapable>().Deactivate(source, target, subMode, deactivateSource);

        public bool IsDismissable(int subMode)
            => base.GetCapability<IDurableCapable>().IsDismissable(subMode);

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => base.GetCapability<IDurableCapable>().DurableSaveKey(targets, workSet, subMode);

        public DurationRule DurationRule(int subMode)
            => base.GetCapability<IDurableCapable>().DurationRule(subMode);

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            IDurableCapable _iDurable = Wrapped.GetCapability<IDurableCapable>();
            foreach (var _pre in _iDurable.GetDurableModePrerequisites(subMode, interact))
            {
                if (_pre is RollPrerequisite _roller)
                {
                    _roller.Roller = MaximizeRoller(_roller.Roller);
                }
                yield return _pre;
            }
            yield break;
        }

        #endregion

        #region IGeneralSubMode Members

        public IEnumerable<int> GeneralSubModes
            => base.GetCapability<IGeneralSubMode>().GeneralSubModes;

        public string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode)
            => base.GetCapability<IGeneralSubMode>().GeneralSaveKey(targetProcess, workSet, subMode);

        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact)
        {
            IGeneralSubMode _iGeneral = Wrapped.GetCapability<IGeneralSubMode>();
            foreach (var _pre in _iGeneral.GetGeneralSubModePrerequisites(subMode, interact))
            {
                if (_pre is RollPrerequisite _roller)
                {
                    _roller.Roller = MaximizeRoller(_roller.Roller);
                }
                yield return _pre;
            }
            yield break;
        }

        #endregion
    }
}
