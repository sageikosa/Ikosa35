namespace Uzi.Visualize
{
    public interface IGeometricSize
    {
        long ZHeight { get; }
        long YLength { get; }
        long XLength { get; }
        double ZExtent { get; }
        double YExtent { get; }
        double XExtent { get; }
        long GetAxialLength(Axis axis);
    }

    public static class GeometricSizeHelper
    {
        public static bool SameSize(this IGeometricSize self, IGeometricSize target)
            => (self.ZHeight == target.ZHeight)
            && (self.YLength == target.YLength)
            && (self.XLength == target.XLength);
    }
}
