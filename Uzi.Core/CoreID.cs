using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class CoreID : ICore
    {
        public CoreID(Guid id)
        {
            ID = id;
        }

        public Guid ID { get; }
    }
}
