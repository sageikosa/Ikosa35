using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RayOfExhaustion : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Ray of Exhaustion";
        public override string Description => @"Ranged touch makes creature exhausted (or at least fatigued).";
        public override MagicStyle MagicStyle => new Necromancy();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new TouchAim(@"Ray", @"Ray", Lethality.AlwaysLethal,
                ImprovedCriticalRayFeat.CriticalThreatStart(actor as Creature),
                this, FixedRange.One, FixedRange.One, new NearRange(), new CreatureTargetType()).ToEnumerable();

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Ray", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // Convert Prerequisites to Targets for SpellDef...
            var _savePre = apply.AllPrerequisites<SavePrerequisite>(@"Save.Fortitude").FirstOrDefault();
            var _effect = apply.DurableMagicEffects.FirstOrDefault();
            if ((_savePre != null) && (!_savePre.Success))
            {
                // must setup the flag to use fatigued instead of exhausted
                _effect.AllTargets.Add(new ValueTarget<bool>(@"Exhausted", false));
            }
            else
            {
                // Exhausted
                _effect.AllTargets.Add(new ValueTarget<bool>(@"Exhausted", true));
            }

            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (target is Creature _critter)
            {
                if (source is MagicPowerEffect _spellEffect)
                {
                    var _willExhaust = (from _t in _spellEffect.AllTargets
                                        where typeof(ValueTarget<bool>).IsAssignableFrom(_t.GetType())
                                        && _t.Key.Equals(@"Exhausted")
                                        select _t as ValueTarget<bool>).FirstOrDefault()?.Value ?? true;
                    if (_willExhaust || _critter.Conditions.Contains(Condition.Fatigued))
                    {
                        // exhausted if not saved, or already fatigued
                        var _exhaust = new Exhausted(source);
                        _critter.AddAdjunct(_exhaust);
                        return _exhaust;
                    }

                    // fatigued if saved
                    var _fatigue = new Fatigued(source);
                    _critter.AddAdjunct(_fatigue);
                    return _fatigue;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Forititude";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        // ISpellSaveCapable Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Fortitude, SaveEffect.Partial,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.Beam;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#D0406040";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Drain;
        public string GetSplashMaterialKey() => @"#D0406040|#80403040|#D0406040";

        #endregion
    }
}
