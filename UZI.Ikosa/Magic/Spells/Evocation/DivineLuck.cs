using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DivineLuck : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName { get { return @"Divine Luck"; } }
        public override string Description { get { return @"+1 Luck on attack and weapon damage per 3 caster levels (min +1, max +3)"; } }
        public override MagicStyle MagicStyle { get { return new Evocation(); } }

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members

        #region public IEnumerable<int> DurableSubModes { get; }
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _spellEffect = source as MagicPowerEffect;
            if (_spellEffect != null)
            {
                int _bonus = _spellEffect.MagicPowerActionSource.CasterLevel / 3;
                if (_bonus < 1) _bonus = 1;
                if (_bonus > 3) _bonus = 3;
                Delta _luck = new Delta(_bonus, typeof(Uzi.Ikosa.Deltas.Luck));
                Creature _critter = target as Creature;
                if (_critter != null)
                {
                    _critter.MeleeDeltable.Deltas.Add(_luck);
                    _critter.RangedDeltable.Deltas.Add(_luck);
                    _critter.OpposedDeltable.Deltas.Add(_luck);
                    _critter.ExtraWeaponDamage.Deltas.Add(_luck);
                }
                return _luck;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            Delta _luck = source.ActiveAdjunctObject as Delta;
            if (_luck != null)
                _luck.DoTerminate();
        }

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return string.Empty; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute())); }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion
    }
}
