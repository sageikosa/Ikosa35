using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Handles most attacks directed at an object
    /// </summary>
    [Serializable]
    public class ObjectAttackHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            // NOTE: objects cannot be critically hit (no vital spots)
            if ((workSet.InteractData is AttackData _atk)
                && (workSet.Target is ObjectBase _object))
            {
                var _attended = _object.Adjuncts.OfType<Attended>().FirstOrDefault();

                // TODO: environmental conditions and ranged/melee
                if (_atk.Attacker?.ObjectLoad.Contains(_object.ID) ?? false)
                {
                    // if attended by the actor, automatic...
                    workSet.Feedback.Add(new AttackFeedback(this, true));
                }
                else if ((_attended != null)
                    && (_attended.Creature != null)
                    && !_attended.Creature.Awarenesses.IsTotalConcealmentMiss(_atk.Attacker?.ID ?? Guid.Empty)
                    && _attended.Creature.IsFriendly(_atk.Attacker?.ID ?? Guid.Empty)
                    && _atk.Harmless)
                {
                    // if the attacker is considered "friendly" to the item's possessor
                    workSet.Feedback.Add(new AttackFeedback(this, true));
                }
                else
                {
                    // otherwise, attack object's AR

                    // attack deltas from the Attack Source
                    if (workSet.Source is IAttackSource _atkSource)
                    {
                        var _srcDelta = new SoftQualifiedDelta(_atkSource.AttackBonus);
                        _atk.AttackScore.Deltas.Add(_srcDelta);
                        if (_atk.CriticalThreat && (_atk.CriticalConfirmation != null))
                            _atk.CriticalConfirmation.Deltas.Add(_srcDelta);
                    }

                    _object.AttendeeAdjustments(workSet.Source as IAttackSource, _atk);

                    // determine result
                    var _check = Deltable.GetCheckNotify(_atk.Attacker?.ID, @"AttackScore", _object.ID, @"Armor");
                    var _finalScore = _atk.AttackScore.QualifiedValue(workSet, _check.CheckInfo);
                    _check.OpposedInfo.SetBaseResult(_object.ArmorRating);
                    var _hit = _atk.IsHit ?? (_finalScore >= _check.OpposedInfo.Result);
                    var _critical = _hit && _atk.CriticalThreat
                        && (_atk.CriticalConfirmation.QualifiedValue(workSet, Deltable.GetDeltaCalcNotify(_atk.Attacker?.ID, @"CriticalScore").DeltaCalc) >= _check.OpposedInfo.Result)
                        && !_atk.Alterations.Contains(typeof(IgnoreCriticalAlteration));

                    workSet.Feedback.Add(new AttackFeedback(this, _hit, _critical));
                }
            }
        }
        #endregion

        #region public IEnumerable<Type> GetInteractionTypes()
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(MeleeAttackData);
            yield return typeof(ReachAttackData);
            yield return typeof(RangedAttackData);
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
