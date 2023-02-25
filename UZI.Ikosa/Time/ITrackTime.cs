namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// Indicates that the object may alter its states as time passes
    /// </summary>
    public interface ITrackTime
    {
        void TrackTime(double timeVal, TimeValTransition direction);
        /// <summary>Minimum time length that this adjunct tracks</summary>
        double Resolution { get; }
    }

    public enum TimeValTransition
    {
        Entering,
        Leaving
    }
}
