using System;
using System.Collections.Generic;

namespace Uzi.Visualize
{
    /// <summary>Snap for grip or contact checks in a cell</summary>
    [Serializable]
    public struct CellSnap
    {
        #region ctor()
        /// <summary>Snap for grip or contact checks in a cell</summary>
        public CellSnap(AnchorFaceList faceList)
        {
            _ZSnap = faceList.GetAxisSnap(Axis.Z);
            _YSnap = faceList.GetAxisSnap(Axis.Y);
            _XSnap = faceList.GetAxisSnap(Axis.X);
        }

        /// <summary>Snap for grip or contact checks in a cell</summary>
        public CellSnap(AxisSnap zSnap, AxisSnap ySnap, AxisSnap xSnap)
        {
            _ZSnap = zSnap;
            _YSnap = ySnap;
            _XSnap = xSnap;
        }
        #endregion

        public static CellSnap UnSnapped
            => new CellSnap(AxisSnap.None, AxisSnap.None, AxisSnap.None);

        #region data
        private AxisSnap _ZSnap;
        private AxisSnap _YSnap;
        private AxisSnap _XSnap;
        #endregion

        public AxisSnap ZSnap { get { return _ZSnap; } set { _ZSnap = value; } }
        public AxisSnap YSnap { get { return _YSnap; } set { _YSnap = value; } }
        public AxisSnap XSnap { get { return _XSnap; } set { _XSnap = value; } }

        /// <summary>Converts the CellSnap to a list of AnchorFaces</summary>
        public List<AnchorFace> ToFaceList()
            => AxisSnapHelper.ToFaceList(ZSnap, YSnap, XSnap);

        /// <summary>Converts the CellSnap to an AnchorFaceList</summary>
        public AnchorFaceList ToAnchorFaceList()
            => AxisSnapHelper.ToAnchorFaceList(ZSnap, YSnap, XSnap);
    }
}
