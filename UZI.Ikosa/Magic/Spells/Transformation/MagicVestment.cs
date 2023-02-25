using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class MagicVestment : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Magic Vestment";
        public override string Description => @"Provides +1 enhancment to armor or shield per 4 caster levels (Max +5)";
        public override MagicStyle MagicStyle => new Transformation();

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, true, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new TouchAim(@"Item", @"Armor/Shield to Enhance", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType()).ToEnumerable();

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if (apply.DeliveryInteraction.Target is IProtectorItem)
            {
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }

        // IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // apply the delta to armor of shield, do not apply the enhanced adjunct
            if (source is MagicPowerEffect _spellEffect)
            {
                var _bonus = Math.Min((_spellEffect.MagicPowerActionSource.CasterLevel / 4), 5);
                var _enhanced = new Delta(_bonus, typeof(Deltas.Enhancement), @"Magic Vestment");

                if (target is IProtectorItem _protector)
                {
                    _protector.ProtectionBonus.Deltas.Add(_enhanced);
                }
                return _enhanced;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Delta)?.DoTerminate();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Hour(), 1));

        // ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
    }
}
