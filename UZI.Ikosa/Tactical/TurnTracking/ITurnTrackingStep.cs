using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Tactical
{
    public interface ITurnTrackingStep
    {
        LocalTurnTracker Tracker { get; }
    }
}
