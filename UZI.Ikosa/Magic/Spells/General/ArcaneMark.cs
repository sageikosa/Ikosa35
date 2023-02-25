using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ArcaneMark : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Arcane Mark";
        public override string Description => @"Inscribe personal rune";
        public override MagicStyle MagicStyle => new General();

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
                yield return (ISpellMode)this;
                yield return new ArcaneMarkEnvironmentMode();
                yield break;
            }
        }
        #endregion

        public static IEnumerable<OptionAimOption> VisibilityOptions
        {
            get
            {
                yield return new OptionAimOption() { Key = @"Visible", Name = @"Visible", Description = @"Visible mark" };
                yield return new OptionAimOption() { Key = @"Invisible", Name = @"Invisible", Description = @"Invisible mark" };
                yield break;
            }
        }

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Target", Lethality.AlwaysNonLethal, 20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType(), new ObjectTargetType());
            yield return new OptionAim(@"Visibility", @"Visibility", true, FixedRange.One, FixedRange.One, VisibilityOptions);
            yield return new CharacterStringAim(@"Mark", @"Personal Mark", FixedRange.One, new FixedRange(6));
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Target", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            CopyActivityTargetsToSpellEffects(apply);

            // add the fully targetted effect
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _visibility = (_spellEffect.FirstTarget(@"Visibility") as OptionTarget).Option;
                var _mark = (_spellEffect.FirstTarget(@"Mark") as CharacterStringTarget).CharacterString;
                var _effect = new MagicMark(_spellEffect, _spellEffect.MagicPowerActionSource.PowerClass.OwnerID,
                    (_visibility.Key == @"Visible"), _mark);
                target.AddAdjunct(_effect);
                return _effect;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct((MagicMark)source.ActiveAdjunctObject);
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
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Permanent);
        #endregion
    }

    [Serializable]
    public class ArcaneMarkEnvironmentMode : ISpellMode, IDurableCapable, IProcessFeedback
    {
        public string DisplayName => @"Arcane Mark (on wall)";
        public string Description => @"Inscribe personal rune";

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new WallSurfaceAim(@"Surface", @"Surface", FixedRange.One, FixedRange.One,
                new MeleeRange(), FixedRange.One, FixedRange.One, FixedRange.One);
            yield return new OptionAim(@"Visibility", @"Visibility", true, FixedRange.One, FixedRange.One, ArcaneMark.VisibilityOptions);
            yield return new CharacterStringAim(@"Mark", @"Mark", FixedRange.One, new FixedRange(6));
            yield break;
        }

        public bool AllowsSpellResistance
            => false;

        public bool IsHarmless
            => true;

        #region public void ActivateSpell(PowerDeliveryStep<SpellSource> activation)
        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _surf = activation.TargetingProcess.Targets.OfType<WallSurfaceTarget>().FirstOrDefault(t => t.Key == @"Surface");
            var _visibility = activation.TargetingProcess.Targets.OfType<OptionTarget>().FirstOrDefault(t => t.Key == @"Visibility");
            if ((_surf != null) && (_visibility != null))
            {
                var _geoInteract = new GeometryInteract
                {
                    ID = Guid.NewGuid(),
                    AnchorFace = _surf.AnchorFace,
                    Position = _surf.Location.ToCellPosition()
                };

                SpellDef.CarryDurableEffectsToCell(activation, _geoInteract, 0);
            }
        }
        #endregion

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _surf = apply.TargetingProcess.Targets.OfType<WallSurfaceTarget>().FirstOrDefault(t => t.Key == @"Surface");
            var _visibility = apply.TargetingProcess.Targets.OfType<OptionTarget>().FirstOrDefault(t => t.Key == @"Visibility");
            if ((_surf != null) && (_visibility != null))
            {
                // create and bind virtual object to the environment
                var _geoInteract = apply.DeliveryInteraction.Target as GeometryInteract;
                var _obj = new SurfaceBoundObject(@"Arcane Mark", _visibility.Option.Key == @"Visible", _geoInteract.AnchorFace);
                var _loc = new Locator(_obj, _surf.MapContext, GeometricSize.UnitSize(), new Cubic(_geoInteract.Position, GeometricSize.UnitSize()));
                _obj.AddIInteractHandler(this);

                // make sure the SpellEffect has all the target information
                var _feedback = apply.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
                MagicPowerEffect _effect = ((MagicPowerEffectTransit<SpellSource>)_feedback.PowerTransit).MagicPowerEffects.First();
                _effect.AddTargets(apply.TargetingProcess.Targets);

                // add the spell effect controlling the ArcaneMarkEffect to the virtual object
                _obj.AddAdjunct(_effect);
            }
        }
        #endregion

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
            => this as IMode;

        #region IDurableSpellMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _visibility = (_spellEffect.FirstTarget(@"Visibility").Target as OptionTarget).Option;
                var _mark = (_spellEffect.FirstTarget(@"Mark") as CharacterStringTarget).CharacterString;
                var _effect = new MagicMark(_spellEffect, _spellEffect.MagicPowerActionSource.PowerClass.OwnerID,
                    (_visibility.Key == @"Visible"), _mark);
                target.AddAdjunct(_effect);
                return _effect;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct((MagicMark)source.ActiveAdjunctObject);
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
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Permanent);
        #endregion

        public IEnumerable<StepPrerequisite> GetPrerequisites(CoreActivity activity)
        {
            yield break;
        }

        #region IProcessFeedback Members

        void IProcessFeedback.ProcessFeedback(Interaction workSet)
        {
            if ((workSet != null) && (workSet.InteractData is RemoveAdjunctData))
            {
                var _data = workSet.InteractData as RemoveAdjunctData;
                if ((_data.Adjunct is DurableMagicEffect)
                    && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_b => _b.Value))
                {
                    var _loc = Locator.FindFirstLocator(workSet.Target);
                    _loc.Context.Remove(_loc);
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        void IInteractHandler.HandleInteraction(Interaction workSet)
        {
        }

        IEnumerable<Type> IInteractHandler.GetInteractionTypes()
        {
            yield return typeof(RemoveAdjunctData); // feedback processing
            yield break;
        }

        bool IInteractHandler.LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor
            if (typeof(RemoveAdjunctData).Equals(interactType))
                return true;

            return false;
        }

        #endregion
    }
}
