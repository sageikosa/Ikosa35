namespace Uzi.Visualize
{
    public interface IWedgeSpace : IPlusCellSpace
    {
        double Offset1 { get; }
        double Offset2 { get; }
        bool CornerStyle { get; }
        bool HasTiling { get; }
        bool HasPlusTiling { get; }
    }
}
