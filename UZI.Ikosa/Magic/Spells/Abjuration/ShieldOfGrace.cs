using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ShieldOfGrace : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Shield of Grace";
        public override string Description => @"Grants Deflection to Armor Rating (2 + 1 per 6 levels)";
        public override MagicStyle MagicStyle => new Abjuration();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal, 20, this,
                FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance
            => true;

        public bool IsHarmless
            => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _bonus = 2 + (_spellEffect.MagicPowerActionSource.CasterLevel / 6);
                if (_bonus > 5)
                {
                    _bonus = 5;
                }

                var _deflect = new Delta(_bonus, typeof(Uzi.Ikosa.Deltas.Deflection));
                if (target is Creature _critter)
                {
                    _critter.NormalArmorRating.Deltas.Add(_deflect);
                    _critter.TouchArmorRating.Deltas.Add(_deflect);
                    _critter.IncorporealArmorRating.Deltas.Add(_deflect);
                }
                return _deflect;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as Delta)?.DoTerminate();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        { yield break; }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
