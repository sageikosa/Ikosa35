using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class PanelCellSpace : CellSpace, IPanelCellSpace
    {
        #region construction
        public PanelCellSpace(CellMaterial fillMaterial, TileSet fillTiling,
            SolidCellMaterial diagonalMaterial, TileSet diagonalTiling)
            : base(fillMaterial, fillTiling)
        {
            _Panels1 = new PanelSet<NormalPanel>();
            _Panels2 = new PanelSet<NormalPanel>();
            _Panels3 = new PanelSet<NormalPanel>();
            _Corners = new PanelSet<CornerPanel>();
            _LFrames = new PanelSet<LFramePanel>();
            _Slopes = new List<SlopeComposite>();
            _Fill0 = new MaterialFill(@"Fill", fillMaterial, fillTiling);
            _Diagonal = new DiagonalComposite(@"Diagonal", diagonalMaterial, diagonalTiling);
        }
        #endregion

        #region private data
        private PanelSet<NormalPanel> _Panels1;
        private PanelSet<NormalPanel> _Panels2;
        private PanelSet<NormalPanel> _Panels3;
        private PanelSet<CornerPanel> _Corners;
        private PanelSet<LFramePanel> _LFrames;
        private List<SlopeComposite> _Slopes;
        private DiagonalComposite _Diagonal;
        private MaterialFill _Fill0;
        private MaterialFill _Fill1;
        private MaterialFill _Fill2;
        private MaterialFill _Fill3;
        private static readonly AnchorFace[] _AllFaces = new AnchorFace[]
        {
            AnchorFace.ZLow, AnchorFace.ZHigh,
            AnchorFace.YLow, AnchorFace.YHigh,
            AnchorFace.XLow, AnchorFace.XHigh
        };
        #endregion

        public static IEnumerable<AnchorFace> AllFaces => _AllFaces.Select(_f => _f);

        public PanelSet<NormalPanel> Panel1s => _Panels1;
        public PanelSet<NormalPanel> Panel2s => _Panels2;
        public PanelSet<NormalPanel> Panel3s => _Panels3;
        public PanelSet<CornerPanel> Corners => _Corners;
        public PanelSet<LFramePanel> LFrames => _LFrames;
        public List<SlopeComposite> Slopes => _Slopes;

        public override string GetDescription(uint param)
            => $@"Pnl:{Name} ({CellMaterialName};{TilingName})";

        protected SlopeComposite GetSlopeComposite(byte index)
        {
            if (Slopes.Count > index)
            {
                return Slopes[index];
            }
            return Slopes.FirstOrDefault();
        }

        #region Fill and Diagonal
        #region sync Fill0
        protected override void OnCellMaterialChanged()
        {
            _Fill0 = new MaterialFill(@"Fill", CellMaterial, Tiling);
            DoPropertyChanged(@"Fill0");
        }

        protected override void OnTilingChanged()
        {
            _Fill0 = new MaterialFill(@"Fill", CellMaterial, Tiling);
            DoPropertyChanged(@"Fill0");
        }
        #endregion

        public MaterialFill Fill0 { get { return _Fill0; } }
        public MaterialFill Fill1
        {
            get { return _Fill1; }
            set { if (value != null) { _Fill1 = value; DoPropertyChanged(@"Fill1"); } }
        }
        public MaterialFill Fill2
        {
            get { return _Fill2; }
            set { if (value != null) { _Fill2 = value; DoPropertyChanged(@"Fill2"); } }
        }
        public MaterialFill Fill3
        {
            get { return _Fill3; }
            set { if (value != null) { _Fill3 = value; DoPropertyChanged(@"Fill3"); } }
        }
        public DiagonalComposite Diagonal
        {
            get { return _Diagonal; }
            set { if (value != null) { _Diagonal = value; DoPropertyChanged(@"Diagonal"); } }
        }
        #endregion

        #region public MaterialFill GetFill(PanelParams param)
        public MaterialFill GetFill(PanelParams param)
        {
            switch (param.PanelFill)
            {
                case PanelFill.Fill1:
                    return Fill1 ?? Fill0;
                case PanelFill.Fill2:
                    return Fill2 ?? Fill0;
                case PanelFill.Fill3:
                    return Fill3 ?? Fill0;
            }

            // use base cell material
            return Fill0;
        }
        #endregion

        #region public BaseNaturalPanel GetNaturalPanel(PanelParams param, AnchorFace panelFace)
        /// <summary>
        /// Supplies one of these panels: 
        /// {NormalPanel} | {CornerPanel} | {LFramePanel} | {NULL}
        /// </summary>
        public BaseNaturalPanel GetNaturalPanel(PanelParams param, AnchorFace panelFace)
        {
            var _pType = param.GetPanelType(panelFace);
            switch (_pType)
            {
                case PanelType.Corner:
                case PanelType.MaskedCorner:
                    #region corner
                    // corners
                    if (Corners[panelFace] != null)
                        return Corners[panelFace];
                    else
                        return Panel1s[panelFace];
                #endregion

                case PanelType.LFrame:
                case PanelType.MaskedLFrame:
                    #region LFrames
                    // lframes
                    if (LFrames[panelFace] != null)
                        return LFrames[panelFace];
                    else
                        return Panel1s[panelFace];
                #endregion

                case PanelType.Panel1:
                case PanelType.Panel2:
                case PanelType.Panel3:
                    // normal panels
                    if (param.IsFaceSlopeBottom(panelFace) && Slopes.Any())
                    {
                        // actually a slope bottom
                        return null;
                    }
                    else
                    {
                        // normal panel
                        switch (_pType)
                        {
                            case PanelType.Panel1:
                                return Panel1s[panelFace];
                            case PanelType.Panel2:
                                return Panel2s[panelFace] ?? Panel1s[panelFace];
                            case PanelType.Panel3:
                            default:
                                return Panel3s[panelFace] ?? Panel1s[panelFace];
                        }
                    }
            }
            return null;
        }
        #endregion

        #region public IEnumerable<BasePanel> GetFacePanels(PanelParams param, AnchorFace panelFace)
        /// <summary>
        /// Supplies one of these sets: 
        /// {NormalPanel} | {CornerPanel, MaterialFill} | {LFramePanel, MaterialFill} | {SlopeComposite} 
        /// | {DiagonalComposite} | {MaterialFill, DiagonalComposite} | {MaterialFill, SlopeComposite} 
        /// | {MaterialFill}
        /// </summary>
        public IEnumerable<BasePanel> GetFacePanels(PanelParams param, AnchorFace panelFace)
        {
            var _pType = param.GetPanelType(panelFace);
            switch (_pType)
            {
                case PanelType.Corner:
                case PanelType.MaskedCorner:
                    #region corner
                    // corners
                    if (Corners[panelFace] != null)
                    {
                        yield return Corners[panelFace];
                        yield return GetFill(param);
                    }
                    else
                    {
                        yield return Panel1s[panelFace];
                    }
                    break;
                #endregion

                case PanelType.LFrame:
                case PanelType.MaskedLFrame:
                    #region LFrames
                    // lframes
                    if (LFrames[panelFace] != null)
                    {
                        yield return LFrames[panelFace];
                        yield return GetFill(param);
                    }
                    else
                    {
                        yield return Panel1s[panelFace];
                    }
                    break;
                #endregion

                case PanelType.Panel1:
                case PanelType.Panel2:
                case PanelType.Panel3:
                    // normal panels
                    if (param.IsFaceSlopeBottom(panelFace) && Slopes.Any())
                    {
                        // rectangular "panel" made of slope fill (bottom of a slope)
                        yield return GetSlopeComposite(param.SlopeIndex);
                    }
                    else
                    {
                        // normal panel
                        switch (_pType)
                        {
                            case PanelType.Panel1:
                                yield return Panel1s[panelFace];
                                break;
                            case PanelType.Panel2:
                                yield return Panel2s[panelFace] ?? Panel1s[panelFace];
                                break;
                            case PanelType.Panel3:
                                yield return Panel3s[panelFace] ?? Panel1s[panelFace];
                                break;
                        }
                    }
                    break;

                case PanelType.NoPanel:
                    {
                        if (param.IsFaceSlopeBottom(panelFace) && Slopes.Any())
                        {
                            // rectangular "panel" made of slope fill (bottom of a slope)
                            yield return GetSlopeComposite(param.SlopeIndex);
                        }
                        else
                        {
                            // no panel
                            if ((param.IsFaceDiagonalBinder(panelFace) || param.IsFaceBendableSource(panelFace))
                                && (Diagonal != null))
                            {
                                // rectangular "panel" made of diagonal fill
                                yield return Diagonal;
                            }
                            else if ((param.IsFaceDiagonalSide(panelFace) || param.IsFaceTriangularSink(panelFace))
                                && (Diagonal != null))
                            {
                                // "panel" made of half material fill and half diagonal fill
                                yield return GetFill(param);
                                yield return Diagonal;
                            }
                            else if ((param.IsFaceSlopeSide(panelFace) || param.IsFaceSlopeEnd(panelFace))
                                 && Slopes.Any())
                            {
                                // slope side: partially made of slope fill, and partially of material fill
                                yield return GetFill(param);
                                yield return GetSlopeComposite(param.SlopeIndex);
                            }
                            else
                            {
                                // no panel, so whatever the material fill is...
                                yield return GetFill(param);
                            }
                        }
                    }
                    break;
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<BasePanel> GetInteriors(PanelParams param)
        /// <summary>Yields MaterialFill [and {DiagonalFill|SlopeFill}] as appropriate</summary>
        public IEnumerable<BasePanel> GetInteriors(PanelParams param)
        {
            if (param.IsInteriorBindable)
            {
                var _diagCtrls = param.DiagonalControls;
                if ((_diagCtrls != AnchorFaceList.None) && (Diagonal != null))
                {
                    yield return GetFill(param);
                    yield return Diagonal;
                }
                else
                {
                    var _bendCtrls = param.BendControls;
                    if ((_bendCtrls != AnchorFaceList.None) && (Diagonal != null))
                    {
                        yield return GetFill(param);
                        yield return Diagonal;
                    }
                    else
                    {
                        if (param.IsTrueSlope && Slopes.Any())
                        {
                            yield return GetFill(param);
                            yield return GetSlopeComposite(param.SlopeIndex);
                        }
                        else
                        {
                            yield return GetFill(param);
                        }
                    }
                }
            }
            else
            {
                yield return GetFill(param);
            }
            yield break;
        }
        #endregion

        // drawing and shading

        #region public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            var _param = new PanelParams(param);

            // get interiors
            var _interiors = GetInteriors(_param).ToList();

            // get natural sides
            var _sides = (from _af in _AllFaces
                          let _p = GetNaturalPanel(_param, _af)
                          where _p != null
                          select new { Panel = _p, Face = _af }).ToDictionary(_x => _x.Face, _y => _y.Panel as INaturalPanel);

            // draw innards of interiors
            foreach (var _i in _interiors.OfType<BaseComposite>())
                _i.AddInnerStructures(_param, addToGroup, z, y, x, effect, _sides, _interiors);

            // draw innards of natural sides
            foreach (var _side in _sides)
                _side.Value.AddInnerStructures(_param, _side.Key, addToGroup, z, y, x, effect, _interiors);
        }
        #endregion

        #region public override void AddOuterSurface(uint param, BuildablePair pair, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        public override void AddOuterSurface(uint param, BuildableGroup pair, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            var _param = new PanelParams(param);
            var _panels = GetFacePanels(_param, face).ToList();
            foreach (var _p in _panels)
                _p.AddOuterSurface(_param, pair, z, y, x, face, face, effect, bump, _panels);

            // for any side face not occluded by the panel faces, must draw natural side face sides...
            foreach (var _of in face.GetOrthoFaces())
                if (_panels.All(_p => !_p.OrthoOcclusion(_param, face, _of)))
                {
                    var _natSide = GetNaturalPanel(_param, _of);
                    if (_natSide != null)
                    {
                        _natSide.AddOuterSurface(_param, pair, z, y, x, _of, face, effect, bump, null);
                    }
                }
        }
        #endregion

        public override bool IsShadeable(uint param)
        {
            return true;
        }

        #region public override bool? ShowFace(uint param, AnchorFace outwardFace)
        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
        {
            var _param = new PanelParams(param);

            // any face is visible
            if (GetFacePanels(_param, outwardFace).Any(_p => !_p.IsInvisible))
                return true;

            // any edged face is visible
            return ((from _ortho in outwardFace.GetOrthoFaces()
                     let _nat = GetNaturalPanel(_param, _ortho)
                     where _nat != null
                     select _nat).Any());

        }
        #endregion

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
        {
            var _param = new PanelParams(param);
            return GetFacePanels(_param, outwardFace).All(_p => !_p.IsInvisible);
        }

        // movement

        #region public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
        public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
        {
            var _param = new PanelParams(param);

            // all interiors blocked (which would include surface slopes)
            if (GetInteriors(_param).All(_i => !movement.CanMoveThrough(_i.Material)))
                return true;

            var _faces = snap.ToFaceList();
            if (_faces.Any())
            {
                // can look at panels
                return _faces.Any(_f => GetFacePanels(_param, _f).Any(_bp => _bp.BlockedAt(_param, _f, movement, _faces)));
            }
            else
            {
                // no blocking
                return false;
            }
        }
        #endregion

        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            var _param = new PanelParams(param);

            // all interiors blocked (which would include surface slopes)
            if (GetInteriors(_param).All(_i => !movement.CanMoveThrough(_i.Material)))
                return new HedralGrip(true);

            // union...
            var _panels = GetFacePanels(_param, surfaceFace);
            var _grip = new HedralGrip(false);
            foreach (var _p in _panels)
                _grip = _grip.Union(_p.HedralGripping(_param, surfaceFace, movement, _panels));
            return _grip;
        }

        #region public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        {
            var _param = new PanelParams(param);

            // does top panel have surface gaps?
            var _surfaces = GetFacePanels(_param, upDirection).ToList();
            if (_surfaces.Any(_s => movement.CanMoveThrough(_s.Material)))
            {
                // top panel was permeable, so inner structures are allowed

                #region always provides a top thickness offset segment
                // provides a top thickness segment
                Func<SlopeSegment> _topSlope = () =>
                {
                    // drop past the permeable top panel
                    var _offset = _surfaces.First().AverageThickness;
                    if (!upDirection.IsLowFace())
                        _offset = 5d - _offset;
                    return new SlopeSegment
                    {
                        Low = _offset + baseElev,
                        High = _offset + baseElev,
                        Run = 5
                    };
                };
                #endregion

                #region sometimes provides a bottom thickness offset segment
                Func<SlopeSegment?> _bottomSlope = () =>
                {
                    // must have a bottom panel with non-zero thickness that blocks movement
                    var _bottoms = GetFacePanels(_param, upDirection.ReverseFace()).ToList();
                    var _good = _bottoms.FirstOrDefault(_b => _b.Thickness > 0 && !movement.CanMoveThrough(_b.Material));
                    if (_good != null)
                    {
                        var _offset = _good.AverageThickness;
                        if (upDirection.IsLowFace())
                        {
                            // therefore, bottom face is at cell top
                            _offset = 5d - _offset;
                        }
                        return new SlopeSegment
                        {
                            Low = _offset + baseElev,
                            High = _offset + baseElev,
                            Run = 5
                        };
                    }
                    return null;
                };
                #endregion

                // get fill
                var _fill = GetFill(_param);

                // check for diagonals
                var _diagCtrls = _param.DiagonalControls;
                if ((_diagCtrls != AnchorFaceList.None) && (Diagonal != null))
                {
                    #region diagonals
                    // possible to move through the interior
                    var _diagBlock = !movement.CanMoveThrough(Diagonal.Material);
                    var _fillBlock = !movement.CanMoveThrough(_fill.Material);
                    if (_diagBlock && _fillBlock)
                    {
                        // no: so top slope only
                        yield return _topSlope();
                    }
                    else
                    {
                        // get sink face
                        var _sink = _param.SinkFace;
                        if (_param.OtherFace == OptionalAnchorFace.None)
                        {
                            #region 1 sink and 1 source
                            // get source face
                            var _source = _param.SourceFace;

                            if ((upDirection.GetAxis() == _sink.GetAxis()) || (upDirection.GetAxis() == _source.GetAxis()))
                            {
                                // order the materials
                                var _first = ((upDirection == _sink) || (upDirection == _source))
                                    ? Diagonal.Material
                                    : _fill.Material;
                                var _second = ((upDirection == _sink) || (upDirection == _source))
                                    ? _fill.Material
                                    : Diagonal.Material;
                                if (movement.CanMoveThrough(_first))
                                {
                                    if (movement.CanMoveThrough(_second))
                                    {
                                        // bottom panel thickness...(if valid bottom panel)
                                        var _bSlope = _bottomSlope();
                                        if (_bSlope.HasValue)
                                            yield return _bSlope.Value;
                                    }
                                    else
                                    {
                                        // full diagonal slope
                                        yield return new SlopeSegment
                                        {
                                            Low = baseElev,
                                            High = 5 + baseElev,
                                            Run = 5
                                        };
                                    }
                                }
                                else
                                {
                                    // top panel thickness...
                                    yield return _topSlope();
                                }
                            }
                            else // sides
                            {
                                // move through one of the materials
                                var _dBlock = !movement.CanMoveThrough(Diagonal.Material);
                                var _fBlock = !movement.CanMoveThrough(_fill.Material);
                                if (_dBlock || _fBlock)
                                {
                                    // top panel thickness...
                                    yield return _topSlope();
                                }
                                if (!_dBlock || !_fBlock)
                                {
                                    // bottom panel thickness...
                                    var _bSlope = _bottomSlope();
                                    if (_bSlope.HasValue)
                                        yield return _bSlope.Value;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region 1 sink and 2 sources
                            if (upDirection.GetAxis() == _sink.GetAxis())
                            {
                                // first material blocking...
                                if ((upDirection == _sink)
                                    ? !movement.CanMoveThrough(Diagonal.Material)
                                    : !movement.CanMoveThrough(_fill.Material))
                                {
                                    // ...top only
                                    yield return _topSlope();
                                }
                                else
                                {
                                    // second material blocking?
                                    if ((upDirection == _sink)
                                        ? !movement.CanMoveThrough(_fill.Material)
                                        : !movement.CanMoveThrough(Diagonal.Material))
                                    {
                                        // full diagonal slope
                                        yield return new SlopeSegment
                                        {
                                            Low = baseElev,
                                            High = 5 + baseElev,
                                            Run = 5
                                        };
                                    }
                                    else
                                    {
                                        // bottom panel thickness...
                                        var _bSlope = _bottomSlope();
                                        if (_bSlope.HasValue)
                                            yield return _bSlope.Value;
                                    }
                                }
                            }
                            else
                            {
                                // if one material blocks, then inner structures
                                if (_diagBlock || _fillBlock)
                                {
                                    // side or back of diagonal
                                    yield return _topSlope();

                                    // touching a control face?
                                    if (_diagCtrls.Contains(upDirection)
                                        ? _fillBlock    // yes: slope if fill is the blocker
                                        : _diagBlock)   // no: slope if diagonal is the blocker
                                    {
                                        // full diagonal slope
                                        yield return new SlopeSegment
                                        {
                                            Low = baseElev,
                                            High = 5 + baseElev,
                                            Run = 5
                                        };
                                    }
                                }
                                else
                                {
                                    // bottom panel thickness...
                                    var _bSlope = _bottomSlope();
                                    if (_bSlope.HasValue)
                                        yield return _bSlope.Value;
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                {
                    var _bends = _param.BendControls;
                    if ((_bends != AnchorFaceList.None) && (Diagonal != null))
                    {
                        #region bend
                        // get source face
                        var _source = _param.SourceFace;
                        var _diagBlock = !movement.CanMoveThrough(Diagonal.Material);
                        var _fillBlock = !movement.CanMoveThrough(_fill.Material);
                        if (upDirection.GetAxis() == _source.GetAxis())
                        {
                            #region source axis
                            // first material blocking...
                            if ((upDirection == _source) ? _diagBlock : _fillBlock)
                            {
                                // ...top only
                                yield return _topSlope();
                            }
                            else
                            {
                                // second material blocking?
                                if ((upDirection == _source) ? _fillBlock : _diagBlock)
                                {
                                    // full diagonal slope
                                    yield return new SlopeSegment
                                    {
                                        Low = baseElev,
                                        High = 5 + baseElev,
                                        Run = 5
                                    };
                                }
                                else
                                {
                                    // bottom panel thickness...
                                    var _bSlope = _bottomSlope();
                                    if (_bSlope.HasValue)
                                        yield return _bSlope.Value;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region sink axes
                            var _checkBottom = false;

                            // sink axis
                            if (_diagBlock && !_fillBlock)
                            {
                                // intended diagonal blockage
                                _checkBottom = true;
                                if (_bends.Contains(upDirection))
                                    yield return _topSlope();
                                else
                                    yield return new SlopeSegment
                                    {
                                        Low = baseElev,
                                        High = 5 + baseElev,
                                        Run = 5
                                    };
                            }
                            else if (!_diagBlock && _fillBlock)
                            {
                                // inverted blockage
                                if (_bends.Contains(upDirection))
                                {
                                    yield return _topSlope();
                                    yield return new SlopeSegment
                                    {
                                        Low = baseElev,
                                        High = 5 + baseElev,
                                        Run = 5
                                    };
                                }
                                else
                                {
                                    yield return _topSlope();
                                }
                            }
                            else
                            {
                                // neither blocks
                                _checkBottom = true;
                            }

                            if (_checkBottom)
                            {
                                // bottom panel thickness...
                                var _bSlope = _bottomSlope();
                                if (_bSlope.HasValue)
                                    yield return _bSlope.Value;
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        // get slope face
                        var _slope = _param.SourceFace;
                        if (_param.IsTrueSlope && Slopes.Any())
                        {
                            var _slopePanel = GetSlopeComposite(_param.SlopeIndex);
                            if (upDirection == _slope)
                            {
                                #region slope face itself
                                // NOTE: slope panel would have blocked or passed already as facePanel
                                if (!movement.CanMoveThrough(_fill.Material))
                                {
                                    // blocking fill with permeable slope-panel
                                    var _lo = upDirection.IsLowFace() ? _slopePanel.LesserThickness : 5 - _slopePanel.GreaterThickness;
                                    var _hi = upDirection.IsLowFace() ? _slopePanel.GreaterThickness : 5 - _slopePanel.LesserThickness;
                                    yield return new SlopeSegment
                                    {
                                        Low = _lo + baseElev,
                                        High = _hi + baseElev,
                                        Run = 5
                                    };
                                }
                                else
                                {
                                    // bottom panel thickness...
                                    var _bSlope = _bottomSlope();
                                    if (_bSlope.HasValue)
                                        yield return _bSlope.Value;
                                }
                                #endregion
                            }
                            else if (upDirection == _slope.ReverseFace())
                            {
                                #region reverse of slope face
                                if (!movement.CanMoveThrough(_fill.Material))
                                {
                                    // top panel thickness
                                    yield return _topSlope();
                                }
                                else
                                {
                                    if (!movement.CanMoveThrough(_slopePanel.Material))
                                    {
                                        // blocking slope-panel
                                        var _lo = !upDirection.IsLowFace() ? _slopePanel.LesserThickness : 5 - _slopePanel.GreaterThickness;
                                        var _hi = !upDirection.IsLowFace() ? _slopePanel.GreaterThickness : 5 - _slopePanel.LesserThickness;
                                        yield return new SlopeSegment
                                        {
                                            Low = _lo + baseElev,
                                            High = _hi + baseElev,
                                            Run = 5
                                        };
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                #region any sides of slope
                                // get sink face
                                var _sink = _param.SinkFace;

                                // side faces
                                var _sBlock = !movement.CanMoveThrough(_slopePanel.Material);
                                var _fBlock = !movement.CanMoveThrough(_fill.Material);

                                // at least one material blocks?
                                if (_sBlock || _fBlock)
                                {
                                    // either material blocks
                                    yield return _topSlope();
                                }

                                // only one material blocks?
                                if (_sBlock ^ _fBlock)
                                {
                                    if (upDirection == _sink)
                                    {
                                        // the sink would be the high end, so would overhang
                                        if (_fBlock)
                                        {
                                            yield return new SlopeSegment
                                            {
                                                Low = baseElev,
                                                High = 5 + baseElev,
                                                Run = Math.Abs(_slopePanel.GreaterThickness - _slopePanel.LesserThickness)
                                            };
                                        }
                                    }
                                    else if (upDirection == _sink.ReverseFace())
                                    {
                                        // the fill would be the high end, so would overhang
                                        if (_sBlock)
                                        {
                                            yield return new SlopeSegment
                                            {
                                                Low = baseElev,
                                                High = 5 + baseElev,
                                                Run = Math.Abs(_slopePanel.GreaterThickness - _slopePanel.LesserThickness)
                                            };
                                        }
                                    }
                                }

                                // at least one material doesn't block?
                                if (!_sBlock || !_fBlock)
                                {
                                    // bottom panel thickness...
                                    var _bSlope = _bottomSlope();
                                    if (_bSlope.HasValue)
                                        yield return _bSlope.Value;
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            // regular old material fill
                            if (!movement.CanMoveThrough(_fill.Material))
                                yield return _topSlope();
                            else
                            {
                                // bottom panel thickness...
                                var _bSlope = _bottomSlope();
                                if (_bSlope.HasValue)
                                    yield return _bSlope.Value;
                            }
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        {
            // TODO: refine for gravity (especially inner materials)
            var _param = new PanelParams(param);
            var _fill = GetFill(_param);
            var _fillBlock = !movement.CanMoveThrough(_fill.Material);

            // NOTE: cannot open towards any blocked face
            var _diagCtrls = _param.DiagonalControls;
            if ((_diagCtrls != AnchorFaceList.None) && (Diagonal != null))
            {
                #region diagonal blocks
                var _diagBlock = !movement.CanMoveThrough(Diagonal.Material);
                if (_diagBlock && _fillBlock)
                {
                    // no openings
                }
                else
                {
                    // natural panels
                    var _pBlocks = (from _af in _AllFaces
                                    let _np = GetNaturalPanel(_param, _af)
                                    select new
                                    {
                                        Face = _af,
                                        RevFace = _af.ReverseFace(),
                                        Thickness = (_np != null) ? _np.Thickness : 0,
                                        Blocks = (_np != null) ? !movement.CanMoveThrough(_np.Material) : false
                                    }).ToDictionary(_i => _i.Face);
                    if (!_diagBlock && !_fillBlock)
                    {
                        #region natural panel blockages
                        foreach (var _b in _pBlocks)
                        {
                            // face blocks, but reverse does not
                            if (_b.Value.Blocks && !_pBlocks[_b.Value.RevFace].Blocks)
                            {
                                yield return new MovementOpening(_b.Value.RevFace, 5 - _b.Value.Thickness, 1);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        // get sink face
                        var _sink = _param.SinkFace;
                        var _source = _param.SourceFace;
                        if (_param.OtherFace == OptionalAnchorFace.None)
                        {
                            #region single source and sink
                            // single source/sink
                            if (_diagBlock && !_fillBlock)
                            {
                                if (!_pBlocks[_source.ReverseFace()].Blocks)
                                    yield return new MovementOpening(_source.ReverseFace(), 2.5d, 1d);
                                if (!_pBlocks[_sink.ReverseFace()].Blocks)
                                    yield return new MovementOpening(_sink.ReverseFace(), 2.5d, 1d);
                            }
                            else if (!_diagBlock && _fillBlock)
                            {
                                if (!_pBlocks[_source].Blocks)
                                    yield return new MovementOpening(_source, 2.5d, 1d);
                                if (!_pBlocks[_sink].Blocks)
                                    yield return new MovementOpening(_sink, 2.5d, 1d);
                            }

                            // sides (panel blocks)
                            foreach (var _side in from _af in _AllFaces
                                                  let _afAxis = _af.GetAxis()
                                                  where (_afAxis != _source.GetAxis()) && (_afAxis != _sink.GetAxis())
                                                  select _af)
                            {
                                var _rev = _pBlocks[_side.ReverseFace()];
                                if (!_pBlocks[_side].Blocks && _rev.Blocks)
                                    yield return new MovementOpening(_side, 5d - _rev.Thickness, 1d);
                            }
                            #endregion
                        }
                        else
                        {
                            #region double source and single sink
                            // double source, single sink
                            var _other = _param.OtherFace.ToAnchorFace();
                            if (_diagBlock && !_fillBlock)
                            {
                                if (!_pBlocks[_sink.ReverseFace()].Blocks)
                                    yield return new MovementOpening(_sink.ReverseFace(), 2.5d, 1d);
                                if (!_pBlocks[_source.ReverseFace()].Blocks)
                                    yield return new MovementOpening(_source.ReverseFace(), 2.5d, 1d);
                                if (!_pBlocks[_other.ReverseFace()].Blocks)
                                    yield return new MovementOpening(_other.ReverseFace(), 2.5d, 1d);
                            }
                            else if (!_diagBlock && _fillBlock)
                            {
                                if (!_pBlocks[_sink].Blocks)
                                    yield return new MovementOpening(_sink, 2.5d, 1d);

                                // source 1 axis
                                var _srcBlock = _pBlocks[_source];
                                var _revSrcBlock = _pBlocks[_source.ReverseFace()];
                                if (_srcBlock.Blocks && !_revSrcBlock.Blocks)
                                {
                                    yield return new MovementOpening(_source.ReverseFace(), 5 - _srcBlock.Thickness, 1d); // TODO: blockage= 0.5d?
                                }
                                else if (!_srcBlock.Blocks)
                                {
                                    // slope
                                    yield return new MovementOpening(_source, 2.5d, 1d);

                                    // opposite panel
                                    if (_revSrcBlock.Blocks)
                                        yield return new MovementOpening(_source, 5 - _revSrcBlock.Thickness, 1d);
                                }

                                // source 2 axis
                                var _etcBlock = _pBlocks[_other];
                                var _revEtcBlock = _pBlocks[_other.ReverseFace()];
                                if (_etcBlock.Blocks && !_revEtcBlock.Blocks)
                                {
                                    yield return new MovementOpening(_other.ReverseFace(), 5 - _etcBlock.Thickness, 1d); // TODO: blockage= 0.5d?
                                }
                                else if (!_etcBlock.Blocks)
                                {
                                    // slope
                                    yield return new MovementOpening(_other, 2.5d, 1d);

                                    // opposite panel
                                    if (_revEtcBlock.Blocks)
                                        yield return new MovementOpening(_other, 5 - _revEtcBlock.Thickness, 1d);
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion
            }
            else
            {
                var _bendCtrls = _param.BendControls;
                if ((_bendCtrls != AnchorFaceList.None) && (Diagonal != null))
                {
                    #region bend blocks
                    // bend
                    var _diagBlock = !movement.CanMoveThrough(Diagonal.Material);
                    if (_diagBlock && _fillBlock)
                    {
                        // no openings
                    }
                    else
                    {
                        // natural panels
                        var _pBlocks = (from _af in _AllFaces
                                        let _np = GetNaturalPanel(_param, _af)
                                        select new
                                        {
                                            Face = _af,
                                            RevFace = _af.ReverseFace(),
                                            Thickness = (_np != null) ? _np.Thickness : 0,
                                            Blocks = (_np != null) ? !movement.CanMoveThrough(_np.Material) : false
                                        }).ToDictionary(_i => _i.Face);
                        var _bSink = _param.SinkFace;
                        var _bSource = _param.SourceFace;
                        var _bOther = _param.OtherFace.ToAnchorFace();

                        // double source, single sink
                        if (!_diagBlock && _fillBlock)
                        {
                            if (!_pBlocks[_bSource].Blocks)
                                yield return new MovementOpening(_bSource, 2.5d, 1d);
                            if (!_pBlocks[_bSink].Blocks)
                                yield return new MovementOpening(_bSink, 2.5d, 1d);
                            if (!_pBlocks[_bOther].Blocks)
                                yield return new MovementOpening(_bOther, 2.5d, 1d);
                        }
                        else if (_diagBlock && !_fillBlock)
                        {
                            if (!_pBlocks[_bSource.ReverseFace()].Blocks)
                                yield return new MovementOpening(_bSource.ReverseFace(), 2.5d, 1d);

                            // sink 1 axis
                            var _snkBlock = _pBlocks[_bSink];
                            var _revSnkBlock = _pBlocks[_bSink.ReverseFace()];
                            if (_revSnkBlock.Blocks && !_snkBlock.Blocks)
                            {
                                yield return new MovementOpening(_bSink, 5 - _revSnkBlock.Thickness, 1d); // TODO: blockage= 0.5d?
                            }
                            else if (!_revSnkBlock.Blocks)
                            {
                                // slope
                                yield return new MovementOpening(_bSink.ReverseFace(), 2.5d, 1d);

                                // opposite panel
                                if (_snkBlock.Blocks)
                                    yield return new MovementOpening(_bSink.ReverseFace(), 5 - _snkBlock.Thickness, 1d);
                            }

                            // sink 2 axis
                            var _etcBlock = _pBlocks[_bOther];
                            var _revEtcBlock = _pBlocks[_bOther.ReverseFace()];
                            if (_revEtcBlock.Blocks && !_etcBlock.Blocks)
                            {
                                yield return new MovementOpening(_bOther, 5 - _revEtcBlock.Thickness, 1d); // TODO: blockage= 0.5d?
                            }
                            else if (!_revEtcBlock.Blocks)
                            {
                                // slope
                                yield return new MovementOpening(_bOther.ReverseFace(), 2.5d, 1d);

                                // opposite panel
                                if (_etcBlock.Blocks)
                                    yield return new MovementOpening(_bOther.ReverseFace(), 5 - _etcBlock.Thickness, 1d);
                            }
                        }
                        else
                        {
                            #region natural panel blockages
                            foreach (var _b in _pBlocks)
                            {
                                // face blocks, but reverse does not
                                if (_b.Value.Blocks && !_pBlocks[_b.Value.RevFace].Blocks)
                                {
                                    yield return new MovementOpening(_b.Value.RevFace, 5 - _b.Value.Thickness, 1);
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                else
                {
                    // get slope face
                    var _slope = _param.SourceFace;
                    var _slopePanel = GetSlopeComposite(_param.SlopeIndex);
                    if (_param.IsTrueSlope && _slopePanel != null)
                    {
                        #region slope in cell
                        // some kind of slope
                        var _sinkFace = _param.SinkFace;
                        var _slopeBlock = !movement.CanMoveThrough(_slopePanel.Material);

                        #region face blocking
                        var _faceBlocks = (from _f in _AllFaces.Where(_f => _f != _slope)
                                           let _fp = GetFacePanels(_param, _f).FirstOrDefault()
                                           where (_fp != null) && (_fp.Thickness > 0)
                                           select new
                                           {
                                               Face = _f,
                                               RevFace = _f.ReverseFace(),
                                               Thickness = _fp.Thickness,
                                               IsBlocking = !movement.CanMoveThrough(_fp.Material)
                                           }).ToDictionary(_b => _b.Face);
                        bool _isBlocking(AnchorFace face)
                            => _faceBlocks.Any(_fb => (_fb.Key == face) && _fb.Value.IsBlocking);
                        #endregion

                        if (!_slopeBlock)
                        {
                            #region not slope blocking
                            if (_fillBlock)
                                // fill blocks, so openings account for slope
                                yield return new MovementOpening(_slope, _slopePanel.AverageThickness, 1d);
                            if (!_isBlocking(_sinkFace))
                            {
                                // sink face allows openings
                                if (_fillBlock)
                                    yield return new MovementOpening(_sinkFace, 2.5d, (5 - _slopePanel.AverageThickness) / 5d);
                                if (_isBlocking(_sinkFace.ReverseFace()))
                                    yield return new MovementOpening(_sinkFace, 5 - _faceBlocks[_sinkFace.ReverseFace()].Thickness, 1d);
                            }
                            else if (!_isBlocking(_sinkFace.ReverseFace()))
                            {
                                // reverse sink face allows openings
                                yield return new MovementOpening(_sinkFace, 5 - _faceBlocks[_sinkFace].Thickness, 1d);
                            }
                            #endregion
                        }
                        else if (!_fillBlock)
                        {
                            #region slope blocking
                            // from blocking slope panel to top 
                            if (!_isBlocking(_slope.ReverseFace()))
                                yield return new MovementOpening(_slope.ReverseFace(), 5 - _slopePanel.AverageThickness, 1d);

                            // sink axis blocking
                            if (_isBlocking(_sinkFace.ReverseFace()))
                            {
                                if (!_isBlocking(_sinkFace))
                                    // reverse sink is blocking, but sink face itself isn't
                                    yield return new MovementOpening(_sinkFace, 5 - _faceBlocks[_sinkFace.ReverseFace()].Thickness, 1);
                            }
                            else
                            {
                                // slope away from sink
                                yield return new MovementOpening(_sinkFace.ReverseFace(), 2.5, _slopePanel.AverageThickness / 5);
                            }
                            if (_isBlocking(_sinkFace))
                                // sink face blocker
                                yield return new MovementOpening(_sinkFace.ReverseFace(), 5 - _faceBlocks[_sinkFace].Thickness, 1);
                            #endregion
                        }

                        if (!_slopeBlock || !_fillBlock)
                        {
                            #region side faces
                            // side faces
                            var _sideFaces = from _f in _AllFaces
                                             where _f.IsLowFace() && (_f.GetAxis() != _slope.GetAxis()) && (_f.GetAxis() != _sinkFace.GetAxis())
                                             select _f;
                            if (_sideFaces.Any())
                            {
                                var _low = _sideFaces.First();
                                var _high = _low.ReverseFace();
                                if (_isBlocking(_low))
                                {
                                    if (!_isBlocking(_high))
                                    {
                                        yield return new MovementOpening(_high, 5 - _faceBlocks[_low].Thickness, 1);
                                    }
                                }
                                else if (_isBlocking(_high))
                                {
                                    yield return new MovementOpening(_low, 5 - _faceBlocks[_high].Thickness, 1);
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        HedralGrip _union(List<BasePanel> panels, AnchorFace face)
                        {
                            var _grip = new HedralGrip(false);
                            foreach (var _bp in panels)
                                _grip = _grip.Union(_bp.HedralGripping(_param, face, movement, panels));
                            return _grip;
                        }

                        #region no special inner structure
                        // regular old material fill
                        var _faceBlocks = (from _f in _AllFaces
                                           let _fp = GetFacePanels(_param, _f).ToList()
                                           select new
                                           {
                                               Face = _f,
                                               RevFace = _f.ReverseFace(),
                                               Blockage = _union(_fp, _f).GripCount(),
                                               Thickness = _fp.Max(_p => _p.Thickness)
                                           }).ToDictionary(_b => _b.Face);

                        // only opens when inside is empty
                        if (!_fillBlock)
                        {
                            // blockage of face must be less than reversed face blockage
                            foreach (var _block in from _fb in _faceBlocks
                                                   let _val = _fb.Value
                                                   where _faceBlocks.ContainsKey(_val.RevFace)
                                                   let _revBlock = _faceBlocks[_val.RevFace]
                                                   where _val.Blockage < _revBlock.Blockage
                                                   select new
                                                   {
                                                       Face = _fb.Key,
                                                       Thickness = _revBlock.Thickness,
                                                       Blockage = _revBlock.Blockage
                                                   })
                            {
                                yield return new MovementOpening(_block.Face, 5d - _block.Thickness, _block.Blockage / 64d);
                            }
                        }
                        #endregion
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        {
            // all valid openings together
            var _vec = new Vector3D();
            foreach (var _open in OpensTowards(param, movement, gravity))
            {
                _vec += _open.OffsetVector3D;
            }
            return _vec;
        }
        #endregion

        #region private AnchorFaceList GetTacticalPointFaces(Point3D point)
        private AnchorFaceList GetTacticalPointFaces(Point3D point)
        {
            var _list = AnchorFaceList.None;
            if (point.X == 0)
                _list = _list.Add(AnchorFace.XLow);
            else if (point.X == 5)
                _list = _list.Add(AnchorFace.XHigh);
            if (point.Y == 0)
                _list = _list.Add(AnchorFace.YLow);
            else if (point.Y == 5)
                _list = _list.Add(AnchorFace.YHigh);
            if (point.Z == 0)
                _list = _list.Add(AnchorFace.ZLow);
            else if (point.Z == 5)
                _list = _list.Add(AnchorFace.ZHigh);
            return _list;
        }
        #endregion

        #region public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        {
            var _param = new PanelParams(param);
            var _fill = GetFill(_param);
            var _fillBlock = !movement.CanMoveThrough(_fill.Material);
            var _diagBlock = (Diagonal != null) ? !movement.CanMoveThrough(Diagonal.Material) : false;

            #region natural panel adjustments
            // natural panel adjustments
            var _pAdjusts = (from _af in _AllFaces
                             let _np = GetNaturalPanel(_param, _af)
                             select new
                             {
                                 Face = _af,
                                 Vector = _af.GetAxis().AxisVector(),
                                 Adjust = (_np != null) && !movement.CanMoveThrough(_np.Material)
                                    ? (_af.IsLowFace() ? _np.AverageThickness : -_np.AverageThickness)
                                    : 0
                             }).ToDictionary(_i => _i.Face);
            Func<double, Axis, OptionalAnchorFace> _optFace = (value, axis) =>
                {
                    // get optional face if on a cell boundary
                    if (value <= 0)
                        return axis.GetLowFace().ToOptionalAnchorFace();
                    if (value >= 5)
                        return axis.GetHighFace().ToOptionalAnchorFace();
                    return OptionalAnchorFace.None;
                };
            Func<Point3D, OptionalAnchorFace, Point3D> _adjustOne = (source, face) =>
                {
                    // adjust one
                    if (face != OptionalAnchorFace.None)
                    {
                        var _adj = _pAdjusts[face.ToAnchorFace()];
                        return source + (_adj.Vector * _adj.Adjust);
                    }
                    return source;
                };
            Func<Point3D, Point3D> _adjust
                = (source) =>
                {
                    // adjust all
                    return _adjustOne(_adjustOne(_adjustOne(source, _optFace(source.Z, Axis.Z)), _optFace(source.Y, Axis.Y)), _optFace(source.X, Axis.X));
                };
            #endregion

            // keeper function
            Func<Point3D, bool> _keep = (pt) => true;
            bool _onlyOne(AnchorFaceList faces, AnchorFace oneFace)
               => (faces.Count() == 1) && (faces.ToAnchorFaces().First() == oneFace);

            var _diagCtrls = _param.DiagonalControls;
            if ((_diagCtrls != AnchorFaceList.None) && (_diagBlock ^ _fillBlock))
            {
                // parameters
                var _source = _param.SourceFace;
                var _sink = _param.SinkFace;
                var _other = _param.OtherFace.ToAnchorFace();
                var _revSource = _source.ReverseFace();
                var _revSink = _sink.ReverseFace();
                var _revOther = _other.ReverseFace();
                var _sourceAxis = _source.GetAxis();
                var _sinkAxis = _sink.GetAxis();

                #region keeper function
                bool _sideOnly(AnchorFaceList faces) =>
                    ((faces.Count() == 1) && ((faces.ToAnchorFaces().First().GetAxis() != _sourceAxis) || (faces.ToAnchorFaces().First().GetAxis() != _sinkAxis)));
                if (_diagBlock)
                {
                    // keeper function
                    if (_param.OtherFace != OptionalAnchorFace.None)
                    {
                        // 2 Diagonals: must have source/sink/other OR (1 face only, opposite source OR opposite other)
                        _keep = (pt) =>
                        {
                            var _faces = GetTacticalPointFaces(pt);
                            return (_faces == AnchorFaceList.None) || _faces.ContainsAny(_source, _sink, _other)
                                || _onlyOne(_faces, _revOther) || _onlyOne(_faces, _revSource);
                        };
                    }
                    else
                    {
                        // 1 Diagonal: must have source/sink OR (1 face only, off source/sink axes)
                        _keep = (pt) =>
                        {
                            var _faces = GetTacticalPointFaces(pt);
                            return (_faces == AnchorFaceList.None) || _faces.ContainsAny(_source, _sink)
                                || _sideOnly(_faces);
                        };
                    }
                }
                else
                {
                    // keeper function
                    if (_param.OtherFace != OptionalAnchorFace.None)
                    {
                        // 2 Diagonals
                        _keep = (pt) =>
                            {
                                var _faces = GetTacticalPointFaces(pt);
                                return (_faces == AnchorFaceList.None) || _faces.Contains(_revSink)
                                    || _onlyOne(_faces, _revSource) || _onlyOne(_faces, _revOther)
                                    || (_faces.Contains(_revSource) && _faces.Contains(_revOther));
                            };
                    }
                    else
                    {
                        // 1 Diagonal
                        _keep = (pt) =>
                            {
                                var _faces = GetTacticalPointFaces(pt);
                                return (_faces == AnchorFaceList.None)
                                    || _faces.ContainsAny(_revSource, _revOther)
                                    || _sideOnly(_faces);
                            };
                    }
                }
                #endregion

                // adjusted for panels
                foreach (var _pt in RawTacticalPoints.Where(_tp => _keep(_tp)))
                {
                    yield return _adjust(_pt);
                }
            }
            else
            {
                var _bendCtrls = _param.BendControls;
                if ((_bendCtrls != AnchorFaceList.None) && (_diagBlock ^ _fillBlock))
                {
                    // parameters
                    var _source = _param.SourceFace;
                    var _sink = _param.SinkFace;
                    var _other = _param.OtherFace.ToAnchorFace();
                    var _revSource = _source.ReverseFace();
                    var _revSink = _sink.ReverseFace();
                    var _revOther = _other.ReverseFace();
                    var _sourceAxis = _source.GetAxis();
                    var _sinkAxis = _sink.GetAxis();

                    #region keeper function
                    if (_diagBlock)
                    {
                        // keeper function
                        if (_param.OtherFace != OptionalAnchorFace.None)
                        {
                            _keep = (pt) =>
                            {
                                var _faces = GetTacticalPointFaces(pt);
                                return (_faces == AnchorFaceList.None) || _faces.Contains(_source)
                                    || _onlyOne(_faces, _sink) || _onlyOne(_faces, _other)
                                    || (_faces.Contains(_sink) && _faces.Contains(_other));
                            };
                        }
                    }
                    else
                    {
                        // keeper function
                        if (_param.OtherFace != OptionalAnchorFace.None)
                        {
                            _keep = (pt) =>
                            {
                                var _faces = GetTacticalPointFaces(pt);
                                return (_faces == AnchorFaceList.None) || _faces.ContainsAny(_revSource, _revSink, _revOther)
                                    || _onlyOne(_faces, _other) || _onlyOne(_faces, _sink);
                            };
                        }
                    }
                    #endregion

                    // adjusted for panels
                    foreach (var _pt in RawTacticalPoints.Where(_tp => _keep(_tp)))
                    {
                        yield return _adjust(_pt);
                    }
                }
                else
                {
                    // get slope face
                    var _slope = _param.SourceFace;
                    var _slopePanel = GetSlopeComposite(_param.SlopeIndex);
                    var _slopeBlock = (_slopePanel != null) ? !movement.CanMoveThrough(_slopePanel.Material) : false;
                    if (_param.IsTrueSlope && (_slopeBlock ^ _fillBlock))
                    {
                        #region slope tactical points
                        var _sink = _param.SinkFace;
                        var _loBlock = _slopeBlock ? _slope.IsLowFace() : !_slope.IsLowFace();
                        var _loOffset = (_sink.IsLowFace() ? _slopePanel.GreaterThickness : _slopePanel.LesserThickness);
                        var _hiOffset = (_sink.IsLowFace() ? _slopePanel.LesserThickness : _slopePanel.GreaterThickness);
                        if (!_slope.IsLowFace())
                        {
                            // since slope panel was a hi face, need to move the offsets from down the top
                            _hiOffset = 5 - _hiOffset;
                            _loOffset = 5 - _loOffset;
                        }
                        foreach (var _pt in SlopeCellSpace.GetTacticalPoints(_loBlock, _slope.GetAxis(), _sink.GetAxis(), _loOffset, _hiOffset))
                        {
                            // adjusted for panels
                            yield return _adjust(_pt);
                        }
                        #endregion
                    }
                    else if (!_fillBlock)
                    {
                        // adjusted for panels
                        foreach (var _pt in RawTacticalPoints)
                        {
                            yield return _adjust(_pt);
                        }
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        {
            var _param = new PanelParams(param);
            var _fill = GetFill(_param);
            var _fillBlock = !movement.CanMoveThrough(_fill.Material);
            var _diagBlock = (Diagonal != null) ? !movement.CanMoveThrough(Diagonal.Material) : false;

            #region natural panel adjustments
            // natural panel adjustments
            var _pAdjusts = (from _af in _AllFaces
                             let _np = GetNaturalPanel(_param, _af)
                             select new
                             {
                                 Face = _af,
                                 Vector = _af.GetAxis().AxisVector(),
                                 Adjust = (_np != null) && !movement.CanMoveThrough(_np.Material)
                                    ? (_af.IsLowFace() ? _np.AverageThickness : -_np.AverageThickness)
                                    : 0
                             }).ToDictionary(_i => _i.Face);
            OptionalAnchorFace _optFace(double value, Axis axis)
            {
                // get optional face if on a cell boundary
                if (value <= 0)
                    return axis.GetLowFace().ToOptionalAnchorFace();
                if (value >= 5)
                    return axis.GetHighFace().ToOptionalAnchorFace();
                return OptionalAnchorFace.None;
            };
            Point3D _adjustOne(Point3D source, OptionalAnchorFace face)
            {
                // adjust one
                if (face != OptionalAnchorFace.None)
                {
                    var _adj = _pAdjusts[face.ToAnchorFace()];
                    return source + (_adj.Vector * _adj.Adjust);
                }
                return source;
            };
            Point3D _adjust(Point3D source)
            {
                // adjust all
                return _adjustOne(_adjustOne(_adjustOne(source, _optFace(source.Z, Axis.Z)), _optFace(source.Y, Axis.Y)), _optFace(source.X, Axis.X));
            };
            #endregion

            var _diagCtrls = _param.DiagonalControls;
            if ((_diagCtrls != AnchorFaceList.None) && (_diagBlock ^ _fillBlock))
            {
                var _factor = _diagBlock ? -2.5d : 2.5d;
                Func<TargetCorner, TargetCorner> _diagAdjust = null;
                var _sourceFace = _param.SourceFace;
                var _sinkFace = _param.SinkFace;
                if (_param.OtherFace != OptionalAnchorFace.None)
                {
                    #region 3 face diagonal blocking
                    var _otherFace = _param.OtherFace.ToAnchorFace();

                    // big corner offset
                    var _big = _diagBlock
                        ? AnchorFaceListHelper.Create(_sourceFace, _sinkFace, _otherFace)
                        : AnchorFaceListHelper.Create(_sourceFace.ReverseFace(), _sinkFace.ReverseFace(), _otherFace.ReverseFace());
                    var _bigVector = (_sourceFace.GetNormalVector() * _factor)
                        + (_sinkFace.GetNormalVector() * _factor)
                        + (_otherFace.GetNormalVector() * _factor);

                    if (_diagBlock)
                    {
                        // source/sink offset
                        var _src = AnchorFaceListHelper.Create(_sourceFace, _sinkFace);
                        var _srcVector = (_sourceFace.GetNormalVector() * -2.5d)
                            + (_sinkFace.GetNormalVector() * -2.5d);

                        // other/sink offset
                        var _oth = AnchorFaceListHelper.Create(_otherFace, _sinkFace);
                        var _othVector = (_sinkFace.GetNormalVector() * -2.5d)
                            + (_otherFace.GetNormalVector() * -2.5d);

                        // adjustment function
                        _diagAdjust = (orig) =>
                        {
                            if (_big.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _bigVector), orig.Faces);
                            else if (_src.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _srcVector), orig.Faces);
                            else if (_oth.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _othVector), orig.Faces);
                            return new TargetCorner(_adjust(orig.Point3D), orig.Faces);
                        };
                    }
                    else
                        _diagAdjust = (orig) =>
                        {
                            if (_big.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _bigVector), orig.Faces);
                            return new TargetCorner(_adjust(orig.Point3D), orig.Faces);
                        };
                    #endregion
                }
                else
                {
                    #region simple two face diagonal adjustments
                    // adjustment vector when test faces match
                    var _adjVector = (_sourceFace.GetNormalVector() * _factor) + (_sinkFace.GetNormalVector() * _factor);

                    // test faces for corners
                    var _adjFaces = _diagBlock
                        ? AnchorFaceListHelper.Create(_sourceFace, _sinkFace)
                        : AnchorFaceListHelper.Create(_sourceFace.ReverseFace(), _sinkFace.ReverseFace());

                    // function
                    _diagAdjust = (orig) =>
                    {
                        if (_adjFaces.IsSubset(orig.Faces))
                            return new TargetCorner(_adjust(orig.Point3D + _adjVector), orig.Faces);
                        return new TargetCorner(_adjust(orig.Point3D), orig.Faces);
                    };
                    #endregion
                }

                // corners (x8)
                yield return _diagAdjust(new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow));
                yield return _diagAdjust(new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow));
                yield return _diagAdjust(new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow));
                yield return _diagAdjust(new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow));

                yield return _diagAdjust(new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh));
                yield return _diagAdjust(new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh));
                yield return _diagAdjust(new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh));
                yield return _diagAdjust(new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh));
            }
            else
            {
                var _bendCtrls = _param.BendControls;
                if ((_bendCtrls != AnchorFaceList.None) && (_diagBlock ^ _fillBlock))
                {
                    #region 3 face bend blocking
                    var _factor = _diagBlock ? -2.5d : 2.5d;
                    var _sourceFace = _param.SourceFace;
                    var _sinkFace = _param.SinkFace;
                    var _otherFace = _param.OtherFace.ToAnchorFace();
                    Func<TargetCorner, TargetCorner> _diagAdjust = null;

                    // big corner offset
                    var _big = _diagBlock
                        ? AnchorFaceListHelper.Create(_sourceFace, _sinkFace, _otherFace)
                        : AnchorFaceListHelper.Create(_sourceFace.ReverseFace(), _sinkFace.ReverseFace(), _otherFace.ReverseFace());
                    var _bigVector = (_sourceFace.GetNormalVector() * _factor)
                        + (_sinkFace.GetNormalVector() * _factor)
                        + (_otherFace.GetNormalVector() * _factor);

                    if (!_diagBlock)
                    {
                        // source/sink offset
                        var _src = AnchorFaceListHelper.Create(_sourceFace.ReverseFace(), _sinkFace.ReverseFace());
                        var _srcVector = (_sourceFace.GetNormalVector() * 2.5d)
                            + (_sinkFace.GetNormalVector() * 2.5d);

                        // source/other offset
                        var _oth = AnchorFaceListHelper.Create(_otherFace.ReverseFace(), _sourceFace.ReverseFace());
                        var _othVector = (_sourceFace.GetNormalVector() * 2.5d)
                            + (_otherFace.GetNormalVector() * 2.5d);

                        // adjustment function
                        _diagAdjust = (orig) =>
                        {
                            if (_big.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _bigVector), orig.Faces);
                            else if (_src.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _srcVector), orig.Faces);
                            else if (_oth.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _othVector), orig.Faces);
                            return new TargetCorner(_adjust(orig.Point3D), orig.Faces);
                        };
                    }
                    else
                        _diagAdjust = (orig) =>
                        {
                            if (_big.IsSubset(orig.Faces))
                                return new TargetCorner(_adjust(orig.Point3D + _bigVector), orig.Faces);
                            return new TargetCorner(_adjust(orig.Point3D), orig.Faces);
                        };
                    #endregion

                    // corners (x8)
                    yield return _diagAdjust(new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow));
                    yield return _diagAdjust(new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow));
                    yield return _diagAdjust(new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow));
                    yield return _diagAdjust(new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow));

                    yield return _diagAdjust(new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh));
                    yield return _diagAdjust(new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh));
                    yield return _diagAdjust(new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh));
                    yield return _diagAdjust(new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh));
                }
                else
                {
                    // get slope face
                    var _slope = _param.SourceFace;
                    var _slopePanel = GetSlopeComposite(_param.SlopeIndex);
                    var _slopeBlock = (_slopePanel != null) ? !movement.CanMoveThrough(_slopePanel.Material) : false;
                    if (_param.IsTrueSlope && (_slopeBlock ^ _fillBlock))
                    {
                        #region slope target corners
                        var _sink = _param.SinkFace;
                        var _loBlock = _slopeBlock ? _slope.IsLowFace() : !_slope.IsLowFace();
                        var _loOffset = (_sink.IsLowFace() ? _slopePanel.GreaterThickness : _slopePanel.LesserThickness);
                        var _hiOffset = (_sink.IsLowFace() ? _slopePanel.LesserThickness : _slopePanel.GreaterThickness);
                        if (!_slope.IsLowFace())
                        {
                            // since slope panel was a hi face, need to move the offsets from down the top
                            _hiOffset = 5 - _hiOffset;
                            _loOffset = 5 - _loOffset;
                        }
                        foreach (var _corner in SlopeCellSpace.GetTargetCorners(_slope.GetAxis(), _sink.GetAxis(), _loBlock, _loOffset, _hiOffset))
                        {
                            // adjusted for panels
                            yield return new TargetCorner(_adjust(_corner.Point3D), _corner.Faces);
                        }
                        #endregion
                    }
                    else if (!_fillBlock)
                    {
                        // corners (x8)
                        yield return new TargetCorner(_adjust(new Point3D(0, 0, 0)), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                        yield return new TargetCorner(_adjust(new Point3D(5, 0, 0)), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                        yield return new TargetCorner(_adjust(new Point3D(0, 5, 0)), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                        yield return new TargetCorner(_adjust(new Point3D(5, 5, 0)), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                        yield return new TargetCorner(_adjust(new Point3D(0, 0, 5)), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                        yield return new TargetCorner(_adjust(new Point3D(5, 0, 5)), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                        yield return new TargetCorner(_adjust(new Point3D(0, 5, 5)), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                        yield return new TargetCorner(_adjust(new Point3D(5, 5, 5)), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public override bool ValidSpace(uint param, MovementBase movement)
        public override bool ValidSpace(uint param, MovementBase movement)
        {
            var _param = new PanelParams(param);

            // simplest test first
            var _fill = GetFill(_param);
            if (movement.CanMoveThrough(_fill.Material))
            {
                // must be able to move through every face panel material on every face to be valid
                return _AllFaces.All(_f => GetFacePanels(_param, _f).All(_fp => movement.CanMoveThrough(_fp.Material)));
            }
            return false;
        }
        #endregion

        // blockage

        #region public override bool BlocksDetect(uint param, int z, int y, int x, Point3D entryPt, Point3D exitPt)
        public override bool BlocksDetect(uint param, int z, int y, int x, Point3D entryPt, Point3D exitPt)
        {
            var _param = new PanelParams(param);

            // side panels
            if ((from _af in _AllFaces
                 let _np = GetNaturalPanel(_param, _af)
                 where (_np != null) && _np.BlocksDetect(_param, _af, z, y, x, entryPt, exitPt)
                 select _np).Any())
                return true;

            // interiors
            var _list = GetInteriors(_param).ToList();
            return GetInteriors(_param).Any(_i => _i.BlocksDetect(_param, z, y, x, entryPt, exitPt, _list));
        }
        #endregion

        #region public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        {
            var _param = new PanelParams(param);

            // side panels
            if ((from _af in _AllFaces
                 let _np = GetNaturalPanel(_param, _af)
                 where (_np != null) && _np.Material.BlocksEffect && _np.BlocksPath(_param, _af, z, y, x, pt1, pt2)
                 select _np).Any())
                return true;

            // interiors
            var _list = GetInteriors(_param).ToList();
            return GetInteriors(_param).Any(_i => _i.Material.BlocksEffect && _i.BlocksPath(_param, z, y, x, pt1, pt2, _list));
        }
        #endregion

        // etc...

        #region public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        {
            var _param = new PanelParams(paramsIn);

            #region flip panel types high and low
            // flip panel types high-low...
            switch (flipAxis)
            {
                case Axis.X:
                    {
                        var _tmp = _param.PanelTypeXHigh;
                        _param.PanelTypeXHigh = _param.PanelTypeXLow;
                        _param.PanelTypeXLow = _tmp;
                    }
                    break;

                case Axis.Y:
                    {
                        var _tmp = _param.PanelTypeYHigh;
                        _param.PanelTypeYHigh = _param.PanelTypeYLow;
                        _param.PanelTypeYLow = _tmp;
                    }
                    break;

                default: // Axis.Z
                    {
                        var _tmp = _param.PanelTypeZHigh;
                        _param.PanelTypeZHigh = _param.PanelTypeZLow;
                        _param.PanelTypeZLow = _tmp;
                    }
                    break;
            }
            #endregion

            if (_param.IsInteriorBindable)
            {
                #region flip interior bindings
                // if needed: flip inner structure faces...
                switch (_param.PanelInterior)
                {
                    case PanelInterior.Slope:
                        if (flipAxis == _param.SourceFace.GetAxis())
                        {
                            _param.SourceFace = _param.SourceFace.ReverseFace();
                        }
                        else if (flipAxis == _param.SinkFace.GetAxis())
                        {
                            _param.SinkFace = _param.SinkFace.ReverseFace();
                        }
                        break;

                    case PanelInterior.Diagonal:
                    case PanelInterior.Bend:
                        if (flipAxis == _param.SourceFace.GetAxis())
                        {
                            _param.SourceFace = _param.SourceFace.ReverseFace();
                        }
                        else if (flipAxis == _param.SinkFace.GetAxis())
                        {
                            _param.SinkFace = _param.SinkFace.ReverseFace();
                        }
                        else if ((_param.OtherFace != OptionalAnchorFace.None) &&
                            flipAxis == _param.OtherFace.ToAnchorFace().GetAxis())
                        {
                            _param.OtherFace = _param.OtherFace
                                .ToAnchorFace().ReverseFace()
                                .ToOptionalAnchorFace();
                        }
                        break;
                }
                #endregion
            }
            else
            {
                // if needed: flip snaps...
                #region void _doFlipFace  (AnchorFace face, bool leftRight)
                void _doFlipFace(AnchorFace face, bool leftRight)
                {
                    switch (_param.GetPanelType(face))
                    {
                        case PanelType.Corner:
                        case PanelType.MaskedCorner:
                            switch (_param.GetPanelEdge(face))
                            {
                                case FaceEdge.Bottom:
                                    if (!leftRight) _param.SetPanelEdge(face, FaceEdge.Top);
                                    break;
                                case FaceEdge.Top:
                                    if (!leftRight) _param.SetPanelEdge(face, FaceEdge.Bottom);
                                    break;
                                case FaceEdge.Left:
                                    if (leftRight) _param.SetPanelEdge(face, FaceEdge.Right);
                                    break;
                                case FaceEdge.Right:
                                    if (leftRight) _param.SetPanelEdge(face, FaceEdge.Left);
                                    break;
                            }
                            break;
                        case PanelType.LFrame:
                        case PanelType.MaskedLFrame:
                            switch (_param.GetPanelCorner(face))
                            {
                                case TriangleCorner.LowerLeft:
                                    _param.SetPanelCorner(face,
                                        leftRight ? TriangleCorner.LowerRight : TriangleCorner.UpperLeft);
                                    break;
                                case TriangleCorner.UpperLeft:
                                    _param.SetPanelCorner(face,
                                        leftRight ? TriangleCorner.UpperRight : TriangleCorner.LowerLeft);
                                    break;
                                case TriangleCorner.LowerRight:
                                    _param.SetPanelCorner(face,
                                        leftRight ? TriangleCorner.LowerLeft : TriangleCorner.UpperRight);
                                    break;
                                case TriangleCorner.UpperRight:
                                    _param.SetPanelCorner(face,
                                        leftRight ? TriangleCorner.UpperLeft : TriangleCorner.LowerRight);
                                    break;
                            }
                            break;
                    }
                };
                #endregion

                #region perform actual flips
                switch (flipAxis)
                {
                    case Axis.X:
                        // all left-right flips
                        _doFlipFace(AnchorFace.ZHigh, true);
                        _doFlipFace(AnchorFace.ZLow, true);
                        _doFlipFace(AnchorFace.YHigh, true);
                        _doFlipFace(AnchorFace.YLow, true);
                        break;

                    case Axis.Y:
                        // half and half on flip style
                        _doFlipFace(AnchorFace.ZHigh, false);
                        _doFlipFace(AnchorFace.ZLow, false);
                        _doFlipFace(AnchorFace.XHigh, true);
                        _doFlipFace(AnchorFace.XLow, true);
                        break;

                    default: // Axis.Z
                        // all top-bottom flips
                        _doFlipFace(AnchorFace.XHigh, false);
                        _doFlipFace(AnchorFace.XLow, false);
                        _doFlipFace(AnchorFace.YHigh, false);
                        _doFlipFace(AnchorFace.YLow, false);
                        break;
                }
                #endregion
            }

            // done
            return _param.Value;
        }
        #endregion

        #region public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        {
            if (axis1 != axis2)
            {
                var _param = new PanelParams(paramsIn);

                #region swap panels
                // swap panels between axes
                {
                    // scope variable names to the most enclosing block
                    var _faceLo1 = axis1.GetLowFace();
                    var _faceHi1 = axis1.GetHighFace();
                    var _faceLo2 = axis2.GetLowFace();
                    var _faceHi2 = axis2.GetHighFace();
                    var _panelLo1 = _param.GetPanelType(_faceLo1);
                    var _panelHi1 = _param.GetPanelType(_faceHi1);
                    var _panelLo2 = _param.GetPanelType(_faceLo2);
                    var _panelHi2 = _param.GetPanelType(_faceHi2);
                    _param.SetPanelType(_faceLo1, _panelLo2);
                    _param.SetPanelType(_faceHi1, _panelHi2);
                    _param.SetPanelType(_faceLo2, _panelLo1);
                    _param.SetPanelType(_faceHi2, _panelHi1);
                }
                #endregion

                if (_param.IsInteriorBindable)
                {
                    // if needed: swap inner structures...
                    switch (_param.PanelInterior)
                    {
                        case PanelInterior.Slope:
                            #region Slope
                            if (axis1 == _param.SourceFace.GetAxis())
                            {
                                // align SourceFace with Axis2
                                _param.SourceFace = _param.SourceFace.IsLowFace()
                                    ? axis2.GetLowFace()
                                    : axis2.GetHighFace();
                                if (axis2 == _param.SinkFace.GetAxis())
                                {
                                    // align SinkFace with Axis1
                                    _param.SinkFace = _param.SinkFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                            }
                            else if (axis1 == _param.SinkFace.GetAxis())
                            {
                                // align SinkFace with Axis2
                                _param.SinkFace = _param.SinkFace.IsLowFace()
                                    ? axis2.GetLowFace()
                                    : axis2.GetHighFace();
                                if (axis2 == _param.SourceFace.GetAxis())
                                {
                                    // align SourceFace with Axis1
                                    _param.SourceFace = _param.SourceFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                            }
                            else
                            {
                                if (axis2 == _param.SourceFace.GetAxis())
                                {
                                    // align SourceFace with Axis1
                                    _param.SourceFace = _param.SourceFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                                else
                                {
                                    // align SinkFace with Axis1
                                    _param.SinkFace = _param.SinkFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                            }
                            #endregion
                            break;

                        case PanelInterior.Diagonal:
                        case PanelInterior.Bend:
                            #region Diagonal/Bend
                            if (axis1 == _param.SourceFace.GetAxis())
                            {
                                // align SourceFace with Axis2
                                _param.SourceFace = _param.SourceFace.IsLowFace()
                                    ? axis2.GetLowFace()
                                    : axis2.GetHighFace();
                                if (axis2 == _param.SinkFace.GetAxis())
                                {
                                    // align SinkFace with Axis1
                                    _param.SinkFace = _param.SinkFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                                else if (_param.OtherFace != OptionalAnchorFace.None)
                                {
                                    // align OtherFace with Axis1
                                    _param.OtherFace = _param.OtherFace.ToAnchorFace().IsLowFace()
                                        ? axis1.GetLowFace().ToOptionalAnchorFace()
                                        : axis1.GetHighFace().ToOptionalAnchorFace();
                                }
                            }
                            else if (axis1 == _param.SinkFace.GetAxis())
                            {
                                // align SinkFace with Axis2
                                _param.SinkFace = _param.SinkFace.IsLowFace()
                                    ? axis2.GetLowFace()
                                    : axis2.GetHighFace();
                                if (axis2 == _param.SourceFace.GetAxis())
                                {
                                    // align SourceFace with Axis1
                                    _param.SourceFace = _param.SourceFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                                else if (_param.OtherFace != OptionalAnchorFace.None)
                                {
                                    // align OtherFace with Axis1
                                    _param.OtherFace = _param.OtherFace.ToAnchorFace().IsLowFace()
                                        ? axis1.GetLowFace().ToOptionalAnchorFace()
                                        : axis1.GetHighFace().ToOptionalAnchorFace();
                                }
                            }
                            else
                            {
                                if (_param.OtherFace != OptionalAnchorFace.None)
                                {
                                    // align OtherFace with Axis2
                                    _param.OtherFace = _param.OtherFace.ToAnchorFace().IsLowFace()
                                        ? axis2.GetLowFace().ToOptionalAnchorFace()
                                        : axis2.GetHighFace().ToOptionalAnchorFace();
                                }

                                if (axis2 == _param.SourceFace.GetAxis())
                                {
                                    // align SourceFace with Axis1
                                    _param.SourceFace = _param.SourceFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                                else
                                {
                                    // align SinkFace with Axis1
                                    _param.SinkFace = _param.SinkFace.IsLowFace()
                                        ? axis1.GetLowFace()
                                        : axis1.GetHighFace();
                                }
                            }
                            #endregion
                            break;
                    }
                }
                else
                {
                    if ((axis1 == Axis.X) || (axis2 == Axis.X))
                    {
                        #region void _swapSnap(AnchorFace xFace, AnchorFace otherFace)
                        void _swapSnap(AnchorFace xFace, AnchorFace otherFace)
                        {
                            #region void _fixupOther()
                            void _fixupOther()
                            {
                                var _oType = _param.GetPanelType(otherFace);
                                switch (_oType)
                                {
                                    case PanelType.Corner:
                                    case PanelType.MaskedCorner:
                                        // other face an edge, needs fixup from x face
                                        _param.SetPanelEdge(otherFace, _param.GetPanelEdge(xFace).GetLeftRightSwap());
                                        break;

                                    case PanelType.LFrame:
                                    case PanelType.MaskedLFrame:
                                        // other face an l-frame, needs fixup from X face
                                        _param.SetPanelCorner(otherFace, _param.GetPanelCorner(xFace).GetLeftRightSwap());
                                        break;
                                }
                            };
                            #endregion

                            var _xType = _param.GetPanelType(xFace);
                            switch (_xType)
                            {
                                case PanelType.Corner:
                                case PanelType.MaskedCorner:
                                    // X changed to a corner, so get edge from other face
                                    var _finalXEdge = _param.GetPanelEdge(otherFace).GetLeftRightSwap();
                                    _fixupOther();

                                    // now fixup X face's edge
                                    _param.SetPanelEdge(xFace, _finalXEdge);
                                    return;

                                case PanelType.LFrame:
                                case PanelType.MaskedLFrame:
                                    // X changed to an l-frame, so get corner from other face
                                    var _finalXCorner = _param.GetPanelCorner(otherFace).GetLeftRightSwap();
                                    _fixupOther();

                                    // now fixup X face's corner
                                    _param.SetPanelCorner(xFace, _finalXCorner);
                                    return;

                                default:
                                    _fixupOther();
                                    return;
                            }
                        };
                        #endregion

                        if ((axis1 == Axis.Y) || (axis2 == Axis.Y))
                        {
                            // swapping X and Y
                            _swapSnap(AnchorFace.XLow, AnchorFace.YLow);
                            _swapSnap(AnchorFace.XHigh, AnchorFace.YHigh);
                        }
                        else
                        {
                            // must be swapping X and Z
                            _swapSnap(AnchorFace.XLow, AnchorFace.ZLow);
                            _swapSnap(AnchorFace.XHigh, AnchorFace.ZHigh);
                        }
                    }
                }
                return _param.Value;
            }
            return paramsIn;
        }
        #endregion

        public override CellSpaceInfo ToCellSpaceInfo()
            => new PanelSpaceInfo(this);

        #region IPanelCellSpace Members

        public IEnumerable<IBasePanel> Panel1Map => Panel1s.ToAnchorFaceMap();
        public IEnumerable<IBasePanel> Panel2Map => Panel2s.ToAnchorFaceMap();
        public IEnumerable<IBasePanel> Panel3Map => Panel3s.ToAnchorFaceMap();
        public IEnumerable<ICornerPanel> CornerMap => Corners.ToAnchorFaceMap();
        public IEnumerable<ILFramePanel> LFrameMap => LFrames.ToAnchorFaceMap();
        public IEnumerable<ISlopeComposite> SlopeCompositeMap => Slopes.Select(_s => _s);
        public IBasePanel DiagonalPanel => Diagonal;
        public IBasePanel Fill0Panel => Fill0;
        public IBasePanel Fill1Panel => Fill1;
        public IBasePanel Fill2Panel => Fill2;
        public IBasePanel Fill3Panel => Fill3;

        #endregion

        #region public override int? InnerGripDifficulty(uint param, AnchorFace gravity, MovementBase movement)
        /// <summary>Used as syntactic sugar for InnerGripDifficulty</summary>
        private class GripState
        {
            public int? GripValue { get; set; }
            public Func<AnchorFace, bool> CanReachPanel { get; set; }
        }

        public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        {
            // marshalling and conversions
            var _param = new PanelParams(param);
            var _revGravity = gravity.ReverseFace();

            // has usable inner structure?
            if (_param.HasStructure)
            {
                // fill blocking
                var _fill = GetFill(_param);
                var _fillBlock = !movement.CanMoveThrough(_fill.Material);

                // function for interior grip
                GripState _interiorGrip()
                {
                    if (_param.IsInteriorBindable)
                    {
                        var _source = _param.SourceFace;
                        switch (_param.PanelInterior)
                        {
                            case PanelInterior.Bend:
                                {
                                    var _diagBlock = (Diagonal != null) ? !movement.CanMoveThrough(Diagonal.Material) : false;
                                    if (_diagBlock ^ _fillBlock)
                                    {
                                        var _controls = _param.DiagonalControls;
                                        return new GripState
                                        {
                                            // canReachPanel determined by controls and material blocks
                                            CanReachPanel = (face) =>
                                                _diagBlock
                                                ? face != _source
                                                : !_controls.Contains(face.ReverseFace()),
                                            // grip value determined by gravity, controls and material blocks
                                            GripValue =
                                                ((_controls.Contains(_revGravity) && _diagBlock)
                                                || (_controls.Contains(gravity) && _fillBlock))
                                                ? _diagBlock ? Diagonal.Dangling : _fill.Dangling
                                                : _diagBlock ? Diagonal.Base : _fill.Base
                                        };
                                    }
                                }
                                break;

                            case PanelInterior.Slope:
                                {
                                    var _slope = GetSlopeComposite(_param.SlopeIndex);
                                    var _slopeBlock = (_slope != null) ? !movement.CanMoveThrough(_slope.Material) : false;
                                    if (_slopeBlock ^ _fillBlock)
                                    {
                                        var _controls = _param.DiagonalControls;
                                        return new GripState
                                        {
                                            // canReachPanel determined by source-face and material blocks
                                            CanReachPanel = (face) =>
                                                _slopeBlock ? face != _source : face != _source.ReverseFace(),
                                            // grip value determined by gravity, controls and material blocks
                                            GripValue =
                                                ((_controls.Contains(_revGravity) && _slopeBlock)
                                                || (_controls.Contains(gravity) && _fillBlock))
                                                ? _slopeBlock ? _slope.Dangling : _fill.Dangling
                                                : _slopeBlock ? _slope.Base : _fill.Base
                                        };
                                    }
                                }
                                break;

                            case PanelInterior.Diagonal:
                                {
                                    var _diagBlock = (Diagonal != null) ? !movement.CanMoveThrough(Diagonal.Material) : false;
                                    if (_diagBlock ^ _fillBlock)
                                    {
                                        var _controls = _param.DiagonalControls;
                                        return new GripState
                                        {
                                            // canReachPanel determined by controls and material blocks
                                            CanReachPanel = (face) =>
                                                _diagBlock
                                                ? !_controls.Contains(face)
                                                : face != _source.ReverseFace(),
                                            // grip value determined by gravity, controls and material blocks
                                            GripValue =
                                                ((_controls.Contains(_revGravity) && _diagBlock)
                                                || (_controls.Contains(gravity) && _fillBlock))
                                                ? _diagBlock ? Diagonal.Dangling : _fill.Dangling
                                                : _diagBlock ? Diagonal.Base : _fill.Base
                                        };
                                    }
                                }
                                break;
                        }
                    }

                    // interior not bindable:  int? = null
                    // canTouch negation of fill block:  Func<AnchorFace, bool> = (face) => !_fillBlock
                    // NOTE: _fillBlock must equal any other composite, so can answer face accessibility
                    return new GripState
                    {
                        GripValue = null,
                        CanReachPanel = (face) => !_fillBlock
                    };
                };

                // evaluate interior gripState
                var _interior = _interiorGrip();
                if (!_param.HasPanels)
                {
                    // all we need
                    return new CellGripResult
                    {
                        Difficulty = _interior.GripValue,
                        // TODO: Faces?
                    };
                }

                // look for grip on the inside of any panels for the cell
                // start with anti-gravity (which will be a dangler)
                int? _dangle = null;
                if (_interior.CanReachPanel(_revGravity))
                {
                    var _revGravPanel = GetNaturalPanel(_param, _revGravity);
                    if (_revGravPanel != null)
                    {
                        // dangling
                        _dangle = _revGravPanel.Dangling;
                    }
                }

                // define how to evaluate grip for a single natural panel
                int? _panelGrip(BaseNaturalPanel panel, AnchorFace face)
                {
                    var _blocks = !movement.CanMoveThrough(panel.Material);
                    if (_fillBlock ^ _blocks)
                    {
                        if (panel is CornerPanel)
                        {
                            var _edge = _param.GetPanelEdge(face);
                            var _snap = face.GetSnappedFace(_edge);
                            return _blocks
                                // panel ledge if corner is snapped to gravity face
                                ? ((_snap == gravity) ? panel.Ledge : panel.Base)
                                // fill ledge if corner is snapped opposite to gravity face
                                : ((_snap == _revGravity) ? _fill.Ledge : _fill.Base);
                        }
                        else if (panel is LFramePanel)
                        {
                            var _corner = _param.GetPanelCorner(face);
                            var _snap = AnchorFaceListHelper.Create(
                                face.HorizontalSnappedFace(_corner),
                                face.VerticalSnappedFace(_corner));
                            return _blocks
                                // L-Frame ledge if L-Frame is snapped to gravity face
                                ? (_snap.Contains(gravity) ? panel.Ledge : panel.Base)
                                // fill ledge if L-Frame is snapped opposite to gravity face
                                : (_snap.Contains(_revGravity) ? _fill.Ledge : _fill.Base);
                        }
                        else if (panel is NormalPanel)
                        {
                            // all about that
                            return panel.Base;
                        }
                    }

                    // no natural panel, or can't use
                    return (int?)null;
                };

                // ortho sides unioned with bottom
                var _maxPanelGrip = (from _ortho in gravity.GetOrthoFaces().Union(gravity.ToEnumerable())
                                     where _interior.CanReachPanel(_ortho)
                                     select _panelGrip(GetNaturalPanel(_param, _ortho), _ortho)).Max();

                // max of values found
                return new CellGripResult
                {
                    Difficulty = CoreEnumerable.GetEnumerable(_maxPanelGrip, _dangle, _interior.GripValue).Max(),
                    // TODO: Faces?
                };
            }

            return base.InnerGripResult(param, gravity, movement);
        }
        #endregion

        #region public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            var _param = new PanelParams(param);
            var _panels = GetFacePanels(_param, gripFace).ToList();
            var _revGravity = gravity.ReverseFace();
            if (gripFace == _revGravity)
            {
                // dangling
                return _panels.Min(_p => _p.Dangling);
            }
            else if (_panels.Count > 1)
            {
                // all multi-panels/composites have material fill
                var _mFill = _panels.OfType<MaterialFill>().First();
                var _mBlock = !movement.CanMoveThrough(_mFill.Material);

                if (_panels.OfType<CornerPanel>().Any())
                {
                    var _cPanel = _panels.OfType<CornerPanel>().First();
                    var _cBlock = !movement.CanMoveThrough(_cPanel.Material);
                    if (_cBlock ^ _mBlock)
                    {
                        // face to which the corner is snapped
                        var _snap = gripFace.GetSnappedFace(_param.GetPanelEdge(gripFace));
                        if (_cBlock)
                            // ledge or base
                            return (gravity == _snap) ? _cPanel.Ledge : _cPanel.Base;
                        else if (_mBlock)
                            // ledge or base
                            return (_revGravity == _snap) ? _mFill.Ledge : _mFill.Base;
                    }
                }
                else if (_panels.OfType<LFramePanel>().Any())
                {
                    var _lPanel = _panels.OfType<LFramePanel>().First();
                    var _lBlock = !movement.CanMoveThrough(_lPanel.Material);
                    if (_lBlock ^ _mBlock)
                    {
                        // faces to which the lframe is snapped
                        var _corner = _param.GetPanelCorner(gripFace);
                        var _snap = AnchorFaceListHelper.Create(
                            gripFace.HorizontalSnappedFace(_corner),
                            gripFace.VerticalSnappedFace(_corner));
                        if (_lBlock)
                            // ledge or base
                            return _snap.Contains(gravity) ? _lPanel.Ledge : _lPanel.Base;
                        else if (_mBlock)
                            // ledge or base
                            return _snap.Contains(_revGravity) ? _mFill.Ledge : _mFill.Base;
                    }
                }
                else if (_panels.OfType<SlopeComposite>().Any())
                {
                    var _sComp = _panels.OfType<SlopeComposite>().First();
                    var _sBlock = !movement.CanMoveThrough(_sComp.Material);
                    if (_sBlock ^ _mBlock)
                    {
                        if (gravity == _param.SourceFace)
                        {
                            // gravity towards source face
                            // composite ledge if composite blocks, otherwise material base
                            return _sBlock ? _sComp.Ledge : _mFill.Base;
                        }
                        else if (_revGravity == _param.SourceFace)
                        {
                            // gravity away from source face
                            // fill ledge if fill blocks, otherwise composite base
                            return _mBlock ? _mFill.Ledge : _sComp.Base;
                        }
                        else if (_param.IsFaceSlopeSide(gripFace))
                        {
                            // must be a slope side orthogonal to gravity
                            if (_sBlock)
                            {
                                // slope is the blocker, ledge if sink towards gravity
                                return gravity == _param.SinkFace ? _sComp.Ledge : _sComp.Base;
                            }
                            else
                            {
                                // fill is the blocker, ledge if sink away from gravity
                                return _revGravity == _param.SinkFace ? _mFill.Ledge : _mFill.Base;
                            }
                        }
                        else
                        {
                            // must be a slope end, orthogonal to gravity
                            // no ledges
                            return _sBlock ? _sComp.Base : _mFill.Base;
                        }
                    }
                }
                else if (_panels.OfType<DiagonalComposite>().Any())
                {
                    var _dComp = _panels.OfType<DiagonalComposite>().First();
                    var _dBlock = !movement.CanMoveThrough(_dComp.Material);
                    if (_dBlock ^ _mBlock)
                    {
                        // composite face controls
                        var _gripControl = _param.DiagonalFaceControlFaces(gripFace);
                        if (_dBlock)
                            // ledge or base
                            return _gripControl.Contains(gravity) ? _dComp.Ledge : _dComp.Base;
                        else if (_mBlock)
                            // ledge or base
                            return _gripControl.Contains(_revGravity) ? _mFill.Ledge : _mFill.Base;
                    }
                }
            }

            // either face is uniform; both materials block; or one one blocks, but not ledge-like...
            return _panels.Min(_p => _p.Base);
        }
        #endregion

        public override int? InnerSwimDifficulty(uint param)
        {
            var _param = new PanelParams(param);
            return (from _panel in GetInteriors(_param)
                    select MaterialSwimDifficulty(_panel.Material)).Max();
        }

        public override bool SuppliesBreathableAir(uint param)
        {
            var _param = new PanelParams(param);
            return (from _panel in GetInteriors(_param)
                    let _gas = _panel.Material as GasCellMaterial
                    where _gas != null
                    select _gas.AirBreathe).Any();
        }

        public override bool SuppliesBreathableWater(uint param)
        {
            var _param = new PanelParams(param);
            return (from _panel in GetInteriors(_param)
                    let _liquid = _panel.Material as LiquidCellMaterial
                    where _liquid != null
                    select _liquid.AquaticBreathe).Any();
        }
    }
}