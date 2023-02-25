using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Non-lethal is ignored, and dead-occurs at 0 health-points
    /// </summary>
    [Serializable]
    public class CreatureNonLivingDamageHandler : IInteractHandler
    {
        #region private void AdjustActorState(Creature critter)
        private void AdjustActorState(Creature critter)
        {
            // just took some damage!
            var _value = critter.HealthPoints.CurrentValue;
            if (_value <= 0)
            {
                // dead effect (only "dead" once)
                var _dead = critter.Adjuncts.OfType<DeadEffect>().FirstOrDefault();
                if (_dead == null)
                {
                    critter.AddAdjunct(new DeadEffect(this, (critter.Setting as ITacticalMap)?.CurrentTime ?? 0));
                }
            }
        }
        #endregion

        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            var _critter = workSet.Target as Creature;

            // status reporting to target
            var _stepSet = workSet as StepInteraction;
            var _dInfo = new List<DamageInfo>();

            // collect from damage
            if (workSet.InteractData is IDeliverDamage _damage)
            {
                var _lethal = _damage.GetLethal();

                // apply damage by type
                if (_lethal > 0)
                {
                    // apply all damage at once...
                    _critter.HealthPoints.CurrentValue -= _lethal;
                    _dInfo.Add(new LethalDamageInfo { Amount = _lethal });

                    // adjust ActorState
                    AdjustActorState(_critter);

                    // distractions as needed
                    var _map = _critter.GetLocated().Locator.Map;
                    if (_map != null)
                    {
                        // get any distractable activities being performed by the target
                        // NOTE: this includes continuous damage as full distracting damage, but that's OK
                        // NOTE: continuous will only interrupt a SPAN activity in this regard, in which case, all distracts anyway
                        var _difficulty = new Deltable(10 + _lethal);

                        // span-reaching activities of the target that are distractable
                        var _spanAdj = _critter.Adjuncts.OfType<SpanActionAdjunct>().FirstOrDefault();
                        if (_spanAdj != null)
                        {
                            if (_spanAdj.Action is IDistractable _spanCon)
                            {
                                // force a success check interaction, failure tracked on the adjunct
                                var _distractStep = new SpanDistractionStep(null, workSet,
                                    _spanCon.ConcentrationBase.EffectiveValue + _difficulty.EffectiveValue);
                                var _distractProcess = new CoreProcess(_distractStep, @"Distraction to Action");
                                _map.ContextSet.ProcessManager.StartProcess(_distractProcess);
                            }
                        }

                        foreach (var _distractable in _critter.Adjuncts.OfType<IDistractable>())
                        {
                            // force a success check interaction, failure tracked on the adjunct
                            var _distractStep = new ConcentrationDistractionStep(null, workSet,
                                _difficulty.EffectiveValue, _distractable);
                            var _distractProcess = new CoreProcess(_distractStep, @"Distraction to Adjunct");
                            _map.ContextSet.ProcessManager.StartProcess(_distractProcess);
                        }

                        // current activities for the actor that are distractable
                        foreach (var _activity in from _act in _map.ContextSet.ProcessManager.AllActivities
                                                  where (_act.Actor == _critter) && _act.IsActive
                                                  && (_act.Action is IDistractable)
                                                  select _act)
                        {
                            _activity.Targets.Add(
                                new ValueTarget<IncidentalDamage>(@"Damage.Incidental",
                                    new IncidentalDamage
                                    {
                                        Damage = _lethal,
                                        Incident = workSet
                                    }));
                        }

                        if (_damage.IsContinuous)
                        {
                            // distracted adjunct
                            // NOTE: distracted adjunct only deals with activities that start after this point
                            var _distract = new ContinuousDamage(this, _lethal / 2);
                            var _exp = new Expiry(_distract, _map.CurrentTime + Round.UnitFactor, TimeValTransition.Entering, Round.UnitFactor);
                            _critter.AddAdjunct(_exp);
                        }
                    }
                    workSet.Feedback.Add(new UnderstoodFeedback(this));
                    if (_stepSet != null)
                    {
                        _stepSet.DoTargetNotifyStep(new DealtDamageNotify(_critter.ID, @"Damage Taken", _dInfo,
                            GetInfoData.GetInfoFeedback(_stepSet.Source as ICoreObject, _critter)));
                        _stepSet.DoTargetNotifyStep(new RefreshNotify(true, true, true, false, false));
                    }
                }

                // secondary attack results
                foreach (var _secondary in _damage.Secondaries)
                {
                    if (_secondary.IsDamageSufficient(_stepSet))
                        _secondary.AttackResult(_stepSet);
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // this will be the last before the defaults, which would be creature damage handler
            return false;
        }
        #endregion
    }
}
