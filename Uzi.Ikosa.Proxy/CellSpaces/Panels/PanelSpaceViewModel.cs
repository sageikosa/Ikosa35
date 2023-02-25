using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.VisualizationSvc
{
    public partial class PanelSpaceViewModel : CellSpaceViewModel
    {
        public PanelSpaceViewModel(PanelSpaceInfo info, LocalMapInfo map)
            : base(info, map)
        {
            Panel1s = info.Panel1s.Select(_p => new NormalPanelViewModel(_p, map)).ToList();
            Panel2s = info.Panel2s.Select(_p => new NormalPanelViewModel(_p, map)).ToList();
            Panel3s = info.Panel3s.Select(_p => new NormalPanelViewModel(_p, map)).ToList();
            Corners = info.Corners.Select(_p => new CornerPanelViewModel(_p, map)).ToList();
            LFrames = info.LFrames.Select(_p => new LFramePanelViewModel(_p, map)).ToList();
            Slopes = info.Slopes.Select(_p => new SlopeCompositeViewModel(_p, map)).ToList();
            Diagonal = new DiagonalCompositeViewModel(info.Diagonal, map);
            Fill0 = new MaterialFillViewModel(info.Fill0, map);
            Fill1 = info.Fill1 != null ? new MaterialFillViewModel(info.Fill1, map) : null;
            Fill2 = info.Fill2 != null ? new MaterialFillViewModel(info.Fill2, map) : null;
            Fill3 = info.Fill3 != null ? new MaterialFillViewModel(info.Fill3, map) : null;
        }

        public PanelSpaceInfo PanelSpaceInfo { get { return Info as PanelSpaceInfo; } }
        public List<NormalPanelViewModel> Panel1s { get; private set; }
        public List<NormalPanelViewModel> Panel2s { get; private set; }
        public List<NormalPanelViewModel> Panel3s { get; private set; }
        public List<CornerPanelViewModel> Corners { get; private set; }
        public List<LFramePanelViewModel> LFrames { get; private set; }
        public List<SlopeCompositeViewModel> Slopes { get; private set; }
        public DiagonalCompositeViewModel Diagonal { get; private set; }
        public MaterialFillViewModel Fill0 { get; private set; }
        public MaterialFillViewModel Fill1 { get; private set; }
        public MaterialFillViewModel Fill2 { get; private set; }
        public MaterialFillViewModel Fill3 { get; private set; }

        protected SlopeCompositeViewModel GetSlopeComposite(byte index)
        {
            if (Slopes.Count > index)
            {
                return Slopes[index];
            }
            return Slopes.FirstOrDefault();
        }

        private static readonly AnchorFace[] _AllFaces = new AnchorFace[] 
        { 
            AnchorFace.ZLow, AnchorFace.ZHigh, 
            AnchorFace.YLow, AnchorFace.YHigh,
            AnchorFace.XLow, AnchorFace.XHigh
        };

        #region public BaseNaturalPanel GetNaturalPanel(PanelParams param, AnchorFace panelFace)
        /// <summary>
        /// Supplies one of these panels: 
        /// {NormalPanel} | {CornerPanel} | {LFramePanel} | {NULL}
        /// </summary>
        public INaturalPanel GetNaturalPanel(PanelParams param, AnchorFace panelFace)
        {
            var _pType = param.GetPanelType(panelFace);
            switch (_pType)
            {
                case PanelType.Corner:
                case PanelType.MaskedCorner:
                    #region corner
                    // corners
                    if (Corners[(byte)panelFace] != null)
                        return Corners[(byte)panelFace];
                    else
                        return Panel1s[(byte)panelFace];
                    #endregion

                case PanelType.LFrame:
                case PanelType.MaskedLFrame:
                    #region LFrames
                    // lframes
                    if (LFrames[(byte)panelFace] != null)
                        return LFrames[(byte)panelFace];
                    else
                        return Panel1s[(byte)panelFace];
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
                                return Panel1s[(byte)panelFace];
                            case PanelType.Panel2:
                                return Panel2s[(byte)panelFace] ?? Panel1s[(byte)panelFace];
                            case PanelType.Panel3:
                            default:
                                return Panel3s[(byte)panelFace] ?? Panel1s[(byte)panelFace];
                        }
                    }
            }
            return null;
        }
        #endregion

        #region public MaterialFillInfo GetFill(PanelParams param)
        public MaterialFillViewModel GetFill(PanelParams param)
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

        #region public IEnumerable<BasePanel> GetInteriors(PanelParams param)
        /// <summary>Yields MaterialFill [and {DiagonalFill|SlopeFill}] as appropriate</summary>
        public IEnumerable<IBasePanel> GetInteriors(PanelParams param)
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

        #region public IEnumerable<BasePanel> GetFacePanels(PanelParams param, AnchorFace panelFace)
        /// <summary>
        /// Supplies one of these sets: 
        /// {NormalPanel} | {CornerPanel, MaterialFill} | {LFramePanel, MaterialFill} | {SlopeFill} 
        /// | {NormalPanel, MaterialFill} | {MaterialFill} | {MaterialFill, DiagonalFill} | {MaterialFill, SlopeFill}
        /// </summary>
        public IEnumerable<IBasePanel> GetFacePanels(PanelParams param, AnchorFace panelFace)
        {
            var _pType = param.GetPanelType(panelFace);
            switch (_pType)
            {
                case PanelType.Corner:
                case PanelType.MaskedCorner:
                    #region corner
                    // corners
                    if (Corners[(byte)panelFace] != null)
                    {
                        yield return Corners[(byte)panelFace];
                        yield return GetFill(param);
                    }
                    else
                    {
                        yield return Panel1s[(byte)panelFace];
                    }
                    break;
                    #endregion

                case PanelType.LFrame:
                case PanelType.MaskedLFrame:
                    #region LFrames
                    // lframes
                    if (LFrames[(byte)panelFace] != null)
                    {
                        yield return LFrames[(byte)panelFace];
                        yield return GetFill(param);
                    }
                    else
                    {
                        yield return Panel1s[(byte)panelFace];
                    }
                    break;
                    #endregion

                case PanelType.Panel1:
                case PanelType.Panel2:
                case PanelType.Panel3:
                    // normal panels
                    if (param.IsFaceSlopeBottom(panelFace) && Slopes.Any())
                    {
                        // actually a slope bottom
                        yield return GetSlopeComposite(param.SlopeIndex);
                    }
                    else
                    {
                        // normal panel
                        switch (_pType)
                        {
                            case PanelType.Panel1:
                                yield return Panel1s[(byte)panelFace];
                                break;
                            case PanelType.Panel2:
                                yield return Panel2s[(byte)panelFace] ?? Panel1s[(byte)panelFace];
                                break;
                            case PanelType.Panel3:
                                yield return Panel3s[(byte)panelFace] ?? Panel1s[(byte)panelFace];
                                break;
                        }
                    }
                    break;

                case PanelType.NoPanel:
                    {
                        if (param.IsFaceSlopeBottom(panelFace) && Slopes.Any())
                        {
                            // actually a slope bottom
                            yield return GetSlopeComposite(param.SlopeIndex);
                        }
                        else
                        {
                            // no panel
                            if ((param.IsFaceDiagonalBinder(panelFace) || param.IsFaceBendableSource(panelFace))
                                && (Diagonal != null))
                            {
                                yield return Diagonal;
                            }
                            else if ((param.IsFaceDiagonalSide(panelFace) || param.IsFaceTriangularSink(panelFace))
                                && (Diagonal != null))
                            {
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
                                yield return GetFill(param);
                            }
                        }
                    }
                    break;
            }
            yield break;
        }
        #endregion

        #region public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, Visualize.VisualEffect effect)
        public override void AddInnerStructures(uint param, BuildableGroup group, int z, int y, int x, Visualize.VisualEffect effect)
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
            foreach (var _i in _interiors.OfType<IBaseComposite>())
                _i.AddInnerStructures(_param, group, z, y, x, effect, _sides, _interiors);

            // draw innards of natural sides
            foreach (var _side in _sides)
                _side.Value.AddInnerStructures(_param, _side.Key, group, z, y, x, effect, _interiors);
        }
        #endregion

        #region public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, Visualize.AnchorFace face, Visualize.VisualEffect effect, System.Windows.Media.Media3D.Transform3D bump)
        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, Visualize.AnchorFace face, Visualize.VisualEffect effect, System.Windows.Media.Media3D.Vector3D bump, IGeometricRegion currentRegion)
        {
            var _param = new PanelParams(param);

            var _panels = GetFacePanels(_param, face).ToList();
            foreach (var _p in _panels)
                _p.AddOuterSurface(_param, group, z, y, x, face, face, effect, bump, _panels);

            // for any side face not occluded by the panel faces, must draw natural side face sides...
            foreach (var _of in face.GetOrthoFaces())
                if (_panels.All(_p => !_p.OrthoOcclusion(_param, face, _of)))
                {
                    var _natSide = GetNaturalPanel(_param, _of);
                    if (_natSide != null)
                    {
                        _natSide.AddOuterSurface(_param, group, z, y, x, _of, face, effect, bump, null);
                    }
                }
        }
        #endregion

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
        {
            var _param = new PanelParams(param);
            return GetFacePanels(_param, outwardFace).All(_p => !_p.IsInvisible);
        }

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
    }
}
