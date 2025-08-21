using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace Uzi.Visualize
{
    public struct PanelParams
    {
        #region bit 32 vector management
        private static readonly BitVector32.Section _TypeZLow;      // bits: 00-02
        private static readonly BitVector32.Section _TypeZHigh;     // bits: 03-05
        private static readonly BitVector32.Section _TypeYLow;      // bits: 06-08
        private static readonly BitVector32.Section _TypeYHigh;     // bits: 09-0B
        private static readonly BitVector32.Section _TypeXLow;      // bits: 0C-0E
        private static readonly BitVector32.Section _TypeXHigh;     // bits: 0F-11

        private static readonly BitVector32.Section _SnapZLow;      // bits: 12-13
        private static readonly BitVector32.Section _SnapZHigh;     // bits: 14-15
        private static readonly BitVector32.Section _SnapYLow;      // bits: 16-17
        private static readonly BitVector32.Section _SnapYHigh;     // bits: 18-19
        private static readonly BitVector32.Section _SnapXLow;      // bits: 1A-1B
        private static readonly BitVector32.Section _SnapXHigh;     // bits: 1C-1D

        private static readonly BitVector32.Section _Fill;          // bits: 1E-1F

        // interiors
        private static readonly BitVector32.Section _Interior;      // bits: 12-13
        private static readonly BitVector32.Section _SourceFace;    // bits: 14-16
        private static readonly BitVector32.Section _SinkFace;      // bits: 17-19
        private static readonly BitVector32.Section _OtherFace;     // bits: 1A-1C
        private static readonly BitVector32.Section _SlopeIndex;    // bits: 1A-1D
        #endregion

        #region static setup
        /// <summary>
        /// suppressing message about static construction, since these fields have an order dependency on initialization
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static PanelParams()
        {
            // paneltype
            _TypeZLow = BitVector32.CreateSection(7);
            _TypeZHigh = BitVector32.CreateSection(7, _TypeZLow);
            _TypeYLow = BitVector32.CreateSection(7, _TypeZHigh);
            _TypeYHigh = BitVector32.CreateSection(7, _TypeYLow);
            _TypeXLow = BitVector32.CreateSection(7, _TypeYHigh);
            _TypeXHigh = BitVector32.CreateSection(7, _TypeXLow);

            // snaps (follows on from panelType)
            _SnapZLow = BitVector32.CreateSection(3, _TypeXHigh);
            _SnapZHigh = BitVector32.CreateSection(3, _SnapZLow);
            _SnapYLow = BitVector32.CreateSection(3, _SnapZHigh);
            _SnapYHigh = BitVector32.CreateSection(3, _SnapYLow);
            _SnapXLow = BitVector32.CreateSection(3, _SnapYHigh);
            _SnapXHigh = BitVector32.CreateSection(3, _SnapXLow);

            // fill (follows on from snaps)
            _Fill = BitVector32.CreateSection(3, _SnapXHigh);

            // interiors (follows on from panelType, as alternate)
            _Interior = BitVector32.CreateSection(3, _TypeXHigh);
            _SourceFace = BitVector32.CreateSection(5, _Interior);
            _SinkFace = BitVector32.CreateSection(5, _SourceFace);
            _OtherFace = BitVector32.CreateSection(6, _SinkFace);

            // rather than OtherFace with an unused bit, slopes are palettized
            _SlopeIndex = BitVector32.CreateSection(15, _SinkFace);
        }
        #endregion

        public PanelParams(uint value)
        {
            // NOTE: sign bit not used in this param
            _Vector = new BitVector32((int)value);
        }

        private BitVector32 _Vector;

        public uint Value { get { return (uint)_Vector.Data; } }

        /// <summary>Has Panels, or (lacking panels) has PanelInterior != None</summary>
        public bool HasStructure
        {
            get { return HasPanels || (PanelInterior != Visualize.PanelInterior.None); }
        }

        /// <summary>Has at least one panel</summary>
        public bool HasPanels
        {
            get { return Panels.Any(_p => _p.Value != PanelType.NoPanel); }
        }

        #region public uint MaskedValue()
        /// <summary>Provides value masked for rendering</summary>
        public uint MaskedValue()
        {
            var _masked = new PanelParams(Value);
            switch (PanelTypeZLow)
            {
                case PanelType.MaskedCorner:
                case PanelType.MaskedLFrame:
                    _masked.PanelTypeZLow = PanelType.Panel1;
                    _masked.EdgeZLow = FaceEdge.Bottom;
                    break;
            }
            switch (PanelTypeYLow)
            {
                case PanelType.MaskedCorner:
                case PanelType.MaskedLFrame:
                    _masked.PanelTypeYLow = PanelType.Panel1;
                    _masked.EdgeYLow = FaceEdge.Bottom;
                    break;
            }
            switch (PanelTypeXLow)
            {
                case PanelType.MaskedCorner:
                case PanelType.MaskedLFrame:
                    _masked.PanelTypeXLow = PanelType.Panel1;
                    _masked.EdgeXLow = FaceEdge.Bottom;
                    break;
            }
            switch (PanelTypeZHigh)
            {
                case PanelType.MaskedCorner:
                case PanelType.MaskedLFrame:
                    _masked.PanelTypeZHigh = PanelType.Panel1;
                    _masked.EdgeZHigh = FaceEdge.Bottom;
                    break;
            }
            switch (PanelTypeYHigh)
            {
                case PanelType.MaskedCorner:
                case PanelType.MaskedLFrame:
                    _masked.PanelTypeYHigh = PanelType.Panel1;
                    _masked.EdgeYHigh = FaceEdge.Bottom;
                    break;
            }
            switch (PanelTypeXHigh)
            {
                case PanelType.MaskedCorner:
                case PanelType.MaskedLFrame:
                    _masked.PanelTypeXHigh = PanelType.Panel1;
                    _masked.EdgeXHigh = FaceEdge.Bottom;
                    break;
            }
            return _masked.Value;
        }
        #endregion

        public PanelType PanelTypeZLow { get => (PanelType)_Vector[_TypeZLow]; set { _Vector[_TypeZLow] = (int)value; } }
        public PanelType PanelTypeYLow { get => (PanelType)_Vector[_TypeYLow]; set { _Vector[_TypeYLow] = (int)value; } }
        public PanelType PanelTypeXLow { get => (PanelType)_Vector[_TypeXLow]; set { _Vector[_TypeXLow] = (int)value; } }
        public PanelType PanelTypeZHigh { get => (PanelType)_Vector[_TypeZHigh]; set { _Vector[_TypeZHigh] = (int)value; } }
        public PanelType PanelTypeYHigh { get => (PanelType)_Vector[_TypeYHigh]; set { _Vector[_TypeYHigh] = (int)value; } }
        public PanelType PanelTypeXHigh { get => (PanelType)_Vector[_TypeXHigh]; set { _Vector[_TypeXHigh] = (int)value; } }

        public FaceEdge EdgeZLow { get => (FaceEdge)_Vector[_SnapZLow]; set { _Vector[_SnapZLow] = (int)value; } }
        public FaceEdge EdgeYLow { get => (FaceEdge)_Vector[_SnapYLow]; set { _Vector[_SnapYLow] = (int)value; } }
        public FaceEdge EdgeXLow { get => (FaceEdge)_Vector[_SnapXLow]; set { _Vector[_SnapXLow] = (int)value; } }
        public FaceEdge EdgeZHigh { get => (FaceEdge)_Vector[_SnapZHigh]; set { _Vector[_SnapZHigh] = (int)value; } }
        public FaceEdge EdgeYHigh { get => (FaceEdge)_Vector[_SnapYHigh]; set { _Vector[_SnapYHigh] = (int)value; } }
        public FaceEdge EdgeXHigh { get => (FaceEdge)_Vector[_SnapXHigh]; set { _Vector[_SnapXHigh] = (int)value; } }

        public TriangleCorner CornerZLow { get => (TriangleCorner)_Vector[_SnapZLow]; set { _Vector[_SnapZLow] = (int)value; } }
        public TriangleCorner CornerYLow { get => (TriangleCorner)_Vector[_SnapYLow]; set { _Vector[_SnapYLow] = (int)value; } }
        public TriangleCorner CornerXLow { get => (TriangleCorner)_Vector[_SnapXLow]; set { _Vector[_SnapXLow] = (int)value; } }
        public TriangleCorner CornerZHigh { get => (TriangleCorner)_Vector[_SnapZHigh]; set { _Vector[_SnapZHigh] = (int)value; } }
        public TriangleCorner CornerYHigh { get => (TriangleCorner)_Vector[_SnapYHigh]; set { _Vector[_SnapYHigh] = (int)value; } }
        public TriangleCorner CornerXHigh { get => (TriangleCorner)_Vector[_SnapXHigh]; set { _Vector[_SnapXHigh] = (int)value; } }

        public PanelFill PanelFill { get => (PanelFill)_Vector[_Fill]; set { _Vector[_Fill] = (int)value; } }

        public AnchorFace SinkFace { get => (AnchorFace)_Vector[_SinkFace]; set { _Vector[_SinkFace] = (int)value; } }
        public AnchorFace SourceFace { get => (AnchorFace)_Vector[_SourceFace]; set { _Vector[_SourceFace] = (int)value; } }
        public PanelInterior PanelInterior { get => (PanelInterior)_Vector[_Interior]; set { _Vector[_Interior] = (int)value; } }
        public OptionalAnchorFace OtherFace { get => (OptionalAnchorFace)_Vector[_OtherFace]; set { _Vector[_OtherFace] = (int)value; } }
        public byte SlopeIndex { get => (byte)_Vector[_SlopeIndex]; set => _Vector[_SlopeIndex] = value; }

        #region private IEnumerable<KeyValuePair<AnchorFace, PanelType>> Panels { get; }
        private IEnumerable<KeyValuePair<AnchorFace, PanelType>> Panels
        {
            get
            {
                yield return new KeyValuePair<AnchorFace, PanelType>(AnchorFace.ZLow, PanelTypeZLow);
                yield return new KeyValuePair<AnchorFace, PanelType>(AnchorFace.YLow, PanelTypeYLow);
                yield return new KeyValuePair<AnchorFace, PanelType>(AnchorFace.XLow, PanelTypeXLow);
                yield return new KeyValuePair<AnchorFace, PanelType>(AnchorFace.ZHigh, PanelTypeZHigh);
                yield return new KeyValuePair<AnchorFace, PanelType>(AnchorFace.YHigh, PanelTypeYHigh);
                yield return new KeyValuePair<AnchorFace, PanelType>(AnchorFace.XHigh, PanelTypeXHigh);
                yield break;
            }
        }
        #endregion

        /// <summary>None of the panels uss Snap parameters, allowing the interior to be set</summary>
        public bool IsInteriorBindable => Panels.All(_p => _p.Value.IsInteriorBindable());

        #region public PanelType GetPanelType(AnchorFace face)
        public PanelType GetPanelType(AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.ZLow: return PanelTypeZLow;
                case AnchorFace.YLow: return PanelTypeYLow;
                case AnchorFace.XLow: return PanelTypeXLow;
                case AnchorFace.ZHigh: return PanelTypeZHigh;
                case AnchorFace.YHigh: return PanelTypeYHigh;
                case AnchorFace.XHigh: return PanelTypeXHigh;
            }
            return PanelType.NoPanel;
        }
        #endregion

        #region public void SetPanelType(AnchorFace face, PanelType panelType)
        public void SetPanelType(AnchorFace face, PanelType panelType)
        {
            switch (face)
            {
                case AnchorFace.ZLow: PanelTypeZLow = panelType; break;
                case AnchorFace.YLow: PanelTypeYLow = panelType; break;
                case AnchorFace.XLow: PanelTypeXLow = panelType; break;
                case AnchorFace.ZHigh: PanelTypeZHigh = panelType; break;
                case AnchorFace.YHigh: PanelTypeYHigh = panelType; break;
                case AnchorFace.XHigh: PanelTypeXHigh = panelType; break;
            }
        }
        #endregion

        #region public TriangleCorner GetPanelCorner(AnchorFace face)
        /// <summary>corner to which an LFramePanel snaps</summary>
        public TriangleCorner GetPanelCorner(AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.ZLow: return CornerZLow;
                case AnchorFace.YLow: return CornerYLow;
                case AnchorFace.XLow: return CornerXLow;
                case AnchorFace.ZHigh: return CornerZHigh;
                case AnchorFace.YHigh: return CornerYHigh;
                case AnchorFace.XHigh: return CornerXHigh;
            }
            return TriangleCorner.LowerLeft;
        }
        #endregion

        #region public void SetPanelCorner(AnchorFace face, TriangleCorner corner)
        public void SetPanelCorner(AnchorFace face, TriangleCorner corner)
        {
            switch (face)
            {
                case AnchorFace.ZLow: CornerZLow = corner; break;
                case AnchorFace.YLow: CornerYLow = corner; break;
                case AnchorFace.XLow: CornerXLow = corner; break;
                case AnchorFace.ZHigh: CornerZHigh = corner; break;
                case AnchorFace.YHigh: CornerYHigh = corner; break;
                case AnchorFace.XHigh: CornerXHigh = corner; break;
            }
            return;
        }
        #endregion

        #region public FaceEdge GetPanelEdge(AnchorFace face)
        /// <summary>edge to which a CornerPanel snaps</summary>
        public FaceEdge GetPanelEdge(AnchorFace face)
        {
            switch (face)
            {
                case AnchorFace.ZLow: return EdgeZLow;
                case AnchorFace.YLow: return EdgeYLow;
                case AnchorFace.XLow: return EdgeXLow;
                case AnchorFace.ZHigh: return EdgeZHigh;
                case AnchorFace.YHigh: return EdgeYHigh;
                case AnchorFace.XHigh: return EdgeXHigh;
            }
            return FaceEdge.Bottom;
        }
        #endregion

        #region public void SetPanelEdge(AnchorFace face, FaceEdge edge)
        public void SetPanelEdge(AnchorFace face, FaceEdge edge)
        {
            switch (face)
            {
                case AnchorFace.ZLow: EdgeZLow = edge; break;
                case AnchorFace.YLow: EdgeYLow = edge; break;
                case AnchorFace.XLow: EdgeXLow = edge; break;
                case AnchorFace.ZHigh: EdgeZHigh = edge; break;
                case AnchorFace.YHigh: EdgeYHigh = edge; break;
                case AnchorFace.XHigh: EdgeXHigh = edge; break;
            }
            return;
        }
        #endregion

        #region public AnchorFaceList BendControls { get; }
        /// <summary>
        /// Source, Sink, OtherSink
        /// </summary>
        public AnchorFaceList BendControls
        {
            get
            {
                // all panels must be at least partially binable, and panel interior is set for diagonal
                if (IsInteriorBindable && (PanelInterior == PanelInterior.Bend))
                {
                    if (OtherFace != OptionalAnchorFace.None)
                    {
                        return AnchorFaceListHelper.Create(SourceFace, SinkFace, OtherFace.ToAnchorFace());
                    }
                }
                return AnchorFaceList.None;
            }
        }
        #endregion

        #region public bool IsFaceTriangularSink(AnchorFace face)
        /// <summary>True if the interior is a Bend, and this face represents the SinkFace or the OtherFace</summary>
        public bool IsFaceTriangularSink(AnchorFace face)
        {
            if (IsInteriorBindable && (PanelInterior == PanelInterior.Bend))
            {
                return (face == SinkFace) || (face == OtherFace.ToAnchorFace());
            }
            return false;
        }
        #endregion

        #region public bool IsFaceBendableSource(AnchorFace face)
        public bool IsFaceBendableSource(AnchorFace face)
        {
            if (IsInteriorBindable && (PanelInterior == PanelInterior.Bend))
            {
                return (face == SourceFace);
            }
            return false;
        }
        #endregion

        #region public AnchorFaceList TriangularSinkEdges(AnchorFace face)
        /// <summary>If face is a triangular sink, this lists the other faces making the edges</summary>
        public AnchorFaceList TriangularSinkEdges(AnchorFace face)
        {
            if (IsFaceTriangularSink(face))
            {
                // sinks are triangular if there are two sinks
                return AnchorFaceListHelper.Create(BendControls.ToAnchorFaces().Where(_c => _c != face));
            }
            return AnchorFaceList.None;
        }
        #endregion

        #region public AnchorFaceList DiagonalControls { get; }
        /// <summary>Sink, Source[, Source2]</summary>
        public AnchorFaceList DiagonalControls
        {
            get
            {
                // all panels must be at least partially binable, and panel interior is set for diagonal
                if (IsInteriorBindable && (PanelInterior == PanelInterior.Diagonal))
                {
                    var _list = AnchorFaceListHelper.Create(SinkFace, SourceFace);
                    if (OtherFace != OptionalAnchorFace.None)
                    {
                        _list = _list.Add(OtherFace.ToAnchorFace());
                    }

                    return _list;
                }
                return AnchorFaceList.None;
            }
        }
        #endregion

        #region public bool IsFaceDiagonalSide(AnchorFace face)
        /// <summary>True if the interior is a Diagonal and the AnchorFace should be a right triangle</summary>
        public bool IsFaceDiagonalSide(AnchorFace face)
        {
            // can only get a diagonal face with no other panel
            if (IsInteriorBindable && (PanelInterior == PanelInterior.Diagonal)
                && (GetPanelType(face) == PanelType.NoPanel))
            {
                // sides can only be triangular if there is one sink
                var _faceAxis = face.GetAxis();
                var _controls = DiagonalControls;
                if (_controls.Count() == 2)
                {
                    // all the control faces are not on the test face axis
                    return _controls.ToAnchorFaces().All(_c => _c.GetAxis() != _faceAxis);
                }
                else
                {
                    // no face match and the sink axis isn't the face axis
                    return _controls.ToAnchorFaces().All(_c => _c != face) && (SinkFace.GetAxis() != _faceAxis);
                }
            }
            return false;
        }
        #endregion

        #region public bool IsFaceDiagonalBinder(AnchorFace face)
        /// <summary>DiagonalControls contains face</summary>
        public bool IsFaceDiagonalBinder(AnchorFace face)
        {
            // Slope bottom can be used on any complete panel or no panel
            return DiagonalControls.Contains(face);
        }
        #endregion

        #region public AnchorFaceList DiagonalFaceControlFaces(AnchorFace face)
        /// <summary>Returns AnchorFaces for the diagonal controls only if the side contains a diagonal face</summary>
        public AnchorFaceList DiagonalFaceControlFaces(AnchorFace face)
        {
            // can only get a diagonal face with no other panel
            if (IsInteriorBindable && (PanelInterior == PanelInterior.Diagonal)
                && (GetPanelType(face) == PanelType.NoPanel))
            {
                // sides can only be triangular if there is one sink
                var _faceAxis = face.GetAxis();
                var _controls = DiagonalControls;
                switch (_controls.Count())
                {
                    case 2:
                        // with 1 source and 1 sink, only the off-axis faces are diagonal
                        if (_controls.ToAnchorFaces().All(_c => _c.GetAxis() != _faceAxis))
                        {
                            // so return all control faces
                            return _controls;
                        }
                        break;
                    case 3:
                        // with 2 sources and 1 sink, 3 faces excluded by being controls, 1 by being on same axis as a sink
                        if (_controls.ToAnchorFaces().All(_c => _c != face)
                            && (SinkFace.GetAxis() != _faceAxis))
                        {
                            // return all control faces that are not on the same axis
                            return AnchorFaceListHelper.Create(_controls.ToAnchorFaces().Where(_c => _c.GetAxis() != _faceAxis));
                        }
                        break;
                }
            }
            return AnchorFaceList.None;
        }
        #endregion

        #region public bool IsTrueSlope { get; }
        public bool IsTrueSlope
        {
            get
            {
                // all panels must be at least partially bindable, and panel interior is set for diagonal
                return (IsInteriorBindable && (PanelInterior == PanelInterior.Slope));
            }
        }
        #endregion

        #region public bool IsFaceSlopeSide(AnchorFace face)
        public bool IsFaceSlopeSide(AnchorFace face)
        {
            // only the off-axis faces are slope sides
            if (IsTrueSlope)
            {
                var _faceAxis = face.GetAxis();
                return _faceAxis != SinkFace.GetAxis() && _faceAxis != SourceFace.GetAxis();
            }
            return false;
        }
        #endregion

        #region public bool IsFaceSlopeEnd(AnchorFace face)
        public bool IsFaceSlopeEnd(AnchorFace face)
        {
            if (IsTrueSlope)
            {
                return SinkFace.GetAxis() == face.GetAxis();
            }
            return false;
        }
        #endregion

        #region public bool IsFaceSlopeBottom(AnchorFace face)
        /// <summary>Interior bindable, using slope interior, and face is SourceFace</summary>
        public bool IsFaceSlopeBottom(AnchorFace face)
        {
            // Slope bottom can be used on any complete panel or no panel
            if (IsTrueSlope)
            {
                return face == SourceFace;
            }

            return false;
        }
        #endregion

        public TriangleCorner? GetDiagonalGrip(AnchorFace face)
        {
            // TODO: generalize this?
            TriangleCorner _cornerFromFaceList(AnchorFaceList controls)
            {
                switch (face.GetAxis())
                {
                    case Axis.Z:    // X L-R; Y U-D
                        return controls.Contains(AnchorFace.XHigh)
                            ? (controls.Contains(AnchorFace.YHigh) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight)
                            : (controls.Contains(AnchorFace.YHigh) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft);

                    case Axis.Y:    // Z L-R; X U-D
                        return controls.Contains(AnchorFace.ZHigh)
                            ? (controls.Contains(AnchorFace.XHigh) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight)
                            : (controls.Contains(AnchorFace.XHigh) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft);

                    case Axis.X:    // Y L-R; Z U-D
                    default:
                        return controls.Contains(AnchorFace.YHigh)
                            ? (controls.Contains(AnchorFace.ZHigh) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight)
                            : (controls.Contains(AnchorFace.ZHigh) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft);
                }
            }
            if (IsInteriorBindable)
            {
                if ((PanelInterior == PanelInterior.Diagonal)
                    && (GetPanelType(face) == PanelType.NoPanel))
                {
                    var _faceAxis = face.GetAxis();
                    var _controls = DiagonalControls;
                    if ((_controls.Count() == 2) && _controls.ToAnchorFaces().All(_c => _c.GetAxis() != _faceAxis))
                    {
                        return _cornerFromFaceList(_controls);
                    }
                    else if (_controls.ToAnchorFaces().All(_c => _c != face) && (SinkFace.GetAxis() != _faceAxis))
                    {
                        // remove co-axial face so only other controls contribute to corner
                        _controls = _controls.Remove(face.ReverseFace());
                        return _cornerFromFaceList(_controls);
                    }
                }
                else if ((PanelInterior == PanelInterior.Bend)
                    && (GetPanelType(face) == PanelType.NoPanel))
                {
                    if ((face == SinkFace) || (face == OtherFace.ToAnchorFace()))
                    {
                        // remove face so only other controls contribute to corner
                        var _controls = BendControls.Remove(face);
                        return _cornerFromFaceList(_controls);
                    }
                }
            }
            return null;
        }
        // TODO: other parameter shortcuts?
    }
}