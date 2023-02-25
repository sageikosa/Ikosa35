using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveMaster : GroupMasterAdjunct, IActionProvider
    {
        public RemoteMoveMaster(object source, RemoteMoveGroup group)
            : base(source, group)
        {
        }

        public RemoteMoveGroup RemoteMoveGroup => Group as RemoteMoveGroup;

        public override object Clone()
            => new RemoteMoveMaster(Source, RemoteMoveGroup);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget is LocalActionBudget _budget) && (_budget.TurnTick != null))
            {
                if (RemoteMoveGroup.Target.AnyMoveLeft)
                {
                    if ((_budget.TopActivity?.Action is StartRemoteMoveAction _start)
                        && (_start.RemoteMoveGroup == RemoteMoveGroup))
                    {
                        // allow continue if currently moving this
                        yield return new RemoteMoveAction(RemoteMoveGroup);
                    }
                    else if (_budget.CanPerformBrief)
                    {
                        // allow start if not currently moving this and can still perform brief
                        yield return new StartRemoteMoveAction(RemoteMoveGroup);
                    }
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return new RemoteMoveInfo
            {
                ControlAdjunct = new AdjunctInfo
                {
                    ID = ID,
                    Message = RemoteMoveGroup.PowerActionSource.DisplayName
                },
                RemoteMove = new MovementInfo
                {
                    Message = RemoteMoveGroup.Movement.Name,
                    ID = RemoteMoveGroup.Movement.ID,
                    CanShiftPosition = RemoteMoveGroup.Movement.CanShiftPosition,
                    Description = RemoteMoveGroup.Movement.Description,
                    Value = RemoteMoveGroup.Movement.EffectiveValue,
                    IsUsable = RemoteMoveGroup.Movement.IsUsable
                },
                RemoteMoveTarget = GetInfoData.GetInfoFeedback(RemoteMoveGroup.Target.Anchor as ICoreObject, budget.Actor)
            };
        }
    }
}
