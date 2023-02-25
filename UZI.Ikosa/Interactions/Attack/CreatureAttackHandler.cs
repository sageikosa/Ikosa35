using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using System.Diagnostics;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Handles most attacks directed at a creature
    /// </summary>
    [Serializable]
    public class CreatureAttackHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            Debug.WriteLine($@"{DateTime.Now:o}:CreatureAttackHandler.HandleInteraction");
            if ((workSet.InteractData is AttackData _atk)
                && (workSet.Target is Creature _critter))
            {
                if (_critter.IsFriendly(_atk.Attacker?.ID ?? Guid.Empty)
                    && !_critter.Awarenesses.IsTotalConcealmentMiss(_atk.Attacker?.ID ?? Guid.Empty)
                    && _atk.Harmless)
                {
                    // if the attacker is considered "friendly" and the attack is harmless
                    workSet.Feedback.Add(new AttackFeedback(this, true));
                    return;
                }

                // NOTE: squeezing handled by the locator as a delta
                // TODO: determine tactical conditions...(higher ground, kneeling, sitting, invisible attacker)
                Deltable _usedArmorRating = null;
                switch (_atk.Impact)
                {
                    case AttackImpact.Penetrating:
                        _usedArmorRating = _critter.NormalArmorRating;
                        break;

                    case AttackImpact.Touch:
                        _usedArmorRating = _critter.TouchArmorRating;
                        break;

                    case AttackImpact.Incorporeal:
                        _usedArmorRating = _critter.IncorporealArmorRating;
                        break;

                    default:
                        break;
                }

                // attack deltas from the Attack Source
                if (workSet.Source is IAttackSource _atkSource)
                {
                    var _srcDelta = new SoftQualifiedDelta(_atkSource.AttackBonus);
                    _atk.AttackScore.Deltas.Add(_srcDelta);
                    if (_atk.CriticalThreat && (_atk.CriticalConfirmation != null))
                        _atk.CriticalConfirmation.Deltas.Add(_srcDelta);
                }

                // determine result
                var _check = Deltable.GetCheckNotify(_atk.Attacker?.ID, @"AttackScore", _critter.ID, @"ArmorRating");
                var _finalScore = _atk.AttackScore.QualifiedValue(workSet, _check.CheckInfo);
                var _finalArmor = _usedArmorRating.QualifiedValue(workSet, _check.OpposedInfo);
                var _hit = _atk.IsHit ?? (_finalScore >= _finalArmor);
                var _critical = _hit && _atk.CriticalThreat
                    && (_atk.CriticalConfirmation.QualifiedValue(workSet, Deltable.GetDeltaCalcNotify(_atk.Attacker?.ID, @"CriticalScore").DeltaCalc) >= _finalArmor)
                    && !_atk.Alterations.Contains(typeof(IgnoreCriticalAlteration));
                workSet.Feedback.Add(new AttackFeedback(this, _hit, _critical));
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

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => false;
    }
}
