using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Ignore criticals (up to 100%)
    /// </summary>
    [Serializable]
    public class CriticalFilterHandler : IInteractHandler
    {
        /// <summary>Ignore criticals (up to 100%)</summary>
        public CriticalFilterHandler(int ignoreChance)
            : base()
        {
            IgnoreChance = ignoreChance;
        }

        /// <summary>Percent of criticals ignored</summary>
        public int IgnoreChance { get; private set; }

        #region public void HandleInteraction(InteractWorkSet workSet)
        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is AttackData _atk)
                && _atk.CriticalThreat)
            {
                if ((IgnoreChance == 100) || (DiceRoller.RollDie(workSet.Target?.ID ?? Guid.Empty, 100, @"Critical Filter", @"Ignore Chance") <= IgnoreChance))
                {
                    _atk.Alterations.Add(workSet.Target, new IgnoreCriticalAlteration(workSet.InteractData, this));
                }
            }
            return;
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
            // put this after the transit handler
            var _type = existingHandler.GetType();
            if ((_type == typeof(TransitAttackHandler)) || (_type == typeof(FlankingCheckHandler)))
                return false;
            else
                return true;
        }
        #endregion

        /// <summary>Has a 100% critical ignore chance</summary>
        public static bool IsImmuneToCriticals(CoreObject coreObj)
            => coreObj.InteractionHandlers.OfType<CriticalFilterHandler>().Any(_h => _h.IgnoreChance >= 100);
    }
}
