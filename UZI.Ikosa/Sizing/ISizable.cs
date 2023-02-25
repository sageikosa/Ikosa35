using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa
{
    public interface ISizable : ICore
    {
        Sizer Sizer { get; }
        IGeometricSize GeometricSize { get; }
    }
}
