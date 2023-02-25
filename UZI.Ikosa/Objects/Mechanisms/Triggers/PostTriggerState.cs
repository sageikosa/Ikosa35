using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public enum PostTriggerState : byte
    {
        /// <summary>Mechanism will be destroyed after activation</summary>
        Destroy = 0,
        /// <summary>Mechanism damaged after activation (repair to reset)</summary>
        Damage,
        /// <summary>Attach DisableObject adjunct</summary>
        Disable,
        /// <summary>Mechanism deactivated after use (manual or automatic reset needed to re-enable)</summary>
        DeActivate,
        /// <summary>Mechanism continues to be usable after activation</summary>
        AutoReset
    }
}
