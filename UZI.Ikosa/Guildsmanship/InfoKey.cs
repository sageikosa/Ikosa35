using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class InfoKey : ModuleElement, ICorePart
    {
        public InfoKey(Description description)
            : base(description)
        {
        }

        public string Name => Description.Message;

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName => typeof(InfoKey).FullName;
    }
}
