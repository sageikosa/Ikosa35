using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Action provider can supply Timeline compatible actions.
    /// </summary>
    public interface ITimelineActions : IActionProvider
    {
        void EnteringTimeline();
        void LeavingTimeline();
    }
}
