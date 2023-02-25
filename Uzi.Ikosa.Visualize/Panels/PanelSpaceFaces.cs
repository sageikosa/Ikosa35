using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;
using HelixToolkit.Wpf;

namespace Uzi.Visualize
{
    public static class PanelSpaceFaces
    {
        #region Static Setup
        static PanelSpaceFaces()
        {
            const double _scale = 4.998d / 5d;
            var _group = new Transform3DGroup();
            _group.Children.Add(new ScaleTransform3D(_scale, _scale, 0, 2.5d, 2.5d, 0d));
            _group.Children.Add(new TranslateTransform3D(0, 0, -0.001));
            _FillReduce = new MatrixTransform3D(_group.Value);
            _FillReduce.Freeze();
        }

        private static Transform3D _FillReduce = null;
        #endregion

        #region public static void AddInnerNormalPanel(BuildableGroup, AnchorFace, double, VisualEffect, IBasePanel)
        public static void AddInnerNormalPanel(BuildableGroup group, AnchorFace panelFace, double thickness, VisualEffect effect, IBasePanel basePanel)
        {
            var _mesh = CellSpaceFaces.Mesh;
            var _material = basePanel.GetSideFaceMaterial(SideIndex.Front, effect);
            var _cellGroup = _material.IsAlpha ? group.Alpha : group.Opaque;
            switch (panelFace)
            {
                case AnchorFace.ZLow:
                    _cellGroup.Children.Add(HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null,
                        new TranslateTransform3D(0, 0, thickness)));
                    break;
                case AnchorFace.ZHigh:
                    _cellGroup.Children.Add(HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null,
                        HedralGenerator.ZMTransform, new TranslateTransform3D(0, 0, 5 - thickness)));
                    break;
                case AnchorFace.YLow:
                    _cellGroup.Children.Add(HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null,
                        HedralGenerator.YPTransform, new TranslateTransform3D(0, thickness - 5, 0)));
                    break;
                case AnchorFace.YHigh:
                    _cellGroup.Children.Add(HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null,
                        HedralGenerator.YMTransform, new TranslateTransform3D(0, 5 - thickness, 0)));
                    break;
                case AnchorFace.XLow:
                    _cellGroup.Children.Add(HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null,
                        HedralGenerator.XPTransform, new TranslateTransform3D(thickness - 5, 0, 0)));
                    break;
                case AnchorFace.XHigh:
                default:
                    _cellGroup.Children.Add(HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null,
                        HedralGenerator.XMTransform, new TranslateTransform3D(5 - thickness, 0, 0)));
                    break;
            }
        }
        #endregion

        #region public static void AddOuterNormalPanel(BuildableGroup, AnchorFace, AnchorFace, double, VisualEffect, IBasePanel, Transform3D)
        public static void AddOuterNormalPanel(BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace showFace,
            double thickness, VisualEffect effect, IBasePanel basePanel)
        {
            if (panelFace == showFace)
            {
                group.Context.GetBuildableMesh(
                    basePanel.GetBuildableMeshKey(panelFace, effect),
                    () => basePanel.GetSideFaceMaterial(SideIndex.Back, effect))
                    .AddCellFace(panelFace, new Vector3D(x * 5.0d, y * 5.0d, z * 5.0d));
            }
            else
            {
                var _idx = panelFace.GetSideIndex(showFace);
                AddOuterRectangularSliver(group, z, y, x, showFace, showFace.GetSnappingEdge(panelFace), thickness,
                    basePanel.GetBuildableMeshKey(_idx, effect),
                    () => basePanel.GetSideFaceMaterial(_idx, effect));
            }
        }
        #endregion

        #region public static void AddOuterNormalPanel(BuildableGroup group, AnchorFace, VisualEffect, IBasePanel)
        public static void AddOuterNormalPanel(BuildableGroup group, ICellLocation location, AnchorFace face, VisualEffect effect, IBasePanel basePanel)
        {
            #region full outer faces
            Func<BuildableMaterial> _material = () => basePanel.GetSideFaceMaterial(SideIndex.Back, effect);
            group.Context.GetBuildableMesh(basePanel.GetBuildableMeshKey(AnchorFace.ZLow, effect), _material)
                .AddCellFace(face, location.Vector3D());
            #endregion
        }
        #endregion

        #region public static void AddOuterMaterialFill(BuildableGroup group, AnchorFace panelFace, IBasePanel panel, VisualEffect effect, Transform3D bump)
        public static void AddOuterMaterialFill(BuildableGroup group, int z, int y, int x, AnchorFace panelFace, IBasePanel panel, VisualEffect effect, Vector3D bump)
        {
            group.Context.GetBuildableMesh(
                panel.GetBuildableMeshKey(panelFace, effect),
                () => panel.GetAnchorFaceMaterial(panelFace, effect))
                .AddCellFace(panelFace, new Vector3D(x * 5.0d, y * 5.0d, z * 5.0d));
        }
        #endregion

        #region public static void AddOuterBinder(BuildableGroup group, AnchorFace panelFace, IBasePanel panel, VisualEffect effect, Transform3D bump)
        public static void AddOuterBinder(BuildableGroup group, AnchorFace panelFace, IBasePanel panel, VisualEffect effect, Transform3D bump)
        {
            var _mesh = CellSpaceFaces.Mesh;
            var _material = panel.GetAnchorFaceMaterial(panelFace, effect);
            var _geom = HedralGenerator.GeometryModel3DTransform(_mesh, _material.Material, null, panelFace.Transform(), bump);
            if (_material.IsAlpha)
                group.Alpha.Children.Add(_geom);
            else
                group.Opaque.Children.Add(_geom);
        }
        #endregion

        #region public static void AddInnerCornerPanel(BuildableGroup, AnchorFace, double, FaceEdge, double, VisualEffect, IBasePanel)
        public static void AddInnerCornerPanel(BuildableGroup group, AnchorFace panelFace, double thickness, FaceEdge edge, double edgeWidth, VisualEffect effect, IBasePanel basePanel)
        {
            var _textureSize = new Vector(5d, 5d);
            var _perp = HedralGenerator.RectangularMesh(new Rect(0, 5 - thickness, 5, thickness), 1, 1, _textureSize);
            MeshGeometry3D _back = null;
            Model3DGroup _group = null;
            BuildableMaterial _material;
            switch (edge)
            {
                case FaceEdge.Bottom:
                    // perp to panel
                    _material = basePanel.GetSideFaceMaterial(SideIndex.Top, effect);
                    _group = _material.IsAlpha ? group.Alpha : group.Opaque;
                    _group.Children.Add(HedralGenerator.GeometryModel3DTransform(_perp, _material.Material, null,
                        AnchorFace.YHigh.Transform(), new TranslateTransform3D(0, edgeWidth - 5d, -5d), panelFace.Transform()));

                    // opposite to panel
                    _back = HedralGenerator.RectangularMesh(new Rect(0, 0, 5, edgeWidth), 1, 1, _textureSize);
                    break;

                case FaceEdge.Top:
                    // perp to panel
                    _material = basePanel.GetSideFaceMaterial(SideIndex.Bottom, effect);
                    _group = _material.IsAlpha ? group.Alpha : group.Opaque;
                    _group.Children.Add(HedralGenerator.GeometryModel3DTransform(_perp, _material.Material, null,
                        AnchorFace.YLow.Transform(), new TranslateTransform3D(0, 5d - edgeWidth, -5d), panelFace.Transform()));

                    // opposite to panel
                    _back = HedralGenerator.RectangularMesh(new Rect(0, 5 - edgeWidth, 5, edgeWidth), 1, 1, _textureSize);
                    break;

                case FaceEdge.Left:
                    // perp to panel
                    _material = basePanel.GetSideFaceMaterial(SideIndex.Left, effect);
                    _group = _material.IsAlpha ? group.Alpha : group.Opaque;
                    _group.Children.Add(HedralGenerator.GeometryModel3DTransform(_perp, _material.Material, null,
                        AnchorFace.XHigh.Transform(), new TranslateTransform3D(edgeWidth - 5d, 0, -5d), panelFace.Transform()));

                    // opposite to panel
                    _back = HedralGenerator.RectangularMesh(new Rect(5 - edgeWidth, 0, edgeWidth, 5), 1, 1, _textureSize);
                    break;

                case FaceEdge.Right:
                default:
                    // perp to panel
                    _material = basePanel.GetSideFaceMaterial(SideIndex.Right, effect);
                    _group = _material.IsAlpha ? group.Alpha : group.Opaque;
                    _group.Children.Add(HedralGenerator.GeometryModel3DTransform(_perp, _material.Material, null,
                        AnchorFace.XLow.Transform(), new TranslateTransform3D(5d - edgeWidth, 0, -5d), panelFace.Transform()));

                    // define opposite to panel
                    _back = HedralGenerator.RectangularMesh(new Rect(0, 0, edgeWidth, 5), 1, 1, _textureSize);
                    break;
            }

            // position opposite to panel
            _material = basePanel.GetSideFaceMaterial(SideIndex.Front, effect);
            _group = _material.IsAlpha ? group.Alpha : group.Opaque;
            _group.Children.Add(HedralGenerator.GeometryModel3DTransform(_back, _material.Material, null,
               AnchorFace.ZLow.Transform(), new TranslateTransform3D(0, 0, 0 - thickness), panelFace.Transform()));
        }
        #endregion

        #region public static void AddOuterCornerPanel(BuildableGroup, AnchorFace, AnchorFace, double, FaceEdge, double, VisualEffect, IBasePanel, Transform3D)
        public static void AddOuterCornerPanel(BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace showFace,
            double thickness, FaceEdge cornerEdge, double edgeWidth, VisualEffect effect, IBasePanel basePanel)
        {
            if (panelFace == showFace)
            {
                // outside of panel
                AddOuterRectangularSliver(group, z, y, x, panelFace, cornerEdge, edgeWidth,
                    basePanel.GetBuildableMeshKey(SideIndex.Back, effect),
                    () => basePanel.GetSideFaceMaterial(SideIndex.Back, effect));
            }
            else if (panelFace.GetSnappingEdge(showFace) == cornerEdge)
            {
                var _idx = panelFace.GetSideIndex(showFace);
                // edge of panel
                AddOuterRectangularSliver(group, z, y, x, showFace, showFace.GetSnappingEdge(panelFace), thickness,
                    basePanel.GetBuildableMeshKey(_idx, effect),
                    () => basePanel.GetSideFaceMaterial(_idx, effect));
            }
            else
            {
                // top and bottoms of panel?
                var _showAxis = showFace.GetAxis();

                // not trying to show a side on the panel face axis, nor on the snapped face axis
                var _snapped = panelFace.GetSnappedFace(cornerEdge);
                if ((_showAxis != panelFace.GetAxis()) && (_showAxis != _snapped.GetAxis()))
                {
                    Action<TriangleCorner, SideIndex, double, double> _addCorner = (tri, idx, horiz, vert) =>
                        {
                            AddOuterRectangularCorner(group, z, y, x, showFace, tri, horiz, vert,
                                basePanel.GetBuildableMeshKey(idx, effect),
                                () => basePanel.GetSideFaceMaterial(idx, effect));
                        };
                    switch (panelFace)
                    {
                        case AnchorFace.ZHigh:
                            #region ZHigh Panel Face
                            switch (_snapped)
                            {
                                case AnchorFace.XLow:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.XHigh:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.YLow:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Left, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Right, edgeWidth, thickness);
                                    break;
                                case AnchorFace.YHigh:
                                default:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Left, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Right, edgeWidth, thickness);
                                    break;
                            }
                            break;
                        #endregion

                        case AnchorFace.ZLow:
                            #region ZLow Panel Face
                            switch (_snapped)
                            {
                                case AnchorFace.XLow:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.XHigh:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.YLow:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Right, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Left, edgeWidth, thickness);
                                    break;
                                case AnchorFace.YHigh:
                                default:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Right, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Left, edgeWidth, thickness);
                                    break;
                            }
                            break;
                        #endregion

                        case AnchorFace.YHigh:
                            #region YHigh Panel Face
                            switch (_snapped)
                            {
                                case AnchorFace.XLow:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.XHigh:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.ZLow:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Right, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Left, thickness, edgeWidth);
                                    break;
                                case AnchorFace.ZHigh:
                                default:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Right, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Left, thickness, edgeWidth);
                                    break;
                            }
                            break;
                        #endregion

                        case AnchorFace.YLow:
                            #region YLow Panel Face
                            switch (_snapped)
                            {
                                case AnchorFace.XLow:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.XHigh:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Top, edgeWidth, thickness);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Bottom, edgeWidth, thickness);
                                    break;
                                case AnchorFace.ZLow:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Left, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Right, thickness, edgeWidth);
                                    break;
                                case AnchorFace.ZHigh:
                                default:
                                    if (showFace == AnchorFace.XHigh)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Left, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.XLow)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Right, thickness, edgeWidth);
                                    break;
                            }
                            break;
                        #endregion

                        case AnchorFace.XHigh:
                            #region XHigh Panel Face
                            switch (_snapped)
                            {
                                case AnchorFace.YLow:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Top, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Bottom, thickness, edgeWidth);
                                    break;
                                case AnchorFace.YHigh:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Top, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Bottom, thickness, edgeWidth);
                                    break;
                                case AnchorFace.ZLow:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Left, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Right, thickness, edgeWidth);
                                    break;
                                case AnchorFace.ZHigh:
                                default:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Left, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Right, thickness, edgeWidth);
                                    break;
                            }
                            break;
                        #endregion

                        case AnchorFace.XLow:
                        default:
                            #region XLow Panel Face
                            switch (_snapped)
                            {
                                case AnchorFace.YLow:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Top, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Bottom, thickness, edgeWidth);
                                    break;
                                case AnchorFace.YHigh:
                                    if (showFace == AnchorFace.ZHigh)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Top, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.ZLow)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Bottom, thickness, edgeWidth);
                                    break;
                                case AnchorFace.ZLow:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.LowerRight, SideIndex.Right, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.LowerLeft, SideIndex.Left, thickness, edgeWidth);
                                    break;
                                case AnchorFace.ZHigh:
                                default:
                                    if (showFace == AnchorFace.YHigh)
                                        _addCorner(TriangleCorner.UpperRight, SideIndex.Right, thickness, edgeWidth);
                                    else if (showFace == AnchorFace.YLow)
                                        _addCorner(TriangleCorner.UpperLeft, SideIndex.Left, thickness, edgeWidth);
                                    break;
                            }
                            break;
                            #endregion
                    }
                }
            }
        }
        #endregion

        public static void AddInnerLFramePanel(BuildableGroup group, AnchorFace panelFace, double thickness,
            TriangleCorner corner, double horizontal, double vertical, VisualEffect effect, IBasePanel basePanel)
        {
            AddInnerCornerPanel(group, panelFace, thickness, corner.HorizontalEdge(), horizontal, effect, basePanel);
            AddInnerCornerPanel(group, panelFace, thickness, corner.VerticalEdge(), vertical, effect, basePanel);
        }

        #region public static void AddOuterLFramePanel(BuildableGroup, AnchorFace, AnchorFace, double, TriangleCorner, double, double, VisualEffect, IBasePanel, Transform3D)
        public static void AddOuterLFramePanel(BuildableGroup group, int z, int y, int x, AnchorFace panelFace, AnchorFace showFace, double thickness,
            TriangleCorner corner, double horizontal, double vertical, VisualEffect effect, IBasePanel basePanel)
        {
            if (panelFace == showFace)
            {
                var _meskKey = basePanel.GetBuildableMeshKey(SideIndex.Back, effect);
                Func<BuildableMaterial> _material = () => basePanel.GetSideFaceMaterial(SideIndex.Back, effect);
                // LFrame outside
                AddOuterRectangularSliver(group, z, y, x, panelFace, corner.HorizontalEdge(), horizontal,
                    _meskKey, _material);
                AddOuterRectangularSliver(group, z, y, x, panelFace, corner.VerticalEdge(), vertical,
                    _meskKey, _material);
            }
            else if ((panelFace.VerticalSnappedFace(corner) == showFace)
                || (panelFace.HorizontalSnappedFace(corner) == showFace))
            {
                // face for part of the lframe
                var _idx = panelFace.GetSideIndex(showFace);
                AddOuterRectangularSliver(group, z, y, x, showFace, showFace.GetSnappingEdge(panelFace),
                    thickness, basePanel.GetBuildableMeshKey(_idx, effect),
                    () => basePanel.GetSideFaceMaterial(_idx, effect));
            }
            else if ((panelFace.VerticalSnappedFace(corner) == showFace.ReverseFace())
                    || (panelFace.HorizontalSnappedFace(corner) == showFace.ReverseFace()))
            {
                Action<TriangleCorner, SideIndex, double, double> _addCorner = (tri, idx, horiz, vert) =>
                {
                    AddOuterRectangularCorner(group, z, y, x, showFace, tri, horiz, vert,
                        basePanel.GetBuildableMeshKey(idx, effect),
                        () => basePanel.GetSideFaceMaterial(idx, effect));
                };
                switch (panelFace)
                {
                    case AnchorFace.ZHigh:
                        #region ZHigh Panel Face
                        switch (showFace)
                        {
                            case AnchorFace.YHigh:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.UpperRight : TriangleCorner.UpperLeft,
                                    SideIndex.Top, horizontal, thickness);
                                break;
                            case AnchorFace.YLow:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.UpperLeft : TriangleCorner.UpperRight,
                                    SideIndex.Bottom, horizontal, thickness);
                                break;
                            case AnchorFace.XHigh:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperRight : TriangleCorner.UpperLeft,
                                    SideIndex.Left, vertical, thickness);
                                break;
                            case AnchorFace.XLow:
                            default:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperLeft : TriangleCorner.UpperRight,
                                    SideIndex.Right, vertical, thickness);
                                break;
                        }
                        break;
                    #endregion

                    case AnchorFace.ZLow:
                        #region ZLow Panel Face
                        switch (showFace)
                        {
                            case AnchorFace.YHigh:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.LowerLeft : TriangleCorner.LowerRight,
                                    SideIndex.Top, horizontal, thickness);
                                break;
                            case AnchorFace.YLow:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.LowerRight : TriangleCorner.LowerLeft,
                                    SideIndex.Top, horizontal, thickness);
                                break;
                            case AnchorFace.XHigh:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.LowerRight : TriangleCorner.LowerLeft,
                                    SideIndex.Right, vertical, thickness);
                                break;
                            case AnchorFace.XLow:
                            default:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.LowerLeft : TriangleCorner.LowerRight,
                                    SideIndex.Left, vertical, thickness);
                                break;
                        }
                        break;
                    #endregion

                    case AnchorFace.YHigh:
                        #region YHigh Panel Face
                        switch (showFace)
                        {
                            case AnchorFace.ZHigh:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.UpperRight : TriangleCorner.UpperLeft,
                                    SideIndex.Top, horizontal, thickness);
                                break;
                            case AnchorFace.ZLow:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.UpperLeft : TriangleCorner.UpperRight,
                                    SideIndex.Bottom, horizontal, thickness);
                                break;
                            case AnchorFace.XHigh:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight,
                                    SideIndex.Right, thickness, vertical);
                                break;
                            case AnchorFace.XLow:
                            default:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft,
                                    SideIndex.Left, thickness, vertical);
                                break;
                        }
                        break;
                    #endregion

                    case AnchorFace.YLow:
                        #region YLow Panel Face
                        switch (showFace)
                        {
                            case AnchorFace.ZHigh:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.LowerLeft : TriangleCorner.LowerRight,
                                    SideIndex.Top, horizontal, thickness);
                                break;
                            case AnchorFace.ZLow:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.LowerRight : TriangleCorner.LowerLeft,
                                    SideIndex.Bottom, horizontal, thickness);
                                break;
                            case AnchorFace.XHigh:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft,
                                    SideIndex.Left, thickness, vertical);
                                break;
                            case AnchorFace.XLow:
                            default:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight,
                                    SideIndex.Right, thickness, vertical);
                                break;
                        }
                        break;
                    #endregion

                    case AnchorFace.XHigh:
                        #region XHigh Panel Face
                        switch (showFace)
                        {
                            case AnchorFace.ZHigh:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.LowerRight : TriangleCorner.UpperRight,
                                    SideIndex.Top, thickness, horizontal);
                                break;
                            case AnchorFace.ZLow:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.LowerLeft : TriangleCorner.UpperLeft,
                                    SideIndex.Bottom, thickness, horizontal);
                                break;
                            case AnchorFace.YHigh:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft,
                                    SideIndex.Left, thickness, vertical);
                                break;
                            case AnchorFace.YLow:
                            default:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight,
                                    SideIndex.Right, thickness, vertical);
                                break;
                        }
                        break;
                    #endregion

                    case AnchorFace.XLow:
                    default:
                        #region XLow Panel Face
                        switch (showFace)
                        {
                            case AnchorFace.ZHigh:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft,
                                    SideIndex.Top, thickness, horizontal);
                                break;
                            case AnchorFace.ZLow:
                                _addCorner((corner.HorizontalEdge() == FaceEdge.Left) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight,
                                    SideIndex.Bottom, thickness, horizontal);
                                break;
                            case AnchorFace.YHigh:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperRight : TriangleCorner.LowerRight,
                                    SideIndex.Right, thickness, vertical);
                                break;
                            case AnchorFace.YLow:
                            default:
                                _addCorner((corner.VerticalEdge() == FaceEdge.Top) ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft,
                                    SideIndex.Left, thickness, vertical);
                                break;
                        }
                        break;
                        #endregion
                }
            }
        }
        #endregion

        #region private static SideIndex SlopeSideIndex(AnchorFace source, AnchorFace sink, AnchorFace show)
        /// <summary>Maps source sink and show face to sideIndex for slopes</summary>
        private static SideIndex SlopeSideIndex(AnchorFace source, AnchorFace sink, AnchorFace show)
        {
            Func<SideIndex, SideIndex, SideIndex> _sameThen =
                (ifTrue, ifFalse) => (sink.IsLowFace() == show.IsLowFace()) ? ifTrue : ifFalse;

            Func<Axis, SideIndex> _leftRight = (axis) =>
            {
                if (sink.GetAxis() == axis)
                    return _sameThen(SideIndex.Left, SideIndex.Right);
                else
                    return _sameThen(SideIndex.Right, SideIndex.Left);
            };

            switch (source)
            {
                case AnchorFace.ZLow:
                    return _leftRight(Axis.Y);

                case AnchorFace.ZHigh:
                    return _leftRight(Axis.X);

                case AnchorFace.YLow:
                    return _leftRight(Axis.X);

                case AnchorFace.YHigh:
                    return _leftRight(Axis.Z);

                case AnchorFace.XLow:
                    return _leftRight(Axis.Z);

                case AnchorFace.XHigh:
                default:
                    return _leftRight(Axis.Y);
            }
        }
        #endregion

        #region public static void AddOuterSlopeComposite(BuildableGroup, PanelParams, AnchorFace, double, double, VisualEffect, IBasePanel, Transform3D)
        public static void AddOuterSlopeComposite(BuildableGroup group, PanelParams param, int z, int y, int x, AnchorFace showFace,
            double thickness, double slopeThickness, VisualEffect effect, IBasePanel panel)
        {
            if (param.IsTrueSlope)
            {
                var _edge = showFace.GetSnappingEdge(param.SourceFace);
                var _maxThick = Math.Max(thickness, slopeThickness);
                var _minThick = Math.Min(thickness, slopeThickness);
                if (param.IsFaceSlopeSide(showFace))
                {
                    // parameters
                    var _srcEdge = showFace.GetSnappingEdge(param.SourceFace);
                    var _snkEdge = showFace.GetSnappingEdge(param.SinkFace);
                    var _isTall = (_srcEdge == FaceEdge.Left) || (_srcEdge == FaceEdge.Right);
                    var _corner = TriangleCornerHelper.GetFromFaceEdges(_srcEdge, _snkEdge);
                    var _side = SlopeSideIndex(param.SourceFace, param.SinkFace, showFace);

                    // rectangular part
                    var _meshKey = panel.GetBuildableMeshKey(_side, effect);
                    Func<BuildableMaterial> _material = () => panel.GetSideFaceMaterial(_side, effect);
                    AddOuterRectangularSliver(group, z, y, x, showFace, _edge, _minThick,
                        _meshKey, _material);

                    // triangular part
                    AddOuterTriangularSlope(group, z, y, x, showFace, _corner, _isTall,
                        _minThick, _maxThick - _minThick,
                        _meshKey, _material);
                }
                else if (param.IsFaceSlopeEnd(showFace))
                {
                    if (showFace == param.SinkFace)
                    {
                        // sink end is the high thickness
                        AddOuterRectangularSliver(group, z, y, x, showFace, _edge,
                            _maxThick, panel.GetBuildableMeshKey(SideIndex.Bottom, effect),
                            () => panel.GetSideFaceMaterial(SideIndex.Bottom, effect));
                    }
                    else
                    {
                        // opposite end is the low thickness
                        AddOuterRectangularSliver(group, z, y, x, showFace, _edge,
                            _minThick, panel.GetBuildableMeshKey(SideIndex.Top, effect),
                            () => panel.GetSideFaceMaterial(SideIndex.Top, effect));
                    }
                }
                else if (param.IsFaceSlopeBottom(showFace))
                {
                    // if slope source, then complete blockage
                    group.Context.GetBuildableMesh(
                        panel.GetBuildableMeshKey(SideIndex.Back, effect),
                        () => panel.GetSideFaceMaterial(SideIndex.Back, effect))
                        .AddCellFace(showFace, new Vector3D(x * 5.0d, y * 5.0d, z * 5.0d));
                }
            }
        }
        #endregion

        #region public static void AddOuterDiagonalComposite(BuildableGroup, PanelParams, AnchorFace, VisualEffect, IBasePanel, Transform3D)
        public static void AddOuterDiagonalComposite(BuildableGroup group, PanelParams param, int z, int y, int x, AnchorFace showFace,
            VisualEffect effect, IBasePanel panel, Vector3D bump)
        {
            var _meshKey = panel.GetBuildableMeshKey(showFace, effect);
            Func<BuildableMaterial> _material = () => panel.GetAnchorFaceMaterial(showFace, effect);
            if (param.IsFaceDiagonalBinder(showFace) || param.IsFaceBendableSource(showFace))
            {
                group.Context.GetBuildableMesh(_meshKey, _material)
                    .AddCellFace(showFace, new Vector3D(x * 5.0d, y * 5.0d, z * 5.0d));
            }
            else
            {
                // right triangle
                var _corner = TriangleCorner.LowerLeft;
                if (param.IsFaceDiagonalSide(showFace))
                {
                    // based on diagonal control faces
                    _corner = TriangleCornerHelper.GetFromFaceEdges(
                        param.DiagonalFaceControlFaces(showFace).ToAnchorFaces()
                        .Select(_f => showFace.GetSnappingEdge(_f)).ToArray());
                }
                else if (param.IsFaceTriangularSink(showFace))
                {
                    // based on 
                    _corner = TriangleCornerHelper.GetFromFaceEdges(
                        param.TriangularSinkEdges(showFace).ToAnchorFaces()
                        .Select(_f => showFace.GetSnappingEdge(_f)).ToArray());
                }
                AddOuterTriangularHalf(group, z, y, x, showFace, _corner, _meshKey, _material);
            }
        }
        #endregion

        #region public static void AddInnerDiagonalComposite(BuildableGroup, PanelParams, VisualEffect, IBasePanel)
        public static void AddInnerDiagonalComposite(BuildableGroup group, PanelParams param, VisualEffect effect, IBasePanel panel)
        {
            var _material = panel.GetWedgeMaterial(effect);
            var _cellGroup = _material.IsAlpha ? group.Alpha : group.Opaque;
            if (param.DiagonalControls != AnchorFaceList.None)
            {
                // diagonal
                AddOneDiagonalFromComposite(group, param.SourceFace, param.SinkFace, _material);
                if (param.OtherFace != OptionalAnchorFace.None)
                {
                    // double diagonal
                    AddOneDiagonalFromComposite(group, param.OtherFace.ToAnchorFace(), param.SinkFace, _material);
                }
            }
            else
            {
                var _bend = param.BendControls;
                if (_bend != AnchorFaceList.None)
                {
                    // bend
                    var _pts = new Point3D[]
                    {
                        (new Point3D(0, 0, 0)),  // 0 = XL YL ZL
                        (new Point3D(0, 0, 5)),  // 1 = XL YL ZH
                        (new Point3D(0, 5, 0)),  // 2 = XL YH ZL
                        (new Point3D(0, 5, 5)),  // 3 = XL YH ZH
                        (new Point3D(5, 0, 0)),  // 4 = XH YL ZL
                        (new Point3D(5, 0, 5)),  // 5 = XH YL ZH
                        (new Point3D(5, 5, 0)),  // 6 = XH YH ZL
                        (new Point3D(5, 5, 5))   // 7 = XH YH ZH
                    };
                    Action<int, int, int, int, int, int, int, int> _addTriangles =
                        (p1, p2, p3, p4, p5, p6, p7, p8) =>
                        {
                            CellSpaceFaces.AddGeometry(_cellGroup, new GeometryModel3D(InnerBendFace(true, new PlanarPoints(_pts[p1], _pts[p2], _pts[p3], _pts[p4])), _material.Material));
                            CellSpaceFaces.AddGeometry(_cellGroup, new GeometryModel3D(InnerBendFace(false, new PlanarPoints(_pts[p5], _pts[p6], _pts[p7], _pts[p8])), _material.Material));
                        };
                    switch (param.SourceFace)
                    {
                        case AnchorFace.XLow:
                            #region XLow
                            if (_bend.Contains(AnchorFace.YLow))
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(2, 3, 5, 4, 3, 1, 4, 6);
                                else
                                    _addTriangles(0, 2, 7, 5, 2, 3, 5, 4);
                            }
                            else
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(3, 1, 4, 6, 1, 0, 6, 7);
                                else
                                    _addTriangles(1, 0, 6, 7, 0, 2, 7, 5);
                            }
                            break;
                        #endregion

                        case AnchorFace.XHigh:
                            #region XHigh
                            if (_bend.Contains(AnchorFace.YLow))
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(5, 7, 2, 0, 7, 6, 0, 1);
                                else
                                    _addTriangles(7, 6, 0, 1, 6, 4, 1, 3);
                            }
                            else
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(4, 5, 3, 2, 5, 7, 2, 0);
                                else
                                    _addTriangles(6, 4, 1, 3, 4, 5, 3, 2);
                            }
                            break;
                        #endregion

                        case AnchorFace.YLow:
                            #region YLow
                            if (_bend.Contains(AnchorFace.XLow))
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(1, 5, 6, 2, 5, 4, 2, 3);
                                else
                                    _addTriangles(5, 4, 2, 3, 4, 0, 3, 7);
                            }
                            else
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(0, 1, 7, 6, 1, 5, 6, 2);
                                else
                                    _addTriangles(4, 0, 3, 7, 0, 1, 7, 6);
                            }
                            break;
                        #endregion

                        case AnchorFace.YHigh:
                            #region YHigh
                            if (_bend.Contains(AnchorFace.XLow))
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(6, 7, 1, 0, 7, 3, 0, 4);
                                else
                                    _addTriangles(2, 6, 5, 1, 6, 7, 1, 0);
                            }
                            else
                            {
                                if (_bend.Contains(AnchorFace.ZLow))
                                    _addTriangles(7, 3, 0, 4, 3, 2, 4, 5);
                                else
                                    _addTriangles(3, 2, 4, 5, 2, 6, 5, 1);
                            }
                            break;
                        #endregion

                        case AnchorFace.ZLow:
                            #region ZLow
                            if (_bend.Contains(AnchorFace.XLow))
                            {
                                if (_bend.Contains(AnchorFace.YLow))
                                    _addTriangles(4, 6, 3, 1, 6, 2, 1, 5);
                                else
                                    _addTriangles(0, 4, 7, 3, 4, 6, 3, 1);
                            }
                            else
                            {
                                if (_bend.Contains(AnchorFace.YLow))
                                    _addTriangles(6, 2, 1, 5, 2, 0, 5, 7);
                                else
                                    _addTriangles(2, 0, 5, 7, 0, 4, 7, 3);
                            }
                            break;
                        #endregion

                        case AnchorFace.ZHigh:
                        default:
                            #region ZHigh
                            if (_bend.Contains(AnchorFace.XLow))
                            {
                                if (_bend.Contains(AnchorFace.YLow))
                                    _addTriangles(3, 7, 4, 0, 7, 5, 0, 2);
                                else
                                    _addTriangles(7, 5, 0, 2, 5, 1, 2, 6);
                            }
                            else
                            {
                                if (_bend.Contains(AnchorFace.YLow))
                                    _addTriangles(1, 3, 6, 4, 3, 7, 4, 0);
                                else
                                    _addTriangles(5, 1, 2, 6, 1, 3, 6, 4);
                            }
                            break;
                            #endregion
                    }
                }
            }
        }
        #endregion

        // building blocks

        #region private static MeshGeometry3D InnerBendFace(bool leftSide, PlanarPoints points)
        private static MeshGeometry3D InnerBendFace(bool leftSide, PlanarPoints points)
        {
            var _mesh = new MeshGeometry3D
            {
                TriangleIndices = new System.Windows.Media.Int32Collection(
                    leftSide
                    ? new int[] { 0, 1, 3 }
                    : new int[] { 0, 1, 2 }),
                Positions = new Point3DCollection(points),
                Normals = new Vector3DCollection(points.Select(_p => points.Normal))
            };
            _mesh.TextureCoordinates.Add(new Point(0, 0));
            _mesh.TextureCoordinates.Add(new Point(0, 1));
            _mesh.TextureCoordinates.Add(new Point(1, 1));
            _mesh.TextureCoordinates.Add(new Point(1, 0));
            return _mesh;
        }
        #endregion

        #region private static void AddOneDiagonalFromComposite(BuildableGroup group, AnchorFace face1, AnchorFace face2, BuildableMaterial material)
        private static void AddOneDiagonalFromComposite(BuildableGroup group, AnchorFace face1, AnchorFace face2, BuildableMaterial material)
        {
            var _builder = new MeshBuilder();
            var _cellGroup = material.IsAlpha ? group.Alpha : group.Opaque;
            var _pts = GetTwoFaceDiagonal(face1, face2);
            _builder.AddQuad(_pts[0], _pts[1], _pts[2], _pts[3], new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0));
            var _mesh = _builder.ToMesh();
            CellSpaceFaces.AddGeometry(_cellGroup, new GeometryModel3D(_mesh, material.Material));
        }
        #endregion

        #region private static PlanarPoints GetTwoFaceDiagonal(params AnchorFace[] faces)
        private static PlanarPoints GetTwoFaceDiagonal(params AnchorFace[] faces)
        {
            var _pts = new Point3D[]
            {
                (new Point3D(0, 0, 0)),  // 0 = XL YL ZL
                (new Point3D(0, 0, 5)),  // 1 = XL YL ZH
                (new Point3D(0, 5, 0)),  // 2 = XL YH ZL
                (new Point3D(0, 5, 5)),  // 3 = XL YH ZH
                (new Point3D(5, 0, 0)),  // 4 = XH YL ZL
                (new Point3D(5, 0, 5)),  // 5 = XH YL ZH
                (new Point3D(5, 5, 0)),  // 6 = XH YH ZL
                (new Point3D(5, 5, 5))   // 7 = XH YH ZH
            };

            Func<int, int, int, int, PlanarPoints> _diagPlane =
                (p1, p2, p3, p4) => new PlanarPoints(_pts[p1], _pts[p2], _pts[p3], _pts[p4]);

            if (faces.Contains(AnchorFace.XLow))
            {
                if (faces.Contains(AnchorFace.YLow))
                    return _diagPlane(2, 3, 5, 4);
                else if (faces.Contains(AnchorFace.YHigh))
                    return _diagPlane(1, 0, 6, 7);
                else if (faces.Contains(AnchorFace.ZLow))
                    return _diagPlane(4, 6, 3, 1);
                else // ZHigh
                    return _diagPlane(0, 2, 7, 5);
            }
            else if (faces.Contains(AnchorFace.XHigh))
            {
                if (faces.Contains(AnchorFace.YLow))
                    return _diagPlane(7, 6, 0, 1);
                else if (faces.Contains(AnchorFace.YHigh))
                    return _diagPlane(4, 5, 3, 2);
                else if (faces.Contains(AnchorFace.ZLow))
                    return _diagPlane(2, 0, 5, 7);
                else // ZHigh
                    return _diagPlane(6, 4, 1, 3);
            }
            else if (faces.Contains(AnchorFace.YLow))
            {
                if (faces.Contains(AnchorFace.ZLow))
                    return _diagPlane(6, 2, 1, 5);
                else // ZHigh
                    return _diagPlane(4, 0, 3, 7);
            }
            else // YHigh
            {
                if (faces.Contains(AnchorFace.ZLow))
                    return _diagPlane(0, 4, 7, 3);
                else // ZHigh
                    return _diagPlane(2, 6, 5, 1);
            }
        }
        #endregion

        #region public static void AddOuterRectangularCorner(BuildableGroup group, AnchorFace panelFace, TriangleCorner corner, double horizontal, double vertical, Material material, Transform3D bump)
        public static void AddOuterRectangularCorner(BuildableGroup group, int z, int y, int x,
            AnchorFace panelFace, TriangleCorner corner, double horizontal, double vertical,
            BuildableMeshKey meshKey, Func<BuildableMaterial> builder)
        {
            if ((horizontal == 0) || (vertical == 0))
                return;

            // mesh parameters
            var _textureSize = new Vector(5d, 5d);
            var _size = new Size(horizontal, vertical);
            var _start = new Point();
            switch (corner)
            {
                case TriangleCorner.LowerLeft:
                    _start = new Point(0d, 0d);
                    break;
                case TriangleCorner.LowerRight:
                    _start = new Point(5d - horizontal, 0d);
                    break;
                case TriangleCorner.UpperLeft:
                    _start = new Point(0d, 5d - vertical);
                    break;
                case TriangleCorner.UpperRight:
                    _start = new Point(5d - horizontal, 5d - vertical);
                    break;
            }

            group.Context.GetBuildableMesh(meshKey, builder)
                .AddRectangularMesh(new Rect(_start, _size), _textureSize, true, panelFace, false, (new Vector3D(x * 5, y * 5, z * 5)));
        }
        #endregion

        #region public static void AddOuterRectangularSliver(BuildableGroup group, AnchorFace panelFace, FaceEdge edge, double thickness, Material material, Transform3D bump)
        public static void AddOuterRectangularSliver(BuildableGroup group, int z, int y, int x,
            AnchorFace panelFace, FaceEdge edge, double thickness,
            BuildableMeshKey meshKey, Func<BuildableMaterial> builder)
        {
            // mesh parameters
            var _textureSize = new Vector(5d, 5d);
            var _size = new Size();
            var _start = new Point();
            switch (edge)
            {
                case FaceEdge.Bottom:
                    _start = new Point(0d, 0d);
                    _size = new Size(5d, thickness);
                    break;
                case FaceEdge.Left:
                    _start = new Point(0d, 0d);
                    _size = new Size(thickness, 5d);
                    break;
                case FaceEdge.Right:
                    _start = new Point(5d - thickness, 0d);
                    _size = new Size(thickness, 5d);
                    break;
                case FaceEdge.Top:
                    _start = new Point(0d, 5d - thickness);
                    _size = new Size(5d, thickness);
                    break;
            }

            group.Context.GetBuildableMesh(meshKey, builder)
                .AddRectangularMesh(new Rect(_start, _size), _textureSize, true, panelFace, false, (new Vector3D(x * 5, y * 5, z * 5)));
        }
        #endregion

        #region public static void AddOuterTriangularHalf(BuildableGroup group, AnchorFace panelFace, TriangleCorner corner, Material material, Transform3D bump)
        public static void AddOuterTriangularHalf(BuildableGroup group, int z, int y, int x,
            AnchorFace panelFace, TriangleCorner corner,
            BuildableMeshKey meshKey, Func<BuildableMaterial> builder)
        {
            // mesh parameters
            var _textureSize = new Vector(5d, 5d);
            var _rect = new Rect(0d, 0d, 5d, 5d);

            group.Context.GetBuildableMesh(meshKey, builder)
                .AddRightTriangularMesh(_rect, corner, _textureSize, true, panelFace, false, (new Vector3D(x * 5, y * 5, z * 5)));
        }
        #endregion

        #region public static void AddOuterTriangularSlope(BuildableGroup group, AnchorFace panelFace, TriangleCorner corner, bool isTall, double offset, double thickness, BuildableMaterial material, Transform3D bump)
        public static void AddOuterTriangularSlope(BuildableGroup group, int z, int y, int x,
            AnchorFace panelFace, TriangleCorner corner, bool isTall, double offset, double thickness,
            BuildableMeshKey meshKey, Func<BuildableMaterial> builder)
        {
            if (thickness == 0d)
                return;

            // mesh parameters
            var _textureSize = new Vector(5d, 5d);
            var _rect = new Rect();
            if (isTall)
            {
                // tall
                switch (corner)
                {
                    case TriangleCorner.LowerLeft:
                    case TriangleCorner.UpperLeft:
                        _rect = new Rect(offset, 0, thickness, 5d);
                        break;
                    case TriangleCorner.LowerRight:
                    case TriangleCorner.UpperRight:
                        _rect = new Rect(5d - (thickness + offset), 0, thickness, 5d);
                        break;
                }
            }
            else
            {
                // wide
                switch (corner)
                {
                    case TriangleCorner.LowerLeft:
                    case TriangleCorner.LowerRight:
                        _rect = new Rect(0, offset, 5, thickness);
                        break;

                    case TriangleCorner.UpperLeft:
                    case TriangleCorner.UpperRight:
                        _rect = new Rect(0, 5d - (thickness + offset), 5d, thickness);
                        break;
                }
            }

            group.Context.GetBuildableMesh(meshKey, builder)
                .AddRightTriangularMesh(_rect, corner, _textureSize, true, panelFace, false, (new Vector3D(x * 5, y * 5, z * 5)));
        }
        #endregion
    }
}
