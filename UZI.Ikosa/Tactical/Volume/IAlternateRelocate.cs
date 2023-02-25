using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public interface IAlternateRelocate
    {
        (Cubic Cube, Vector3D Offset) GetRelocation(IGeometricRegion region, Locator locator);
    }
}
