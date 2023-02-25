using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class TradeOffer
    {
        public TradeOffer(IEnumerable<CoreInfo> items, IEnumerable<CoinTradeInfo> coins)
        {
            _Items = items.ToList();
            _Coins = coins.ToList();
        }

        #region state
        private List<CoreInfo> _Items;
        private List<CoinTradeInfo> _Coins;
        #endregion

        public IEnumerable<CoreInfo> Items => _Items;
        public IEnumerable<CoinTradeInfo> Coins => _Coins;

        public double TimeNeeded => (Items.Count() + (Coins.Sum(_c => _c.Count) / 50) + 1) * Round.UnitFactor;

        public TradeOfferInfo ToInfo()
            => new TradeOfferInfo
            {
                Items = _Items,
                Coins = _Coins
            };
    }
}
