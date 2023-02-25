using System;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public enum AwarenessLevel
    {
        /// <summary>Line of sight, but too dark to see, must still present the target</summary>
        DarkDraw = -1,
        /// <summary>Not even possible to detect (no line of sight)</summary>
        None,
        /// <summary>Nearby, but not sure where (30' invisible presence)</summary>
        Presence,
        /// <summary>Could detect, but haven't (most likely made a hide check)</summary>
        UnAware,
        /// <summary>You know where it is</summary>
        Aware
    }
}