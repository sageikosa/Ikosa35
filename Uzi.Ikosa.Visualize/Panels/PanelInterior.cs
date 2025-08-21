namespace Uzi.Visualize
{
    /// <summary>
    /// None | Slope | Diagonal | Bend
    /// </summary>
    public enum PanelInterior
    {
        /// <summary>No interior parts</summary>
        None,
        /// <summary>Sloped panel</summary>
        Slope,
        /// <summary>Diagonal panel(s)...one or two slopes</summary>
        Diagonal,
        /// <summary>Diagonal bend...two triangular and one square panel</summary>
        Bend
    }
}
