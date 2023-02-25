using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    /// <summary>Low, None, High</summary>
    [Serializable]
    public enum AxisSnap
    {
        Low, None, High
    }

    public static class AxisSnapHelper
    {
        /// <summary>Converts the three AxisSnap values to a list of AnchorFaces</summary>
        public static List<AnchorFace> ToFaceList(AxisSnap zSnap, AxisSnap ySnap, AxisSnap xSnap)
        {
            var _list = new List<AnchorFace>();
            if (zSnap != AxisSnap.None)
                _list.Add(zSnap == AxisSnap.High ? AnchorFace.ZHigh : AnchorFace.ZLow);
            if (ySnap != AxisSnap.None)
                _list.Add(ySnap == AxisSnap.High ? AnchorFace.YHigh : AnchorFace.YLow);
            if (xSnap != AxisSnap.None)
                _list.Add(xSnap == AxisSnap.High ? AnchorFace.XHigh : AnchorFace.XLow);
            return _list;
        }

        public static AnchorFaceList ToAnchorFaceList(AxisSnap zSnap, AxisSnap ySnap, AxisSnap xSnap)
        {
            var _faces = AnchorFaceList.None;
            if (zSnap != AxisSnap.None)
                _faces = _faces.Add(zSnap == AxisSnap.High ? AnchorFace.ZHigh : AnchorFace.ZLow);
            if (ySnap != AxisSnap.None)
                _faces = _faces.Add(ySnap == AxisSnap.High ? AnchorFace.YHigh : AnchorFace.YLow);
            if (xSnap != AxisSnap.None)
                _faces = _faces.Add(xSnap == AxisSnap.High ? AnchorFace.XHigh : AnchorFace.XLow);
            return _faces;
        }
    }
}
