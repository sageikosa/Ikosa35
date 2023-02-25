using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class TimelineActionBudgetInfo : CoreActionBudgetInfo
    {
        #region ctor()
        public TimelineActionBudgetInfo()
            : base()
        {
        }
        #endregion

        // TODO: scheduled activity

        [DataMember]
        public double CurrentTime { get; set; }

        public CreatureLoginInfo CreatureLoginInfo { get; set; }
    }
}
