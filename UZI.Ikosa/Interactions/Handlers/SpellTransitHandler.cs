using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Handles spell transit and always provides SpellTransitFeedback, and occasionally SpellResistanceFeedback also
    /// </summary>
    [Serializable]
    public class SpellTransitHandler : IInteractHandler
    {
        #region public InteractionFeedback HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            // basically, we need to drag the spell transit through the environment
            if (workSet.InteractData is PowerActionTransit<SpellSource> _transit)
            {
                if (workSet.Target is ICoreObject _target)
                {
                    // look for an unaltered line
                    // NOTE: spell transit is serial, so we look for an unaltered, or viable path
                    SegmentSet _useLine = null;
                    var _map = _target.GetLocated().Locator.Map;
                    var _targetLocator = Locator.FindFirstLocator(_target);
                    if (!_transit.PowerSource.SpellDef.HasPlanarCompatibility(_targetLocator.PlanarPresence, _transit.AnchorPresence))
                    {
                        // cannot transit with no planar compatibility
                        workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, _target, false));
                    }
                    else
                    {
                        // nothing that leaves the spell unaltered, so check for viability
                        foreach (var _lSet in _transit.Anchor
                            .EffectLinesToTarget(_targetLocator.GeometricRegion,
                            ITacticalInquiryHelper.GetITacticals(_target).ToArray(),
                            _transit.AnchorPresence))
                        {
                            // checking each effect line for viability
                            if (_lSet.CarryInteraction(workSet))
                            {
                                _useLine = _lSet;
                                break;
                            }
                        }

                        if (_useLine == null)
                        {
                            // no viable paths
                            workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, _target, false));
                        }
                        else
                        {
                            // carry the interaction through the selected line
                            if (_useLine.CarryInteraction(workSet))
                            {
                                if (_target is Creature _creature)
                                {
                                    // if there is a MindAffecting descriptor, the creature must have intelligence
                                    if (_transit.PowerSource.SpellDef.Descriptors.Count(_t => typeof(MindAffecting).IsAssignableFrom(_t.GetType())) > 0)
                                    {
                                        if (_creature.Abilities.Intelligence.IsNonAbility || (_creature.Abilities.Intelligence.EffectiveValue < 1))
                                        {
                                            workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, _target, false));
                                            return;
                                        }
                                    }

                                    // if this is a creature with effective spell resistance to the spell, it must be checked
                                    if (_transit.CapabilityRoot is ISpellMode _spellMode)
                                    {
                                        if (_spellMode.AllowsSpellResistance)
                                        {
                                            switch (_transit.PowerTracker.DoesAffect(_creature.ID))
                                            {
                                                case false:
                                                    workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, _target, false));
                                                    return;

                                                case null:
                                                    // creature has not tested out, so make the check
                                                    if (_creature.SpellResistance.QualifiedValue(workSet, Deltable.GetDeltaCalcNotify(_creature.ID, @"Spell Resistance").DeltaCalc) > 0)
                                                    {
                                                        if (workSet is StepInteraction _stepInteract
                                                            && (_stepInteract.Step != null))
                                                        {
                                                            // append pre-emption to allow target to lower spell-resistance
                                                            // and then make a caster level check
                                                            _stepInteract.Step.Process.AppendPreEmption(
                                                                new LowerSpellResistanceStep(workSet, _creature, _transit.PowerTracker));
                                                        }
                                                        else
                                                        {
                                                            // without a step interaction, simply check spell resistance 
                                                            // with no option for target to lower
                                                            workSet.Feedback.Add(new SpellResistanceFeedback(workSet, _creature, _transit.PowerTracker));
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }

                                // since these are feedbacks, a retry interaction step will occur
                                workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, _target, true));
                                return;
                            }

                            // since these are feedbacks, a retry interaction step will occur
                            workSet.Feedback.Add(new PowerActionTransitFeedback<SpellSource>(workSet, _target, false));
                        }
                    }
                }
            }
            return;
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(PowerActionTransit<SpellSource>);
            yield return typeof(MagicPowerEffectTransit<SpellSource>);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return false;
        }
        #endregion
    }
}
