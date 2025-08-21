using System;

namespace Uzi.Visualize
{
    /// <summary>
    /// Upright = 0 | OnSideTopOut = 2 | Inverted = 4 | OnSideBottomOut = 6
    /// </summary>
    [Serializable]
    public enum Verticality
    {
        Upright = 0,
        OnSideTopOut = 2,
        Inverted = 4,
        OnSideBottomOut = 6
    }
}
