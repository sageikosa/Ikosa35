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
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ArcaneLock : SpellDef, ISpellMode, IDurableCapable, IRegionCapable
    {
        public override string DisplayName => @"Arcane Lock";
        public override string Description => @"Magically lock portal or container";
        public override MagicStyle MagicStyle => new Abjuration();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new CostlyMaterialComponent(typeof(CostlyComponent<ArcaneLock>), 25m);
                yield break;
            }
        }
        #endregion

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Object to Lock", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Target", 0);
        }

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // size limits...
            var _rMode = apply.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            var _maxArea = _rMode.Dimensions(apply.Actor, apply.PowerUse.PowerActionSource.CasterLevel).First();

            if ((apply.DeliveryInteraction.Target is PortalBase _portal) && _portal.OpenState.IsClosed)
            {
                // if a portal
                if (_maxArea >= _portal.Area)
                {
                    // apply the arcane lock effect
                    SpellDef.ApplyDurableMagicEffects(apply);
                }
            }
            else if (apply.DeliveryInteraction.Target is Furnishing _furnish)
            {
                // furnishing with openable compartments
                var _openable = _furnish.Connected.OfType<IOpenable>().ToList();
                if (_openable.Any())
                {
                    if ((_maxArea > _furnish.Width * _furnish.Height)
                        && (_maxArea > _furnish.Length * _furnish.Height)
                        && (_maxArea > _furnish.Length * _furnish.Width))
                    {
                        // apply the arcane lock effect
                        SpellDef.ApplyDurableMagicEffects(apply);
                    }
                }
            }
            else if ((apply.DeliveryInteraction.Target is ContainerItemBase)
                || (apply.DeliveryInteraction.Target is SlottedContainerItemBase))
            {
                // container items: apply the arcane lock effect
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }
        #endregion

        #region IDurableMode

        public bool IsDismissable(int subMode) => false;
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) => string.Empty;

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _lock = new ArcaneLockEffect(source as MagicPowerActionSource);
            target?.AddAdjunct(_lock);
            return _lock;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as ArcaneLockEffect)?.Eject();
        }

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Permanent);

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return Convert.ToDouble(30 * casterLevel);
            yield break;
        }
        #endregion
    }

    /// <summary>Attached to portals, and furnishings</summary>
    [Serializable]
    public class ArcaneLockEffect : Adjunct, IInteractHandler
    {
        /// <summary>Attached to portals, and furnishings</summary>
        public ArcaneLockEffect(IMagicPowerActionSource source)
            : base(source)
        {
            _Hold = new Delta(10, typeof(ArcaneLock));
        }

        private Delta _Hold;

        public IMagicPowerActionSource MagicPowerActionSource => Source as IMagicPowerActionSource;

        public override object Clone() => new ArcaneLockEffect(MagicPowerActionSource);

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            // add when activated
            base.OnActivate(source);

            // block standard open/close
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);

            if (Anchor is Furnishing _furnish)
            {
                // any openable directly on furnishing
                foreach (var _openable in _furnish.Connected.OfType<IOpenable>().Cast<IAdjunctable>().ToList())
                {
                    _openable.AddAdjunct(new ArcaneLockCompartmentEffect(this));
                }
            }

            // only apply if portal
            (Anchor as PortalBase)?.ForceOpenDifficulty.Deltas.Add(_Hold);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // remove when de-activated
            _Hold?.DoTerminate();

            if (Anchor is Furnishing _furnish)
            {
                // any openable directly on furnishing
                foreach (var _openable in _furnish.Connected.OfType<IOpenable>().Cast<IAdjunctable>().ToList())
                {
                    foreach (var _mini in _openable.Adjuncts.OfType<ArcaneLockCompartmentEffect>().Where(_m => _m.ArcaneLockEffect == this).ToList())
                    {
                        _mini.Eject();
                    }
                }
            }

            // block standard open/close
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }
        #endregion

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is OpenCloseInteractData _openClose)
            {
                if ((_openClose.OpenState == 0) && !((Anchor as IOpenable)?.OpenState.IsClosed ?? true))
                {
                    // allow it to close
                    workSet.Feedback.Add(new ValueFeedback<double>(this, _openClose.OpenState));
                }
                else if (workSet.Source is ForceOpenData
                    || workSet.Actor.ID == MagicPowerActionSource.PowerClass.OwnerID)
                {
                    // force open must have been successful, or actor who cast the spell tried to open
                    workSet.Feedback.Add(new ValueFeedback<double>(this, _openClose.OpenState));
                }
                else
                {
                    workSet.Feedback.Add(new UnderstoodFeedback(this));
                }
            }
        }
        #endregion

        public IEnumerable<Type> GetInteractionTypes() => typeof(OpenCloseInteractData).ToEnumerable();
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler) => true;
    }

    /// <summary>Attached to compartments on furnishings, eject if removed from furnishing</summary>
    [Serializable]
    public class ArcaneLockCompartmentEffect : Adjunct, IPathDependent, IInteractHandler
    {
        /// <summary>Attached to compartments on furnishings, eject if removed from furnishing</summary>
        public ArcaneLockCompartmentEffect(ArcaneLockEffect source)
            : base(source)
        {
        }

        public ArcaneLockEffect ArcaneLockEffect => Source as ArcaneLockEffect;

        public override object Clone()
            => new ArcaneLockCompartmentEffect(ArcaneLockEffect);

        protected override void OnActivate(object source)
        {
            // add when activated
            base.OnActivate(source);
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);
        }

        protected override void OnDeactivate(object source)
        {
            // remove when de-activated
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        #region public void HandleInteraction(Interaction workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is OpenCloseInteractData _openClose)
            {
                if ((_openClose.OpenState == 0) && !((Anchor as IOpenable)?.OpenState.IsClosed ?? true))
                {
                    // allow it to close
                    workSet.Feedback.Add(new ValueFeedback<double>(this, _openClose.OpenState));
                }
                else if (workSet.Source is ForceOpenData
                    || workSet.Actor.ID == ArcaneLockEffect?.MagicPowerActionSource.PowerClass.OwnerID)
                {
                    // force open must have been successful, or actor who cast the spell tried to open
                    workSet.Feedback.Add(new ValueFeedback<double>(this, _openClose.OpenState));
                }
                else
                {
                    workSet.Feedback.Add(new UnderstoodFeedback(this));
                }
            }
        }
        #endregion

        public IEnumerable<Type> GetInteractionTypes() => typeof(OpenCloseInteractData).ToEnumerable();
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler) => true;

        public void PathChanged(Pathed source)
        {
            if ((source is ObjectBound) && (source.Anchor == null))
            {
                // no longer object bound...break the effect
                Eject();
            }
        }
    }
}
