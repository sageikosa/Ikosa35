using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class FlamingSphere : SpellDef, ISpellMode, ISaveCapable, IDurableCapable, IDurableAnchorCapable, IDamageCapable
    {
        public override string DisplayName => @"Flaming sphere";
        public override string Description => @"Controllable ball of fire does damage to things it touches";
        public override MagicStyle MagicStyle => new Evocation();

        public override IEnumerable<Descriptor> Descriptors => (new Fire()).ToEnumerable();

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, true, false, false);

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        // ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new LocationAim(@"Location", @"Starting Location", LocationAimMode.Cell,
                FixedRange.One, FixedRange.One, new MediumRange())
            .ToEnumerable();

        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            var _location = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Location");
            var _geoInteract = new GeometryInteract
            {
                ID = Guid.NewGuid(),
                Point3D = _location.SupplyPoint3D(),
                Position = _location.Location.ToCellPosition()
            };
            SpellDef.CarryDurableEffectsToCell(activation, _geoInteract, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // favor material plane
            var _activity = apply.TargetingProcess;
            var _ethereal = !(apply.Actor?.GetLocated()?.Locator.PlanarPresence.HasMaterialPresence() ?? true);

            // set up virtual object
            var _locTarget = _activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Location")) as LocationTarget;
            var _evocObj = MagicEffectHolder.CreateMagicEffectHolder(@"Flaming Sphere", Size.Medium, GeometricSize.UnitSize(),
                @"flaming_sphere", @"flaming_sphere", _ethereal, _locTarget.Location, _locTarget.MapContext, true);

            // get delivered effect
            var _effect = apply.DurableMagicEffects.FirstOrDefault();

            // setup remote control group
            var _move = new FlightSuMovement(30, _evocObj, _effect, FlightManeuverability.Perfect, false, false);
            var _powerSource = apply.PowerUse.PowerActionSource;
            var _remoteMoveGroup = new RemoteMoveGroup(_powerSource, _move);

            // track the group in the effect
            _effect.AllTargets.Add(new ValueTarget<RemoteMoveGroup>(nameof(RemoteMoveGroup), _remoteMoveGroup));

            // connect remote move group to actor
            // group will be attached to target in IDurableAnchorMode.OnAnchor()
            apply.Actor?.AddAdjunct(new RemoteMoveMaster(_powerSource, _remoteMoveGroup));

            // add magic power effect to target
            _evocObj.AddAdjunct(_effect);
        }

        // ISaveCapable
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource actionSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Reflex, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, actionSource as SpellSource, workSet.Target));

        // IDurableCapable: durable for the physical presence and the connection to the caster
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => false;

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // enable if possible
            (source.AnchoredAdjunctObject as FlamingSphereEffect)?.ActivateAdjunct();
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // disable if possible
            (source.AnchoredAdjunctObject as FlamingSphereEffect)?.DeActivateAdjunct();
        }

        // IDurableAnchorCapable
        public object OnAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _remoteMove = _spellEffect.GetTargetValue<RemoteMoveGroup>(nameof(RemoteMoveGroup));
                if (_remoteMove != null)
                {
                    // create effect
                    var _flameSphere = new FlamingSphereEffect(_spellEffect, _remoteMove) { InitialActive = false };
                    target.AddAdjunct(_flameSphere);
                    return _flameSphere;
                }
            }
            return null;
        }

        public void OnEndAnchor(IAdjunctTracker source, IAdjunctable target)
        {
            // remove the adjunct group and all members from their anchors
            (source as MagicPowerEffect)?
                .GetTargetValue<RemoteMoveGroup>(nameof(RemoteMoveGroup))?
                .EjectMembers();

            // when the durable spell effect is removed from the virtual object, the object is removed from context
            (target as MagicEffectHolder)?.Destroy();
        }

        // IDamageMode
        public IEnumerable<int> DamageSubModes => 0.ToEnumerable();
        public bool CriticalFailDamagesItems(int subMode) => true;

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            // NOTE: damage is not in the delivery of the spell
            // NOTE: saves are handled by the actual sphere object itself
            return @"Save.Reflex";
        }

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Fire.Damage",
                new DiceRange(@"Fire", DisplayName, new DiceRoller(2, 6)), @"Fire Damage", EnergyType.Fire);
            yield break;
        }
    }


    [Serializable]
    public class FlamingSphereEffect : RemoteMoveTarget, IInteractHandler, IPowerUse<SpellSource>
    {
        public FlamingSphereEffect(MagicPowerEffect magicPowerEffect, RemoteMoveGroup remoteMoveGroup)
            : base(magicPowerEffect, remoteMoveGroup)
        {
        }

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public MagicEffectHolder EvocationHolder => Anchor as MagicEffectHolder;

        public override object Clone()
            => new FlamingSphereEffect(MagicPowerEffect, RemoteMoveGroup);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as IInteractHandlerExtendable)?.AddIInteractHandler(this);

            // activate move master
            RemoteMoveGroup.Master.Activation = new Activation(this, true);

            // illumination
            Anchor.AddAdjunct(new Illumination(this, 20, 40, false));
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as IInteractHandlerExtendable)?.RemoveIInteractHandler(this);

            // remove illumination effect
            Anchor.Adjuncts.OfType<Illumination>().FirstOrDefault(_i => _i.Source == this)?.Eject();

            // deactivate move master
            RemoteMoveGroup.Master.Activation = new Activation(this, false);
            base.OnDeactivate(source);
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (Anchor != null)
            {
                // starting up
            }
            else
            {
                // tearing down
                EvocationHolder?.Abandon();
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }

        // IInteractHandler
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is LocatorMove _locMove)
            {
                var _effect = MagicPowerEffect;
                var _dmg = _effect.CapabilityRoot.GetCapability<IDamageCapable>();

                // TODO: check max range, but also, if locator of master moves, max range can be exceeded...
                // TODO: if max range exceeded, spell ends
                // TODO: ignite flammables

                switch (_locMove.LocatorMoveState)
                {
                    case LocatorMoveState.TargetPassedBy:
                    case LocatorMoveState.PassingBy:
                    case LocatorMoveState.TargetArrival:
                    case LocatorMoveState.ArrivingTo:
                        var _critter = RemoteMoveGroup.Master.Anchor as Creature;
                        var _loc = EvocationHolder.GetLocated()?.Locator;
                        if ((_critter != null) && (_loc != null))
                        {
                            var _other = _loc.MapContext.LocatorsInRegion(_loc.GeometricRegion, _loc.PlanarPresence)
                                .Where(_l => _l != _loc).ToList();
                            if (_other.Any())
                            {
                                // if flaming sphere is active mover and stops in cell with creature, it stops moving
                                if (_locMove.LocatorMoveState == LocatorMoveState.PassingBy
                                    || _locMove.LocatorMoveState == LocatorMoveState.ArrivingTo)
                                {
                                    // still overlap after movment?
                                    if (_other.Any(_o => _o.GeometricRegion.ContainsGeometricRegion(_loc.GeometricRegion)
                                                        && (_o.Chief is Creature)))
                                    {
                                        // finish moving
                                        DoneMove();
                                    }
                                }

                                // overlap after the movment?
                                foreach (var _target in _other
                                    .Where(_o => _o.GeometricRegion.ContainsGeometricRegion(_loc.GeometricRegion)))
                                {
                                    var _act = new PowerActivationStep<SpellSource>(null, this, _critter);
                                    var _process = new CoreTargetingProcess(_act, RemoteMoveGroup.Master.Anchor as Creature,
                                        @"Flaming Sphere Impact", new List<AimTarget>
                                        {
                                            new AimTarget(@"Target", _target.ICore as IInteract)
                                        });
                                    _critter.ProcessManager.StartProcess(_process);
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
            => typeof(LocatorMove).ToEnumerable();

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => interactType == typeof(LocatorMove);

        // IPowerUse
        public ICapabilityRoot CapabilityRoot => MagicPowerEffect.CapabilityRoot;
        public SpellSource PowerActionSource => MagicPowerEffect.MagicPowerActionSource as SpellSource;

        public void ApplyPower(PowerApplyStep<SpellSource> step)
        {
            SpellDef.ApplyDamage(step, step, 0);
        }

        public void ActivatePower(PowerActivationStep<SpellSource> step)
        {
            SpellDef.DeliverDamage(step, 1, 1, step.TargetingProcess.GetFirstTarget<AimTarget>(@"Target"), 0);
        }

        public PowerAffectTracker PowerTracker => MagicPowerEffect.PowerTracker;
    }
}
