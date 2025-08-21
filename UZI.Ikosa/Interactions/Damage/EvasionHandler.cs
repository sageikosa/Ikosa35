using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Handles evasion (but only when a successful save roll is attached)</summary>
    [Serializable]
    public class EvasionHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is SaveableDamageData _saveDamage)
            {
                if ((workSet.Target is Creature _critter)
                    && !_critter.Conditions.Contains(Condition.Helpless)
                    && ((_critter.EncumberanceCheck.ArmorWorn?.ProficiencyType ?? ArmorProficiencyType.None)
                    <= ArmorProficiencyType.Light))
                {
                    if ((_saveDamage.SaveMode.SaveType == SaveType.Reflex)
                        && (_saveDamage.SaveMode.SaveEffect == SaveEffect.Half))
                    {
                        if (_saveDamage.Success(workSet))
                        {
                            // absorb all damage (no need to alter, as subsequent evasion handlers will not have a chance to deal with this)
                            workSet.Feedback.Add(new UnderstoodFeedback(this));
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
            yield return typeof(SaveableDamageData);
            yield break;
        }
        #endregion

        #region public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // NOTE: Improved Evasion must link after this, and add an alteration to prevent multiply sourced improved evasion from
            //       further reducing damage
            if (existingHandler.GetType() == typeof(EnergyResistanceHandler))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}