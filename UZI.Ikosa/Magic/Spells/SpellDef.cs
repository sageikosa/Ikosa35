using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Objects;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Spell definition (by the book).  Implementers must override Equals(obj)
    /// </summary>
    [Serializable]
    public abstract class SpellDef : PowerActionDef<SpellSource>, ISpellDef, IRangedSourceProvider
    {
        protected SpellDef()
        {
        }
        public abstract MagicStyle MagicStyle { get; }

        public virtual IEnumerable<SpellComponent> DivineComponents
            => ArcaneComponents;

        public abstract IEnumerable<SpellComponent> ArcaneComponents { get; }

        /// <summary>Spell mode includes aiming and save information</summary>
        public abstract IEnumerable<ISpellMode> SpellModes { get; }

        public virtual IMode GetCapability<IMode>() where IMode : class, ICapability
            => this as IMode;

        /// <summary>Returns itself</summary>
        public SpellDef SeedSpellDef => this;

        public MagicPowerDefInfo ToMagicPowerDefInfo()
            => this.GetSpellDefInfo();

        public override PowerDefInfo ToPowerDefInfo()
            => ToMagicPowerDefInfo();

        /// <summary>Indicates intelligence cannot be used to power this spell while using Use Magic Device</summary>
        public virtual bool ArcaneCharisma => false;

        /// <summary>Used when a spell can be treated as if it were another spell</summary>
        public virtual IEnumerable<Type> SimilarSpells
            => GetType().ToEnumerable();

        #region ApplyDurableToCell(...)
        /// <summary>Applies durable effect at source and target, without regard to intervening medium</summary>
        public static void ApplyDurableToCell(PowerActivationStep<SpellSource> activation, GeometryInteract geoInteract, params int[] durableModes)
        {
            // get effect and transit
            var _source = activation.PowerUse.PowerActionSource;
            var _mode = activation.PowerUse.CapabilityRoot;
            var _tracker = activation.PowerUse.PowerTracker;
            var _durable = _mode.GetCapability<IDurableCapable>();
            var _actor = activation.Actor;

            MagicPowerEffect _getMagicEffect(DurationRule durationRule, int subMode)
                => durationRule.DurationType switch
                {
                    DurationType.Concentration
                        => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode),
                    DurationType.ConcentrationPlusSpan
                        => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode),
                    _ => new DurableMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode)
                };

            // spell effect
            if (durableModes.Length == 0)
                durableModes = _durable.DurableSubModes.FirstOrDefault().ToEnumerable().ToArray();
            var _effects = (from _subMode in durableModes
                            let _dr = _durable.DurationRule(_subMode)
                            select _getMagicEffect(_dr, _subMode)).ToArray();
            var _aLoc = _actor.GetLocated()?.Locator;
            var _transit = new MagicPowerEffectTransit<SpellSource>(_source, _mode, _tracker,
                _effects, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets);

            // "transit" at actor, "transit" at target, not carry
            var _delivery = new StepInteraction(activation, _actor, _source, geoInteract, _transit);
            var _zones = _aLoc.MapContext.GetInteractionTransitZones(_delivery).ToList();
            if (_zones.Any())
            {
                // failed at source
                foreach (var _cell in _aLoc.GeometricRegion.AllCellLocations())
                {
                    if (_zones.Any(_z => _z.Geometry.Region.ContainsCell(_cell)
                                    && _z.WillDestroyInteraction(_delivery, _cell as ITacticalContext)))
                    {
                        activation.Notify(@"Activation failure", @"Failed", false);
                        return;
                    }
                }

                // failed at target
                if (_zones.Any(_z => _z.Geometry.Region.ContainsCell(geoInteract.Position)
                                && _z.WillDestroyInteraction(_delivery, new CellLocation(geoInteract.Position))))
                {
                    activation.Notify(@"Activation failure", @"Failed", false);
                    return;
                }
            }

            // no zones that will destroy the interaction
            _delivery.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(_delivery, geoInteract, true));
            activation.AppendFollowing(new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor,
                null, _delivery, false, false));
        }
        #endregion

        #region CarryDurableEffectsToCell(...)
        /// <summary>Carries durable spell effects to a cell</summary>
        public static void CarryDurableEffectsToCell(PowerActivationStep<SpellSource> activation, GeometryInteract geoInteract, params int[] durableModes)
        {
            // get effect and transit
            var _source = activation.PowerUse.PowerActionSource;
            var _mode = activation.PowerUse.CapabilityRoot;
            var _tracker = activation.PowerUse.PowerTracker;
            var _durable = _mode.GetCapability<IDurableCapable>();
            var _actor = activation.Actor;

            MagicPowerEffect _getMagicEffect(DurationRule durationRule, int subMode)
                => durationRule.DurationType switch
                {
                    DurationType.Concentration
                        => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode),
                    DurationType.ConcentrationPlusSpan
                        => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode),
                    _ => new DurableMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode)
                };

            // spell effect
            if (durableModes.Length == 0)
                durableModes = _durable.DurableSubModes.FirstOrDefault().ToEnumerable().ToArray();
            var _effects = (from _subMode in durableModes
                            let _dr = _durable.DurationRule(_subMode)
                            select _getMagicEffect(_dr, _subMode)).ToArray();
            var _aLoc = _actor.GetLocated()?.Locator;
            var _transit = new MagicPowerEffectTransit<SpellSource>(_source, _mode, _tracker,
                _effects, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets);

            // try to move it through the environment
            var _delivery = new StepInteraction(activation, _actor, _source, geoInteract, _transit);
            foreach (var _lSet in _aLoc.EffectLinesToTarget(geoInteract.Position, ITacticalInquiryHelper.EmptyArray, _aLoc.PlanarPresence))
            {
                if (_lSet.CarryInteraction(_delivery))
                {
                    // viable path that doesn't destroy the spell
                    _delivery.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(_delivery, geoInteract, true));
                    activation.AppendFollowing(new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor,
                        null, _delivery, false, false));
                    return;
                }
            }
            activation.Notify(@"Activation failure", @"Failed", false);
        }
        #endregion

        #region CarryDurableEffectsToIntersection(...)
        /// <summary>Carries durable spell effects to an intersection</summary>
        public static void CarryDurableEffectsToIntersection(PowerActivationStep<SpellSource> activation,
            GeometryInteract geoInteract, params int[] durableModes)
        {
            // get effect and transit
            var _source = activation.PowerUse.PowerActionSource;
            var _mode = activation.PowerUse.CapabilityRoot;
            var _tracker = activation.PowerUse.PowerTracker;
            var _durable = _mode.GetCapability<IDurableCapable>();
            var _actor = activation.Actor;

            MagicPowerEffect _getMagicEffect(DurationRule durationRule, int subMode)
                => durationRule.DurationType switch
                {
                    DurationType.Concentration
                        => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode),
                    DurationType.ConcentrationPlusSpan
                        => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode),
                    _ => new DurableMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, geoInteract), TimeValTransition.Entering, subMode)
                };

            // spell effect
            if (durableModes.Length == 0)
                durableModes = _durable.DurableSubModes.FirstOrDefault().ToEnumerable().ToArray();
            var _effects = (from _subMode in durableModes
                            let _dr = _durable.DurationRule(_subMode)
                            select _getMagicEffect(_dr, _subMode)).ToArray();
            var _aLoc = _actor.GetLocated()?.Locator;
            var _transit = new MagicPowerEffectTransit<SpellSource>(_source, _mode, _tracker,
                _effects, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets);

            // try to move it through the environment
            var _loc = Locator.FindFirstLocator(_actor);
            var _delivery = new StepInteraction(activation, _actor, _source, geoInteract, _transit);
            foreach (var _lSet in _loc.EffectLinesToTarget(geoInteract.Point3D, geoInteract.Position, ITacticalInquiryHelper.EmptyArray, _loc.PlanarPresence))
            {
                if (_lSet.CarryInteraction(_delivery))
                {
                    // viable path that doesn't destroy the spell
                    _delivery.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(_delivery, geoInteract, true));
                    activation.AppendFollowing(new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor,
                        null, _delivery, false, false));
                    return;
                }
            }
            activation.Notify(@"Activation failure", @"Failed", false);
            return;
        }
        #endregion

        #region TransitSpellToIntersection(...)
        /// <summary>Transits a spell to an intersection</summary>
        public static bool TransitSpellToIntersection(PowerActivationStep<SpellSource> activation, Intersection intersect)
        {
            // get effect and transit
            var _source = activation.PowerUse.PowerActionSource;

            // spell effect
            var _actor = activation.Actor;
            var _aLoc = _actor.GetLocated()?.Locator;
            var _transit = new PowerActionTransit<SpellSource>(_source, activation.PowerUse.CapabilityRoot,
                activation.PowerUse.PowerTracker, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets);

            // try to move it through the environment
            var _loc = Locator.FindFirstLocator(_actor);
            var _transitSet = new StepInteraction(activation, _actor, _source, null, _transit);
            foreach (var _lSet in _loc.EffectLinesToTarget(intersect.Point3D(), null, ITacticalInquiryHelper.EmptyArray, _loc.PlanarPresence))
            {
                if (_lSet.CarryInteraction(_transitSet))
                {
                    // viable path that doesn't destroy the spell
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region DeliverDurableDirectFromBurst(...)
        /// <summary>
        /// Deliver a durable spell where the target was acquired by a burst.  
        /// Only affect ICore directly on the Locator (not as parts or object load)
        /// </summary>
        public static IEnumerable<CoreStep> DeliverDurableDirectFromBurst(PowerBurstCapture<SpellSource> burst, Locator locator,
            DeliverBurstFilter deliverAllow, ApplyBurstFilter applyAllow, params int[] durableModes)
        {
            // get the burst
            var _activation = burst.Activation;
            var _actor = _activation.Actor;
            var _aLoc = _actor.GetLocated()?.Locator;
            foreach (var _core in from _c in locator.GetCapturable<ICore>()
                                  where deliverAllow(locator, _c)
                                  select _c)
            {
                var _interactCore = _core as IInteract;
                var _target = new AimTarget(@"Target", _interactCore);
                var _step = SpellDef.DeliverDurableNextStep(_activation.MasterStep, _actor, _aLoc, _aLoc.PlanarPresence,
                    _activation.PowerUse, _activation.TargetingProcess.Targets,
                    _target, true, durableModes);
                if ((_step != null) && applyAllow(_step))
                    yield return _step;
            }
            yield break;
        }
        #endregion

        #region CarryInCluster(...)
        public static void CarryInCluster(List<AimTarget> targets, bool singleTargets, bool sequential, double farthest,
            Action<AimTarget, int> deliveryAction)
        {
            // range checking
            var _cluster = new List<Locator>();
            var _sequence = 0;
            foreach (var _aim in targets)
            {
                // get tactical target info
                var _term = Locator.FindFirstLocator(_aim.Target);
                // check clustering range
                var _dist = from _prev in _cluster
                            let _d = _prev.GeometricRegion.NearDistance(_term.GeometricRegion)
                            orderby _d descending
                            select new { Loc = _prev, Dist = _d };
                var _far = _dist.FirstOrDefault();
                if ((_far != null) && (_far.Dist > farthest))
                {
                    // must be able to re-target in order to deliver to things already targetted
                    if (!singleTargets)
                    {
                        // the farthest distance is greater than expected feet, so get the closest locator and aim target
                        var _near = _dist.LastOrDefault();
                        var _reAim = targets.FirstOrDefault(_t => Locator.FindFirstLocator(_t.Target).Equals(_near));

                        if (_reAim != null)
                        {
                            // apply to valid target...
                            deliveryAction(_aim, _sequence);
                        }
                    }
                }
                else
                {
                    // add new locator for checking distance
                    _cluster.Add(_term);

                    // apply to valid target...
                    deliveryAction(_aim, _sequence);
                }

                // increase sequence
                if (sequential)
                    _sequence++;
            }
        }
        #endregion

        /// <summary>Deliver a spell, step to InfoStep (on failed), or ApplySpell</summary>
        public static void DeliverSpell(PowerActivationStep<SpellSource> activation, int sequence, AimTarget target, params int[] generalModes)
            => activation.AppendFollowing(DeliverNextStep(activation, sequence, target, false, generalModes));

        public static void DeliverSpellInCluster(PowerActivationStep<SpellSource> activation, List<AimTarget> targets,
            bool singleTargets, bool sequential, double farthest, params int[] generalModes)
            => CarryInCluster(targets, singleTargets, sequential, farthest,
                (target, sequence) => SpellDef.DeliverSpell(activation, sequence, target, generalModes));

        /// <summary>Attempt to deliver durable spell mode, then step to InfoStep (delivery failed) or ApplySpell</summary>
        public static void DeliverDurable(PowerActivationStep<SpellSource> activation, AimTarget target, params int[] durableModes)
        {
            var _actor = activation.Actor;
            var _aLoc = _actor.GetLocated()?.Locator;
            activation.AppendFollowing(DeliverDurableNextStep(activation, _actor, _aLoc, _aLoc.PlanarPresence,
                  activation.PowerUse, activation.TargetingProcess.Targets, target, false, durableModes));
        }

        #region DeliverDurableToTouch(...)
        /// <summary>Attempt to deliver durable spell mode to targets indicated by the targetKey, then step to InfoStep (delivery failed) or ApplySpell; or if MeleeRange, step to HoldCharge (attack failed).</summary>
        public static void DeliverDurableToTouch(PowerActivationStep<SpellSource> activation, string targetKey, params int[] durableModes)
        {
            var _sequence = 0;
            foreach (var _target in activation.TargetingProcess.Targets.Where(_t => _t.Key.Equals(targetKey)).ToList())
            {
                var delivery = SpellDef.InteractDurableSpellModeSingleTargetAttack(activation, _sequence, _target, durableModes);
                _sequence++;

                // if no attack feedback, then we didn't miss
                if (delivery.Feedback.OfType<AttackFeedback>().FirstOrDefault() == null)
                {
                    // get the transit
                    if (delivery.InteractData is MagicPowerEffectTransit<SpellSource> _transit)
                    {
                        // get the durable mode prerequisites
                        var _durable = _transit.CapabilityRoot.GetCapability<IDurableCapable>();
                        var _preReqs = (from _subMode in durableModes
                                        from _pre in _durable.GetDurableModePrerequisites(_subMode, delivery)
                                        select _pre).ToArray();

                        // save modes?
                        var _spellSave = _transit.CapabilityRoot.GetCapability<ISaveCapable>();
                        if (_spellSave != null)
                        {
                            var _saveKeys = (from _subMode in durableModes
                                             let _saveKey = _durable.DurableSaveKey(activation.TargetingProcess.Targets, delivery, _subMode)
                                             where (_saveKey != null) && !_saveKey.Equals(string.Empty)
                                             select _saveKey).Distinct();
                            _preReqs = AddSavePrerequisites(_spellSave, activation.Actor, activation.PowerUse.PowerActionSource,
                                delivery, activation.PowerUse.PowerActionSource, _preReqs, _saveKeys);
                        }

                        if (_durable != null)
                        {
                            activation.AppendFollowing(new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor,
                                _preReqs, delivery, false, false));
                        }
                        else
                        {
                            activation.Notify(@"IDurableMode not available", @"Failed", false);
                        }
                    }
                    else
                    {
                        activation.Notify(@"SpellEffectTransit not available", @"Failed", false);
                    }
                }
                else
                {
                    // since we got back the attack feedback, we obviosuly missed!
                    var _spellMode = (activation.PowerUse as CastSpell).SpellMode;
                    var _rangeAim = _spellMode.AimingMode(activation.Actor, _spellMode).FirstOrDefault() as RangedAim;
                    if (_rangeAim?.Range is MeleeRange)
                    {
                        // clear the target (indicates it needs to be re-selected)
                        activation.TargetingProcess.Targets[0] = null;
                        activation.AppendFollowing(new HoldChargeStep(activation));
                    }
                    else
                    {
                        activation.Notify(@"Attack Missed", @"Failed", false);
                    }
                }
            }
        }
        #endregion

        public static void DeliverDurableInCluster(PowerActivationStep<SpellSource> deliver, List<AimTarget> targets,
            bool singleTargets, double farthest, params int[] durableModes)
            => CarryInCluster(targets, singleTargets, false, farthest,
                (target, sequence) => SpellDef.DeliverDurable(deliver, target, durableModes));

        #region private DeliverOneDamageToTouch(...)
        /// <summary>Attempt to deliver damage spell mode to target, then step to InfoStep (delivery failed) or ApplySpell; or if MeleeRange, step to HoldCharge (attack failed).</summary>
        private static void DeliverOneDamageToTouch(PowerActivationStep<SpellSource> activation, AimTarget target, int sequence, int maxSequence, params int[] damageModes)
        {
            var _delivery = SpellDef.InteractSingleTargetAttack(activation, sequence, target);
            var _atkFB = _delivery.Feedback.OfType<ISuccessIndicatorFeedback>().FirstOrDefault();
            if (_atkFB?.Success ?? false)
            {
                // attack hit
                var _critical = (_atkFB as AttackFeedback)?.CriticalHit ?? false;
                activation.AppendFollowing(new PowerApplyStep<SpellSource>(activation, activation.PowerUse, activation.Actor,
                    SpellDef.GetDamageModePreRequisites(activation, _delivery, damageModes, sequence, maxSequence, _critical),
                    _delivery, _critical, false));
            }
            else
            {
                // attack miss
                var _spellMode = (activation.PowerUse as CastSpell).SpellMode;
                var _rangeAim = _spellMode.AimingMode(activation.Actor, _spellMode).FirstOrDefault() as RangedAim;
                if ((maxSequence == 1) && (_rangeAim?.Range is MeleeRange))
                {
                    if (_atkFB is AttackFeedback)
                    {
                        // clear the target (indicates it needs to be re-selected)
                        activation.TargetingProcess.Targets[0] = null;
                        activation.AppendFollowing(new HoldChargeStep(activation));
                    }
                }
                else
                {
                    activation.Notify(@"Attack Missed", @"Failed", false);
                }
            }
        }
        #endregion

        /// <summary>Attempt to deliver damage spell mode to targets[0], then step to InfoStep (delivery failed) or ApplySpell; or if MeleeRange, step to HoldCharge (attack failed).</summary>
        public static void DeliverDamageToTouch(PowerActivationStep<SpellSource> deliver, params int[] damageModes)
            => SpellDef.DeliverOneDamageToTouch(deliver, deliver.TargetingProcess.Targets[0], 0, 1, damageModes);

        #region DeliverDamageInCluster(...)
        /// <summary>Attempts to deliver damage to multiple targets within a limiting distance of each other.</summary>
        /// <param name="deliver"></param>
        /// <param name="targets"></param>
        /// <param name="singleTargets">indicates that a target cannot be used more than once</param>
        /// <param name="sequential">indicates that visualizers should be sequentially timelined for multiple targets</param>
        /// <param name="farthest">maximum distance between any two targets</param>
        /// <param name="damageModes"></param>
        public static void DeliverDamageInCluster(PowerActivationStep<SpellSource> deliver, List<AimTarget> targets, bool singleTargets, bool sequential, double farthest, params int[] damageModes)
            => CarryInCluster(targets, singleTargets, sequential, farthest,
                (target, sequence) => SpellDef.DeliverDamage(deliver, sequence, targets.Count, target, damageModes));
        #endregion

        #region DeliverDamageToTouchInCluster(...)
        /// <summary>Attempts to deliver damage to multiple targets within a limiting distance of each other.</summary>
        public static void DeliverDamageToTouchInCluster(PowerActivationStep<SpellSource> deliver, List<AimTarget> targets, double farthest, params int[] damageModes)
        {
            // range checking
            var _cluster = new List<Locator>();
            var _sequence = 0;
            foreach (var _aim in targets)
            {
                // get tactical target info
                var _term = Locator.FindFirstLocator(_aim.Target);

                // check clustering range
                var _dist = from _prev in _cluster
                            let _d = _prev.GeometricRegion.NearDistance(_term.GeometricRegion)
                            orderby _d descending
                            select new { Loc = _prev, Dist = _d };
                var _far = _dist.FirstOrDefault();
                if ((_far != null) && (_far.Dist > farthest))
                {
                    // the farthest distance is greater than 15 feet, so get the closest locator and aim target
                    var _near = _dist.LastOrDefault();
                    var _reAim = targets.FirstOrDefault(_t => Locator.FindFirstLocator(_t.Target).Equals(_near));

                    if (_reAim != null)
                    {
                        // apply to valid target...
                        SpellDef.DeliverOneDamageToTouch(deliver, _aim, _sequence, targets.Count, damageModes);
                    }
                }
                else
                {
                    // add new locator for checking distance
                    _cluster.Add(_term);

                    // apply to valid target...
                    SpellDef.DeliverOneDamageToTouch(deliver, _aim, _sequence, targets.Count, damageModes);
                }

                // increase sequence
                _sequence++;
            }
        }
        #endregion

        #region public static Interaction InteractDurableSpellModeSingleTargetAttack(PowerDeliveryStep<SpellSource> activation, int sequence, AimTarget target, int [] durableModes)
        /// <summary>Delivers a durable spell mode to a single target via an attack roll</summary>
        public static Interaction InteractDurableSpellModeSingleTargetAttack(PowerActivationStep<SpellSource> activation, int sequence, AimTarget target, int[] durableModes)
        {
            // cast parameters to approriate types
            var _target = target as AttackTarget;
            var _interactor = _target.Target as IInteract;
            var _atkInteract = new StepInteraction(activation, activation.Actor, activation.PowerUse.PowerActionSource, _target.Target, _target.Attack);

            // first, perform an attack against the target (if one is defined)
            if (_interactor != null)
            {
                _interactor.HandleInteraction(_atkInteract);
            }
            else
            {
                // otherwise, just transit the attack
                var _handler = new TransitAttackHandler();
                _handler.HandleInteraction(_atkInteract);
            }

            // first, perform an attack against the target
            var _atkFB = _atkInteract.Feedback.OfType<AttackFeedback>().FirstOrDefault();

            var _powerUser = activation.PowerUse;
            var _source = _powerUser.PowerActionSource;
            var _mode = _powerUser.CapabilityRoot;
            var _tracker = _powerUser.PowerTracker;
            var _actor = activation.Actor as CoreActor;
            var _aLoc = _actor.GetLocated()?.Locator;
            if (_atkFB?.Hit ?? false)
            {
                // create a durable spell effect that should last until the elapsed duration time has past
                var _durable = _mode.GetCapability<IDurableCapable>();
                _mode.GeneratePowerDeliverVisualizers(_aLoc.MapContext, _target.SourcePoint, _target.TargetPoint, sequence, true);

                MagicPowerEffect _getMagicEffect(DurationRule durationRule, int subMode)
                    => durationRule.DurationType switch
                    {
                        DurationType.Concentration
                            => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, target.Target), TimeValTransition.Entering, subMode),
                        DurationType.ConcentrationPlusSpan
                            => new FragileMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, target.Target), TimeValTransition.Entering, subMode),
                        _ => new DurableMagicEffect(_source, _mode, _tracker, durationRule.EndTime(_actor, _source, target.Target), TimeValTransition.Entering, subMode)
                    };

                // define all effects
                var _effects = (from _subMode in durableModes
                                let _dr = _durable.DurationRule(_subMode)
                                select _getMagicEffect(_dr, _subMode)).ToArray();


                // tell the interactor it is a spell target
                var _spellDelivery = new StepInteraction(activation, _actor, _source, _target.Target,
                    new MagicPowerEffectTransit<SpellSource>(_source, _mode, _tracker,
                    _effects, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets));
                _interactor.HandleInteraction(_spellDelivery);
                return _spellDelivery;
            }
            else
            {
                _mode.GeneratePowerDeliverVisualizers(_aLoc.MapContext, _target.SourcePoint, _target.TargetPoint, sequence, false);
                return _atkInteract;
            }
        }
        #endregion

        #region public static Interaction InteractSpellTransitToRegion(PowerDeliveryStep<SpellSource> activation, GeometricRegionTarget target)
        public static Interaction InteractSpellTransitToRegion(PowerActivationStep<SpellSource> activation, GeometricRegionTarget target)
        {
            if (activation.Actor is ICore _iLoc)
            {
                // spell transit
                var _actor = activation.Actor as CoreActor;
                var _aLoc = _actor.GetLocated()?.Locator;
                var _transit = new PowerActionTransit<SpellSource>(activation.PowerUse.PowerActionSource,
                    activation.PowerUse.CapabilityRoot, activation.PowerUse.PowerTracker, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets);

                // try to move it through the environment
                var _loc = Locator.FindFirstLocator(_iLoc);
                var _transitSet = new StepInteraction(activation, _actor, activation.PowerUse.PowerActionSource, null, _transit);
                foreach (var _lSet in _loc.EffectLinesToTarget(target.Region, ITacticalInquiryHelper.EmptyArray, _loc.PlanarPresence))
                {
                    if (_lSet.CarryInteraction(_transitSet))
                    {
                        // viable path that doesn't destroy the spell
                        _transitSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(_transitSet, null, true));
                        return _transitSet;
                    }
                }
                _transitSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(_transitSet, null, false));
                return _transitSet;
            }
            return null;
        }
        #endregion

        #region public static void ApplyDurableMagicEffects(PowerApplyStep<SpellSource> apply)
        /// <summary>Apply durable magic effects, unless an expected save roll succeeded, otherwise do nothing</summary>
        public static void ApplyDurableMagicEffects(PowerApplyStep<SpellSource> apply)
        {
            var _durable = apply.PowerUse.CapabilityRoot.GetCapability<IDurableCapable>();
            if (apply.DeliveryInteraction.Target is IAdjunctable _target)
            {
                foreach (var _effect in apply.DurableMagicEffects)
                {
                    // assume the effect will be added
                    var _add = true;
                    var _dismiss = false;
                    if (_durable != null)
                    {
                        // see if there is an expected save
                        var _saveKey = _durable.DurableSaveKey(apply.TargetingProcess.Targets, apply.DeliveryInteraction, _effect.SubMode);
                        if (!_saveKey.Equals(string.Empty))
                        {
                            // get the prerequisite for the expected save, 
                            var _savePre = apply.AllPrerequisites<SavePrerequisite>(_saveKey).FirstOrDefault();

                            // if save does not negate effect, or save is not successful; continue to add
                            _add = ((_savePre.SaveMode.SaveEffect != SaveEffect.Negates) || !_savePre.Success);
                        }
                        _dismiss = _durable.IsDismissable(_effect.SubMode);
                    }

                    // if still adding, add the effect
                    if (_add)
                    {
                        // first, see if this might dispel an existing effect
                        var _dispel = apply.PowerUse.CapabilityRoot.GetCapability<ICounterDispelCapable>();
                        if (_dispel != null)
                        {
                            // see if an existing effect is present
                            var _exist = (from _spellEffect in _target.Adjuncts.OfType<MagicPowerEffect>()
                                          where _spellEffect.MagicPowerActionSource is SpellSource
                                          let _spellType = (_spellEffect.MagicPowerActionSource as SpellSource)
                                          .SpellDef.SeedSpellDef.GetType()       // get the spell definition's type
                                          from _t in _dispel.CounterableSpells   // compare against counterable spells
                                          where _t.IsAssignableFrom(_spellType)
                                          && (_spellEffect.MagicPowerActionSource.PowerLevel <= _effect.MagicPowerActionSource.PowerLevel)    // having the new spell level higher
                                          select _spellEffect).FirstOrDefault();
                            if (_exist != null)
                            {
                                // remove the spell effect
                                _exist.Eject();
                                return;
                            }

                            // counter descriptors
                            _exist = (from _spellEffect in _target.Adjuncts.OfType<MagicPowerEffect>()
                                      where _spellEffect.MagicPowerActionSource is SpellSource
                                      from _descr in (_spellEffect.MagicPowerActionSource as SpellSource)
                                      .SpellDef.Descriptors                  // look at it's descriptors
                                      let _descrType = _descr.GetType()
                                      from _t in _dispel.DescriptorTypes     // match against counterable descriptors
                                      where _t.IsAssignableFrom(_descrType)
                                      && (_spellEffect.MagicPowerActionSource.PowerLevel <= _effect.MagicPowerActionSource.PowerLevel)    // having the new spell level higher
                                      select _spellEffect).FirstOrDefault();
                            if (_exist != null)
                            {
                                // remove the spell effect
                                _exist.Eject();
                                return;
                            }
                        }

                        // add effect
                        if (_dismiss)
                        {
                            // add a dismiss controller if needed
                            DismissibleMagicEffectControl.CreateControl(_effect, apply.Actor, _target);
                        }
                        _target.AddAdjunct(_effect);
                    }
                }
            }
        }
        #endregion

        #region public static void ApplySpellEffectAmmunitionBundle(PowerApplyStep<SpellSource> apply)
        /// <summary>Apply a cloned spell effect to all ammo sets in a bundle, unless an expected save roll succeeded, otherwise do nothing. Activation anchor will be an IAmmunitionBase</summary>
        public static void ApplySpellEffectAmmunitionBundle(PowerApplyStep<SpellSource> apply)
        {
            var _durable = apply.PowerUse.CapabilityRoot.GetCapability<IDurableCapable>();
            if (apply.DeliveryInteraction.Target is IAmmunitionBundle _bundle)
            {
                foreach (var _effect in apply.DurableMagicEffects)
                {
                    // assume the effect will be added
                    var _add = true;
                    if (_durable != null)
                    {
                        // see if there is an expected save
                        var _saveKey = _durable.DurableSaveKey(apply.TargetingProcess.Targets, apply.DeliveryInteraction, _effect.SubMode);
                        if (!_saveKey.Equals(string.Empty))
                        {
                            // get the prerequisite for the expected save, 
                            var _savePre = apply.AllPrerequisites<SavePrerequisite>(_saveKey).FirstOrDefault();

                            // if save does not negate effect, or save is not successful; continue to add
                            _add = ((_savePre.SaveMode.SaveEffect != SaveEffect.Negates) || !_savePre.Success);
                        }
                    }

                    // if still adding, add the effect
                    if (_add)
                    {
                        // first, see if this might dispel an existing effect
                        var _dispel = apply.PowerUse.CapabilityRoot.GetCapability<ICounterDispelCapable>();
                        if (_dispel != null)
                        {
                            foreach (var _set in _bundle.AmmoSets)
                            {
                                // see if an existing effect is present
                                var _exist = (from _spellEffect in _set.Ammunition.Adjuncts.OfType<MagicPowerEffect>()
                                              where _spellEffect.MagicPowerActionSource is SpellSource
                                              let _spellType = (_spellEffect.MagicPowerActionSource as SpellSource)
                                              .SpellDef.SeedSpellDef.GetType()       // get the spell definition's type
                                              from _t in _dispel.CounterableSpells   // compare against counterable spells
                                              where _t.IsAssignableFrom(_spellType)
                                              && (_spellEffect.MagicPowerActionSource.PowerLevel <= _effect.MagicPowerActionSource.PowerLevel)    // having the new spell level higher
                                              select _spellEffect).FirstOrDefault();
                                if (_exist != null)
                                {
                                    // remove the spell effect
                                    _exist.Eject();
                                    break;
                                }

                                // counter descriptors
                                _exist = (from _spellEffect in _set.Ammunition.Adjuncts.OfType<MagicPowerEffect>()
                                          where _spellEffect.MagicPowerActionSource is SpellSource
                                          from _descr in (_spellEffect.MagicPowerActionSource as SpellSource)
                                          .SpellDef.Descriptors                  // look at it's descriptors
                                          let _descrType = _descr.GetType()
                                          from _t in _dispel.DescriptorTypes     // match against counterable descriptors
                                          where _t.IsAssignableFrom(_descrType)
                                          && (_spellEffect.MagicPowerActionSource.PowerLevel <= _effect.MagicPowerActionSource.PowerLevel)    // having the new spell level higher
                                          select _spellEffect).FirstOrDefault();
                                if (_exist != null)
                                {
                                    // remove the spell effect
                                    _exist.Eject();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            foreach (var _set in _bundle.AmmoSets)
                            {
                                // add effect
                                _set.Ammunition.AddAdjunct(_effect.Clone() as MagicPowerEffect);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>make sure the SpellEffect has all the target information from the activity</summary>
        public static void CopyActivityTargetsToSpellEffects(PowerApplyStep<SpellSource> apply)
        {
            // make sure the SpellEffect has all the target information
            var _feedback = apply.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
            ((MagicPowerEffectTransit<SpellSource>)_feedback.PowerTransit).MagicPowerEffects.First().AddTargets(apply.TargetingProcess.Targets);
        }

        public static DeltaCalcInfo SpellDifficultyCalcInfo(CoreActor actor, SpellSource source, IInteract target)
            => new ScoreDeltable(source.PowerLevel, source.CasterClass.SpellDifficultyBase, @"")
            .Score.GetDeltaCalcInfo(new SpellDCCondition(actor, source, target), @"Spell-Resist Difficulty");

        public IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target)
            => new SpellProjectile(aim, actor, action as PowerUse<SpellSource>, target);

        protected IEnumerable<SpellComponent> YieldComponents(bool verbal, bool somatic, bool material, bool focus, bool divineFocus)
        {
            if (verbal) yield return new VerbalComponent();
            if (somatic) yield return new SomaticComponent();
            if (material) yield return new MaterialComponent();
            if (focus) yield return new FocusComponent();
            if (divineFocus) yield return new DivineFocusComponent();
            yield break;
        }
    }
}
