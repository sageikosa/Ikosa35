using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Time
{
    [Serializable]
    public class TradeExchange : AdjunctGroup, IActionSource
    {
        public TradeExchange()
            : base(typeof(TradeExchange))
        {
        }

        #region state
        #endregion

        public IEnumerable<TradeParticipant> TradeParticipants
            => Members.OfType<TradeParticipant>();

        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        public double TimeNeeded => TradeParticipants.Sum(_tp => (_tp.TradeOffer?.TimeNeeded ?? 0d));

        public override void ValidateGroup()
        {
            // must be on the same plane
            this.ValidateParticipantsPlanarGroup();
            if (Members.Count() > 2)
            {
                // no more than two members in an exchange
                // keeping the operation simple
                EjectMembers();
            }
        }
    }
}
