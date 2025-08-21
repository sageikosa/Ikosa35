using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Barkskin : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Barkskin";
        public override string Description => @"Provides +2 or greater enhancment to natural armor.";
        public override MagicStyle MagicStyle => new Transformation();

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

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One,
                new MeleeRange(), new CreatureTargetType());
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
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _spellEffect = source as MagicPowerEffect;
            if (_spellEffect != null)
            {
                // calulate bonus
                int _bonus = 1 + (_spellEffect.MagicPowerActionSource.CasterLevel / 3);
                if (_bonus > 5)
                {
                    _bonus = 5;
                }

                // apply the delta creature's natural armor
                var _enhanced = new Delta(_bonus, typeof(Deltas.Enhancement));
                (source as Creature)?.Body?.NaturalArmor.Deltas.Add(_enhanced);
                return _enhanced;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            ((Delta)source.ActiveAdjunctObject).DoTerminate();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
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
            => string.Empty;

        public DurationRule DurationRule(int subMode)
        {
            return new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
        }
        #endregion
    }
}
