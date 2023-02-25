using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship
{
    public enum ModuleUse
    {
        /// <summary>ModulePart is NULL, Resources are set and unsaved</summary>
        New,
        /// <summary>ModulePart is set, Resources are NULL</summary>
        Referenced,
        /// <summary>ModulePart and Resources are both set</summary>
        Open
    }
}
