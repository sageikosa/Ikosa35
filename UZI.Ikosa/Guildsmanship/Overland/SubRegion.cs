using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class SubRegion : ModuleLink<Region>
    {
        private Point3D _Location;
        private Vector3D _Size;

        public SubRegion(Description description)
            : base(description)
        {
        }

        public Point3D Location { get => _Location; set => _Location = value; }
        public Vector3D Size { get => _Size; set => _Size = value; }
    }
}
