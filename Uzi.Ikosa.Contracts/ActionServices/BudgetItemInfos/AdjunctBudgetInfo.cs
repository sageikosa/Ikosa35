using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class AdjunctBudgetInfo : BudgetItemInfo
    {
        [DataMember]
        public AdjunctInfo Adjunct { get; set; }
    }
}
