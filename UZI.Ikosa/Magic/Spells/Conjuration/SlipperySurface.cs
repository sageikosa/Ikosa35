using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;
using System.Linq;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SlipperySurface : SpellDef, ISpellMode, IDurableCapable, IDurableAnchorCapable
    {
        public override string DisplayName => @"Slippery Surface";
        public override string Description => @"Makes surfaces slippery";

        public override MagicStyle MagicStyle
            => new Conjuration(Conjuration.SubConjure.Creation);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, true, false, false);

        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return (ISpellMode)this;
                yield return new SlipperyObject();
                yield break;
            }
        }

        // ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            // TODO: or intersection with IRegionMode?
            yield return new WallSurfaceAim(@"Surface", @"Surface", new FixedRange(4), new FixedRange(4),
                new NearRange(), new FixedRange(2), new FixedRange(2), new FixedRange(2));
            yield break;
        }

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _idx = 1;
            foreach (var _wall in activation.TargetingProcess.Targets
                .OfType<WallSurfaceTarget>()
                .Where(_t => _t.Key == @"Surface")
                .ToList())
            {
                var _geoInteract = new GeometryInteract
                {
                    Index = _idx,
                    ID = Guid.NewGuid(),
                    AnchorFace = _wall.AnchorFace,
                    Position = _wall.Location.ToCellPosition()
                };
                SpellDef.CarryDurableEffectsToCell(activation, _geoInteract, 0);
                _idx++;
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // set up virtual object(s) each wall section has its own virtual object
            if (apply.DeliveryInteraction.Target is GeometryInteract _geoInteract)
            {
                var _sTarget = apply.TargetingProcess.GetFirstTarget<WallSurfaceTarget>(@"Location");
                var _obj = new SurfaceBoundObject(@"Slippery Surface", true, _geoInteract.AnchorFace);
                var _loc = new Locator(_obj, _sTarget.MapContext, GeometricSize.UnitSize(),
                    new Cubic(_geoInteract.Position, GeometricSize.UnitSize()));

                // add durable magic effect
                _obj.AddAdjunct(apply.DurableMagicEffects.FirstOrDefault());
            }
        }

        // IDurableMode Members
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode) => true;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // TODO: must balance if on gravity face and using overland movement (or crawl)
            // TODO: if not on gravity face, then +20 climb
            // TODO: virtual conjuration object with capture zone
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // TODO: suppress capture zone
        }

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
        {
            // TODO:
            return string.Empty;
        }

        // IDurableAnchor
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                //var _builder = _spellEffect.GetTargetValue<IGeometryBuilder>(nameof(IGeometryBuilder));
                //var _silence = new SilenceZone(_spellEffect, _builder) { InitialActive = false };
                //target.AddAdjunct(_silence);
                //return _silence;
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // when the durable spell effect is removed from the virtual object, the object is removed from context
            var _intersect = target as IntersectionObject;
            _intersect?.UnPath();
            _intersect?.UnGroup();
        }
    }

    [Serializable]
    public class SlipperyObject : ISpellMode, IDurableCapable, ISaveCapable
    {
        // ISpellMode Members
        public string DisplayName => @"Slippery Object";
        public string Description => @"Makes object slippery";

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new AwarenessAim(@"Object", @"Object", FixedRange.One, FixedRange.One, new NearRange(), new ObjectTargetType())
            .ToEnumerable();

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => false;

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
            => this as IMode;

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // TODO: deliver durable to object
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // TODO: apply durable
        }

        // IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // TODO: apply slippery adjunct
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // TODO: remove slippery adjunct
        }

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode) => true;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Reflex";

        // ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Reflex, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
    }
}
