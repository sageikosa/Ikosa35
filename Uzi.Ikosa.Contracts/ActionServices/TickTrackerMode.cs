using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public enum TickTrackerMode
    {
        /// <summary>Currently in a time tick</summary>
        [EnumMember]
        TimeTick = 1,
        /// <summary>Currently in an intiative tracker turn tick</summary>
        [EnumMember]
        TurnTick = 2,
        /// <summary>Currently in an initiative tracker round marker tick</summary>
        [EnumMember]
        RoundMarker = 3,
        /// <summary>Currently in an initiative tracker, waiting for creatures that need turn ticks to be added</summary>
        [EnumMember]
        NeedsTurnTick = 4,
        /// <summary>Detected that turn tracking could start due to hostile awarenesses</summary>
        [EnumMember]
        PromptTurnTracker = 5,
        /// <summary>Rolling for initiative to start turn tracker</summary>
        [EnumMember]
        InitiativeStartup = 6,

        /// <summary>Timeline compatible providers detected on budgets with action potential</summary>
        [EnumMember]
        TimelinePending = 7,
        /// <summary>All budgets will allow timeline to flow, system is paused for master</summary>
        [EnumMember]
        TimelineReady = 8,
        /// <summary>All budgets are allowing timeline to flow</summary>
        [EnumMember]
        TimelineFlowing = 9,

        /// <summary>Time has been borrowed from timeline and can be spent.</summary>
        [EnumMember]
        CapitalSpending = 10
    }
}
