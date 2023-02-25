using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Flag alteration that negates criticals</summary>
    [Serializable]
    public class IgnoreCriticalAlteration : InteractionAlteration
    {
        /// <summary>Flag alteration that negates criticals</summary>
        public IgnoreCriticalAlteration(InteractData interact, object source)
            : base(interact, source)
        {
        }

        public override IEnumerable<Info> Information
        {
            get
            {
                yield return new Info { Message = @"Ignores Criticals" };
                yield break;
            }
        }
    }
}
