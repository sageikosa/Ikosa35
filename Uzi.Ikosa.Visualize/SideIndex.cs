namespace Uzi.Visualize
{
    /// <summary>
    /// Top = 0 | Bottom = 1 | Front = 2 | Back = 3 | Left = 4 | Right = 5
    /// </summary>
    public enum SideIndex : byte
    {
        Top,
        Bottom,
        /// <summary>Also used as inside of panels</summary>
        Front,
        /// <summary>Also used as outside of panels</summary>
        Back,
        Left,
        Right
    }
}
