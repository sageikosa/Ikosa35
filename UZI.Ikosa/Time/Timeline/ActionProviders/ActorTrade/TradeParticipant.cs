using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class TradeParticipant : GroupParticipantAdjunct, ITimelineActions
    {
        public TradeParticipant(TradeExchange tradeExchange)
            : base(typeof(TradeExchange), tradeExchange)
        {
            _Accepted = false;
        }

        #region state
        private bool _Accepted;
        private TradeOffer _Offer;
        #endregion

        public TradeExchange TradeExchange => Group as TradeExchange;
        public Creature Creature => Anchor as Creature;

        public TradeParticipant Partner => TradeExchange.TradeParticipants
            .FirstOrDefault(_p => _p != this);

        /// <summary>Defines new trade offering, marks IsReviewed, clears IsAccepted</summary>
        public TradeOffer TradeOffer
        {
            get => _Offer;
            set
            {
                _Offer = value;
                DoPropertyChanged(nameof(TradeOffer));
                IsAccepted = false;
            }
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Creature?.Actions.Providers.Add(this, this);
        }

        protected override void OnDeactivate(object source)
        {
            Creature?.Actions.Providers.Remove(this);
            base.OnDeactivate(source);
        }

        /// <summary>Sets IsAccepted as supplied</summary>
        public bool IsAccepted
        {
            get => _Accepted;
            set
            {
                _Accepted = value;
                DoPropertyChanged(nameof(IsAccepted));
            }
        }

        public override object Clone()
            => new TradeParticipant(TradeExchange);


        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield return new AcceptTradeOffer(TradeExchange, @"300");
            yield return new RejectTradeOffer(TradeExchange, @"301");
            yield return new OfferTrade(TradeExchange, Partner.Creature, @"310");
            yield return new EndTradeNegotiation(TradeExchange, @"311");
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new TimelineActionProviderInfo($@"Trade Exchange", ID); // TODO: more detail about to whom

        public void EnteringTimeline()
        {
        }

        public void LeavingTimeline()
        {
        }

        public static TradeParticipant GetTradeParticipant(CoreActor principal, Creature other)
            => principal.Adjuncts.OfType<TradeParticipant>()
            .FirstOrDefault(_p => _p.Partner.Creature == other);
    }
}
