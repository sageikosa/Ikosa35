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
    public class RejectTradeOffer : SimpleActionBase
    {
        public RejectTradeOffer(TradeExchange exchange, string orderKey)
            : base(exchange, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"Trade.Reject";
        public override string DisplayName(CoreActor observer) => @"Reject trade";
        public TradeExchange TradeExchange => ActionSource as TradeExchange;

        public override bool DoStep(CoreStep actualStep)
            => true;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Rejecting trade", activity.Actor, observer);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
        {
            // notify actor and participant
            var _notify = activity.GetActivityResultNotifyStep(@"Rejected trade");
            _notify.InfoReceivers = TradeExchange.TradeParticipants.Select(_tp => _tp.Creature.ID).ToArray();
            return _notify;
        }
    }
}
