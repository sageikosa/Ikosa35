using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public struct ActorLocalState
    {
        public Guid ID { get; set; }
        public Guid? MovementInfoID { get; set; }
        public MoveStart MoveStart { get; set; }
        public bool UseDouble { get; set; }
        public ViewPointType ViewPointType { get; set; }
        public uint GameBoardState { get; set; }
        public uint ThirdPersonState { get; set; }
    }
}
