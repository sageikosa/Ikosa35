using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [Serializable]
    [Flags]
    public enum AnchorFaceList : byte
    {
        None = 0,
        XLow = 1,
        XHigh = 2,
        YLow = 4,
        YHigh = 8,
        ZLow = 0x10,
        ZHigh = 0x20,
        All = 0x3F,
        LowMask = XLow | YLow | ZLow,
        HighMask = XHigh | YHigh | ZHigh,
        ZMask = ZLow | ZHigh,
        YMask = YLow | YHigh,
        XMask = XLow | XHigh,
        XYMask = XMask | YMask,
        YZMask = YMask | ZMask,
        ZXMask = ZMask | XMask
    }

    public static class AnchorFaceListHelper
    {
        /// <summary>Creates an unordered list of AnchorFaces</summary>
        public static AnchorFaceList Create(params AnchorFace[] faces)
        {
            var _list = AnchorFaceList.None;
            foreach (var _f in faces)
                _list = _list.Add(_f);
            return _list;
        }

        /// <summary>Creates an unordered list of AnchorFaces</summary>
        public static AnchorFaceList Create(IEnumerable<AnchorFace> faces)
        {
            var _list = AnchorFaceList.None;
            foreach (var _f in faces)
                _list = _list.Add(_f);
            return _list;
        }

        /// <summary>Unions two AnchorFaceLists and returns the new value</summary>
        public static AnchorFaceList Union(this AnchorFaceList self, AnchorFaceList faces)
            => self | faces;

        public static AnchorFaceList Intersection(this AnchorFaceList self, AnchorFaceList faces)
            => self & faces;

        /// <summary>Adds an AnchorFace to an AnchorFaceList and returns the new value</summary>
        public static AnchorFaceList Add(this AnchorFaceList self, AnchorFace face)
            => self.Union(face.ToAnchorFaceList());

        /// <summary>Removes an AnchorFace if present</summary>
        public static AnchorFaceList Remove(this AnchorFaceList self, AnchorFace face)
            => self.Remove(face.ToAnchorFaceList());

        public static AnchorFaceList Remove(this AnchorFaceList self, AnchorFaceList removers)
            => self ^ self.Intersection(removers);

        /// <summary>Counts unique items in the list</summary>
        public static int Count(this AnchorFaceList self)
            => AnchorFaceHelper.GetAllFaces().Count(_f => self.Contains(_f));

        /// <summary>Tests to see if the AnchorFaceList contains the AnchorFace</summary>
        public static bool Contains(this AnchorFaceList self, AnchorFace face)
            => (self & face.ToAnchorFaceList()) > 0;

        /// <summary>True if any intersection occurs</summary>
        public static bool Intersects(this AnchorFaceList self, IEnumerable<AnchorFace> faces)
            => faces.Any(_f => self.Contains(_f));

        /// <summary>True if any intersection occurs</summary>
        public static bool Intersects(this AnchorFaceList self, AnchorFaceList faces)
            => self.Intersection(faces) != AnchorFaceList.None;

        /// <summary>All elements of list are in a subset of faces</summary>
        public static bool IsSubset(this AnchorFaceList self, IEnumerable<AnchorFace> faces)
            => self.ToAnchorFaces().All(_f => faces.Contains(_f));

        /// <summary>All elements of list are in a subset of faces</summary>
        public static bool IsSubset(this AnchorFaceList self, AnchorFaceList faces)
            => self.Union(faces) == self;

        /// <summary>List contains any face</summary>
        public static bool ContainsAny(this AnchorFaceList self, params AnchorFace[] faces)
            => self.Intersects(faces);

        /// <summary>Yields all AnchorFaces in the AnchorFaceList</summary>
        public static IEnumerable<AnchorFace> ToAnchorFaces(this AnchorFaceList self)
            => AnchorFaceHelper.GetAllFaces().Where(_f => self.Contains(_f));

        #region public static IEnumerable<AnchorFaceList> EdgeCellOffsets()
        /// <summary>Yields all AnchorFace arrays that when added to a cell will generate another cell with a single edge touching</summary>
        public static IEnumerable<AnchorFaceList> EdgeCellOffsets()
        {
            yield return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.YHigh);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.YLow);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.XHigh);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZHigh, AnchorFace.XLow);
            yield return AnchorFaceListHelper.Create(AnchorFace.YHigh, AnchorFace.XHigh);
            yield return AnchorFaceListHelper.Create(AnchorFace.YHigh, AnchorFace.XLow);
            yield return AnchorFaceListHelper.Create(AnchorFace.YLow, AnchorFace.XHigh);
            yield return AnchorFaceListHelper.Create(AnchorFace.YLow, AnchorFace.XLow);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.YHigh);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.YLow);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.XHigh);
            yield return AnchorFaceListHelper.Create(AnchorFace.ZLow, AnchorFace.XLow);
            yield break;
        }
        #endregion

        /// <summary>For every low, provide a high, for every high, provide a low</summary>
        public static AnchorFaceList Invert(this AnchorFaceList self)
            => AnchorFaceList.None
            | (AnchorFaceList)((byte)(self & AnchorFaceList.LowMask) << 1)
            | (AnchorFaceList)((byte)(self & AnchorFaceList.HighMask) >> 1);

        public static AnchorFaceList StripAxis(this AnchorFaceList self, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return self & AnchorFaceList.YZMask;

                case Axis.Y:
                    return self & AnchorFaceList.ZXMask;

                case Axis.Z:
                default:
                    return self & AnchorFaceList.XYMask;
            }
        }

        public static AxisSnap GetAxisSnap(this AnchorFaceList self, Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    if (self.Contains(AnchorFace.XHigh))
                        return AxisSnap.High;
                    else if (self.Contains(AnchorFace.XLow))
                        return AxisSnap.Low;
                    return AxisSnap.None;
                case Axis.Y:
                    if (self.Contains(AnchorFace.YHigh))
                        return AxisSnap.High;
                    else if (self.Contains(AnchorFace.YLow))
                        return AxisSnap.Low;
                    return AxisSnap.None;
                case Axis.Z:
                default:
                    if (self.Contains(AnchorFace.ZHigh))
                        return AxisSnap.High;
                    else if (self.Contains(AnchorFace.ZLow))
                        return AxisSnap.Low;
                    return AxisSnap.None;
            }
        }

        public static ICellLocation GetAnchorOffset(this AnchorFaceList self)
            => new CellPosition(
                0 + (self.Intersects(AnchorFaceList.ZHigh) ? 1 : 0) - (self.Intersects(AnchorFaceList.ZLow) ? 1 : 0),
                0 + (self.Intersects(AnchorFaceList.YHigh) ? 1 : 0) - (self.Intersects(AnchorFaceList.YLow) ? 1 : 0),
                0 + (self.Intersects(AnchorFaceList.XHigh) ? 1 : 0) - (self.Intersects(AnchorFaceList.XLow) ? 1 : 0));
    }
}
