using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace Uzi.Visualize
{
    public static class WedgeSpaceFaces
    {
        /// <summary>AnchorFaceList containing the two sides making the wedge edge</summary>
        public static AnchorFaceList GetWedgeEdge(uint param, double offset1, double offset2)
        {
            var _param = new WedgeParams(param);
            var _primeHigh = _param.PrimaryOffset(offset1, offset2) < 0;
            var _secondHigh = _param.SecondaryOffset(offset1, offset2) < 0;
            switch (_param.Axis)
            {
                case Axis.Z:
                    return AnchorFaceListHelper.Create(_primeHigh ? AnchorFace.XHigh : AnchorFace.XLow, _secondHigh ? AnchorFace.YHigh : AnchorFace.YLow);

                case Axis.Y:
                    return AnchorFaceListHelper.Create(_primeHigh ? AnchorFace.ZHigh : AnchorFace.ZLow, _secondHigh ? AnchorFace.XHigh : AnchorFace.XLow);

                case Axis.X:
                default:
                    return AnchorFaceListHelper.Create(_primeHigh ? AnchorFace.YHigh : AnchorFace.YLow, _secondHigh ? AnchorFace.ZHigh : AnchorFace.ZLow);
            }
        }

        public static void AddOuterSurface(uint param, IWedgeSpace wedge, BuildableGroup addToGroup, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            GenerateOuterFillerModel(param, wedge, addToGroup, z, y, x, face, effect, bump);
            GenerateOuterWedgeModel(param, wedge, addToGroup, z, y, x, face, effect, bump);
        }

        #region private static void GenerateOuterWedgeModel(uint param, IWedgeSpace wedge, BuildableGroup addToGroup, AnchorFace face, VisualEffect effect, Transform3D bump)
        private static void GenerateOuterWedgeModel(uint param, IWedgeSpace wedge, BuildableGroup addToGroup,
            int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            // invisible gases need not apply
            if (!wedge.IsInvisible)
            {
                var _param = new WedgeParams(param);
                var _axis = _param.Axis;
                var _primeOff = _param.PrimaryOffset(wedge.Offset1, wedge.Offset2);
                var _secondOff = _param.SecondaryOffset(wedge.Offset1, wedge.Offset2);
                var _corner = wedge.CornerStyle;

                Func<BuildableMaterial> _builder =
                    () => wedge.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);

                var _meshKey = wedge.GetBuildableMeshKey(face, effect);

                // mesh and faces
                var _move = new Vector3D(x * 5, y * 5, z * 5);

                BuildOuterWedgeModel(addToGroup, face, _axis, _primeOff, _secondOff, _corner,
                    _meshKey, _builder, _move);
            }
        }
        #endregion

        internal static void BuildOuterWedgeModel(BuildableGroup addToGroup, AnchorFace face,
            Axis axis, double primeOff, double secondOff, bool corner, BuildableMeshKey meshKey,
            Func<BuildableMaterial> builder, params Vector3D[] bump)
        {
            // needed stuff
            var _textureSize = new Vector(5d, 5d);
            var _wedgeTextureSize = new Vector(5d, 7.07d);
            var _magPrimary = Math.Abs(primeOff);
            var _magSecondary = Math.Abs(secondOff);
            var _revPrimary = 5d - _magPrimary;
            var _revSecondary = 5d - _magSecondary;

            void _addRect(Rect rect)
                => addToGroup.Context.GetBuildableMesh(meshKey, builder)
                .AddRectangularMesh(rect, _textureSize, true, face, false, bump);

            void _addTri(Rect rect, TriangleCorner triCorner)
                => addToGroup.Context.GetBuildableMesh(meshKey, builder)
                .AddRightTriangularMesh(rect, triCorner, _textureSize, true, face, false, bump);

            switch (axis)
            {
                case Axis.Z:
                    #region Z Axis Outer Surfaces
                    switch (face)
                    {
                        case AnchorFace.XLow:
                            if (primeOff > 0)
                            {
                                if (secondOff > 0)
                                    _addRect(new Rect(5d - secondOff, 0d, secondOff, 5d));
                                else if (secondOff < 0)
                                    _addRect(new Rect(0, 0, Math.Abs(secondOff), 5d));
                            }
                            break;

                        case AnchorFace.XHigh:
                            if (primeOff < 0)
                            {
                                if (secondOff > 0)
                                    _addRect(new Rect(0, 0, secondOff, 5d));
                                else if (secondOff < 0)
                                    _addRect(new Rect(5d + secondOff, 0d, Math.Abs(secondOff), 5d));
                            }
                            break;

                        case AnchorFace.YLow:
                            if (secondOff > 0)
                            {
                                if (primeOff > 0)
                                    _addRect(new Rect(0, 0, primeOff, 5d));
                                else if (primeOff < 0)
                                    _addRect(new Rect(_revPrimary, 0d, _magPrimary, 5d));
                            }
                            break;

                        case AnchorFace.YHigh:
                            if (secondOff < 0)
                            {
                                if (primeOff > 0)
                                    _addRect(new Rect(_revPrimary, 0d, primeOff, 5d));
                                else if (primeOff < 0)
                                    _addRect(new Rect(0, 0, _magPrimary, 5d));
                            }
                            break;

                        case AnchorFace.ZLow:
                            {
                                #region ZM Face
                                // ZM Face
                                if (primeOff > 0)
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                            _addTri(new Rect(_revPrimary, 0, primeOff, secondOff), TriangleCorner.LowerRight);
                                        else
                                            _addRect(new Rect(_revPrimary, 0, primeOff, secondOff));
                                    }
                                    else
                                    {
                                        if (!corner)
                                            _addTri(new Rect(_revPrimary, _revSecondary, primeOff, _magSecondary), TriangleCorner.UpperRight);
                                        else
                                            _addRect(new Rect(_revPrimary, _revSecondary, primeOff, _magSecondary));
                                    }
                                }
                                else
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, 0, _magPrimary, secondOff), TriangleCorner.LowerLeft);
                                        else
                                            _addRect(new Rect(0, 0, _magPrimary, secondOff));
                                    }
                                    else
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, _revSecondary, _magPrimary, _magSecondary), TriangleCorner.UpperLeft);
                                        else
                                            _addRect(new Rect(0, _revSecondary, _magPrimary, _magSecondary));
                                    }
                                }
                                #endregion
                            }
                            break;

                        default:
                            {
                                #region ZP Face
                                // ZP Face
                                if (primeOff > 0)
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, 0, primeOff, secondOff), TriangleCorner.LowerLeft);
                                        else
                                            _addRect(new Rect(0, 0, primeOff, secondOff));
                                    }
                                    else
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, _revSecondary, primeOff, _magSecondary), TriangleCorner.UpperLeft);
                                        else
                                            _addRect(new Rect(0, _revSecondary, primeOff, _magSecondary));
                                    }
                                }
                                else
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                            _addTri(new Rect(_revPrimary, 0, _magPrimary, secondOff), TriangleCorner.LowerRight);
                                        else
                                            _addRect(new Rect(_revPrimary, 0, _magPrimary, secondOff));
                                    }
                                    else
                                    {
                                        if (!corner)
                                            _addTri(new Rect(_revPrimary, _revSecondary, _magPrimary, _magSecondary), TriangleCorner.UpperRight);
                                        else
                                            _addRect(new Rect(_revPrimary, _revSecondary, _magPrimary, _magSecondary));
                                    }
                                }
                                #endregion
                            }
                            break;
                    }
                    #endregion
                    break;

                case Axis.Y:
                    #region YAxis Outer Surfaces
                    switch (face)
                    {
                        case AnchorFace.ZLow:
                            if (primeOff > 0)
                            {
                                if (secondOff > 0)
                                    _addRect(new Rect(_revSecondary, 0d, secondOff, 5d));
                                else if (secondOff < 0)
                                    _addRect(new Rect(0, 0, _magSecondary, 5d));
                            }
                            break;

                        case AnchorFace.ZHigh:
                            if (primeOff < 0)
                            {
                                // ZP Face
                                if (secondOff > 0)
                                    _addRect(new Rect(0, 0, secondOff, 5d));
                                else if (secondOff < 0)
                                    _addRect(new Rect(5d + secondOff, 0d, Math.Abs(secondOff), 5d));
                            }
                            break;

                        case AnchorFace.XLow:
                            if (secondOff > 0)
                            {
                                // XM Face
                                if (primeOff > 0)
                                    _addRect(new Rect(0, 0, 5d, primeOff));
                                else if (primeOff < 0)
                                    _addRect(new Rect(0d, _revPrimary, 5d, _magPrimary));
                            }
                            break;

                        case AnchorFace.XHigh:
                            if (secondOff < 0)
                            {
                                // XP Face
                                if (primeOff > 0)
                                    _addRect(new Rect(0, 0, 5d, primeOff));
                                else if (primeOff < 0)
                                    _addRect(new Rect(0d, _revPrimary, 5d, _magPrimary));
                            }
                            break;

                        case AnchorFace.YLow:
                            {
                                #region YM Face
                                // YM Face
                                if (primeOff > 0)
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, 0, secondOff, primeOff), TriangleCorner.LowerLeft);
                                        else
                                            _addRect(new Rect(0, 0, secondOff, primeOff));
                                    }
                                    else
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(_revSecondary, 0, _magSecondary, primeOff),
                                                TriangleCorner.LowerRight);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(_revSecondary, 0, _magSecondary, primeOff));
                                        }
                                    }
                                }
                                else
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(0, _revPrimary, secondOff, _magPrimary),
                                                TriangleCorner.UpperLeft);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(0, _revPrimary, secondOff, _magPrimary));
                                        }
                                    }
                                    else
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(_revSecondary, _revPrimary, _magSecondary, _magPrimary),
                                                TriangleCorner.UpperRight);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(_revSecondary, _revPrimary, _magSecondary, _magPrimary));
                                        }
                                    }
                                }
                                #endregion
                            }
                            break;

                        default:
                            {
                                #region YP Face
                                // YP Face
                                if (primeOff > 0)
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(_revSecondary, 0, secondOff, primeOff),
                                                TriangleCorner.LowerRight);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(_revSecondary, 0, secondOff, primeOff));
                                        }
                                    }
                                    else
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(0, 0, _magSecondary, primeOff),
                                                TriangleCorner.LowerLeft);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(0, 0, _magSecondary, primeOff));
                                        }
                                    }
                                }
                                else
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(_revSecondary, _revPrimary, _magSecondary, _magPrimary),
                                                TriangleCorner.UpperRight);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(_revSecondary, _revPrimary, _magSecondary, _magPrimary));
                                        }
                                    }
                                    else
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(0, _revPrimary, _magSecondary, _magPrimary),
                                                TriangleCorner.UpperLeft);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(0, _revPrimary, _magSecondary, _magPrimary));
                                        }
                                    }
                                }
                                #endregion
                            }
                            break;
                    }
                    #endregion
                    break;

                default:
                    #region XAxis Outer Surfaces
                    switch (face)
                    {
                        case AnchorFace.YLow:
                            if (primeOff > 0)
                            {
                                // YM Face
                                if (secondOff > 0)
                                    _addRect(new Rect(0, 0, 5d, secondOff));
                                else if (secondOff < 0)
                                    _addRect(new Rect(0d, _revSecondary, 5d, _magSecondary));
                            }
                            break;

                        case AnchorFace.YHigh:
                            if (primeOff < 0)
                            {
                                // YP Face
                                if (secondOff > 0)
                                    _addRect(new Rect(0, 0, 5d, secondOff));
                                else if (secondOff < 0)
                                    _addRect(new Rect(0d, _revSecondary, 5d, _magSecondary));
                            }
                            break;

                        case AnchorFace.ZLow:
                            if (secondOff > 0)
                            {
                                // ZM Face
                                if (primeOff > 0)
                                    _addRect(new Rect(0, 0, 5d, primeOff));
                                else if (primeOff < 0)
                                    _addRect(new Rect(0d, _revPrimary, 5d, _magPrimary));
                            }
                            break;

                        case AnchorFace.ZHigh:
                            if (secondOff < 0)
                            {
                                // ZP Face
                                if (primeOff > 0)
                                    _addRect(new Rect(0, 0, 5d, primeOff));
                                else if (primeOff < 0)
                                    _addRect(new Rect(0d, _revPrimary, 5d, _magPrimary));
                            }
                            break;

                        case AnchorFace.XLow:
                            {
                                #region XM Face
                                // XM Face
                                if (primeOff > 0)
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(_revPrimary, 0, primeOff, secondOff),
                                                TriangleCorner.LowerRight);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(_revPrimary, 0, primeOff, secondOff));
                                        }
                                    }
                                    else
                                    {
                                        if (!corner)
                                        {
                                            _addTri(new Rect(_revPrimary, _revSecondary, primeOff, _magSecondary),
                                                TriangleCorner.UpperRight);
                                        }
                                        else
                                        {
                                            _addRect(new Rect(_revPrimary, _revSecondary, primeOff, _magSecondary));
                                        }
                                    }
                                }
                                else
                                {
                                    if (secondOff > 0)
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, 0, _magPrimary, secondOff), TriangleCorner.LowerLeft);
                                        else
                                            _addRect(new Rect(0, 0, _magPrimary, secondOff));
                                    }
                                    else
                                    {
                                        if (!corner)
                                            _addTri(new Rect(0, _revSecondary, _magPrimary, _magSecondary), TriangleCorner.UpperLeft);
                                        else
                                            _addRect(new Rect(0, _revSecondary, _magPrimary, _magSecondary));
                                    }
                                }
                                #endregion
                            }
                            break;

                        default:
                            {
                                #region XP Face
                                // XP Face
                                if (!corner)
                                {
                                    if (primeOff > 0)
                                    {
                                        if (secondOff > 0)
                                        {
                                            _addTri(new Rect(0, 0, primeOff, secondOff), TriangleCorner.LowerLeft);
                                        }
                                        else
                                        {
                                            _addTri(new Rect(0, 5 + secondOff, primeOff, Math.Abs(secondOff)),
                                                TriangleCorner.UpperLeft);
                                        }
                                    }
                                    else
                                    {
                                        if (secondOff > 0)
                                        {
                                            _addTri(new Rect(5 + primeOff, 0, Math.Abs(primeOff), secondOff), TriangleCorner.LowerRight);
                                        }
                                        else
                                        {
                                            _addTri(new Rect(5 + primeOff, 5 + secondOff, Math.Abs(primeOff), Math.Abs(secondOff)),
                                                TriangleCorner.UpperRight);
                                        }
                                    }
                                }
                                else
                                {
                                    if (primeOff > 0)
                                    {
                                        if (secondOff > 0)
                                            _addRect(new Rect(0, 0, primeOff, secondOff));
                                        else
                                            _addRect(new Rect(0, 5 + secondOff, primeOff, Math.Abs(secondOff)));
                                    }
                                    else
                                    {
                                        if (secondOff > 0)
                                            _addRect(new Rect(5 + primeOff, 0, Math.Abs(primeOff), secondOff));
                                        else
                                            _addRect(new Rect(5 + primeOff, 5 + secondOff, Math.Abs(primeOff), Math.Abs(secondOff)));
                                    }
                                }
                                // Inward Faces?
                                // TODO: _face.BackMaterial...
                                #endregion
                            }
                            break;
                    }
                    #endregion
                    break;
            }
        }

        #region private static void GenerateOuterFillerModel(uint param, IWedgeSpace wedge, Model3DGroup addToGroup, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        private static void GenerateOuterFillerModel(uint param, IWedgeSpace wedge, BuildableGroup addToGroup,
            int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        {
            // invisible gases need not apply
            if (!wedge.IsPlusInvisible)
            {
                var _param = new WedgeParams(param);
                var _axis = _param.Axis;
                var _primeOff = _param.PrimaryOffset(wedge.Offset1, wedge.Offset2);
                var _secondOff = _param.SecondaryOffset(wedge.Offset1, wedge.Offset2);
                var _corner = wedge.CornerStyle;

                // mesh and faces
                var _move = new Vector3D(x * 5, y * 5, z * 5);
                var _meskKey = wedge.GetPlusBuildableMeshKey(face, effect);

                Func<BuildableMaterial> _plusBuilder =
                    () => wedge.GetPlusOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);

                BuildOuterFillerModel(addToGroup, face, effect, _primeOff, _secondOff, _axis, _corner,
                    _meskKey, _plusBuilder, _move, bump);
            }
        }
        #endregion

        #region internal static void BuildOuterFillerModel(BuildableGroup addToGroup, AnchorFace face, VisualEffect effect, Transform3D bump, double primeOff, double secondOff, Axis axis, bool corner, Func<Axis, bool, BuildableMaterial> plusOrthoMaterial)
        internal static void BuildOuterFillerModel(BuildableGroup addToGroup, AnchorFace face,
            VisualEffect effect, double primeOff, double secondOff, Axis axis, bool corner,
            BuildableMeshKey meshKey, Func<BuildableMaterial> plusBuilder, params Vector3D[] bump)
        {
            var _textureSize = new Vector(5d, 5d);
            var _wedgeTextureSize = new Vector(5d, 7.07d);

            var _magPrimary = Math.Abs(primeOff);
            var _magSecondary = Math.Abs(secondOff);
            var _revPrimary = 5d - _magPrimary;
            var _revSecondary = 5d - _magSecondary;

            void _addRect(Rect rect)
                => addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                .AddRectangularMesh(rect, _textureSize, true, face, false, bump);

            void _addTri(Rect rect, TriangleCorner tri)
                => addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                .AddRightTriangularMesh(rect, tri, _textureSize, true, face, false, bump);

            var _rect = new Rect();
            switch (axis)
            {
                case Axis.Z:
                    {
                        #region Axis.Z
                        #region XM and XP
                        if (primeOff > 0)
                        {
                            if (face == AnchorFace.XLow)
                            {
                                // XM Partial
                                if (secondOff > 0)
                                    _rect = new Rect(0d, 0d, _revSecondary, 5d);
                                else
                                    _rect = new Rect(_magSecondary, 0d, _revSecondary, 5d);
                                _addRect(_rect);
                            }
                            else if (face == AnchorFace.XHigh)
                            {
                                // XP Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                        }
                        else
                        {
                            if (face == AnchorFace.XLow)
                            {
                                // XM Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                            else if (face == AnchorFace.XHigh)
                            {
                                // XP Partial
                                if (secondOff > 0)
                                    _rect = new Rect(secondOff, 0d, _revSecondary, 5d);
                                else
                                    _rect = new Rect(0d, 0d, _revSecondary, 5d);
                                _addRect(_rect);
                            }
                        }
                        #endregion

                        #region YM and YP
                        if (secondOff > 0)
                        {
                            if (face == AnchorFace.YLow)
                            {
                                // YM Partial
                                if (primeOff > 0)
                                    _rect = new Rect(primeOff, 0, _revPrimary, 5d);
                                else
                                    _rect = new Rect(0, 0, _revPrimary, 5d);
                                _addRect(_rect);
                            }
                            else if (face == AnchorFace.YHigh)
                            {
                                // YP Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                        }
                        else
                        {
                            if (face == AnchorFace.YLow)
                            {
                                // YM Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                            else if (face == AnchorFace.YHigh)
                            {
                                // YP Partial
                                if (primeOff > 0)
                                    _rect = new Rect(0d, 0d, _revPrimary, 5d);
                                else
                                    _rect = new Rect(_magPrimary, 0d, _revPrimary, 5d);
                                _addRect(_rect);
                            }
                        }
                        #endregion

                        if (face == AnchorFace.ZLow)
                        {
                            {
                                #region ZMR1
                                // ZM R1
                                double _x = 0;
                                var _y = (secondOff >= 0d ? secondOff : 0);
                                double _w = 5;
                                var _h = 5d - Math.Abs(secondOff);
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            {
                                #region ZMR2
                                // ZM R2
                                var _x = (primeOff >= 0d ? 0d : 0d - primeOff);
                                var _y = (secondOff >= 0d ? 0 : 5 + secondOff);
                                var _w = 5d - Math.Abs(primeOff);
                                var _h = Math.Abs(secondOff);
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            if (!corner)
                            {
                                #region ZMT
                                // ZM T
                                var _tCorner = (primeOff >= 0d ?
                                    (secondOff >= 0d ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft) :
                                    (secondOff >= 0d ? TriangleCorner.UpperRight : TriangleCorner.LowerRight));
                                _addTri(new Rect((primeOff >= 0d ? 5d - primeOff : 0d),
                                    (secondOff >= 0d ? 0 : 5 + secondOff), Math.Abs(primeOff), Math.Abs(secondOff)), _tCorner);
                                #endregion
                            }
                        }

                        if (face == AnchorFace.ZHigh)
                        {
                            {
                                #region ZPR1
                                // ZP R1
                                double _x = 0;
                                var _y = (secondOff >= 0d ? secondOff : 0);
                                double _w = 5;
                                var _h = 5d - Math.Abs(secondOff);
                                // TODO: merge with ZMR1 when done
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            {
                                #region ZPR2
                                // ZP R2
                                var _x = (primeOff >= 0d ? primeOff : 0d);
                                var _y = (secondOff >= 0d ? 0 : 5 + secondOff);
                                var _w = 5d - Math.Abs(primeOff);
                                var _h = Math.Abs(secondOff);
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            if (!corner)
                            {
                                #region ZPT
                                // ZM T
                                var _tCorner = (primeOff >= 0d ?
                                    (secondOff >= 0d ? TriangleCorner.UpperRight : TriangleCorner.LowerRight) :
                                    (secondOff >= 0d ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft));
                                _addTri(new Rect((primeOff >= 0d ? 0d : 5d + primeOff),
                                    (secondOff >= 0d ? 0d : 5d + secondOff), Math.Abs(primeOff), Math.Abs(secondOff)),
                                    _tCorner);
                                #endregion
                            }
                        }
                        #endregion
                    }
                    break;

                case Axis.Y:
                    {
                        #region Axis.Y
                        #region ZM and ZP
                        if (primeOff > 0)
                        {
                            if (face == AnchorFace.ZLow)
                            {
                                // ZM Partial
                                if (secondOff > 0)
                                    _rect = new Rect(0d, 0d, _revSecondary, 5d);
                                else
                                    _rect = new Rect(_magSecondary, 0d, _revSecondary, 5d);
                                _addRect(_rect);
                            }

                            if (face == AnchorFace.ZHigh)
                            {
                                // ZP Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                        }
                        else
                        {
                            if (face == AnchorFace.ZLow)
                            {
                                // ZM Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }

                            if (face == AnchorFace.ZHigh)
                            {
                                // ZP Partial
                                if (secondOff > 0)
                                    _rect = new Rect(secondOff, 0d, _revSecondary, 5d);
                                else
                                    _rect = new Rect(0d, 0d, _revSecondary, 5d);
                                _addRect(_rect);
                            }
                        }
                        #endregion

                        #region XM and XP
                        if (secondOff > 0)
                        {
                            if (face == AnchorFace.XLow)
                            {
                                // XM Partial
                                if (primeOff > 0)
                                    _rect = new Rect(0d, primeOff, 5d, _revPrimary);
                                else
                                    _rect = new Rect(0d, 0d, 5d, _revPrimary);
                                _addRect(_rect);
                            }

                            if (face == AnchorFace.XHigh)
                            {
                                // XP Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                        }
                        else
                        {
                            if (face == AnchorFace.XLow)
                            {
                                // XM Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }

                            if (face == AnchorFace.XHigh)
                            {
                                // XP Partial
                                if (primeOff > 0)
                                    _rect = new Rect(0, primeOff, 5d, _revPrimary);
                                else
                                    _rect = new Rect(0d, 0d, 5d, _revPrimary);
                                _addRect(_rect);
                            }
                        }
                        #endregion

                        if (face == AnchorFace.YLow)
                        {
                            {
                                #region YMR1
                                // YM R1
                                var _x = (secondOff >= 0d ? secondOff : 0d);
                                var _y = 0d;
                                var _w = _revSecondary;
                                var _h = 5d;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            {
                                #region YMR2
                                // YM R2
                                var _x = (secondOff >= 0d ? 0d : _revSecondary);
                                var _y = (primeOff >= 0d ? primeOff : 0d);
                                var _w = _magSecondary;
                                var _h = _revPrimary;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }

                            if (!corner)
                            {
                                #region YMT
                                // YM T
                                var _tCorner = (primeOff >= 0d ?
                                    (secondOff >= 0d ? TriangleCorner.UpperRight : TriangleCorner.UpperLeft) :
                                    (secondOff >= 0d ? TriangleCorner.LowerRight : TriangleCorner.LowerLeft));
                                _addTri(new Rect((secondOff >= 0d ? 0d : _revSecondary),
                                    (primeOff >= 0d ? 0d : _revPrimary), _magSecondary, _magPrimary), _tCorner);
                                #endregion
                            }
                        }

                        if (face == AnchorFace.YHigh)
                        {
                            {
                                #region YPR1
                                // YP R1
                                var _x = (secondOff >= 0d ? 0d : _magSecondary);
                                var _y = 0d;
                                var _w = _revSecondary;
                                var _h = 5d;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            {
                                #region YPR2
                                // YP R2
                                var _x = (secondOff >= 0d ? _revSecondary : 0d);
                                var _y = (primeOff >= 0d ? primeOff : 0d);
                                var _w = _magSecondary;
                                var _h = _revPrimary;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }

                            if (!corner)
                            {
                                #region YPT
                                // YP T
                                var _tCorner = (primeOff >= 0d ?
                                    (secondOff >= 0d ? TriangleCorner.UpperLeft : TriangleCorner.UpperRight) :
                                    (secondOff >= 0d ? TriangleCorner.LowerLeft : TriangleCorner.LowerRight));
                                _addTri(new Rect((secondOff >= 0d ? _revSecondary : 0d),
                                    (primeOff >= 0d ? 0d : _revPrimary), _magSecondary, _magPrimary),
                                    _tCorner);
                                #endregion
                            }
                        }
                        #endregion
                    }
                    break;

                default:
                    {
                        #region Axis.X
                        #region YM and YP
                        if (primeOff > 0)
                        {
                            if (face == AnchorFace.YLow)
                            {
                                // YM Partial
                                if (secondOff > 0)
                                    _rect = new Rect(0d, secondOff, 5d, _revSecondary);
                                else
                                    _rect = new Rect(0d, 0d, 5d, _revSecondary);
                                _addRect(_rect);
                            }

                            if (face == AnchorFace.YHigh)
                            {
                                // YP Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                        }
                        else
                        {
                            if (face == AnchorFace.YLow)
                            {
                                // YM Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }

                            if (face == AnchorFace.YHigh)
                            {
                                // YP Partial
                                if (secondOff > 0)
                                    _rect = new Rect(0d, secondOff, 5d, _revSecondary);
                                else
                                    _rect = new Rect(0d, 0d, 5d, _revSecondary);
                                _addRect(_rect);
                            }
                        }
                        #endregion

                        #region ZM and ZP
                        if (secondOff > 0)
                        {
                            if (face == AnchorFace.ZLow)
                            {
                                // ZM Partial
                                if (primeOff > 0)
                                    _rect = new Rect(0d, primeOff, 5d, _revPrimary);
                                else
                                    _rect = new Rect(0d, 0d, 5d, _revPrimary);
                                _addRect(_rect);
                            }

                            if (face == AnchorFace.ZHigh)
                            {
                                // ZP Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }
                        }
                        else
                        {
                            if (face == AnchorFace.ZLow)
                            {
                                // ZM Full
                                addToGroup.Context.GetBuildableMesh(meshKey, plusBuilder)
                                    .AddCellFace(face, bump);
                            }

                            if (face == AnchorFace.ZHigh)
                            {
                                // ZP Partial
                                if (primeOff > 0)
                                    _rect = new Rect(0d, primeOff, 5d, _revPrimary);
                                else
                                    _rect = new Rect(0d, 0d, 5d, _revPrimary);
                                _addRect(_rect);
                            }
                        }
                        #endregion

                        if (face == AnchorFace.XLow)
                        {
                            {
                                #region XMR1
                                // XM R1
                                var _x = (primeOff >= 0d ? 0d : _magPrimary);
                                var _y = 0d;
                                var _w = _revPrimary;
                                var _h = 5d;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            {
                                #region XMR2
                                // XM R2
                                var _x = (primeOff >= 0d ? _revPrimary : 0d);
                                var _y = (secondOff >= 0d ? secondOff : 0d);
                                var _w = _magPrimary;
                                var _h = _revSecondary;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            if (!corner)
                            {
                                #region XMT
                                // XM T
                                var _tCorner = (primeOff >= 0d ?
                                    (secondOff >= 0d ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft) :
                                    (secondOff >= 0d ? TriangleCorner.UpperRight : TriangleCorner.LowerRight));
                                _addTri(new Rect((primeOff >= 0d ? _revPrimary : 0d),
                                    (secondOff >= 0d ? 0d : _revSecondary), _magPrimary, _magSecondary), _tCorner);
                                #endregion
                            }
                        }

                        if (face == AnchorFace.XHigh)
                        {
                            {
                                #region XPR1
                                // XP R1
                                var _x = (primeOff >= 0d ? primeOff : 0d);
                                var _y = 0d;
                                var _w = _revPrimary;
                                var _h = 5d;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            {
                                #region XPR2
                                // XP R2
                                var _x = (primeOff >= 0d ? 0d : _revPrimary);
                                var _y = (secondOff >= 0d ? secondOff : 0d);
                                var _w = _magPrimary;
                                var _h = _revSecondary;
                                _addRect(new Rect(_x, _y, _w, _h));
                                #endregion
                            }
                            if (!corner)
                            {
                                #region XPT
                                // XP T
                                var _tCorner = (primeOff >= 0d ?
                                    (secondOff >= 0d ? TriangleCorner.UpperRight : TriangleCorner.LowerRight) :
                                    (secondOff >= 0d ? TriangleCorner.UpperLeft : TriangleCorner.LowerLeft));
                                _addTri(new Rect((primeOff >= 0d ? 0d : _revPrimary),
                                    (secondOff >= 0d ? 0d : _revSecondary), _magPrimary, _magSecondary),
                                    _tCorner);
                                #endregion
                            }
                        }
                        #endregion
                    }
                    break;
            }
        }
        #endregion

        #region public static void AddInnerStructures(uint param, IWedgeSpace wedge, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        public static void AddInnerStructures(uint param, IWedgeSpace wedge, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            // invisible gases need not apply
            var _wedgeVisible = !wedge.IsInvisible;
            var _fillVisible = !wedge.IsPlusInvisible;

            if (!_wedgeVisible && !_fillVisible)
                return;

            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            var _primeOff = _param.PrimaryOffset(wedge.Offset1, wedge.Offset2);
            var _secondOff = _param.SecondaryOffset(wedge.Offset1, wedge.Offset2);
            var _corner = wedge.CornerStyle;
            var _hasTiling = wedge.HasTiling;
            var _hasPlusTiling = wedge.HasPlusTiling;
            Func<AnchorFace, Func<BuildableMaterial>> _orthoMaterial =
                (face) => () => wedge.GetOrthoFaceMaterial(face.GetAxis(), !face.IsLowFace(), effect);
            Func<AnchorFace, Func<BuildableMaterial>> _plusOrthoMaterial =
                (face) => () => wedge.GetPlusOrthoFaceMaterial(face.GetAxis(), face.IsLowFace(), effect);

            Func<AnchorFace, BuildableMeshKey> _meshKeyBuilder =
                (face) => wedge.GetBuildableMeshKey(face, effect);
            Func<AnchorFace, BuildableMeshKey> _plusMeshKeyBuilder =
                (face) => wedge.GetPlusBuildableMeshKey(face, effect);

            Func<BuildableMaterial> _otherMaterial =
                () => wedge.GetOtherFaceMaterial(0, effect);
            Func<BuildableMaterial> _plusOtherMaterial =
                () => wedge.GetPlusOtherFaceMaterial(0, effect);

            BuildInnerStructures(addToGroup, z, y, x, _axis, _primeOff, _secondOff, _corner, _hasTiling, _hasPlusTiling,
                _meshKeyBuilder, _plusMeshKeyBuilder, _orthoMaterial, _plusOrthoMaterial, _otherMaterial, _plusOtherMaterial);
        }
        #endregion

        internal static void BuildInnerStructures(BuildableGroup addToGroup, int z, int y, int x, Axis axis,
            double primeOff, double secondOff, bool corner, bool hasTiling, bool hasPlusTiling,
            Func<AnchorFace, BuildableMeshKey> meshKeyBuilder, Func<AnchorFace, BuildableMeshKey> plusMeshKeyBuilder,
            Func<AnchorFace, Func<BuildableMaterial>> orthoMaterial, Func<AnchorFace, Func<BuildableMaterial>> plusOrthoMaterial,
            Func<BuildableMaterial> otherMaterial, Func<BuildableMaterial> plusOtherMaterial)
        {
            var _textureSize = new Vector(5d, 5d);
            var _wedgeTextureSize = new Vector(5d, 7.07d);
            var _magPrimary = Math.Abs(primeOff);
            var _magSecondary = Math.Abs(secondOff);
            var _revPrimary = 5d - _magPrimary;
            var _revSecondary = 5d - _magSecondary;

            var _move = new Vector3D(x * 5, y * 5, z * 5);

            void _addRect(bool plusMaterial, AnchorFace face, Vector3D bump, Rect rect)
                => addToGroup.Context.GetBuildableMesh(
                    !plusMaterial ? meshKeyBuilder(face) : plusMeshKeyBuilder(face),
                    !plusMaterial ? orthoMaterial(face) : plusOrthoMaterial(face))
                    .AddRectangularMesh(rect, _textureSize, true, face, true, bump, _move);

            // build inner structure
            switch (axis)
            {
                case Axis.Z:
                    #region Axis.Z
                    if (corner)
                    {
                        if (primeOff < 0)
                        {
                            #region XM Face
                            // XM Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(_revPrimary, 0d, 0d);
                            if (secondOff > 0)
                                _rect = new Rect(5d - secondOff, 0d, secondOff, 5d);
                            else if (secondOff < 0)
                                _rect = new Rect(0, 0, Math.Abs(secondOff), 5d);

                            if (hasTiling)
                                _addRect(false, AnchorFace.XLow, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.XLow, _bump, _rect);
                            #endregion
                        }
                        else if (primeOff > 0)
                        {
                            #region XP Face
                            // XP Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0 - _revPrimary, 0d, 0d);
                            if (secondOff > 0)
                                _rect = new Rect(0, 0, secondOff, 5d);
                            else if (secondOff < 0)
                                _rect = new Rect(5d + secondOff, 0d, Math.Abs(secondOff), 5d);

                            if (hasTiling)
                                _addRect(false, AnchorFace.XHigh, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.XHigh, _bump, _rect);
                            #endregion
                        }
                        if (secondOff < 0)
                        {
                            #region YM Face
                            // YM Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, _revSecondary, 0d);
                            if (primeOff > 0)
                                _rect = new Rect(0, 0, primeOff, 5d);
                            else if (primeOff < 0)
                                _rect = new Rect(_revPrimary, 0d, _magPrimary, 5d);

                            if (hasTiling)
                                _addRect(false, AnchorFace.YLow, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.YLow, _bump, _rect);
                            #endregion
                        }
                        else if (secondOff > 0)
                        {
                            #region YP Face
                            // YP Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, 0d - _revSecondary, 0d);
                            if (primeOff > 0)
                                _rect = new Rect(_revPrimary, 0d, primeOff, 5d);
                            else if (primeOff < 0)
                                _rect = new Rect(0, 0, _magPrimary, 5d);

                            if (hasTiling)
                                _addRect(false, AnchorFace.YHigh, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.YHigh, _bump, _rect);
                            #endregion
                        }
                    }
                    else
                    {
                        #region wedge
                        // Wedge
                        Transform3D _wRot = null;
                        Transform3D _wReg = null;

                        if (primeOff > 0)
                        {
                            if (secondOff > 0)
                            {
                                // wedge position
                                var _angle = Math.Atan2(secondOff, 0 - primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1d), _angle));
                                _wReg = new TranslateTransform3D(primeOff, 0, 0);
                            }
                            else
                            {
                                // wedge position
                                var _angle = Math.Atan2(0 - secondOff, primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1d), _angle));
                                _wReg = new TranslateTransform3D(0, _revSecondary, 0);
                            }
                        }
                        else
                        {
                            if (secondOff > 0)
                            {
                                // wedge position
                                var _angle = Math.Atan2(0 - secondOff, primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1d), _angle));
                                _wReg = new TranslateTransform3D(5, secondOff, 0);
                            }
                            else
                            {
                                // wedge position
                                var _angle = Math.Atan2(secondOff, 0 - primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1d), _angle));
                                _wReg = new TranslateTransform3D(_revPrimary, 5, 0);
                            }
                        }

                        var _mesh = HedralGenerator.RectangularMesh(new Rect(0, 0, Math.Sqrt(Math.Pow(primeOff, 2) + Math.Pow(secondOff, 2)), 5d),
                            2, 2, _wedgeTextureSize);
                        _mesh.Freeze();

                        if (hasTiling)
                        {
                            var _material = otherMaterial();
                            var _wedge = new GeometryModel3D(_mesh, _material.Material);
                            // ZM face conditions also determine wedge face transforms
                            CellSpaceFaces.AddGeometry(_material.IsAlpha ? addToGroup.Alpha : addToGroup.Opaque, _wedge, HedralGenerator.YMTransform, _wRot, _wReg);
                        }
                        if (hasPlusTiling)
                        {
                            var _material = plusOtherMaterial();
                            var _wedge = new GeometryModel3D(_mesh, _material.Material);
                            // ZM face conditions also determine wedge face transforms
                            CellSpaceFaces.AddGeometry(_material.IsAlpha ? addToGroup.Alpha : addToGroup.Opaque, _wedge, HedralGenerator.ZMAltFlip, HedralGenerator.YMTransform, _wRot, _wReg);
                        }
                        #endregion
                    }
                    #endregion
                    break;

                case Axis.Y:
                    #region Axis.Y
                    if (corner)
                    {
                        if (primeOff < 0)
                        {
                            #region ZM Face
                            // ZM Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, 0d, _revPrimary);
                            if (secondOff > 0)
                                _rect = new Rect(_revSecondary, 0d, secondOff, 5d);
                            else if (secondOff < 0)
                                _rect = new Rect(0, 0, _magSecondary, 5d);

                            if (hasTiling)
                                _addRect(false, AnchorFace.ZLow, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.ZLow, _bump, _rect);
                            #endregion
                        }
                        if (primeOff > 0)
                        {
                            #region ZP Face
                            // ZP Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, 0d, _magPrimary);
                            if (secondOff > 0)
                                _rect = new Rect(0, 0, secondOff, 5d);
                            else if (secondOff < 0)
                                _rect = new Rect(5d + secondOff, 0d, Math.Abs(secondOff), 5d);

                            if (hasTiling)
                                _addRect(false, AnchorFace.ZHigh, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.ZHigh, _bump, _rect);
                            #endregion
                        }
                        if (secondOff < 0)
                        {
                            #region XM Face
                            // XM Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(_revSecondary, 0d, 0d);
                            if (primeOff > 0)
                                _rect = new Rect(0, 0, 5d, primeOff);
                            else if (primeOff < 0)
                                _rect = new Rect(0d, _revPrimary, 5d, _magPrimary);

                            if (hasTiling)
                                _addRect(false, AnchorFace.XLow, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.XLow, _bump, _rect);
                            #endregion
                        }
                        if (secondOff > 0)
                        {
                            #region XP Face
                            // XP Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0 - _revSecondary, 0d, 0d);
                            if (primeOff > 0)
                                _rect = new Rect(0, 0, 5d, primeOff);
                            else if (primeOff < 0)
                                _rect = new Rect(0d, _revPrimary, 5d, _magPrimary);

                            if (hasTiling)
                                _addRect(false, AnchorFace.XHigh, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.XHigh, _bump, _rect);
                            #endregion
                        }
                    }
                    else
                    {
                        #region Wedge
                        // Wedge
                        Transform3D _wRot = null;
                        Transform3D _wReg = null;

                        // YM Face
                        if (primeOff > 0)
                        {
                            if (secondOff > 0)
                            {
                                // wedge position
                                var _angle = Math.Atan2(primeOff, secondOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1d, 0d), _angle));
                                _wReg = new TranslateTransform3D(0, 0, primeOff);
                            }
                            else
                            {
                                // wedge position
                                var _angle = Math.Atan2(0 - primeOff, 0 - secondOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1d, 0), _angle));
                                _wReg = new TranslateTransform3D(_revSecondary, 0, 0);
                            }
                        }
                        else
                        {
                            if (secondOff > 0)
                            {
                                // wedge position
                                var _angle = Math.Atan2(0 - primeOff, 0 - secondOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1d, 0), _angle));
                                _wReg = new TranslateTransform3D(secondOff, 0, 5d);
                            }
                            else
                            {
                                // wedge position
                                var _angle = Math.Atan2(primeOff, secondOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1d, 0d), _angle));
                                _wReg = new TranslateTransform3D(5d, 0, _revPrimary);
                            }
                        }

                        var _mesh = HedralGenerator.RectangularMesh(new Rect(0, 0, Math.Sqrt(Math.Pow(primeOff, 2) + Math.Pow(secondOff, 2)), 5d),
                            2, 2, _wedgeTextureSize);
                        _mesh.Freeze();

                        if (hasTiling)
                        {
                            var _material = otherMaterial();
                            var _wedge = new GeometryModel3D(_mesh, _material.Material);
                            CellSpaceFaces.AddGeometry(_material.IsAlpha ? addToGroup.Alpha : addToGroup.Opaque, _wedge, _wRot, _wReg);
                        }
                        if (hasPlusTiling)
                        {
                            var _material = plusOtherMaterial();
                            var _wedge = new GeometryModel3D(_mesh, _material.Material);
                            CellSpaceFaces.AddGeometry(_material.IsAlpha ? addToGroup.Alpha : addToGroup.Opaque, _wedge, HedralGenerator.ZMAltFlip, _wRot, _wReg);
                        }
                        #endregion
                    }
                    #endregion
                    break;

                default:
                    #region Axis.X
                    if (corner)
                    {
                        if (primeOff < 0)
                        {
                            #region YM Face
                            // YM Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, _revPrimary, 0d);
                            if (secondOff > 0)
                                _rect = new Rect(0, 0, 5d, secondOff);
                            else if (secondOff < 0)
                                _rect = new Rect(0d, _revSecondary, 5d, _magSecondary);

                            if (hasTiling)
                                _addRect(false, AnchorFace.YLow, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.YLow, _bump, _rect);
                            #endregion
                        }
                        if (primeOff > 0)
                        {
                            #region YP Face
                            // YP Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, 0d - _revPrimary, 0d);
                            if (secondOff > 0)
                                _rect = new Rect(0, 0, 5d, secondOff);
                            else if (secondOff < 0)
                                _rect = new Rect(0d, _revSecondary, 5d, _magSecondary);

                            if (hasTiling)
                                _addRect(false, AnchorFace.YHigh, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.YHigh, _bump, _rect);
                            #endregion
                        }
                        if (secondOff < 0)
                        {
                            #region ZM Face
                            // ZM Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, 0d, _revSecondary);
                            if (primeOff > 0)
                                _rect = new Rect(0, 0, 5d, primeOff);
                            else if (primeOff < 0)
                                _rect = new Rect(0d, _revPrimary, 5d, _magPrimary);

                            if (hasTiling)
                                _addRect(false, AnchorFace.ZLow, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.ZLow, _bump, _rect);
                            #endregion
                        }
                        if (secondOff > 0)
                        {
                            #region ZP Face
                            // ZP Face
                            var _rect = new Rect();
                            var _bump = new Vector3D(0d, 0d, _magSecondary);
                            if (primeOff > 0)
                                _rect = new Rect(0, 0, 5d, primeOff);
                            else if (primeOff < 0)
                                _rect = new Rect(0d, _revPrimary, 5d, _magPrimary);

                            if (hasTiling)
                                _addRect(false, AnchorFace.ZHigh, _bump, _rect);
                            if (hasPlusTiling)
                                _addRect(true, AnchorFace.ZHigh, _bump, _rect);
                            #endregion
                        }
                    }
                    else
                    {
                        #region Wedge
                        // Wedge
                        Transform3D _xSpin = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1d), -90d), 2.5d, 2.5d, 0d);
                        Transform3D _wRot = null;
                        Transform3D _wReg = null;

                        // XM Face
                        if (primeOff > 0)
                        {
                            if (secondOff > 0)
                            {
                                // wedge position
                                var _angle = Math.Atan2(0 - secondOff, primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), _angle), 0d, 5d, 0d);
                                _wReg = new TranslateTransform3D(0d, primeOff - 5d, 0d);
                            }
                            else
                            {
                                // wedge position
                                var _angle = Math.Atan2(secondOff, 0 - primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), _angle), 0d, 5d, 0d);
                                _wReg = new TranslateTransform3D(0, -5d, _revSecondary);
                            }
                        }
                        else
                        {
                            if (secondOff > 0)
                            {
                                // wedge position
                                var _angle = Math.Atan2(secondOff, 0 - primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), _angle), 0d, 5d, 0d);
                                _wReg = new TranslateTransform3D(0, 0, secondOff);
                            }
                            else
                            {
                                // wedge position
                                var _angle = Math.Atan2(0 - secondOff, primeOff) * 180 / Math.PI;
                                _wRot = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1d, 0d, 0d), _angle), 0d, 5d, 0d);
                                _wReg = new TranslateTransform3D(0, primeOff, 5d);
                            }
                        }

                        var _mesh = HedralGenerator.RectangularMesh(new Rect(0, 0, Math.Sqrt(Math.Pow(primeOff, 2) + Math.Pow(secondOff, 2)), 5d),
                            2, 2, _wedgeTextureSize);
                        _mesh.Freeze();

                        if (hasTiling)
                        {
                            var _material = otherMaterial();
                            var _wedge = new GeometryModel3D(_mesh, _material.Material);
                            CellSpaceFaces.AddGeometry(_material.IsAlpha ? addToGroup.Alpha : addToGroup.Opaque, _wedge, _xSpin, _wRot, _wReg);
                        }
                        if (hasPlusTiling)
                        {
                            var _material = plusOtherMaterial();
                            var _wedge = new GeometryModel3D(_mesh, _material.Material);
                            CellSpaceFaces.AddGeometry(_material.IsAlpha ? addToGroup.Alpha : addToGroup.Opaque, _wedge, HedralGenerator.ZMAltFlip, _xSpin, _wRot, _wReg);
                        }
                        #endregion
                    }
                    #endregion
                    break;
            }
        }

        #region public static bool OccludesFace(uint param, IWedgeSpace wedge, AnchorFace outwardFace)
        /// <summary>True if face has no invisible components</summary>
        public static bool OccludesFace(uint param, IWedgeSpace wedge, AnchorFace outwardFace)
        {
            var _param = new WedgeParams(param);
            var _ortho = _param.Axis;
            var _wedgeBlock = !wedge.IsInvisible;
            var _fillBlock = !wedge.IsPlusInvisible;
            if (_wedgeBlock && _fillBlock)
                return true;
            else if (!_wedgeBlock && !_fillBlock)
                return false;
            else if (_fillBlock)
            {
                // only sides opposite wedge corner can occlude!
                var _faces = GetWedgeEdge(param, wedge.Offset1, wedge.Offset2);
                return _faces.Contains(outwardFace.ReverseFace());
            }
            return false;
        }
        #endregion

        #region public static bool ShowFace(uint param, IWedgeSpace wedge, AnchorFace outwardFace)
        /// <summary>True if face has only visible components</summary>
        public static bool ShowFace(uint param, IWedgeSpace wedge, AnchorFace outwardFace)
        {
            var _param = new WedgeParams(param);
            var _ortho = _param.Axis;
            if (!wedge.IsPlusInvisible)
                return true;
            if (!wedge.IsInvisible)
            {
                // any side except the opposite sides of the wedge corner should be shown!
                var _faces = GetWedgeEdge(param, wedge.Offset1, wedge.Offset2);
                return !_faces.Contains(outwardFace.ReverseFace());
            }
            return false;
        }
        #endregion
    }
}