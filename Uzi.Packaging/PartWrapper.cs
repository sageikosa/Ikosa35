using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Packaging
{
    public class PartWrapper<Wrapped> : ICorePart
    {
        public PartWrapper(Wrapped wrapped, string name)
        {
            _Name = name;
            _Wrapped = wrapped;
        }

        #region private data
        private string _Name;
        private Wrapped _Wrapped;
        #endregion

        public Wrapped WrappedPart { get { return _Wrapped; } }

        #region ICorePart Members

        public string Name { get { return _Name; } }
        public IEnumerable<ICorePart> Relationships { get { yield break; } }
        public string TypeName { get { return typeof(Wrapped).FullName; } }

        #endregion
    }
}
