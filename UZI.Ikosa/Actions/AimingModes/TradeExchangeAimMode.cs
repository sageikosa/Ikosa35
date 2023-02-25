using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class TradeExchangeAimMode : AimingMode
    {
        public TradeExchangeAimMode(string key, string displayName, Creature other)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
            _Other = other;
        }

        #region state
        private Creature _Other;
        #endregion

        public Creature Other => _Other;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            var _exchange = SelectedTargets<TradeExchangeTargetInfo>(actor, action, infos).FirstOrDefault();
            if ((actor is Creature _creature)
                && (_exchange != null))
            {
                var _loc = actor.GetLocated()?.Locator;
                if (_loc != null)
                {
                    // validate infos relate to items
                    var _items = (from _info in _exchange.PrincipalOffer.Items
                                  let _itm = _creature.Possessions[_info.ID] as IItemBase
                                  where _itm?.IsLocal(_loc) ?? false
                                  select _info).ToList();

                    // group offering into coin types (just in case they have duplicates)
                    var _gCoins = (from _cOffer in _exchange.PrincipalOffer.Coins
                                   where _cOffer.Count > 0
                                   group _cOffer by _cOffer.CoinType.Name
                                   into _group
                                   select new CoinTradeInfo
                                   {
                                       CoinType = _group.FirstOrDefault()?.CoinType,
                                       Count = _group.Sum(_g => _g.Count)
                                   }).ToList();

                    // correct coins for available sets
                    var _cCoins = (from _gc in _gCoins
                                   from _set in _creature.IkosaPosessions.GetLocallyAccessible(_loc).OfType<CoinSet>()
                                   from _sub in _set.Coins                                      // at all subsets
                                   where _set.IsCounted
                                   where string.Equals(_gc.CoinType.Name, _sub.CoinType.Name)   // for this coin type
                                   group _sub.Count by _gc                                      // and group them
                                   into _actual
                                   select new CoinTradeInfo                                     // return actual is less than target
                                   {
                                       CoinType = _actual.Key.CoinType,
                                       Count = Math.Min(_actual.Sum(), _actual.Key.Count)
                                   }).ToList();

                    yield return new TradeExchangeTarget(
                        _exchange.Key,
                        new TradeOfferInfo
                        {
                            Coins = _cCoins,
                            Items = _items
                        });
                }
            }
            yield break;
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _trade = TradeParticipant.GetTradeParticipant(actor as Creature, Other);
            if (_trade != null)
            {
                var _info = ToInfo<TradeExchangeAimInfo>(action, actor);
                _info.Offer = _trade.TradeOffer.ToInfo();
                _info.IsAccepted = _trade.IsAccepted;
                _info.Partner = GetInfoData.GetInfoFeedback(_trade.Partner.Creature, actor) as CoreInfo;
                _info.PartnerOffer = _trade.Partner.TradeOffer.ToInfo();
                _info.PartnerAccepted = _trade.Partner.IsAccepted;
                return _info;
            }
            else
            {
                var _info = ToInfo<TradeExchangeAimInfo>(action, actor);
                _info.Offer = new TradeOfferInfo();
                _info.IsAccepted = false;
                _info.Partner = GetInfoData.GetInfoFeedback(Other, actor) as CoreInfo;
                _info.PartnerOffer = new TradeOfferInfo();
                _info.PartnerAccepted = false;
                return _info;
            }
        }
    }
}
