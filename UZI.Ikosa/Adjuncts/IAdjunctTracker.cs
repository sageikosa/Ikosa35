namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Defines association with adjuncts on activation and anchor
    /// </summary>
    public interface IAdjunctTracker
    {
        object ActiveAdjunctObject { get; }
        object AnchoredAdjunctObject { get; }
    }
}
