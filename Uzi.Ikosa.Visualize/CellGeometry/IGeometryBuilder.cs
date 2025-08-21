namespace Uzi.Visualize
{
    /// <summary>
    /// Allows a geometry to be built when a location is supplied.
    /// </summary>
    public interface IGeometryBuilder
    {
        IGeometricRegion BuildGeometry(LocationAimMode aimMode, ICellLocation location);
    }
}
