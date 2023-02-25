using System.Collections.Generic;
using Uzi.Core;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    public class GetPointsFeedback : ValueFeedback<IList<Point3D>>
    {
        public GetPointsFeedback(object source, IList<Point3D> value, bool downWard, AnchorFace downFace)
            : base(source, value)
        {
            DownWard = downWard;
            DownFace = downFace;
        }

        /// <summary>Indicates that downward attack was made</summary>
        public bool DownWard { get; set; }

        /// <summary>Indicates which face is considered downward</summary>
        public AnchorFace DownFace { get; set; }
    }
}
