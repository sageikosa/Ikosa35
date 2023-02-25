using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Wealth;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa
{
    [Serializable]
    public class IkosaPossessions : PossessionSet, ICreatureBound
    {
        public IkosaPossessions(Creature owner)
            : base(owner)
        {
        }

        protected override void OnAdd(CoreItem item)
        {
            base.OnAdd(item);
            DoPropertyChanged(@"SellPrice");
            DoPropertyChanged(@"BasePrice");
        }

        protected override void OnRemove(CoreItem item)
        {
            base.OnRemove(item);
            DoPropertyChanged(@"SellPrice");
            DoPropertyChanged(@"BasePrice");
        }

        /// <summary>Signals a change in price</summary>
        internal void DoChangePrice()
        {
            DoPropertyChanged(@"SellPrice");
            DoPropertyChanged(@"BasePrice");
        }

        public decimal SellPrice => All.OfType<ItemBase>().Sum(_i => _i.Price.SellPrice);
        public decimal BasePrice => All.OfType<ItemBase>().Sum(_i => _i.Price.BasePrice);

        public Creature Creature => Owner as Creature;

        public void GatherCoins(CoinSet gather, CoinTradeInfo coinTrade, Locator locator)
        {
            var _lookingFor = coinTrade.Count;

            // step through all coinsets
            foreach (var _cSet in (locator != null
                                    ? GetLocallyAccessible(locator).OfType<CoinSet>()
                                    : All.OfType<CoinSet>())
                .ToList())
            {
                // get coins from this set that match the type
                var _count = _cSet.GetCoins(coinTrade.CoinType.Name, coinTrade.Count);
                if (_count.Count > 0)
                {
                    // since something was found in this set, don't look for that amount
                    _lookingFor -= _count.Count;

                    // and put the count into the gathering set
                    gather.AddCoins(_count);
                }

                // if not still looking for coins, done
                if (_lookingFor <= 0)
                    break;
            }
        }

        public void EjectLocalUnstored(bool doDrop)
        {
            var _loc = Creature.GetLocated();
            if (_loc != null)
            {
                foreach (var _item in AllLocalUnstored().ToList())
                {
                    _item.Adjuncts.OfType<LocalUnstored>().FirstOrDefault()?.Eject();
                    if (doDrop)
                    {
                        Drop.DoDropEject(Creature, _item);
                    }
                }
            }
        }

        public IEnumerable<CoreItem> AllLocalUnstored()
            => All.Where(_i => _i.HasActiveAdjunct<LocalUnstored>());

        public IEnumerable<IItemBase> GetLocallyAccessible(Locator locator)
            => All.OfType<IItemBase>().Where(_i => _i.IsLocal(locator));
    }
}
