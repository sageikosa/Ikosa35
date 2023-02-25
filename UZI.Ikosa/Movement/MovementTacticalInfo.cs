using Uzi.Visualize;

namespace Uzi.Ikosa.Movement
{
    /// <summary>
    /// Not-Serializable
    /// </summary>
    public struct MovementTacticalInfo
    {
        public MovementBase Movement { get; set; }
        public AnchorFace[] TransitFaces { get; set; }
        public ICellLocation SourceCell { get; set; }
        public ICellLocation TargetCell { get; set; }
    }
}
