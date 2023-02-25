using System;
using System.Collections.Generic;
using System.Text;

namespace Ikosa.Packaging
{
    public class PartWrapper<Wrapped> : IRetrievablePart
    {
        public PartWrapper(Wrapped wrapped, string name)
        {
            _Name = name;
            _Wrapped = wrapped;
        }

        #region state
        private string _Name;
        private Wrapped _Wrapped;
        #endregion

        public Wrapped WrappedPart { get { return _Wrapped; } }
        public string PartName => _Name;
        public IEnumerable<IRetrievablePart> Parts  { get { yield break; } }
        public string PartType => typeof(Wrapped).FullName;
    }
}
