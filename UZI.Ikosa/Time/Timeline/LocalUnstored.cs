using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Indicates that the item hasn't been stored, but is considered local.  
    /// When the condition that allows this ends, all local unstored objects should be dropped where the possessor is located,
    /// </summary>
    [Serializable]
    public class LocalUnstored : Adjunct
    {
        /// <summary>
        /// Indicates that the item hasn't been stored, but is considered local.  
        /// When the condition that allows this ends, all local unstored objects should be dropped where the possessor is located,
        /// </summary>
        public LocalUnstored(object source)
            : base(source)
        {
        }

        public override object Clone()
            => new LocalUnstored(Source);
    }
}
