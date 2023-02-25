using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class RemoteMoveGroup : AdjunctGroup, IActionSource
    {
        public RemoteMoveGroup(IPowerActionSource source, MovementBase movement)
            : base(source)
        {
            _Movement = movement;
        }

        #region state
        private MovementBase _Movement;
        #endregion

        public IPowerActionSource PowerActionSource => Source as IPowerActionSource;
        public RemoteMoveMaster Master => Members.OfType<RemoteMoveMaster>().FirstOrDefault();
        public RemoteMoveTarget Target => Members.OfType<RemoteMoveTarget>().FirstOrDefault();
        public MovementBase Movement => _Movement;

        public IVolatileValue ActionClassLevel => PowerActionSource.ActionClassLevel;

        public override void ValidateGroup()
            => this.ValidateOneToOnePlanarGroup();
    }
}
