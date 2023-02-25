namespace Uzi.Ikosa.Tactical
{
    /// <summary>None, Soft, Hard, Improved</summary>
    public enum CoverLevel
    {
        /// <summary>No cover available</summary>
        None,
        /// <summary>Soft-cover (no reflex bonus or hide checks); only good for ranged</summary>
        Soft,
        /// <summary>Hard-cover +4 Armor Rating (provides +2 reflex bonus and allows hide checks); no reflex bonus versus spread</summary>
        Hard,
        /// <summary>Improved cover +8 Armor Rating (provides +4 reflex bonus, improved evasion and +10 hide checks)</summary>
        Improved
    }

    /// <summary>None, Partial, Total</summary>
    public enum CoverConcealmentResult
    {
        /// <summary>No cover nor concealment</summary>
        None,
        /// <summary>Has cover or concealment</summary>
        Partial,
        /// <summary>Blocked or total concealment</summary>
        Total
    }
}
