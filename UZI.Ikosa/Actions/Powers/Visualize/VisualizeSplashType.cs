namespace Uzi.Ikosa.Actions
{
    /// <summary>None, Uniform, Pop, Pulse</summary>
    public enum VisualizeSplashType
    {
        /// <summary>No splash type</summary>
        None,

        /// <summary>Uniform sized marker-ball when contact is made</summary>
        Uniform,

        /// <summary>Marker-ball that grows when contact is made then vanishes</summary>
        Pop,

        /// <summary>Marker-ball that shrinks when contact is made then vanishes</summary>
        Drain,

        /// <summary>Marker-ball that grows when contact is made, then shrinks after contact ends</summary>
        Pulse
    }
}
