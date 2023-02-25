using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Armor;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// If damage allows a saving throw, this is the thing that implements it
    /// </summary>
    [Serializable]
    public class SaveFromDamageHandler : IInteractHandler
    {
        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is SaveableDamageData _saveDamage)
            {
                if (workSet.Target is Creature _critter)
                {
                    if (_saveDamage.Success(workSet))
                    {
                        foreach (var _dmg in _saveDamage.Damages)
                        {
                            // subtract from each damage
                            _dmg.Amount -= Convert.ToInt32((double)_dmg.Amount * _saveDamage.SaveFactor);
                        }
                    }
                    else
                    if ((_saveDamage.SaveRoll.BaseValue == 1)
                        && (_saveDamage.SaveMode.SaveType >= SaveType.Fortitude)
                        && (workSet is StepInteraction _stepInteract))
                    {
                        var _candidates = _critter.Body.ItemSlots.AllSlots
                            .Where(_is => _is.SlottedItem != null)
                            .Select(_is => _is.SlottedItem.BaseObject)
                            .OfType<IProvideSaves>()
                            .Distinct().ToList();
                        var _dmgs = _saveDamage.Damages
                            .Select(_d => _d.Clone())
                            .OfType<DamageData>()
                            .ToList();
                        if (_candidates.Any() && _dmgs.Any())
                        {
                            new PowerApplySaveFailedStep(_stepInteract.Step, _stepInteract.Actor,
                                _stepInteract.Source, _candidates, _saveDamage.SaveMode.SaveType, 
                                _saveDamage.SaveMode.SaveEffect, _saveDamage.SaveMode.Difficulty, 
                                _saveDamage.SaveFactor, _dmgs);
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

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;
    }
}
