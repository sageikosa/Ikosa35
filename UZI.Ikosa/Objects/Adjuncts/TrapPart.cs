using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Indicates the object is part of a trap (for searching, detects and saves)</summary>
    [Serializable]
    public class TrapPart : Adjunct
    {
        /// <summary>Indicates the object is part of a trap (for searching, detects and saves)</summary>
        public TrapPart(bool isProtected)
            : base(typeof(TrapPart))
        {
            _Protect = IsProtected;
        }

        #region data
        private bool _Protect;
        #endregion

        public override bool IsProtected => _Protect;

        public override object Clone()
            => new TrapPart(IsProtected);
    }
}
