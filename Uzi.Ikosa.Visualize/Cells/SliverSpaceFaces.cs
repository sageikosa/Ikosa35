using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Uzi.Visualize
{
    public static class SliverSpaceFaces
    {
        public static void AddInnerStructures(SliverSlopeParams param, IPlusCellSpace plus, ICellEdge edge,
            BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            var _sliver = plus;
            var _flipAxis = param.Flip;
            if (_flipAxis)
                _sliver = new SliverInverter(plus);

            if (!(_sliver.IsInvisible))
                GenerateInnerSubModel(param, z, y, x, _sliver, addToGroup, false, effect);
            if (!(_sliver.IsPlusInvisible))
                GenerateInnerSubModel(param, z, y, x, _sliver, addToGroup, true, effect);
            foreach (var _edge in param.GetSliverEdges(edge))
            {
                WedgeSpaceFaces.BuildInnerStructures(addToGroup, z, y, x, _edge.Axis, _edge.PrimeOff, _edge.SecondOff,
                    true, true, false,
                    (face) => edge.GetBuildableMeshKey(face, effect),
                    (face) => edge.GetBuildableMeshKey(face, effect),
                    (face) => (Func<BuildableMaterial>)(() => _edge.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect)),
                    (face) => (Func<BuildableMaterial>)(() => _edge.GetPlusOrthoFaceMaterial(face.GetAxis(), face.IsLowFace(), effect)),
                    () => _edge.GetOtherFaceMaterial(0, effect),
                    () => _edge.GetPlusOtherFaceMaterial(0, effect));
            }
        }

        #region private static void GenerateMesh(...)
        private static void GenerateMesh(Axis orthoAxis, AnchorFace face,
            bool isPlusMaterial, double offSet, ICellLocation location,
            BuildableGroup addToGroup, BuildableMeshKey meshKey, Func<BuildableMaterial> material,
            params Vector3D[] trans)
        {
            var _textureSize = new Vector(5d, 5d);

            // generate model, apply material, and move into place
            void _add(Rect rect) => addToGroup.Context.GetBuildableMesh(meshKey, material)
                .AddRectangularMesh(rect, _textureSize, true, face, false, trans);

            switch (orthoAxis)
            {
                case Axis.Y:
                    if (isPlusMaterial)
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                                _add(new Rect(0, 0, 5d - offSet, 5d));
                                break;
                            case AnchorFace.XHigh:
                                _add(new Rect(offSet, 0, 5d - offSet, 5d));
                                break;
                            case AnchorFace.YLow:
                            case AnchorFace.YHigh:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;
                            case AnchorFace.ZLow:
                            default:
                                _add(new Rect(0, offSet, 5d, 5d - offSet));
                                break;
                        }
                    }
                    else
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                                _add(new Rect(5d - offSet, 0, offSet, 5d));
                                break;
                            case AnchorFace.XHigh:
                                _add(new Rect(0, 0, offSet, 5d));
                                break;
                            case AnchorFace.YLow:
                            case AnchorFace.YHigh:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;
                            case AnchorFace.ZLow:
                            default:
                                _add(new Rect(0, 0, 5d, offSet));
                                break;
                        }
                    }

                    break;

                case Axis.Z:
                    if (isPlusMaterial)
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                            case AnchorFace.XHigh:
                            case AnchorFace.YLow:
                            case AnchorFace.YHigh:
                                _add(new Rect(0d, offSet, 5d, 5d - offSet));
                                break;
                            case AnchorFace.ZLow:
                            default:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;
                        }
                    }
                    else
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                            case AnchorFace.XHigh:
                            case AnchorFace.YLow:
                            case AnchorFace.YHigh:
                                _add(new Rect(0d, 0d, 5d, offSet));
                                break;
                            case AnchorFace.ZLow:
                            default:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;
                        }
                    }

                    break;

                case Axis.X:
                    if (isPlusMaterial)
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                            case AnchorFace.XHigh:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;
                            case AnchorFace.YHigh:
                            case AnchorFace.ZLow:
                                _add(new Rect(0, 0, 5d - offSet, 5d));
                                break;
                            case AnchorFace.YLow:
                            default:
                                _add(new Rect(offSet, 0, 5d - offSet, 5d));
                                break;
                        }
                    }
                    else
                    {
                        switch (face)
                        {
                            case AnchorFace.XLow:
                            case AnchorFace.XHigh:
                                addToGroup.Context.GetBuildableMesh(meshKey, material)
                                    .AddCellFace(face, trans);
                                break;
                            case AnchorFace.YHigh:
                            case AnchorFace.ZLow:
                                _add(new Rect(5d - offSet, 0, offSet, 5d));
                                break;
                            case AnchorFace.YLow:
                            default:
                                _add(new Rect(0, 0, offSet, 5d));
                                break;
                        }
                    }
                    break;
            }
        }
        #endregion

        #region private static void GenerateInnerSubModel(SliverSlopeParams param, IPlusCellSpace plus, BuildableGroup group, bool isPlus, VisualEffect effect)
        private static void GenerateInnerSubModel(SliverSlopeParams param, int z, int y, int x, IPlusCellSpace plus, BuildableGroup group, bool isPlus, VisualEffect effect)
        {
            var _ortho = param.Axis;
            var _move = new Vector3D(x * 5, y * 5, z * 5);

            void _addRect(AnchorFace face, Vector3D bump)
                => group.Context.GetBuildableMesh(
                    isPlus ? plus.GetPlusBuildableMeshKey(face, effect) : plus.GetBuildableMeshKey(face, effect),
                    isPlus ? (Func<BuildableMaterial>)(() => plus.GetPlusOrthoFaceMaterial(_ortho, isPlus, effect)) : () => plus.GetOrthoFaceMaterial(_ortho, !isPlus, effect))
                    .AddRectangularMesh(new Rect(0d, 0d, 5d, 5d), new Vector(5d, 5d), true,
                    face, true, bump, _move);

            switch (_ortho)
            {
                case Axis.X:
                    {
                        if (isPlus)
                        {
                            _addRect(AnchorFace.XLow, new Vector3D(param.LoFlippableOffset, 0, 0));
                        }
                        else
                        {
                            _addRect(AnchorFace.XHigh, new Vector3D(param.LoFlippableOffset - 5d, 0, 0));
                        }
                    }
                    break;

                case Axis.Y:
                    if (isPlus)
                    {
                        _addRect(AnchorFace.YLow, new Vector3D(0, param.LoFlippableOffset, 0));
                    }
                    else
                    {
                        _addRect(AnchorFace.YHigh, new Vector3D(0, param.LoFlippableOffset - 5d, 0));
                    }
                    break;

                case Axis.Z:
                    if (isPlus)
                    {
                        _addRect(AnchorFace.ZLow, new Vector3D(0, 0, param.LoFlippableOffset));
                    }
                    else
                    {
                        _addRect(AnchorFace.ZHigh, new Vector3D(0, 0, param.LoFlippableOffset));
                    }
                    break;
            }
        }
        #endregion

        #region public static void AddOuterSurface(SliverSlopeParams param, IPlusCellSpace plus, ICellEdge edge, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        public static void AddOuterSurface(SliverSlopeParams param, IPlusCellSpace plus, ICellEdge edge, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            var _sliver = plus;
            var _flipAxis = param.Flip;
            if (_flipAxis)
                _sliver = new SliverInverter(plus);

            if (!_sliver.IsInvisible)
                GenerateOuterSubModel(param, _sliver, group, _sliver.IsGas || _sliver.IsLiquid,
                    z, y, x, false, face, effect, bump);
            if (!_sliver.IsPlusInvisible)
                GenerateOuterSubModel(param, _sliver, group, _sliver.IsPlusGas || _sliver.IsPlusLiquid,
                    z, y, x, true, face, effect, bump); // NOTE: fixed(?) isPlus flag

            Func<BuildableMaterial> _builder =
                () => edge.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
            var _move = new Vector3D(x * 5, y * 5, z * 5);
            var _meshKey = edge.GetBuildableMeshKey(face, effect);

            foreach (var _edge in param.GetSliverEdges(edge))
            {
                WedgeSpaceFaces.BuildOuterWedgeModel(group, face, _edge.Axis, _edge.PrimeOff, _edge.SecondOff, true,
                    _meshKey, _builder, _move);
            }
        }
        #endregion

        #region private static void GenerateOuterSubModel(SliverSlopeParams param, IPlusCellSpace plus, BuildableGroup group, bool showInner, int z, int y, int x, bool isPlus, VisualEffect effect, Transform3D bump)
        private static void GenerateOuterSubModel(SliverSlopeParams param, IPlusCellSpace plus, BuildableGroup group, bool showInner,
            int z, int y, int x, bool isPlus, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            #region face visible logic
            switch (face)
            {
                case AnchorFace.ZLow:
                    z--;
                    break;

                case AnchorFace.ZHigh:
                    z++;
                    break;

                case AnchorFace.YLow:
                    y--;
                    break;

                case AnchorFace.YHigh:
                    y++;
                    break;

                case AnchorFace.XLow:
                    x--;
                    break;

                case AnchorFace.XHigh:
                    x++;
                    break;
            }
            #endregion

            #region Inward visibility Tests
            var _sliverAxis = param.Axis;

            // inward faces (liquids and non-invisible gases)
            if (showInner)
            {
                if (!isPlus &&
                    (((_sliverAxis == Axis.X) && (face == AnchorFace.XHigh))
                    || ((_sliverAxis == Axis.Y) && (face == AnchorFace.YHigh))
                    || ((_sliverAxis == Axis.Z) && (face == AnchorFace.ZHigh))))
                {
                    // minus material does not expose itself to the upper face of the ortho axis
                    return;
                }
                else if (isPlus &&
                    (((_sliverAxis == Axis.X) && (face == AnchorFace.XLow))
                    || ((_sliverAxis == Axis.Y) && (face == AnchorFace.YLow))
                    || ((_sliverAxis == Axis.Z) && (face == AnchorFace.ZLow))))
                {
                    // plus material does not expose itself to the lower face of the ortho axis
                    return;
                }
            }
            #endregion

            // TODO: hidden face optimizations

            var _builder = isPlus
                ? (Func<BuildableMaterial>)(() => plus.GetPlusOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect))
                : (Func<BuildableMaterial>)(() => plus.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect));

            var _meshKey = isPlus
                ? plus.GetPlusBuildableMeshKey(face, effect)
                : plus.GetBuildableMeshKey(face, effect);

            var _move = new Vector3D(x * 5, y * 5, z * 5);

            // generate model, apply material, and move into place
            GenerateMesh(_sliverAxis, face, isPlus, param.LoFlippableOffset, new CellPosition(z, y, x),
                group, _meshKey, _builder, _move, bump);
        }
        #endregion

        #region public static bool OccludesFace(uint param, IPlusCellSpace plus, AnchorFace outwardFace)
        /// <summary>True if face has no invisible components</summary>
        public static bool OccludesFace(uint param, IPlusCellSpace plus, AnchorFace outwardFace)
        {
            var _param = new SliverSlopeParams(param);
            var _ortho = _param.Axis;
            if (outwardFace.IsOrthogonalTo(_ortho))
            {
                var _flip = _param.Flip;
                if (outwardFace.IsLowFace() ^ _flip)
                    return !plus.IsInvisible;
                else
                    return !plus.IsPlusInvisible;
            }
            else
                return !plus.IsInvisible && !plus.IsPlusInvisible;
        }
        #endregion

        #region public static bool ShowFace(uint param, IPlusCellSpace plus, AnchorFace outwardFace)
        /// <summary>True if face has only visible components</summary>
        public static bool ShowFace(uint param, IPlusCellSpace plus, AnchorFace outwardFace)
        {
            var _param = new SliverSlopeParams(param);
            var _ortho = _param.Axis;
            if (outwardFace.IsOrthogonalTo(_ortho))
            {
                var _flip = _param.Flip;
                return ((outwardFace.IsLowFace() ^ _flip)
                    ? !plus.IsInvisible
                    : !plus.IsPlusInvisible) || _param.HasEdges(outwardFace);
            }
            else
                return !plus.IsInvisible || !plus.IsPlusInvisible || _param.HasEdges(outwardFace);
        }
        #endregion
    }
}
