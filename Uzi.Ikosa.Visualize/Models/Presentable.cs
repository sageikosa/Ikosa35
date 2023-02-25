using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class Presentable
    {
        public object Presenter { get; set; }
        public IEnumerable<IPresentation> Presentations { get; set; }
        public Model3D Model3D { get; set; }
        public Vector3D MoveFrom { get; set; }
        public ulong SerialState { get; set; }
    }
}
