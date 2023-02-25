using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Time
{
    /// <summary>Used to accept or reject a trade offer.</summary>
    [Serializable]
    public class AcceptTradeOffer : SimpleActionBase
    {
        public AcceptTradeOffer(TradeExchange exchange, string orderKey)
            : base(exchange, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"Trade.Accept";
        public override string DisplayName(CoreActor actor) => @"Accept trade";
        public TradeExchange TradeExchange => ActionSource as TradeExchange;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Accepting trade", activity.Actor, observer);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
            => activity.GetActivityResultNotifyStep(@"Accepted trade");

        protected List<IItemBase> GetItems(TradeParticipant participant, Locator locator)
        {
            var _items = (from _info in participant.TradeOffer.Items
                          let _item = participant.Creature.Possessions[_info.ID] as IItemBase
                          where (_item?.IsLocal(locator) ?? false) && !(_item is CoinSet)
                          select _item).ToList();
            return _items;
        }

        protected CoinSet GetCoins(TradeParticipant participant, Locator locator)
        {
            var _gather = new CoinSet();
            foreach (var _cOffer in participant.TradeOffer.Coins)
            {
                participant.Creature.IkosaPosessions.GatherCoins(_gather, _cOffer, locator);
                _gather.IsCounted = true;
            }
            return _gather;
        }

        public override bool DoStep(CoreStep actualStep)
        {
            if (TradeExchange.TradeParticipants.All(_tp => _tp.IsAccepted))
            {
                var _principal = TradeExchange.TradeParticipants.FirstOrDefault(_tp => _tp.Creature == Budget.Creature);
                var _locA = _principal.Creature.GetLocated()?.Locator;
                var _itemsA = GetItems(_principal, _locA);
                var _coinsA = GetCoins(_principal, _locA);

                var _other = TradeExchange.TradeParticipants.FirstOrDefault(_tp => _tp != _principal);
                var _locB = _other.Creature.GetLocated()?.Locator;
                var _itemsB = GetItems(_other, _locB);
                var _coinsB = GetCoins(_other, _locB);

                // A offer goes to B
                foreach (var _item in _itemsA)
                {
                    _item.UnPath();
                    _item.Possessor = _other.Creature;
                    _item.AddAdjunct(new LocalUnstored(typeof(TradeExchange)));
                }
                if (_coinsA.Coins.Any())
                {
                    _coinsA.Possessor = _other.Creature;
                    _coinsA.AddAdjunct(new LocalUnstored(typeof(TradeExchange)));
                }

                // B offer goes to A
                foreach (var _item in _itemsB)
                {
                    _item.UnPath();
                    _item.Possessor = _principal.Creature;
                    _item.AddAdjunct(new LocalUnstored(typeof(TradeExchange)));
                }
                if (_coinsB.Coins.Any())
                {
                    _coinsB.Possessor = _principal.Creature;
                    _coinsB.AddAdjunct(new LocalUnstored(typeof(TradeExchange)));
                }

                // TODO: ensure any other pending trades for either party are "revalidated"/"invalidated"...

                foreach (var _partner in TradeExchange.TradeParticipants)
                {
                    // TODO: then consume time budgets
                }
            }
            return true;
        }
    }
}
