using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Dice;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public abstract class PowerActionDef<PowerSrc> : PowerDef<PowerSrc>, IPowerActionDef
        where PowerSrc : class, IPowerActionSource
    {
        protected PowerActionDef()
        {
        }

        public virtual ActionTime ActionTime
            => new ActionTime(TimeType.Regular);

        protected static StepPrerequisite[] AddSavePrerequisites(ISaveCapable saveMode, CoreActor actor, IPowerActionSource actionSource,
            Interaction interact, object source, IEnumerable<StepPrerequisite> preReqs, IEnumerable<string> saveKeys)
            => preReqs.Union((from _sKey in saveKeys
                              let _saveMode = saveMode.GetSaveMode(actor, actionSource, interact, _sKey)
                              where (_saveMode != null) && (_saveMode.SaveType >= SaveType.Fortitude)
                              select new SavePrerequisite(source, interact, _sKey, @"Save", _saveMode) as StepPrerequisite))
            .ToArray();


        public override PowerDefInfo ToPowerDefInfo()
            => this.GetPowerDefInfo();

        #region public static bool TransitPowerToIntersection<PowerSrc>(PowerDeliveryStep<PowerSrc> deliver, IntersectionTarget target)
        /// <summary>Transits a supernatural ability to an intersection</summary>
        public static bool TransitPowerToIntersection(PowerActivationStep<PowerSrc> activation, Intersection intersect)
        {
            // get effect and transit
            var _source = activation.PowerUse.PowerActionSource;

            // spell effect
            var _actor = activation.Actor;
            var _aLoc = _actor.GetLocated()?.Locator;
            var _transit = new PowerActionTransit<PowerSrc>(_source, activation.PowerUse.CapabilityRoot,
                activation.PowerUse.PowerTracker, _actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets);

            // try to move it through the environment
            var _transitSet = new StepInteraction(activation, _actor, _source, null, _transit);
            foreach (var _lSet in _aLoc.EffectLinesToTarget(intersect.Point3D(), null, ITacticalInquiryHelper.EmptyArray,
                _aLoc.PlanarPresence))
            {
                if (_lSet.CarryInteraction(_transitSet))
                {
                    // viable path that doesn't destroy the ability
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region public static Interaction InteractPowerTransit(PowerDeliveryStep<PowerSrc> acivation, int sequence, AimTarget target)
        /// <summary>Transits a power to a target</summary>
        public static Interaction InteractPowerTransit(PowerActivationStep<PowerSrc> activation, int sequence, AimTarget target)
        {
            // interact to aim target
            var _bObj = target.Target as ICoreObject;
            var _aLoc = activation.Actor.GetLocated()?.Locator;
            var _powerUse = activation.PowerUse;
            var _root = _powerUse.CapabilityRoot;
            var _interact = new StepInteraction(activation, activation.Actor, _powerUse, _bObj,
                new PowerActionTransit<PowerSrc>(_powerUse.PowerActionSource, _root, _powerUse.PowerTracker,
                activation.Actor, _aLoc, _aLoc.PlanarPresence, activation.TargetingProcess.Targets));
            _bObj.HandleInteraction(_interact);

            // get visualizer geometries
            var _sLoc = activation.Actor.GetLocated()?.Locator;
            var _tLoc = _bObj.GetLocated()?.Locator;

            var _target = target as AwarenessTarget;
            if (_target?.PlanLine != null)
            {
                // awareness plan line provides fallback if available
                var _startPoint = _sLoc?.MiddlePoint ?? _target.PlanLine.StartPoint;
                var _endPoint = _tLoc?.MiddlePoint ?? _target.PlanLine.EndPoint;
                _root.GeneratePowerDeliverVisualizers(_sLoc.MapContext, _startPoint, _endPoint, sequence, true);

            }
            else if ((_sLoc != null) && (_tLoc != null))
            {
                // otherwise, need both endpoints
                _root.GeneratePowerDeliverVisualizers(_sLoc.MapContext, _sLoc.MiddlePoint, _tLoc.MiddlePoint, sequence, true);
            }

            return _interact;
        }
        #endregion

        /// <summary>Default implementation of applying spell damage</summary>
        public static void ApplyDamage(PowerApplyStep<PowerSrc> apply, PreReqListStepBase damagePreReqStep, params int[] subModes)
            => ApplyDamage(apply.PowerUse.PowerActionSource, apply.Actor as Creature, apply.PowerUse.CapabilityRoot,
                apply.DeliveryInteraction, apply, damagePreReqStep, apply.IsCriticalHit, subModes);

        #region public static void ApplyDamage(PowerApplyStep<PowerSrc> apply, PreReqListStepBase damagePreReqSource, params int [] subModes)
        /// <summary>Default implementation of applying spell damage</summary>
        /// <param name="applyStep">Usually PowerApplyStep or AttackResultStep</param>
        /// <param name="damagePreReqStep">Usually the same as applyStep, unless one damage roll for multiple targets</param>
        public static void ApplyDamage(PowerSrc powerSource, Creature creature, ICapabilityRoot capabilityRoot,
            Interaction deliveryInteraction, CoreStep applyStep, PreReqListStepBase damagePreReqStep, bool critical, params int[] subModes)
        {
            var _dmgMode = capabilityRoot.GetCapability<IDamageCapable>();
            if (_dmgMode != null)
            {
                // for each damage submode specified
                var _unsaveable = new List<DamageData>();
                foreach (var _subMode in subModes)
                {
                    // check key prefix if more than one submode
                    bool _inMode(string bindKey) => (subModes.Length > 1) ? bindKey.StartsWith($@"({_subMode})|") : true;

                    // get the save key (if any)
                    var _saveKey = _dmgMode.DamageSaveKey(deliveryInteraction, _subMode);
                    if ((_saveKey != null) && !_saveKey.Equals(string.Empty))
                    {
                        // get the save prerequisite matching the save key for the damage
                        var _savePre = applyStep.AllPrerequisites<SavePrerequisite>(_saveKey).FirstOrDefault();
                        var _saveMode = _savePre.SaveMode;

                        // if save negates, then the save factor is 0
                        // if save half, then the save factor is 0.5
                        // otherwise it is 1.0 (save does not affect delivered damage)
                        var _factor = _saveMode.SaveEffect switch
                        {
                            SaveEffect.Negates => 0,
                            SaveEffect.Half => 0.5,
                            _ => 1.0
                        };

                        // all damages fall under a single save effect (NOTE: the save effect can be NoSave, basically just aggregating damage)
                        var _damages = (from _dmgRoll in damagePreReqStep.AllPrerequisites<DamageRollPrerequisite>(_inMode)
                                        from _getDmg in _dmgRoll.GetDamageData()
                                        select _getDmg).ToList();

                        // deliver damage
                        var _saveDmgInteract = new StepInteraction(applyStep, creature, powerSource, deliveryInteraction.Target,
                            new SaveableDamageData(creature, _damages, _saveMode, _factor, _savePre?.SaveRoll ?? new Deltable(1), false,
                                _dmgMode.CriticalFailDamagesItems(_subMode)));
                        deliveryInteraction.Target.HandleInteraction(_saveDmgInteract);
                        if (_saveDmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
                        {
                            new RetryInteractionStep(applyStep, @"Retry", _saveDmgInteract);
                        }
                    }
                    else
                    {
                        // unsaveable damage (includes energy damages)
                        _unsaveable.AddRange(from _dmgRoll in damagePreReqStep.AllPrerequisites<DamageRollPrerequisite>(_inMode)
                                             from _getDmg in _dmgRoll.GetDamageData()
                                             select _getDmg);
                    }
                }
                if (_unsaveable.Any())
                {
                    // all unsaveable damages are delivered as a unit
                    var _deliver = new DeliverDamageData(creature, _unsaveable, false, critical);
                    SendDamage(applyStep, creature, deliveryInteraction, powerSource, _deliver);
                }
            }
            return;
        }

        private static void SendDamage(CoreStep step, CoreActor actor, Interaction deliveryInteraction, PowerSrc powerSrc, DeliverDamageData deliverDamage)
        {
            var _dmgInteract = new StepInteraction(step, actor, powerSrc, deliveryInteraction.Target, deliverDamage);
            deliveryInteraction.Target.HandleInteraction(_dmgInteract);
            if (_dmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
            {
                new RetryInteractionStep(step, @"Retry", _dmgInteract);
            }
        }
        #endregion

        /// <summary>Attempt to deliver damage spell mode, then step to InfoStep (delivery failed) or ApplySpell.</summary>
        public static void DeliverDamage(PowerActivationStep<PowerSrc> activation, int sequence, int maxSequence, AimTarget target, params int[] damageModes)
        {
            var _delivery = InteractPowerTransit(activation, sequence, target);
            activation.AppendFollowing(new PowerApplyStep<PowerSrc>(activation, activation.PowerUse, activation.Actor,
                GetDamageModePreRequisites(activation, _delivery, damageModes, sequence, maxSequence, false), _delivery, false, false));
        }

        /// <summary>Used in delivery to add damage roller prerequisites to the delivery interaction</summary>
        public static IEnumerable<StepPrerequisite> GetDamageModePreRequisites(PowerActivationStep<PowerSrc> activation,
            Interaction workSet, int[] subModes, int targetIndex, int targetCount, bool isCriticalHit)
            => GetDamageModePreRequisites(activation.PowerUse.PowerActionSource, activation.Actor as Creature,
                activation.PowerUse.CapabilityRoot, workSet, subModes, targetIndex, targetCount, isCriticalHit);

        #region public static IEnumerable<StepPrerequisite> GetDamageModePreRequisites(PowerDeliveryStep<SpellSource> deliver, Interaction workSet, int [] subModes)
        /// <summary>Used in delivery to add damage roller prerequisites to the delivery interaction</summary>
        public static IEnumerable<StepPrerequisite> GetDamageModePreRequisites(IPowerActionSource powerSource,
            Creature creature, ICapabilityRoot capabilityRoot, Interaction attack,
            int[] subModes, int targetIndex, int targetCount, bool isCriticalHit)
        {
            string _preName(DamageRule dmgRule)
            {
                // TODO: perhaps include other information about the power...
                if (targetCount > 1)
                    return $@"{dmgRule.Name} @[{targetIndex + 1}]";
                else
                    return $@"{dmgRule.Name}";
            };
            var _damageMode = capabilityRoot.GetCapability<IDamageCapable>();
            if (_damageMode != null)
            {
                // key prefix if more than one submode
                string _modePre(int subMode) => (subModes.Length > 1) ? $@"({subMode})|" : string.Empty;

                foreach (var _subMode in subModes)
                {
                    // each is a separate damage rule
                    string _preKey(DamageRule dmgRule) => $@"{_modePre(_subMode)}{dmgRule.Key}";

                    foreach (var _rule in _damageMode.GetDamageRules(_subMode, isCriticalHit))
                    {
                        var _dice = _rule.Range as DiceRange;
                        if (_rule is EnergyDamageRule _energyRule)
                        {
                            if (_dice != null)
                            {
                                var _energyPre =
                                    new EnergyDamageRollPrerequisite(powerSource, attack, _preKey(_energyRule), _preName(_energyRule),
                                        _dice.EffectiveRoller(creature.ID, powerSource.PowerClass.ClassPowerLevel.QualifiedValue(attack)),
                                        false, @"Power", _subMode, _energyRule.EnergyType);
                                yield return _energyPre;
                            }
                            else
                            {
                                var _energyPre =
                                    new EnergyDamageRollPrerequisite(powerSource, attack, _preKey(_energyRule), _preName(_energyRule),
                                        new ConstantRoller(Convert.ToInt32(_rule.Range.EffectiveRange(creature,
                                            powerSource.PowerClass.ClassPowerLevel.QualifiedValue(attack)))),
                                        false, @"Power", _subMode, _energyRule.EnergyType);
                                yield return _energyPre;
                            }
                        }
                        else
                        {
                            if (_dice != null)
                            {
                                var _rollPre =
                                    new DamageRollPrerequisite(powerSource, attack, _preKey(_rule), _preName(_rule),
                                        _dice.EffectiveRoller(creature.ID, powerSource.PowerClass.ClassPowerLevel.QualifiedValue(attack)),
                                        false, _rule.NonLethal, @"Power", _subMode);
                                yield return _rollPre;
                            }
                            else
                            {
                                var _rollPre =
                                    new DamageRollPrerequisite(powerSource, attack, _preKey(_rule), _preName(_rule),
                                        new ConstantRoller(Convert.ToInt32(_rule.Range.EffectiveRange(creature,
                                            powerSource.PowerClass.ClassPowerLevel.QualifiedValue(attack)))),
                                        false, _rule.NonLethal, @"Power", _subMode);
                                yield return _rollPre;
                            }
                        }
                    }

                    var _saveMode = capabilityRoot.GetCapability<ISaveCapable>();
                    if (_saveMode != null)
                    {
                        var _saveSub = _damageMode.DamageSaveKey(attack, _subMode);
                        if (!_saveSub.Equals(string.Empty))
                        {
                            var _save = _saveMode.GetSaveMode(creature, powerSource, attack, _saveSub);
                            if (_save?.SaveType >= SaveType.Fortitude)
                                yield return new SavePrerequisite(powerSource, attack, _saveSub, @"Save", _save);
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public static CoreStep DeliverNextStep(PowerDeliveryStep<PowerSrc> deliver, int sequence, AimTarget target, params int[] generalModes)
        /// <summary>Get the next step needs to deliver a super natural power to a target.  Does not enqueue the step.</summary>
        public static CoreStep DeliverNextStep(PowerActivationStep<PowerSrc> activation, int sequence, AimTarget target, bool silentFail,
            params int[] generalModes)
        {
            // transit
            var _delivery = InteractPowerTransit(activation, sequence, target);

            var _powerBack = _delivery.Feedback.OfType<PowerActionTransitFeedback<PowerSrc>>().FirstOrDefault();
            if ((_powerBack != null) && _powerBack.Success)
            {
                // get general prerequisites
                var _source = activation.PowerUse.PowerActionSource;
                var _root = activation.PowerUse.CapabilityRoot as ICapabilityRoot;
                var _general = _root.GetCapability<IGeneralSubMode>();
                var _preReqs = (from _subMode in generalModes
                                from _pre in _general.GetGeneralSubModePrerequisites(_subMode, _delivery)
                                select _pre).ToArray();

                // save modes?
                var _saveMode = _root.GetCapability<ISaveCapable>();
                if (_saveMode != null)
                {
                    var _saveKeys = (from _subMode in generalModes
                                     let _saveKey = _general.GeneralSaveKey(activation.TargetingProcess, _delivery, _subMode)
                                     where (_saveKey != null) && !_saveKey.Equals(string.Empty)
                                     select _saveKey).Distinct();
                    _preReqs = AddSavePrerequisites(_saveMode, activation.Actor, activation.PowerUse.PowerActionSource,
                        _delivery, _source, _preReqs, _saveKeys);
                }

                // TODO: figure out how to get generalModes into this
                return new PowerApplyStep<PowerSrc>(activation, activation.PowerUse, activation.Actor,
                    _preReqs, _delivery, false, silentFail);
            }
            else
            {
                return activation.GetNotifyStep(@"Power activation failed", @"Failed", false);
            }
        }
        #endregion

        #region public static CoreStep DeliverDurableNextStep<DurableSrc>(...)
        public static CoreStep DeliverDurableNextStep<DurableSrc>(CoreStep predecessor,
            CoreActor actor, IGeometricContext anchor, PlanarPresence anchorPresence, IPowerUse<DurableSrc> powerUse,
            IEnumerable<AimTarget> targets, AimTarget target, bool silentFail, params int[] durableModes)
            where DurableSrc : MagicPowerActionSource
        {
            // cast parameters to approriate types
            var _capabilities = powerUse.CapabilityRoot;
            var _source = powerUse.PowerActionSource;
            var _tracker = powerUse.PowerTracker;
            var _durable = _capabilities.GetCapability<IDurableCapable>();
            if (_durable != null)
            {
                var _target = target.Target as IInteract;

                MagicPowerEffect _getMagicEffect(DurationRule durationRule, int subMode)
                    => durationRule.DurationType switch
                    {
                        DurationType.Concentration
                            => new FragileMagicEffect(_source, _capabilities, _tracker,
                                durationRule.EndTime(actor, _source, _target), TimeValTransition.Entering, subMode),
                        DurationType.ConcentrationPlusSpan
                            => new FragileMagicEffect(_source, _capabilities, _tracker,
                                durationRule.EndTime(actor, _source, _target), TimeValTransition.Entering, subMode),
                        _ => new DurableMagicEffect(_source, _capabilities, _tracker,
                            durationRule.EndTime(actor, _source, _target), TimeValTransition.Entering, subMode)
                    };

                // define all effects
                if (durableModes.Length == 0)
                    durableModes = _durable.DurableSubModes.FirstOrDefault().ToEnumerable().ToArray();
                var _effects = (from _subMode in durableModes
                                let _dr = _durable.DurationRule(_subMode)
                                select _getMagicEffect(_dr, _subMode)).ToList();

                // transit (through regular means)
                var _transit = new MagicPowerEffectTransit<DurableSrc>(_source, _capabilities, _tracker,
                    _effects, actor, anchor, anchorPresence, targets);
                var _delivery = new StepInteraction(predecessor, actor, _source, _target, _transit);
                _target.HandleInteraction(_delivery);

                // must have successfully transitted
                if (_delivery.Feedback.OfType<PowerActionTransitFeedback<DurableSrc>>().Any(_paf => _paf.Success))
                {
                    // get visualizer geometries
                    var _tLoc = (_target as ICoreObject)?.GetLocated();
                    if (_tLoc != null)
                    {
                        // both endpoints must be available
                        _capabilities.GeneratePowerDeliverVisualizers(_tLoc.Locator.MapContext,
                            anchor.GeometricRegion.GetPoint3D(), _tLoc.Locator.MiddlePoint, 0, true);
                    }

                    // get the durable mode prerequisites
                    var _preReqs = (from _subMode in durableModes
                                    from _pre in _durable.GetDurableModePrerequisites(_subMode, _delivery)
                                    select _pre).ToArray();

                    // save modes?
                    var _spellSave = _capabilities.GetCapability<ISaveCapable>();
                    if (_spellSave != null)
                    {
                        var _saveKeys = (from _subMode in durableModes
                                         let _saveKey = _durable.DurableSaveKey(targets, _delivery, _subMode)
                                         where !string.IsNullOrWhiteSpace(_saveKey)
                                         select _saveKey).Distinct();
                        _preReqs = AddSavePrerequisites(_spellSave, actor, _source, _delivery, _source, _preReqs, _saveKeys);
                    }

                    return new PowerApplyStep<DurableSrc>(predecessor, powerUse, actor, _preReqs, _delivery, false, silentFail);
                }
            }
            return null;
        }
        #endregion

        #region public static Interaction InteractSingleTargetAttack(PowerDeliveryStep<PowerSrc> activation, int sequence, AimTarget target)
        /// <summary>Delivers a spell to a target as an attack</summary>
        public static Interaction InteractSingleTargetAttack(PowerActivationStep<PowerSrc> activation, int sequence, AimTarget target)
        {
            // cast parameters to approriate types
            var _target = target as AttackTarget;
            var _interactor = _target.Target as IInteract;
            var _atkInteract = new StepInteraction(activation, activation.Actor, activation.PowerUse.PowerActionSource, _target.Target, _target.Attack);

            // first, perform a ranged touch attack against the target (if one is defined)
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

            var _atkFB = _atkInteract.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            var _aLoc = activation.Actor.GetLocated()?.Locator;
            var _powerUse = activation.PowerUse;
            var _mode = _powerUse.CapabilityRoot;
            var _source = _powerUse.PowerActionSource;
            var _tracker = _powerUse.PowerTracker;

            // then, transit the spell...
            if (_atkFB?.Hit ?? false)
            {
                // tell the interactor it is a spell target
                _mode.GeneratePowerDeliverVisualizers(_aLoc.MapContext, _target.SourcePoint, _target.TargetPoint, sequence, true);
                var _spellDelivery = new StepInteraction(activation, activation.Actor, _source, _target.Target,
                    new PowerActionTransit<PowerSrc>(_source, _mode, _tracker, activation.Actor, _aLoc, _aLoc.PlanarPresence,
                    activation.TargetingProcess.Targets));
                _interactor?.HandleInteraction(_spellDelivery);
                return _spellDelivery;
            }
            else
            {
                _mode.GeneratePowerDeliverVisualizers(_aLoc?.MapContext, _target.SourcePoint, _target.TargetPoint, sequence, false);
                return _atkInteract;
            }
        }
        #endregion

        #region public static IEnumerable<CoreStep> DeliverDirectFromBurst(PowerBurstCapture<PowerSrc> burst, Locator locator, DeliverBurstFilter allow, params int[] generalModes)
        /// <summary>
        /// Deliver a power where the target was acquired by a burst.  
        /// Only affect ICore directly on the Locator (not as parts or object load)
        /// </summary>
        public static IEnumerable<CoreStep> DeliverDirectFromBurst(PowerBurstCapture<PowerSrc> burst, Locator locator,
            DeliverBurstFilter deliverAllow, ApplyBurstFilter applyAllow, params int[] generalModes)
        {
            // get the burst
            var _delivery = burst.Activation;
            foreach (var _core in from _c in locator.GetCapturable<ICore>()
                                  where deliverAllow(locator, _c)
                                  select _c)
            {
                var _interactCore = _core as IInteract;
                var _target = new AimTarget(@"Target", _interactCore);

                // NOTE: we use sequence=0 for bursts so that they do not animate in sequence
                var _step = DeliverNextStep(_delivery, 0, _target, true, generalModes);
                if (applyAllow(_step))
                    yield return _step;
            }
            yield break;
        }
        #endregion

        #region public static void DeliverBurstToMultipleSteps(PowerDeliveryStep<PowerSrc> deliver, Intersection intersect, Geometry geometry, IEnumerable<StepPrerequisite> preReqs)
        /// <summary>
        /// Captures locators via a burst effect (calling back into IBurstCapture.Capture to get the steps)
        /// and adds a MultiStep with all the subsequent burst steps for each affected locator.  (Sounds simple, huh?)
        /// </summary>
        public static void DeliverBurstToMultipleSteps(PowerActivationStep<PowerSrc> activation, Intersection intersect,
            Geometry geometry, IEnumerable<StepPrerequisite> preReqs)
        {
            var _source = activation.PowerUse.PowerActionSource;
            if (TransitPowerToIntersection(activation, intersect))
            {
                // setup burst
                var _bCapture = activation.PowerUse.CapabilityRoot.GetCapability<IBurstCaptureCapable>();

                // get the actor, so we can figure out the map context
                var _locator = Locator.FindFirstLocator(activation.Actor);

                // define burst capture
                var _burst = new PowerBurstCapture<PowerSrc>(_locator.MapContext, activation, geometry, intersect, _bCapture,
                    _locator.PlanarPresence);
                _bCapture.PostInitialize(_burst);

                // do the burst (construction of step automatically enqueues it)
                _ = new MultiNextStep(activation, _burst.DoBurst(), preReqs);

                // finish use
                _source.UsePower();
            }
        }
        #endregion

        public static IGeometryBuilder GetDeliveryGeometry(PowerActivationStep<PowerSrc> delivery)
            => delivery.PowerUse.CapabilityRoot.GetCapability<IGeometryCapable<PowerSrc>>()
            .GetBuilder(delivery.PowerUse, delivery.Actor);

        public static IGeometryBuilder GetApplyGeometry(PowerApplyStep<PowerSrc> apply)
            => apply.PowerUse.CapabilityRoot.GetCapability<IGeometryCapable<PowerSrc>>()
            .GetBuilder(apply.PowerUse, apply.Actor);
    }
}
