using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class LocalTurnTrackerInfo
    {
        [DataMember]
        public bool IsInitiative { get; set; }

        [DataMember]
        public bool IsAutoTimeTick { get; set; }

        [DataMember]
        public double CurrentTime { get; set; }

        [DataMember]
        public List<LocalActionBudgetInfo> ReactableBudgets { get; set; }

        [DataMember]
        public List<LocalActionBudgetInfo> DelayedBudgets { get; set; }

        [DataMember]
        public List<TickInfo> UpcomingTicks { get; set; }

        [DataMember]
        public TickInfo LeadingTick { get; set; }

        [DataMember]
        public TickTrackerMode TickTrackerMode { get; set; }

        public bool IsEndTurnAvailable => IsInitiative && UpcomingTicks.Any();

        public IDictionary<Guid, LocalActionBudgetInfo> AllBudgets
            => UpcomingTicks
            .Where(_t => _t.Budgets != null)
            .SelectMany(_ut => _ut.Budgets)
            .Distinct()
            .ToDictionary(_b => _b.ActorID);

        public IList<LocalActionBudgetInfo> UpcomingBudgets
            => UpcomingTicks
            .Where(_t => _t.Budgets != null)
            .SelectMany(_ut => _ut.Budgets)
            .Distinct()
            .ToList();
    }
}
