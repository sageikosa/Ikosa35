using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class StartRemoteMoveAction : RemoteMoveAction
    {
        public StartRemoteMoveAction(RemoteMoveGroup remoteMoveGroup)
           : base(remoteMoveGroup, new ActionTime(Contracts.TimeType.Brief), @"201")
        {
        }

        public override string Key => @"RemoteMove.Start";
    }
}
