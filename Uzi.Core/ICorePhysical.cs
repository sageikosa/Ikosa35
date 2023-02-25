namespace Uzi.Core
{
    public interface ICorePhysical : ICore, IControlChange<Physical>
    {
        double Height { get; set; }
        double Length { get; set; }
        double Weight { get; set; }
        double Width { get; set; }
    }
}
