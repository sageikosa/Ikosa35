using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Shield : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Shield";
        public override string Description => @"+4 Shield Armor Rating and Magic Missile Block";
        public override MagicStyle MagicStyle => new Abjuration();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Force();
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
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;
        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0]);
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
            var _shield = new ShieldEffect((source as MagicPowerEffect).MagicPowerActionSource);
            target.AddAdjunct(_shield);
            return _shield;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct(source.ActiveAdjunctObject as ShieldEffect);
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion
    }

    [Serializable]
    public class ShieldEffect : Adjunct, IInteractHandler
    {
        public ShieldEffect(object source)
            : base(source)
        {
            _ShieldAR = new Delta(4, typeof(IShield));
        }

        private Delta _ShieldAR;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;

            // add shield AC bonus
            _critter?.NormalArmorRating.Deltas.Add(_ShieldAR);
            _critter?.IncorporealArmorRating.Deltas.Add(_ShieldAR);

            // add handler
            _critter?.AddIInteractHandler(this);

            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // end shield AC bonus
            _ShieldAR.DoTerminate();

            // remove handler
            (Anchor as Creature)?.RemoveIInteractHandler(this);

            base.OnDeactivate(source);
        }
        #endregion

        #region IInteractHandler Members
        public void HandleInteraction(Interaction workSet)
        {
            var _transit = workSet.InteractData as PowerActionTransit<SpellSource>;
            if (_transit != null)
            {
                // if this spell can be treated as Magic Missile
                if ((from _t in _transit.PowerSource.SpellDef.SimilarSpells
                     where typeof(MagicForceMissile).IsAssignableFrom(_t)
                     select _t).Count() > 0)
                {
                    // ignore it completely (no transit, shield blocked it)
                    workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, workSet.Target, false));
                }
            }
            return;
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(PowerActionTransit<SpellSource>);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (existingHandler.GetType() == typeof(SpellTransitHandler));
        #endregion

        public override object Clone()
            => new ShieldEffect(Source);
    }
}
