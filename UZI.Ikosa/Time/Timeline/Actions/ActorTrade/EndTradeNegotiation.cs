using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class EndTradeNegotiation : SimpleActionBase
    {
        public EndTradeNegotiation(TradeExchange exchange, string orderKey)
            : base(exchange, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"Trade.End";
        public override string DisplayName(CoreActor actor) => @"End trade negotiation";
        public TradeExchange TradeExchange => ActionSource as TradeExchange;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Ending trade", activity.Actor, observer);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
            => activity.GetActivityResultNotifyStep(@"Trade negotiation ended");

        public override bool DoStep(CoreStep actualStep)
        {
            TradeExchange.EjectMembers();
            return true;
        }
    }
}
