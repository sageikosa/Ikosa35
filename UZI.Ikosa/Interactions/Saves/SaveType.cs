using System;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public enum SaveType
    {
        /// <summary>No save allowed</summary>
        None,
        /// <summary>No save allowed, but all damage is aggregated as a single interaction</summary>
        NoSave,
        Fortitude,
        Reflex,
        Will
    }
}
