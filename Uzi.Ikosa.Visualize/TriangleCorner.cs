using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Visualize
{
    /// <summary>
    /// LowerLeft | UpperLeft | LowerRight | UpperRight
    /// </summary>
    [Serializable]
    public enum TriangleCorner { LowerLeft, UpperLeft, LowerRight, UpperRight };

    public static class TriangleCornerHelper
    {
        #region public static AnchorFaceList GetEdgeFaces(this TriangleCorner snap, AnchorFace panelFace)
        public static AnchorFaceList GetEdgeFaces(this TriangleCorner snap, AnchorFace panelFace)
        {
            switch (panelFace)
            {
                case AnchorFace.XLow:
                    switch (snap)
                    {
                        case TriangleCorner.LowerLeft: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.YHigh);
                        case TriangleCorner.UpperLeft: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.YHigh);
                        case TriangleCorner.LowerRight: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.YLow);
                        case TriangleCorner.UpperRight: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.YLow);
                    }
                    break;
                case AnchorFace.YLow:
                    switch (snap)
                    {
                        case TriangleCorner.LowerLeft: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.XLow);
                        case TriangleCorner.UpperLeft: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.XLow);
                        case TriangleCorner.LowerRight: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.XHigh);
                        case TriangleCorner.UpperRight: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.XHigh);
                    }
                    break;
                case AnchorFace.ZLow:
                    switch (snap)
                    {
                        case TriangleCorner.LowerLeft: return AnchorFaceListHelper.Create(AnchorFace.YLow, AnchorFace.XHigh);
                        case TriangleCorner.UpperLeft: return AnchorFaceListHelper.Create(AnchorFace.YHigh, AnchorFace.XHigh);
                        case TriangleCorner.LowerRight: return AnchorFaceListHelper.Create(AnchorFace.YLow, AnchorFace.XLow);
                        case TriangleCorner.UpperRight: return AnchorFaceListHelper.Create(AnchorFace.YHigh, AnchorFace.XLow);
                    }
                    break;
                case AnchorFace.XHigh:
                    switch (snap)
                    {
                        case TriangleCorner.LowerLeft: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.YLow);
                        case TriangleCorner.UpperLeft: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.YLow);
                        case TriangleCorner.LowerRight: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.YHigh);
                        case TriangleCorner.UpperRight: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.YHigh);
                    }
                    break;
                case AnchorFace.YHigh:
                    switch (snap)
                    {
                        case TriangleCorner.LowerLeft: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.XHigh);
                        case TriangleCorner.UpperLeft: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.XHigh);
                        case TriangleCorner.LowerRight: return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.XLow);
                        case TriangleCorner.UpperRight: return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.XLow);
                    }
                    break;
                case AnchorFace.ZHigh:
                    switch (snap)
                    {
                        case TriangleCorner.LowerLeft: return AnchorFaceListHelper.Create(AnchorFace.YHigh, AnchorFace.XLow);
                        case TriangleCorner.UpperLeft: return AnchorFaceListHelper.Create(AnchorFace.YLow, AnchorFace.XLow);
                        case TriangleCorner.LowerRight: return AnchorFaceListHelper.Create(AnchorFace.YHigh, AnchorFace.XHigh);
                        case TriangleCorner.UpperRight: return AnchorFaceListHelper.Create(AnchorFace.YLow, AnchorFace.XHigh);
                    }
                    break;
            }
            return AnchorFaceList.None;
        }
        #endregion

        /// <summary>Returns FaceEdge.Top or FaceEdge.Bottom</summary>
        public static FaceEdge VerticalEdge(this TriangleCorner self)
        {
            if ((self == TriangleCorner.UpperLeft) || (self == TriangleCorner.UpperRight))
                return FaceEdge.Top;
            return FaceEdge.Bottom;
        }

        /// <summary>Returns FaceEdge.Left or FaceEdge.Right</summary>
        public static FaceEdge HorizontalEdge(this TriangleCorner self)
        {
            if ((self == TriangleCorner.UpperLeft) || (self == TriangleCorner.LowerLeft))
                return FaceEdge.Left;
            return FaceEdge.Right;
        }

        /// <summary>Converts 2 FaceEdges to a TriangleCorner</summary>
        public static TriangleCorner GetFromFaceEdges(params FaceEdge[] edges)
        {
            if (edges.Contains(FaceEdge.Left))
            {
                if (edges.Contains(FaceEdge.Top))
                    return TriangleCorner.UpperLeft;
                return TriangleCorner.LowerLeft;
            }
            else
            {
                if (edges.Contains(FaceEdge.Top))
                    return TriangleCorner.UpperRight;
                return TriangleCorner.LowerRight;
            }
        }

        public static TriangleCorner GetLeftRightSwap(this TriangleCorner corner)
        {
            switch (corner)
            {
                case TriangleCorner.LowerLeft: return TriangleCorner.LowerRight;
                case TriangleCorner.UpperLeft: return TriangleCorner.UpperRight;
                case TriangleCorner.LowerRight: return TriangleCorner.LowerLeft;
                case TriangleCorner.UpperRight: return TriangleCorner.UpperLeft;
                default: return corner;
            }
        }

        public static TriangleCorner GetUpDownSwap(this TriangleCorner corner)
        {
            switch (corner)
            {
                case TriangleCorner.LowerLeft: return TriangleCorner.UpperLeft;
                case TriangleCorner.UpperLeft: return TriangleCorner.LowerLeft;
                case TriangleCorner.LowerRight: return TriangleCorner.UpperRight;
                case TriangleCorner.UpperRight: return TriangleCorner.LowerRight;
                default: return corner;
            }
        }

        public static TriangleCorner GetFullSwap(this TriangleCorner corner)
        {
            switch (corner)
            {
                case TriangleCorner.LowerLeft: return TriangleCorner.UpperRight;
                case TriangleCorner.UpperLeft: return TriangleCorner.LowerRight;
                case TriangleCorner.LowerRight: return TriangleCorner.UpperLeft;
                case TriangleCorner.UpperRight: return TriangleCorner.LowerLeft;
                default: return corner;
            }
        }
    }
}
