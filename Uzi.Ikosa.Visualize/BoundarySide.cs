using System;

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
