using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts;
using System.Windows;

namespace Uzi.Visualize
{
    /// <summary>AnchorFace represents the face of a cube to which something is attached (XLow, XHigh, YLow, YHigh, ZLow, ZHigh)</summary>
    [DataContract(Namespace = Statics.Namespace)]
    [Serializable]
    public enum AnchorFace
    {
        /// <summary>0</summary>
        XLow,
        /// <summary>1</summary>
        XHigh,
        /// <summary>2</summary>
        YLow,
        /// <summary>3</summary>
        YHigh,
        /// <summary>4</summary>
        ZLow,
        /// <summary>5</summary>
        ZHigh
    }
    public static class AnchorFaceHelper
    {
        #region static ctor()
        static AnchorFaceHelper()
        {
            // vector function
            Vector3D _getVector(AnchorFace face, int heading)
            {
                var _vector = new Vector3D(0, 0, 0);
                foreach (var _face in MovementFaces(face, heading, 0))
                    _vector += _face.GetNormalVector();
                return _vector;
            };

            // fill heading vector cache
            _HeadingVectors = new Dictionary<AnchorFace, Dictionary<int, Vector3D>>();
            foreach (var _face in AnchorFaceHelper.GetAllFaces())
            {
                // build heading reference dictionary
                var _ref = new Dictionary<int, Vector3D>();
                for (var _hx = 0; _hx < 8; _hx++)
                {
                    _ref.Add(_hx, _getVector(_face, _hx));
                }

                // add heading reference vector dictionary by gravity face
                _HeadingVectors.Add(_face, _ref);
            }

            // fill heading face list cache
            _HeadingFaces = new Dictionary<AnchorFace, List<(int Heading, AnchorFaceList Crossings)>>();
            foreach (var _face in AnchorFaceHelper.GetAllFaces())
            {
                // build set
                var _ref = new List<(int Heading, AnchorFaceList Crossings)>();
                for (var _hx = 0; _hx < 8; _hx++)
                {
                    var _afl = AnchorFaceListHelper.Create(MovementFaces(_face, _hx, 0));
                    _ref.Add((_hx, _afl));
                }

                // track set
                _HeadingFaces.Add(_face, _ref);
            }
        }
        #endregion

        /// <summary>pre-calculated heading vectors</summary>
        private static readonly Dictionary<AnchorFace, Dictionary<int, Vector3D>> _HeadingVectors;

        private static readonly Dictionary<AnchorFace, List<(int Heading, AnchorFaceList Crossings)>> _HeadingFaces;

        /// <summary>Indicates that the test face opposes (directly opposite) this AnchorFace</summary>
        public static bool IsOppositeTo(this AnchorFace self, AnchorFace test)
            => self.ReverseFace() == test;

        #region public static AnchorFace ReverseFace(this AnchorFace self)
        /// <summary>Gets reverse face</summary>
        public static AnchorFace ReverseFace(this AnchorFace self)
        {
            switch (self)
            {
                case AnchorFace.XHigh:
                    return AnchorFace.XLow;
                case AnchorFace.XLow:
                    return AnchorFace.XHigh;
                case AnchorFace.YHigh:
                    return AnchorFace.YLow;
                case AnchorFace.YLow:
                    return AnchorFace.YHigh;
                case AnchorFace.ZHigh:
                    return AnchorFace.ZLow;
                default:
                    return AnchorFace.ZHigh;
            }
        }
        #endregion

        #region public static bool IsOrthogonalTo(this AnchorFace self, Axis test)
        /// <summary>True if the axis is orthogonal to the specified face</summary>
        public static bool IsOrthogonalTo(this AnchorFace self, Axis test)
        {
            if (test == Axis.Z)
                return (self == AnchorFace.ZLow) || (self == AnchorFace.ZHigh);
            if (test == Axis.Y)
                return (self == AnchorFace.YLow) || (self == AnchorFace.YHigh);
            return (self == AnchorFace.XLow) || (self == AnchorFace.XHigh);
        }
        #endregion

        /// <summary>True if the face is a LowFace</summary>
        public static bool IsLowFace(this AnchorFace self)
            => (self == AnchorFace.XLow) || (self == AnchorFace.YLow) || (self == AnchorFace.ZLow);

        private static readonly Vector3D[] _FaceNormals =
        {
            new Vector3D(-1, 0, 0),
            new Vector3D(1, 0, 0),
            new Vector3D(0, -1, 0),
            new Vector3D(0, 1, 0),
            new Vector3D(0, 0, -1),
            new Vector3D(0, 0, 1)
        };

        #region public static Vector3D GetNormalVector(this AnchorFace self)
        /// <summary>Normal Vector for the face: ZLow = 0,0,-1</summary>
        public static Vector3D GetNormalVector(this AnchorFace self)
            => ((self >= AnchorFace.XLow) && (self <= AnchorFace.ZHigh))
            ? _FaceNormals[(int)self]
            : _FaceNormals[5];
        #endregion

        #region public static Axis GetAxis(this AnchorFace self)
        /// <summary>Provides the Axis for the AnchorFace</summary>
        public static Axis GetAxis(this AnchorFace self)
        {
            switch (self)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                    return Axis.Z;

                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    return Axis.Y;

                case AnchorFace.XLow:
                default:
                    return Axis.X;
            }
        }
        #endregion

        #region public static CellPosition GetAnchorOffset(this AnchorFace face, bool reversed = false)
        /// <summary>Provides a relative CellPosition implied by a step across the specified face</summary>
        public static CellPosition GetAnchorOffset(this AnchorFace face, bool reversed = false)
        {
            int _faceOffset(AnchorFace faceHigh, AnchorFace faceLow)
                => face == faceHigh
                ? (reversed ? -1 : 1)
                : (face == faceLow ? (reversed ? 1 : -1) : 0);
            return new CellPosition
            {
                Z = _faceOffset(AnchorFace.ZHigh, AnchorFace.ZLow),
                Y = _faceOffset(AnchorFace.YHigh, AnchorFace.YLow),
                X = _faceOffset(AnchorFace.XHigh, AnchorFace.XLow)
            };
        }
        #endregion

        #region public static CellPosition GetAnchorOffset(this AnchorFace[] faces, bool reversed = false)
        /// <summary>Provides a relative CellPosition implied by a step across faces in the array</summary>
        public static CellPosition GetAnchorOffset(this AnchorFace[] faces, bool reversed = false)
        {
            int _faceOffset(AnchorFace faceHigh, AnchorFace faceLow)
                => faces.Contains(faceHigh)
                ? (reversed ? -1 : 1)
                : (faces.Contains(faceLow) ? (reversed ? 1 : -1) : 0);
            return new CellPosition
            {
                Z = _faceOffset(AnchorFace.ZHigh, AnchorFace.ZLow),
                Y = _faceOffset(AnchorFace.YHigh, AnchorFace.YLow),
                X = _faceOffset(AnchorFace.XHigh, AnchorFace.XLow)
            };
        }
        #endregion

        /// <summary>Gets headings in range 0..7 (allowing diagonals)</summary>
        public static int GetHeadingVector(this AnchorFace gravity, Point3D source, Point3D target)
        {
            // get vector with smallest angular displacement to pre-defined vectors
            var _displace = target - source;
            return (from _vector in _HeadingVectors[gravity]
                    orderby Vector3D.AngleBetween(_vector.Value, _displace)
                    select _vector.Key).FirstOrDefault();
        }

        /// <summary>Gets non-diagonal headings in range 0..3</summary>
        public static int GetOrthoHeadingVector(this AnchorFace gravity, Point3D source, Point3D target)
        {
            // get vector with smallest angular displacement to pre-defined vectors
            var _displace = target - source;
            return (from _vector in _HeadingVectors[gravity]
                    where (_vector.Key % 2) != 1
                    orderby Vector3D.AngleBetween(_vector.Value, _displace)
                    select _vector.Key).FirstOrDefault() / 2;
        }

        #region public static List<AnchorFace> MovementFaces(AnchorFace gravity, int heading, int upDownAdjust)
        /// <summary>Get List of AnchorFaces to be used for movement targeting</summary>
        public static List<AnchorFace> MovementFaces(AnchorFace gravity, int heading, int upDownAdjust)
        {
            var _faces = new List<AnchorFace>();

            #region relative heading adjustment
            // NOTE: initial heading of eight means straight up or down
            if (Math.Abs(upDownAdjust) <= 1)
            {
                // "true" heading adjustment based on direction moved and camera heading
                switch (heading)
                {
                    case 0:
                        AddForwardFace(gravity, _faces);
                        break;

                    case 1:
                        AddForwardFace(gravity, _faces);
                        AddLeftFace(gravity, _faces);
                        break;

                    case 2:
                        AddLeftFace(gravity, _faces);
                        break;

                    case 3:
                        AddBackwardFace(gravity, _faces);
                        AddLeftFace(gravity, _faces);
                        break;

                    case 4:
                        AddBackwardFace(gravity, _faces);
                        break;

                    case 5:
                        AddBackwardFace(gravity, _faces);
                        AddRightFace(gravity, _faces);
                        break;

                    case 6:
                        AddRightFace(gravity, _faces);
                        break;

                    case 7:
                        AddForwardFace(gravity, _faces);
                        AddRightFace(gravity, _faces);
                        break;

                    default:
                        // no heading adjustment (up or down only)
                        break;
                }
            }
            #endregion

            #region up-down face adjustment
            // up down face adjustment by gravity
            if (upDownAdjust != 0)
            {
                switch (gravity)
                {
                    case AnchorFace.ZLow:
                        _faces.Add(upDownAdjust > 0 ? AnchorFace.ZHigh : AnchorFace.ZLow);
                        break;
                    case AnchorFace.ZHigh:
                        _faces.Add(upDownAdjust < 0 ? AnchorFace.ZHigh : AnchorFace.ZLow);
                        break;
                    case AnchorFace.YLow:
                        _faces.Add(upDownAdjust > 0 ? AnchorFace.YHigh : AnchorFace.YLow);
                        break;
                    case AnchorFace.YHigh:
                        _faces.Add(upDownAdjust < 0 ? AnchorFace.YHigh : AnchorFace.YLow);
                        break;
                    case AnchorFace.XLow:
                        _faces.Add(upDownAdjust > 0 ? AnchorFace.XHigh : AnchorFace.XLow);
                        break;
                    case AnchorFace.XHigh:
                        _faces.Add(upDownAdjust < 0 ? AnchorFace.XHigh : AnchorFace.XLow);
                        break;
                }
            }
            #endregion

            return _faces;
        }

        #region private static void AddRightFace(AnchorFace gravity,List<AnchorFace> faces)
        private static void AddRightFace(AnchorFace gravity, List<AnchorFace> faces)
        {
            switch (gravity)
            {
                case AnchorFace.ZLow:
                    faces.Add(AnchorFace.YLow);
                    break;
                case AnchorFace.ZHigh:
                    faces.Add(AnchorFace.YHigh);
                    break;
                case AnchorFace.YLow:
                    faces.Add(AnchorFace.ZHigh);
                    break;
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.ZLow);
                    break;
                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.YLow);
                    break;
            }
        }
        #endregion

        #region private static void AddLeftFace(AnchorFace gravity, List<AnchorFace> faces)
        private static void AddLeftFace(AnchorFace gravity, List<AnchorFace> faces)
        {
            switch (gravity)
            {
                case AnchorFace.ZLow:
                    faces.Add(AnchorFace.YHigh);
                    break;
                case AnchorFace.ZHigh:
                    faces.Add(AnchorFace.YLow);
                    break;
                case AnchorFace.YLow:
                    faces.Add(AnchorFace.ZLow);
                    break;
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.ZHigh);
                    break;
                case AnchorFace.XLow:
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.YHigh);
                    break;
            }
        }
        #endregion

        #region private static void AddBackwardFace(AnchorFace gravity, List<AnchorFace> faces)
        private static void AddBackwardFace(AnchorFace gravity, List<AnchorFace> faces)
        {
            switch (gravity)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.XLow);
                    break;
                case AnchorFace.XLow:
                    faces.Add(AnchorFace.ZHigh);
                    break;
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.ZLow);
                    break;
            }
        }
        #endregion

        #region private static void AddForwardFace(AnchorFace gravity, List<AnchorFace> faces)
        private static void AddForwardFace(AnchorFace gravity, List<AnchorFace> faces)
        {
            // forward
            switch (gravity)
            {
                case AnchorFace.ZLow:
                case AnchorFace.ZHigh:
                case AnchorFace.YLow:
                case AnchorFace.YHigh:
                    faces.Add(AnchorFace.XHigh);
                    break;
                case AnchorFace.XLow:
                    faces.Add(AnchorFace.ZLow);
                    break;
                case AnchorFace.XHigh:
                    faces.Add(AnchorFace.ZHigh);
                    break;
            }
        }
        #endregion

        #endregion

        #region public static AnchorFace? FrontFace(this AnchorFace self, int heading)
        /// <summary>Gets the front face for a given bottom-face when using a specific heading.  Headings must be 0,2,4 or 6</summary>
        public static AnchorFace? FrontFace(this AnchorFace self, int heading)
        {
            if ((heading % 2) == 1)
                return null;

            if (!self.IsLowFace())
                return self.ReverseFace().FrontFace(heading).Value.ReverseFace();

            switch (self)
            {
                case AnchorFace.ZLow:
                    switch (heading)
                    {
                        case 0:
                            return AnchorFace.XHigh;
                        case 2:
                            return AnchorFace.YHigh;
                        case 4:
                            return AnchorFace.XLow;
                        default:
                            return AnchorFace.YLow;
                    }

                case AnchorFace.YLow:
                    switch (heading)
                    {
                        case 0:
                            return AnchorFace.ZHigh;
                        case 2:
                            return AnchorFace.XHigh;
                        case 4:
                            return AnchorFace.ZLow;
                        default:
                            return AnchorFace.XLow;
                    }

                default: //case AnchorFace.XLow:
                    switch (heading)
                    {
                        case 0:
                            return AnchorFace.YHigh;
                        case 2:
                            return AnchorFace.ZHigh;
                        case 4:
                            return AnchorFace.YLow;
                        default:
                            return AnchorFace.ZLow;
                    }
            }
        }
        #endregion

        #region public static AnchorFace? BackFace(this AnchorFace self, int heading)
        /// <summary>Gets the back face for a given bottom-face when using a specific heading.  Headings must be 0,2,4 or 6</summary>
        public static AnchorFace? BackFace(this AnchorFace self, int heading)
        {
            var _front = self.FrontFace(heading);
            if (!_front.HasValue)
                return null;
            return _front.Value.ReverseFace();
        }
        #endregion

        #region public static AnchorFace? LeftFace(this AnchorFace self, int heading)
        /// <summary>Gets the left face for a given bottom-face when using a specific heading.  Headings must be 0,2,4 or 6</summary>
        public static AnchorFace? LeftFace(this AnchorFace self, int heading)
        {
            if ((heading % 2) == 1)
                return null;

            // high-left-faces are reverse of low-left-faces
            if (!self.IsLowFace())
                return self.ReverseFace().LeftFace(heading).Value.ReverseFace();

            return self switch
            {
                AnchorFace.ZLow => heading switch
                {
                    0 => AnchorFace.YHigh,
                    2 => AnchorFace.XLow,
                    4 => AnchorFace.YLow,
                    _ => AnchorFace.XHigh,
                },
                AnchorFace.YLow => heading switch
                {
                    0 => AnchorFace.XHigh,
                    2 => AnchorFace.ZLow,
                    4 => AnchorFace.XLow,
                    _ => AnchorFace.ZHigh,
                },
                // AnchorFace.XLow:
                _ => heading switch
                {
                    0 => AnchorFace.ZHigh,
                    2 => AnchorFace.YLow,
                    4 => AnchorFace.ZLow,
                    _ => AnchorFace.YHigh,
                },
            };
        }
        #endregion

        #region public static AnchorFace? RightFace(this AnchorFace self, int heading)
        /// <summary>Gets the right face for a given bottom-face when using a specific heading.  Headings must be 0,2,4 or 6</summary>
        public static AnchorFace? RightFace(this AnchorFace self, int heading)
        {
            var _left = self.LeftFace(heading);
            if (!_left.HasValue)
                return null;
            return _left.Value.ReverseFace();
        }
        #endregion

        public static OptionalAnchorFace ToOptionalAnchorFace(this AnchorFace self)
            => (OptionalAnchorFace)((int)self + 1);

        /// <summary>Yields all faces except self</summary>
        public static IEnumerable<AnchorFace> GetOtherFaces(this AnchorFace self)
            => AnchorFaceHelper.GetAllFaces().Where(_f => _f != self);

        /// <summary>Yields all faces</summary>
        public static IEnumerable<AnchorFace> GetAllFaces()
        {
            yield return AnchorFace.ZHigh;
            yield return AnchorFace.ZLow;
            yield return AnchorFace.YHigh;
            yield return AnchorFace.YLow;
            yield return AnchorFace.XHigh;
            yield return AnchorFace.XLow;
            yield break;
        }

        #region public static Point3D MapMeshPoint(this AnchorFace self, Point point, bool forInner)
        public static Point3D MapMeshPoint(this AnchorFace self, Point point, bool forInner)
        {
            switch (self)
            {
                case AnchorFace.XLow:
                    return new Point3D(0, 5 - point.X, point.Y);
                case AnchorFace.XHigh:
                    return new Point3D(5, point.X, point.Y);
                case AnchorFace.YLow:
                    return new Point3D(point.X, 0, point.Y);
                case AnchorFace.YHigh:
                    return new Point3D(5 - point.X, 5, point.Y);
                case AnchorFace.ZLow:
                    return new Point3D(5 - point.X, point.Y, 0);
                case AnchorFace.ZHigh:
                default:
                    return new Point3D(point.X, point.Y, forInner ? 0 : 5);
            }
        }
        #endregion

        #region public static IEnumerable<AnchorFace> GetOrthoFaces(this AnchorFace self)
        /// <summary>Yields all off-axis faces</summary>
        public static IEnumerable<AnchorFace> GetOrthoFaces(this AnchorFace self)
        {
            switch (self.GetAxis())
            {
                case Axis.Z:
                    yield return AnchorFace.YHigh;
                    yield return AnchorFace.YLow;
                    yield return AnchorFace.XHigh;
                    yield return AnchorFace.XLow;
                    break;
                case Axis.Y:
                    yield return AnchorFace.ZHigh;
                    yield return AnchorFace.ZLow;
                    yield return AnchorFace.XHigh;
                    yield return AnchorFace.XLow;
                    break;
                case Axis.X:
                default:
                    yield return AnchorFace.ZHigh;
                    yield return AnchorFace.ZLow;
                    yield return AnchorFace.YHigh;
                    yield return AnchorFace.YLow;
                    break;
            }
            yield break;
        }
        #endregion

        #region public static Transform3D Transform(this AnchorFace self)
        /// <summary>
        /// Transform needed to move a flat X-Y (Z=0) Geometry3D to a cube side
        /// </summary>
        public static Transform3D Transform(this AnchorFace self, bool forInner = false)
        {
            switch (self)
            {
                case AnchorFace.ZHigh:
                    return !forInner ? HedralGenerator.ZPTransform : HedralGenerator.NullTransform;
                case AnchorFace.ZLow:
                    return HedralGenerator.ZMTransform;
                case AnchorFace.YHigh:
                    return HedralGenerator.YPTransform;
                case AnchorFace.YLow:
                    return HedralGenerator.YMTransform;
                case AnchorFace.XHigh:
                    return HedralGenerator.XPTransform;
                case AnchorFace.XLow:
                default:
                    return HedralGenerator.XMTransform;
            }
        }
        #endregion

        #region public static Transform3D Transform(this AnchorFace self)
        /// <summary>
        /// Transform needed to move a flat X-Y (Z=0) Geometry3D to a cube side (but flipped)
        /// </summary>
        public static Transform3D FlippedTransform(this AnchorFace self, bool forInner = false)
        {
            switch (self)
            {
                case AnchorFace.ZHigh:
                    return !forInner ? HedralGenerator.FlippedZPTransform : HedralGenerator.ZMAltFlip;
                case AnchorFace.ZLow:
                    return HedralGenerator.FlippedZMTransform;
                case AnchorFace.YHigh:
                    return HedralGenerator.FlippedYPTransform;
                case AnchorFace.YLow:
                    return HedralGenerator.FlippedYMTransform;
                case AnchorFace.XHigh:
                    return HedralGenerator.FlippedXPTransform;
                case AnchorFace.XLow:
                default:
                    return HedralGenerator.FlippedXMTransform;
            }
        }
        #endregion

        #region public static FaceEdge GetSnappingEdge(this AnchorFace self, AnchorFace faceForEdge)
        /// <summary>Returns the edge needed to snap this AnchorFace to the faceForEdge</summary>
        public static FaceEdge GetSnappingEdge(this AnchorFace self, AnchorFace faceForEdge)
        {
            switch (self)
            {
                case AnchorFace.ZHigh:
                    switch (faceForEdge)
                    {
                        case AnchorFace.YHigh: return FaceEdge.Top;
                        case AnchorFace.YLow: return FaceEdge.Bottom;
                        case AnchorFace.XHigh: return FaceEdge.Right;
                        case AnchorFace.XLow:
                        default: return FaceEdge.Left;
                    }
                case AnchorFace.ZLow:
                    switch (faceForEdge)
                    {
                        case AnchorFace.YHigh: return FaceEdge.Top;
                        case AnchorFace.YLow: return FaceEdge.Bottom;
                        case AnchorFace.XHigh: return FaceEdge.Left;
                        case AnchorFace.XLow:
                        default: return FaceEdge.Right;
                    }
                case AnchorFace.YHigh:
                    switch (faceForEdge)
                    {
                        case AnchorFace.ZHigh: return FaceEdge.Top;
                        case AnchorFace.ZLow: return FaceEdge.Bottom;
                        case AnchorFace.XHigh: return FaceEdge.Left;
                        case AnchorFace.XLow:
                        default: return FaceEdge.Right;
                    }
                case AnchorFace.YLow:
                    switch (faceForEdge)
                    {
                        case AnchorFace.ZHigh: return FaceEdge.Top;
                        case AnchorFace.ZLow: return FaceEdge.Bottom;
                        case AnchorFace.XHigh: return FaceEdge.Right;
                        case AnchorFace.XLow:
                        default: return FaceEdge.Left;
                    }
                case AnchorFace.XHigh:
                    switch (faceForEdge)
                    {
                        case AnchorFace.ZHigh: return FaceEdge.Top;
                        case AnchorFace.ZLow: return FaceEdge.Bottom;
                        case AnchorFace.YHigh: return FaceEdge.Right;
                        case AnchorFace.YLow:
                        default: return FaceEdge.Left;
                    }
                case AnchorFace.XLow:
                default:
                    switch (faceForEdge)
                    {
                        case AnchorFace.ZHigh: return FaceEdge.Top;
                        case AnchorFace.ZLow: return FaceEdge.Bottom;
                        case AnchorFace.YHigh: return FaceEdge.Left;
                        case AnchorFace.YLow:
                        default: return FaceEdge.Right;
                    }
            }
        }
        #endregion

        #region public static SideIndex GetSideIndex(this AnchorFace self, AnchorFace sideFace)
        /// <summary>When this face is the outside (back), returns the SideIndex representing the sideFace</summary>
        public static SideIndex GetSideIndex(this AnchorFace self, AnchorFace sideFace)
        {
            switch (self)
            {
                case AnchorFace.ZHigh:
                    switch (sideFace)
                    {
                        case AnchorFace.ZHigh: return SideIndex.Back;
                        case AnchorFace.ZLow: return SideIndex.Front;
                        case AnchorFace.YHigh: return SideIndex.Top;
                        case AnchorFace.YLow: return SideIndex.Bottom;
                        case AnchorFace.XHigh: return SideIndex.Left;
                        case AnchorFace.XLow:
                        default: return SideIndex.Right;
                    }
                case AnchorFace.ZLow:
                    switch (sideFace)
                    {
                        case AnchorFace.ZHigh: return SideIndex.Front;
                        case AnchorFace.ZLow: return SideIndex.Back;
                        case AnchorFace.YHigh: return SideIndex.Top;
                        case AnchorFace.YLow: return SideIndex.Bottom;
                        case AnchorFace.XHigh: return SideIndex.Right;
                        case AnchorFace.XLow:
                        default: return SideIndex.Left;
                    }
                case AnchorFace.YHigh:
                    switch (sideFace)
                    {
                        case AnchorFace.ZHigh: return SideIndex.Top;
                        case AnchorFace.ZLow: return SideIndex.Bottom;
                        case AnchorFace.YHigh: return SideIndex.Back;
                        case AnchorFace.YLow: return SideIndex.Front;
                        case AnchorFace.XHigh: return SideIndex.Right;
                        case AnchorFace.XLow:
                        default: return SideIndex.Left;
                    }
                case AnchorFace.YLow:
                    switch (sideFace)
                    {
                        case AnchorFace.ZHigh: return SideIndex.Top;
                        case AnchorFace.ZLow: return SideIndex.Bottom;
                        case AnchorFace.YHigh: return SideIndex.Front;
                        case AnchorFace.YLow: return SideIndex.Back;
                        case AnchorFace.XHigh: return SideIndex.Left;
                        case AnchorFace.XLow:
                        default: return SideIndex.Right;
                    }
                case AnchorFace.XHigh:
                    switch (sideFace)
                    {
                        case AnchorFace.ZHigh: return SideIndex.Top;
                        case AnchorFace.ZLow: return SideIndex.Bottom;
                        case AnchorFace.YHigh: return SideIndex.Left;
                        case AnchorFace.YLow: return SideIndex.Right;
                        case AnchorFace.XHigh: return SideIndex.Back;
                        case AnchorFace.XLow:
                        default: return SideIndex.Front;
                    }
                case AnchorFace.XLow:
                default:
                    switch (sideFace)
                    {
                        case AnchorFace.ZHigh: return SideIndex.Top;
                        case AnchorFace.ZLow: return SideIndex.Bottom;
                        case AnchorFace.YHigh: return SideIndex.Right;
                        case AnchorFace.YLow: return SideIndex.Left;
                        case AnchorFace.XHigh: return SideIndex.Front;
                        case AnchorFace.XLow:
                        default: return SideIndex.Back;
                    }
            }
        }
        #endregion

        #region public static AnchorFace GetSnappedFace(this AnchorFace panelFace, FaceEdge snap)
        /// <summary>Returns the AnchorFace to which this AnchorFace will be snapped by the edge</summary>
        public static AnchorFace GetSnappedFace(this AnchorFace panelFace, FaceEdge snap)
        {
            switch (panelFace)
            {
                case AnchorFace.ZLow:
                    switch (snap)
                    {
                        case FaceEdge.Bottom: return AnchorFace.YLow;
                        case FaceEdge.Top: return AnchorFace.YHigh;
                        case FaceEdge.Right: return AnchorFace.XLow;
                        case FaceEdge.Left:
                        default: return AnchorFace.XHigh;
                    }

                case AnchorFace.YLow:
                    switch (snap)
                    {
                        case FaceEdge.Bottom: return AnchorFace.ZLow;
                        case FaceEdge.Top: return AnchorFace.ZHigh;
                        case FaceEdge.Left: return AnchorFace.XLow;
                        case FaceEdge.Right:
                        default: return AnchorFace.XHigh;
                    }

                case AnchorFace.XLow:
                    switch (snap)
                    {
                        case FaceEdge.Bottom: return AnchorFace.ZLow;
                        case FaceEdge.Top: return AnchorFace.ZHigh;
                        case FaceEdge.Right: return AnchorFace.YLow;
                        case FaceEdge.Left:
                        default: return AnchorFace.YHigh;
                    }

                case AnchorFace.ZHigh:
                    switch (snap)
                    {
                        case FaceEdge.Top: return AnchorFace.YHigh;
                        case FaceEdge.Bottom: return AnchorFace.YLow;
                        case FaceEdge.Left: return AnchorFace.XLow;
                        case FaceEdge.Right:
                        default: return AnchorFace.XHigh;
                    }

                case AnchorFace.YHigh:
                    switch (snap)
                    {
                        case FaceEdge.Bottom: return AnchorFace.ZLow;
                        case FaceEdge.Top: return AnchorFace.ZHigh;
                        case FaceEdge.Right: return AnchorFace.XLow;
                        case FaceEdge.Left:
                        default: return AnchorFace.XHigh;
                    }

                case AnchorFace.XHigh:
                default:
                    switch (snap)
                    {
                        case FaceEdge.Bottom: return AnchorFace.ZLow;
                        case FaceEdge.Top: return AnchorFace.ZHigh;
                        case FaceEdge.Left: return AnchorFace.YLow;
                        case FaceEdge.Right:
                        default: return AnchorFace.YHigh;
                    }
            }
        }
        #endregion

        #region public static AnchorFace VerticalSnappedFace(this AnchorFace panelFace, TriangleCorner corner)
        /// <summary>Returns the AnchorFace associated with the vertical component of the corner for this panelFace</summary>
        public static AnchorFace VerticalSnappedFace(this AnchorFace panelFace, TriangleCorner corner)
        {
            switch (panelFace)
            {
                case AnchorFace.ZHigh:
                case AnchorFace.ZLow:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.LowerRight:
                            return AnchorFace.YLow;
                        case TriangleCorner.UpperLeft:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.YHigh;
                    }

                case AnchorFace.YHigh:
                case AnchorFace.YLow:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.LowerRight:
                            return AnchorFace.ZLow;
                        case TriangleCorner.UpperLeft:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.ZHigh;
                    }
                case AnchorFace.XHigh:
                case AnchorFace.XLow:
                default:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.LowerRight:
                            return AnchorFace.ZLow;
                        case TriangleCorner.UpperLeft:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.ZHigh;
                    }
            }
        }
        #endregion

        #region public static AnchorFace HorizontalSnappedFace(this AnchorFace panelFace, TriangleCorner corner)
        /// <summary>Returns the AnchorFace associated with the horizontal component of the corner for this panelFace</summary>
        public static AnchorFace HorizontalSnappedFace(this AnchorFace panelFace, TriangleCorner corner)
        {
            switch (panelFace)
            {
                case AnchorFace.ZHigh:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.UpperLeft:
                            return AnchorFace.XLow;
                        case TriangleCorner.LowerRight:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.XHigh;
                    }
                case AnchorFace.ZLow:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.UpperLeft:
                            return AnchorFace.XHigh;
                        case TriangleCorner.LowerRight:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.XLow;
                    }
                case AnchorFace.YHigh:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.UpperLeft:
                            return AnchorFace.XHigh;
                        case TriangleCorner.LowerRight:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.XLow;
                    }
                case AnchorFace.YLow:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.UpperLeft:
                            return AnchorFace.XLow;
                        case TriangleCorner.LowerRight:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.XHigh;
                    }
                case AnchorFace.XHigh:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.UpperLeft:
                            return AnchorFace.YLow;
                        case TriangleCorner.LowerRight:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.YHigh;
                    }
                case AnchorFace.XLow:
                default:
                    switch (corner)
                    {
                        case TriangleCorner.LowerLeft:
                        case TriangleCorner.UpperLeft:
                            return AnchorFace.YHigh;
                        case TriangleCorner.LowerRight:
                        case TriangleCorner.UpperRight:
                        default:
                            return AnchorFace.YLow;
                    }
            }
        }
        #endregion

        public static Vector3D GetHeadingVector(this AnchorFace self, int heading)
            => _HeadingVectors[self][heading % 8];

        public static AnchorFaceList ToAnchorFaceList(this AnchorFace self)
            => (AnchorFaceList)(1 << (int)self);

        /// <summary>Forward faces for a heading when this AnchorFace is the bottom face.</summary>
        public static AnchorFaceList GetHeadingFaces(this AnchorFace self, int heading)
            => _HeadingFaces[self][heading % 8].Crossings;

        public static int GetHeadingValue(this AnchorFace self, AnchorFaceList facing)
        {
            var _facing = facing.StripAxis(self.GetAxis());
            return _HeadingFaces[self].FirstOrDefault(_t => _t.Crossings == _facing).Heading;
        }
    }
}