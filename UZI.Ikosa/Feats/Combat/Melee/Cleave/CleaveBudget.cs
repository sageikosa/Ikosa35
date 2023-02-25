using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public class CleaveBudget : IResetBudgetItem
    {
        public CleaveBudget(Deltable capacity)
        {
            _Capacity = capacity;
            _Used = 0;
        }

        #region data
        private Deltable _Capacity;
        private int _Used;
        #endregion

        /// <summary>Returns the number of available cleave attacks in the budget (capacity - count)</summary>
        public int Available
            => _Capacity.EffectiveValue - _Used;

        /// <summary>Number of cleave attacks that can be made between turn resets.</summary>
        public Deltable Capacity => _Capacity;

        public string Name => @"Cleave Attacks";
        public string Description => $@"{Available} of {Capacity.EffectiveValue} remaining";
        public object Source => typeof(CleaveBudget);

        public void Added(CoreActionBudget budget) { }
        public void Removed() { }

        /// <summary>Resets the number used</summary>
        public bool Reset()
        {
            _Used = 0;
            return false;
        }

        public void RegisterUse()
        {
            _Used++;
        }

        public CapacityBudgetInfo ToCapacityBudgetInfo()
        {
            var _info = this.ToBudgetItemInfo<CapacityBudgetInfo>();
            _info.Available = Available;
            _info.Capacity = Capacity.EffectiveValue;
            return _info;
        }
    }
}
