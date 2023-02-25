using System.Collections.Generic;

namespace Uzi.Visualize.Contracts.Tactical
{
    public interface IIconReference
    {
        IDictionary<string, string> IconColorMap { get; }
        double IconAngle { get; set; }
        double IconScale { get; set; }
    }
}