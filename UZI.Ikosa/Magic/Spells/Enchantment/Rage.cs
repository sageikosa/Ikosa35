using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Rage : SpellDef, ISpellMode, IDurableCapable, IPowerDeliverVisualize
    {
        public override string DisplayName
            => @"Rage";

        public override string Description
            => @"+2 morale to strength and constitution, +1 morale to will, -2 armor rating.";

        public override MagicStyle MagicStyle
            => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        public override IEnumerable<Descriptor> Descriptors
            => new MindAffecting().ToEnumerable();

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        // IDurableMode
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => true;
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.ConcentrationPlusSpan, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => new WillingPrerequisite(interact.Source, interact, @"Rage.Allow", @"Allow rage to affect?", true).ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _rage = new Raging(source, 2, 1, 0, true);
            target.AddAdjunct(_rage);
            return _rage;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as Adjunct)?.Eject();

        // ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new AwarenessAim(@"Creature", @"Willing Creature", new FixedRange(1), new PowerLevelRange(1d / 3d),
                new MediumRange(), new CreatureTargetType()).ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _targets = deliver.TargetingProcess.Targets.OfType<AwarenessTarget>().Cast<AimTarget>().ToList();
            if (_targets.Count == 1)
            {
                SpellDef.DeliverSpell(deliver, 0, _targets[0], 0);
            }
            else
            {
                SpellDef.DeliverSpellInCluster(deliver, _targets, true, true, 30, 0);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IPowerDeliverVisualize
        public VisualizeTransferType GetTransferType() => VisualizeTransferType.SurgeTo;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0FF4040";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#C0FF4040|#8040FF40|#C0FF4040";
    }
}
