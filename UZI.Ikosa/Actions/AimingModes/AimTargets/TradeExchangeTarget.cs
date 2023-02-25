using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class TradeExchangeTarget : AimTarget
    {
        public TradeExchangeTarget(string key, TradeOfferInfo offer)
            : base(key, null)
        {
            _Offer = offer;
        }

        #region state
        private TradeOfferInfo _Offer;
        #endregion

        public TradeOfferInfo PrincipalOffer => _Offer;

        // TODO: convert into actual targets...
    }
}
