using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Placed on an attack if concealment came into play</summary>
    [Serializable]
    public class ConcealmentAlteration : InteractionAlteration
    {
        /// <summary>Placed on an attack if concealment came into play</summary>
        public ConcealmentAlteration(InteractData interact, object source)
            : base(interact, source)
        {
        }

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
