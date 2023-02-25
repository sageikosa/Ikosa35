using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveBudget : IResetBudgetItem
    {
        public RemoteMoveBudget(RemoteMoveGroup remoteMoveGroup)
        {
            _Group = remoteMoveGroup;
            _Remaining = remoteMoveGroup.Movement.EffectiveValue;
        }

        #region state
        protected RemoteMoveGroup _Group;
        protected double _Remaining;
        #endregion

        public double Remaining => _Remaining;
        public RemoteMoveGroup RemoteMoveGroup => _Group;
        public object Source => _Group;

        public string Name => @"Remote Movement Range";
        public string Description => $@"{_Remaining} remaining of {RemoteMoveGroup.Movement.EffectiveValue}";

        public void Added(CoreActionBudget budget) { }
        public void Removed() { }

        public bool Reset()
        {
            _Remaining = RemoteMoveGroup.Movement.EffectiveValue;
            return true;
        }
    }
}
