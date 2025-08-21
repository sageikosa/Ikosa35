using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveTarget : GroupMemberAdjunct, ITrackTime
    {
        public RemoteMoveTarget(object source, RemoteMoveGroup group)
            : base(source, group)
        {
            _MoveBudget = 1d;
        }

        #region state
        private double _MoveBudget;
        private bool _DiagFlag;
        private double _ResetTime;
        #endregion

        protected override void OnActivate(object source)
        {
            _ResetTime = (Anchor?.GetCurrentTime() ?? 0) + Round.UnitFactor;
            base.OnActivate(source);
        }

        public RemoteMoveGroup RemoteMoveGroup => Group as RemoteMoveGroup;
        public void DoneMove() => _MoveBudget = 0;
        public bool AnyMoveLeft => _MoveBudget > 0d;

        private double Needed(bool diagonal, double cost)
            => (((diagonal && _DiagFlag) ? 2 * cost : cost) * 5) / (RemoteMoveGroup.Movement.EffectiveValue);

        /// <summary>True if the total budget can accomodate the additional distance.  If high-stealth, then double not counted.</summary>
        public bool CanMove(bool diagonal, double cost)
            => Needed(diagonal, cost) <= _MoveBudget;

        #region public void DoMove(RemoteMoveAction moveAct, bool diagonal, int cost)
        /// <summary>Register the movement square</summary>
        public void DoMove(bool diagonal, double cost)
        {
            var _needed = Needed(diagonal, cost);

            _MoveBudget -= _needed;

            // flip diagonal flag
            if (diagonal)
            {
                _DiagFlag = !_DiagFlag;
            }
        }
        #endregion

        public override object Clone()
            => new RemoteMoveTarget(Source, RemoteMoveGroup);

        // ITrackTime
        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((direction == TimeValTransition.Entering) && (timeVal >= _ResetTime))
            {
                _MoveBudget = 1d;
                _ResetTime = timeVal + Round.UnitFactor;
            }
        }
    }
}
