using System;

namespace Uzi.Ikosa
{
    /// <summary>Neuter, Masculine, Feminine, Hermaphroditic</summary>
    [Serializable]
    public enum Gender
    {
        /// <summary>No gender</summary>
        Genderless,
        /// <summary>Masculine</summary>
        Male,
        /// <summary>Feminine</summary>
        Female,
        /// <summary>Both Masculine and Feminine</summary>
        Hermaphroditic
    }
}
