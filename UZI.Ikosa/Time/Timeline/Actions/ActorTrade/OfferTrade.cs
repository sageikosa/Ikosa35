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
    public class OfferTrade : SimpleActionBase
    {
        /// <summary>Exchange</summary>
        public OfferTrade(IActionSource source, Creature other, string orderKey)
            : base(source, new ActionTime(TimeType.Free), false, false, orderKey)
        {
            _Other = other;
        }

        #region data
        private readonly Creature _Other;
        #endregion

        public Creature Other => _Other;

        public override string Key => @"Trade.Offer";
        public override string DisplayName(CoreActor actor) => @"Offer trade";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new TradeExchangeAimMode(@"Trade.Build", @"Define Trade Offer", Other);
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Offering trade", activity.Actor, observer, Other);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
            => activity.GetActivityResultNotifyStep($@"trade offered");

        public override bool DoStep(CoreStep actualStep)
        {
            if (actualStep.Process is CoreActivity _activity)
            {
                var _target = _activity.GetFirstTarget<TradeExchangeTarget>(@"Trade.Build");
                if (_target != null)
                {
                    // look for a trade exchange
                    var _trade = TradeParticipant.GetTradeParticipant(_activity.Actor, Other);
                    if (_trade == null)
                    {
                        // setup a new trade exchange between this and other
                        var _exchange = new TradeExchange();
                        _trade = new TradeParticipant(_exchange);
                        _activity.Actor.AddAdjunct(_trade);
                        Other.AddAdjunct(new TradeParticipant(_exchange));
                    }

                    // put trade offer together from target information (consider other pending trades)

                    // TODO: validate
                    // TODO: notifty both parties of changes...
                }
            }
            return true;
        }
    }
}
