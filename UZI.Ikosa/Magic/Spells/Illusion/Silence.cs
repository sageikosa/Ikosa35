using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Silence : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, IRegionCapable, IGeometryCapable<SpellSource>
    {
        public override string DisplayName => @"Silence (creature/object)";
        public override string Description => @"Nullify sound in a region surrounding a target.";
        public override MagicStyle MagicStyle => new Illusion(Illusion.SubIllusion.Glamer);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, false, false);

        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield return new SilenceFixed();
                yield break;
            }
        }

        // ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new AwarenessAim(@"Target", @"Target", FixedRange.One, FixedRange.One, new NearRange(),
                new TargetType[] { new ObjectTargetType(), new CreatureTargetType() }).ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            apply.DurableMagicEffects.FirstOrDefault()?.AllTargets
                .Add(new ValueTarget<IGeometryBuilder>(nameof(IGeometryBuilder), GetApplyGeometry(apply)));

            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // ISaveCapable
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource actionSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, actionSource as SpellSource, workSet.Target), true);

        // IDurableCapable
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _builder = _spellEffect.GetTargetValue<IGeometryBuilder>(nameof(IGeometryBuilder));
                var _silence = new SilenceZone(_spellEffect, _builder);
                target.AddAdjunct(_silence);
                return _silence;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.AnchoredAdjunctObject as Adjunct)?.Eject();
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        // IRegionCapable
        public IEnumerable<double> Dimensions(CoreActor actor, int powerLevel)
            => 20d.ToEnumerable();

        // IGeometryCapable Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get radius
            var _radius = powerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(actor, powerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault();
            return new SphereBuilder(Convert.ToInt32(_radius / 5));
        }
    }

    [Serializable]
    public class SilenceFixed : ISpellMode, IDurableCapable, IDurableAnchorCapable, IRegionCapable, IGeometryCapable<SpellSource>
    {
        public string DisplayName => @"Silence (point)";
        public string Description => @"Nullify sound in a region of fixed space.";
        public MagicStyle MagicStyle => new Illusion(Illusion.SubIllusion.Glamer);

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
            => this as IMode;

        // ISpellMode
        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new LocationAim(@"Location", @"Center of Region", LocationAimMode.Any, FixedRange.One, FixedRange.One, new FarRange())
            .ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _location = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Location");
            var _geoInteract = new GeometryInteract
            {
                ID = Guid.NewGuid(),
                Point3D = _location.SupplyPoint3D(),
                Position = _location.Location.ToCellPosition(),
                AimMode = _location.LocationAimMode
            };
            SpellDef.CarryDurableEffectsToIntersection(activation, _geoInteract, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // set up virtual object
            var _iTarget = apply.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Location")) as LocationTarget;
            var _intObj = new IntersectionObject(DisplayName, new Intersection(_iTarget.Location));
            var _loc = new Locator(_intObj, _iTarget.MapContext, _intObj.GeometricSize,
                new Cubic(_iTarget.Location, _intObj.GeometricSize));

            var _effect = apply.DurableMagicEffects.FirstOrDefault();
            _effect?.AllTargets.Add(new ValueTarget<IGeometryBuilder>(nameof(IGeometryBuilder), SpellDef.GetApplyGeometry(apply)));
            _intObj.AddAdjunct(_effect);
        }

        // IDurableCapable
        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source.AnchoredAdjunctObject is SilenceZone _silence)
            {
                _silence.IsActive = true;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.AnchoredAdjunctObject is SilenceZone _silence)
            {
                _silence.IsActive = false;
            }
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        // IDurableAnchorCapable
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _builder = _spellEffect.GetTargetValue<IGeometryBuilder>(nameof(IGeometryBuilder));
                var _silence = new SilenceZone(_spellEffect, _builder) { InitialActive = false };
                target.AddAdjunct(_silence);
                return _silence;
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

        // IRegionCapable
        public IEnumerable<double> Dimensions(CoreActor actor, int powerLevel)
            => 20d.ToEnumerable();

        // IGeometryCapable Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get radius
            var _radius = powerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(actor, powerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault();
            return new SphereBuilder(Convert.ToInt32(_radius / 5));
        }
    }
}
