using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Visualize
{
    [Serializable]
    [Flags]
    public enum BoundarySide
    {
        Same = 1,
        Other = 2,
        Stradle = 3
    }
}
