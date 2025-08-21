using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class GraspProbeBudget : ITurnEndBudgetItem, IActionProvider
    {
        public GraspProbeBudget(int number)
        {
            _Number = number;
            _Budget = null;
        }

        #region data
        private int _Number;
        private Guid _ID = Guid.NewGuid();
        private CoreActionBudget _Budget;
        #endregion

        public object Source => typeof(GraspProbeBudget);
        public Guid ID => _ID;
        public Guid PresenterID => _ID;

        /// <summary>Gets any defined grasp/probe budget</summary>
        public static GraspProbeBudget GetBudget(CoreActionBudget budget)
        {
            if (budget.BudgetItems.ContainsKey(typeof(GraspProbeBudget)))
            {
                return budget.BudgetItems[typeof(GraspProbeBudget)] as GraspProbeBudget;
            }

            return null;
        }

        public void UseGrasp()
        {
            _Number--;
            return;
        }

        #region ITurnEndBudgetItem Members

        public bool EndTurn()
        {
            // must remove
            return true;
        }

        #endregion

        #region IBudgetItem Members

        public string Name => @"Grasp/Probe";

        public string Description => @"Grasp/Probe";

        public void Added(CoreActionBudget budget)
        {
            _Budget = budget;
            _Budget.Actor.Actions.Providers.Add(this, this);
        }

        public void Removed()
        {
            _Budget.Actor.Actions.Providers.Remove(this);
        }

        #endregion

        #region IActionProvider Members

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if ((_Number > 0) && (_budget.HasActivity))
            {
                var _origin = _budget.TopActivity?.Action;
                if (_origin is Grasp)
                {
                    yield return new Grasp(_origin.ActionSource, new ActionTime(TimeType.SubAction), @"901");
                }
                else if (_origin is Probe)
                {
                    yield return new Probe((_origin as Probe).WeaponHead, new ActionTime(TimeType.SubAction), @"901");
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget) { return new Info { Message = Name }; }

        #endregion
    }
}
