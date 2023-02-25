using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    public class NamedKeysPart : ICorePart
    {
        private Module _Module;

        public NamedKeysPart(Module module)
        {
            _Module = module;
        }

        public Module Module => _Module;

        public string Name => @"Named Keys";

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName => typeof(NamedKeysPart).FullName;
    }
}
