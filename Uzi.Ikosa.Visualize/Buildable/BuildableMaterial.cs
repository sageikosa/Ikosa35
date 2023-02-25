using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public struct BuildableMaterial
    {
        public Material Material { get; set; }
        public bool IsAlpha { get; set; }
    }
}
