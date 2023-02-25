using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Adjunct associated with the budget.</summary>
    [Serializable]
    public class AdjunctBudget : IBudgetItem
    {
        /// <summary>Adjunct associated with the budget.</summary>
        public AdjunctBudget(Adjunct adjunct, string name, string description)
        {
            _Adjunct = adjunct;
            _Name = name;
            _Description = description;
        }

        #region data
        private Adjunct _Adjunct;
        private string _Name;
        private string _Description;
        #endregion

        public Adjunct Adjunct => _Adjunct;
        public string Name => _Name;
        public string Description => _Description;
        public object Source => Adjunct;

        public void Added(CoreActionBudget budget)
        {
            budget.Actor.AddAdjunct(Adjunct);
        }

        public void Removed()
        {
            Adjunct?.Eject();
        }

        public AdjunctBudgetInfo ToAdjunctBudgetInfo()
        {
            var _info = this.ToBudgetItemInfo<AdjunctBudgetInfo>();
            _info.Adjunct = new AdjunctInfo(Adjunct?.GetType().FullName, Adjunct?.ID ?? Guid.Empty);
            return _info;
        }
    }
}
