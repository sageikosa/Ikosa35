using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public enum VariableValueEnablerValidation
    {
        /// <summary>Any value enabler can make the value enabled</summary>
        Any,
        /// <summary>All value enabler must allow the value to be enabled for it to be enabled</summary>
        All
    }
}
