using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RemoteSensing : SpellDef, IDurableCapable, IDurableAnchorCapable, ISpellMode
    {
        public override string DisplayName => @"Remote Sensing";
        public override string Description => @"Create immobile invisible sensor at a distance";

        public override MagicStyle MagicStyle => new Divination(Divination.SubDivination.RemoteSensing);
        public override ActionTime ActionTime => new ActionTime(10 * Minute.UnitFactor);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, true, false);

        // TODO: known or described location as "ISpellMode"
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        // ISpellMode
        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        // TODO: sight versus sound?
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new LocationAim(@"Location", @"Sensor Location", LocationAimMode.Cell, FixedRange.One, FixedRange.One,
                new FarRange()).ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _location = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Location");
            var _geoInteract = new GeometryInteract
            {
                ID = Guid.NewGuid(),
                Point3D = _location.SupplyPoint3D(),
                Position = _location.Location.ToCellPosition()
            };
            SpellDef.ApplyDurableToCell(activation, _geoInteract, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // get delivered effect
            var _effect = apply.DurableMagicEffects.FirstOrDefault();

            // define senses
            var _senses = new SensorySet();
            _senses.Add(new RemoteVisionSense(_effect));
            _senses.Add(new Darkvision(10, _effect));

            // favor material plane
            var _activity = apply.TargetingProcess;
            var _ethereal = !(apply.Actor?.GetLocated()?.Locator.PlanarPresence.HasMaterialPresence() ?? true);

            // set up virtual object
            var _locTarget = _activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Location")) as LocationTarget;
            var _divObj = MagicSensors.CreateSensors(@"Remote Sensor", Size.Fine, _senses, GeometricSize.UnitSize(),
                @"sensor", @"sensor", _ethereal, _locTarget.Location, _locTarget.MapContext, false, true);

            // setup remote sensor group
            var _powerSource = apply.PowerUse.PowerActionSource;
            var _remoteSenseGroup = new RemoteSenseGroup(_powerSource, true);

            // track the group in the effect
            _effect.AllTargets.Add(new ValueTarget<RemoteSenseGroup>(nameof(RemoteSenseGroup), _remoteSenseGroup));

            // connect remote move group to actor
            // group will be attached to target in IDurableAnchorMode.OnAnchor()
            apply.Actor?.AddAdjunct(new RemoteSenseMaster(_powerSource, _remoteSenseGroup));

            // add magic power effect to target
            _divObj.AddAdjunct(_effect);
        }

        // IDurableCapable
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public virtual bool IsDismissable(int subMode) => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // enable if possible
            (source.AnchoredAdjunctObject as RemoteSenseTarget)?.ActivateAdjunct();
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // disable if possible
            (source.AnchoredAdjunctObject as RemoteSenseTarget)?.DeActivateAdjunct();
        }

        // IDurableAnchorCapable
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _remoteSense = _spellEffect.GetTargetValue<RemoteSenseGroup>(nameof(RemoteSenseGroup));
                if (_remoteSense != null)
                {
                    // create target
                    var _senseChannel = new RemoteSenseTarget(_spellEffect, _remoteSense) { InitialActive = false };
                    target.AddAdjunct(_senseChannel);
                    return _senseChannel;
                }
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // remove the adjunct group and all members from their anchors
            (source as MagicPowerEffect)?
                .GetTargetValue<RemoteSenseGroup>(nameof(RemoteSenseGroup))?
                .EjectMembers();

            // when the durable spell effect is removed from the virtual object, the object is removed from context
            (target as MagicSensors)?.Destroy();
        }
    }

    [Serializable]
    public class RemoteVisionSense : SensoryBase
    {
        protected SensoryBase _Sense;

        public RemoteVisionSense(object source)
            : base(source)
        {
        }

        protected override void OnBind()
        {
            var _sense = Creature.Senses.BestTerrainSenses.FirstOrDefault(_s => _s.Source is Species);
            if (_sense != null)
            {
                _Sense = _sense;
            }
            else
            {
                _Sense = new Vision(false, Source);
            }
            LowLight = _Sense.LowLight;
            base.OnBind();
        }

        protected override void OnUnbind()
        {
            _Sense = null;
            base.OnUnbind();
        }

        public override string Name => _Sense != null
            ? _Sense.Name
            : @"Clairvoyance (unattached)";

        public override Type ExpressedType() => _Sense?.ExpressedType() ?? GetType();

        public override int Precedence => _Sense?.Precedence ?? -100;

        public override bool ForTargeting => _Sense?.ForTargeting ?? false;
        public override bool ForTerrain => _Sense?.ForTerrain ?? false;
        public override bool UsesLineOfEffect => _Sense?.UsesLineOfEffect ?? true;
        public override bool UsesSenseTransit => _Sense?.UsesSenseTransit ?? true;
        public override bool UsesSight => _Sense?.UsesSight ?? true;
        public override bool UsesLight => _Sense?.UsesLight ?? true;
        public override bool UsesHearing => _Sense?.UsesHearing ?? false;
        public override bool IgnoresConcealment => _Sense?.IgnoresConcealment ?? false;
        public override bool IgnoresInvisibility => _Sense?.IgnoresInvisibility ?? false;
        public override bool IgnoresVisualEffects => _Sense?.IgnoresVisualEffects ?? false;
        public override PlanarPresence PlanarPresence => _Sense?.PlanarPresence ?? PlanarPresence.None;
    }
}
