using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RayOfEnfeeblement : SpellDef, ISpellMode, IDurableCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Ray of Enfeeblement"; 
        public override string Description => @"Ranged touch 1d6+1 deals Strength penalty."; 
        public override MagicStyle MagicStyle => new Necromancy();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Ray", @"Ray", Lethality.AlwaysLethal, 
                ImprovedCriticalRayFeat.CriticalThreatStart(actor as Creature), 
                this, FixedRange.One, FixedRange.One, new NearRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance { get { return true; } }
        public bool IsHarmless { get { return false; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Ray", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // get roll prerequisite, use to indicate maximum penalty
            RollPrerequisite _roll = apply.AllPrerequisites<RollPrerequisite>(@"Strength.Penalty").FirstOrDefault();
            if (_roll != null)
            {
                var _transit = apply.DeliveryInteraction.InteractData as MagicPowerEffectTransit<SpellSource>;
                var _cl = 
                    new ValueTarget<int>(@"Caster.Level", _transit.PowerSource.CasterClass.ClassPowerLevel.QualifiedValue(apply.DeliveryInteraction));

                // setup AimTargets for the roll and caster level
                _transit.MagicPowerEffects.First().AllTargets.Add(new PrerequisiteTarget(_roll));
                _transit.MagicPowerEffects.First().AllTargets.Add(_cl);
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (target is Creature _critter)
            {
                if (source is MagicPowerEffect _spellEffect)
                {
                    // get roll value (and caster level)
                    var _val = ((_spellEffect.AllTargets[0] as PrerequisiteTarget).PreRequisite as RollPrerequisite).RollValue;
                    _val += Math.Min((_spellEffect.AllTargets[1] as ValueTarget<int>).Value / 2, 5);

                    // apply penalty
                    var _penalty = new Delta(0 - _val, typeof(RayOfEnfeeblement), @"Enfeeblement");
                    _critter.Abilities.Strength.Deltas.Add(_penalty);

                    // if this penalty made the strength less than 1, reduce the penalty value and reset
                    if (_critter.Abilities.Strength.EffectiveValue < 1)
                    {
                        _val += _critter.Abilities.Strength.EffectiveValue - 1;
                        _penalty.Value = 0 - _val;
                    }
                    return _penalty;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is Delta _penalty)
                _penalty.DoTerminate();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield return new RollPrerequisite(interact.Source, interact, interact.Actor,
                @"Strength.Penalty", @"Strength Penalty", new DieRoller(6), false);
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public bool IsDismissable(int subMode)
        {
            return false;
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
        {
            return string.Empty;
        }

        public DurationRule DurationRule(int subMode)
        {
            return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        }
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() { return VisualizeTransferType.Beam; }
        public VisualizeTransferSize GetTransferSize() { return VisualizeTransferSize.Medium; }
        public string GetTransferMaterialKey() { return @"#C0808080"; }
        public VisualizeSplashType GetSplashType() { return VisualizeSplashType.Drain; }
        public string GetSplashMaterialKey() { return @"#C0808080|#80808080|#C0808080"; }

        #endregion
    }
}
