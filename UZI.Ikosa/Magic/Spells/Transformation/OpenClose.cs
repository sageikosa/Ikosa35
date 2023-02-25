using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Interactions;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class OpenClose : SpellDef, ISpellMode, ISaveCapable, IGeneralSubMode
    {
        public override string DisplayName => @"Open or Close";
        public override string Description => @"Open or close doors or containers.";
        public override MagicStyle MagicStyle => new Transformation();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, true, false);

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public IEnumerable<OptionAimOption> Options
        {
            get
            {
                yield return new OptionAimOption() { Key = @"0", Description = @"All the way closed", Name = @"Closed" };
                yield return new OptionAimOption() { Key = @"1", Description = @"All the way open", Name = @"Opened" };
                yield break;
            }
        }

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Portal/Container", @"Portal/Container", FixedRange.One, FixedRange.One, new NearRange(), new ObjectTargetType());
            yield return new OptionAim(@"Open", @"Open", true, FixedRange.One, FixedRange.One, Options);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            AimTarget _target = deliver.TargetingProcess.Targets.Where(t => t.Key.Equals(@"Portal/Container")).First();
            DeliverSpell(deliver, 0, _target, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // check on the status of the save prerequisite
            var _savePre = apply.AllPrerequisites<SavePrerequisite>().Where(_p => _p.BindKey.Equals(@"Save.Will")).FirstOrDefault();
            if ((_savePre != null) && !_savePre.Success)
            {
                // opening or closing?
                var _option = (apply.TargetingProcess.Targets.Where(t => t.Key.Equals(@"Open")).First() as OptionTarget).Option;

                if (apply.DeliveryInteraction.Target is IOpenable _openable)
                {
                    // max weight
                    if (_openable.OpenWeight <= 30)
                    {
                        if (_option.Key.Equals(@"1"))
                        {
                            apply.AppendFollowing(new StartOpenCloseStep(apply.Process, _openable, apply.Actor, apply.PowerUse.PowerActionSource, 1));
                        }
                        else
                        {
                            apply.AppendFollowing(new StartOpenCloseStep(apply.Process, _openable, apply.Actor, apply.PowerUse.PowerActionSource, 0));
                        }
                    }
                }
            }
        }
        #endregion

        // ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            // attended or magical objects
            => new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target), true);

        // IGeneralSubMode Members
        public IEnumerable<int> GeneralSubModes => 0.ToEnumerable();
        public string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode) => @"Save.Will";

        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();
    }
}
