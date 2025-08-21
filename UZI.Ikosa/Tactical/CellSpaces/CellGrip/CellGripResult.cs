using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public struct CellGripResult
    {
        public int? Difficulty { get; set; }
        public AnchorFaceList Faces { get; set; }
        public AnchorFaceList InnerFaces { get; set; }

        public AnchorFace GetBaseFace(AnchorFace gravity, AnchorFace baseFace)
        {
            // find gravity?
            if (Faces.Contains(gravity))
            {
                return gravity;
            }

            // find last base?
            if (Faces.Contains(baseFace))
            {
                return baseFace;
            }

            // ignore gravity axis
            var _gAxis = gravity.GetAxis();
            var _faces = Faces.StripAxis(_gAxis);

            // nothing left?
            if (_faces == AnchorFaceList.None)
            {
                return gravity.ReverseFace();
            }

            // first inner face (without gravity)
            var _inner = InnerFaces.StripAxis(_gAxis);
            if (_inner != AnchorFaceList.None)
            {
                return _inner.ToAnchorFaces().First();
            }

            // first other face (without gravity)
            return _faces.ToAnchorFaces().First();
        }
    }
}
