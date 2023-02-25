using Uzi.Core.Contracts;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Created by an observe handler to transit a sense line from the source-observer to the target-handler.
    /// Never actually used as an handeable interaction (just a transit interaction).
    /// </summary>
    public class SenseTransit : InteractData
    {
        public SenseTransit(SensoryBase sense)
            : base(sense.Creature)
        {
            _Sense = sense;
        }

        private SensoryBase _Sense;
        /// <summary>Source cast as sensoryBase</summary>
        public SensoryBase Sense { get { return _Sense; } }

        /// <summary>Indicates whether the sense transit was altered with concealment</summary>
        public bool IsConcealed { get { return this.Alterations.OfType<SenseConcealmentAlteration>().Any(); } }
    }

    /// <summary>
    /// Used to track concealment sources.  Some concealment effects are cumulative until blocked...
    /// </summary>
    public class SenseConcealmentAlteration : InteractionAlteration
    {
        public SenseConcealmentAlteration(InteractData interact, object source)
            : base(interact, source)
        {
            Count = 1;
        }

        /// <summary>Number of cubes through which this concealment has interacted (if applicable)</summary>
        public int Count;

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = @"Concealment" };
                yield break;
            }
        }
    }
}
