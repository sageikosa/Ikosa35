using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Core
{
    [Serializable]
    public class GeometryInteract : IInteract
    {
        public int Index { get; set; }
        public LocationAimMode AimMode { get; set; }
        public CellPosition Position { get; set; }
        public Point3D Point3D { get; set; }
        public AnchorFace AnchorFace { get; set; }
        public Guid ID { get; set; }

        public void HandleInteraction(Interaction interact)
        {
        }
    }
}
