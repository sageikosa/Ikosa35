using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class InitStartupMode : TickTrackerModeBase
    {
        public InitStartupMode(IsMasterModel master, LocalTurnTrackerInfo tracker)
            : base(master, tracker)
        {
        }
    }
}
