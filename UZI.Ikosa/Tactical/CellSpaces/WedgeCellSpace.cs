using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class WedgeCellSpace : CellSpace, IWedgeSpace
    {
        #region Construction
        public WedgeCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles,
            double primaryOffset, double secondardyOffset)
            : base(minusMaterial, minusTiles)
        {
            _PlusMaterial = plusMaterial;
            _PlusTiling = plusTiles;
            _Offset1 = primaryOffset;
            _Offset2 = secondardyOffset;
            _CornerStyle = false;
        }

        protected WedgeCellSpace(CellMaterial minusMaterial, TileSet minusTiles, CellMaterial plusMaterial, TileSet plusTiles,
            double primaryOffset, double secondardyOffset, bool cornerStyle)
            : base(minusMaterial, minusTiles)
        {
            _PlusMaterial = plusMaterial;
            _PlusTiling = plusTiles;
            _Offset1 = primaryOffset;
            _Offset2 = secondardyOffset;
            _CornerStyle = cornerStyle;
        }
        #endregion

        #region data
        protected bool _CornerStyle;
        private CellMaterial _PlusMaterial;
        private TileSet _PlusTiling;
        private double _Offset1;
        private double _Offset2;
        #endregion

        #region public CellMaterial PlusMaterial { get; set; }
        public CellMaterial PlusMaterial
        {
            get { return _PlusMaterial; }
            set
            {
                if ((_PlusMaterial != value) && OnCanPlusMaterialChange(value))
                {
                    _PlusMaterial = value;
                    OnCellMaterialChanged();
                    DoPropertyChanged(nameof(PlusMaterial));
                }
            }
        }
        protected virtual bool OnCanPlusMaterialChange(CellMaterial material) { return true; }
        protected override void OnCellMaterialChanged()
        {
            GripRules.InitializeMaterials(CellMaterial, PlusMaterial);
        }
        #endregion

        #region public TileSet PlusTiling { get; set; }
        public TileSet PlusTiling
        {
            get { return _PlusTiling; }
            set
            {
                if (_PlusTiling != value)
                {
                    _PlusTiling = value;
                    DoPropertyChanged(nameof(PlusTiling));
                }
            }
        }
        #endregion

        public override IEnumerable<CellMaterial> AllMaterials
        {
            get
            {
                yield return CellMaterial;
                yield return PlusMaterial;
                yield break;
            }
        }

        public override (string collectionKey, string brushKey) InnerBrushKeys(uint param, Point3D point)
            => (IsPlusGas || IsPlusLiquid)
            ? (_PlusTiling?.BrushCollectionKey, _PlusTiling?.BrushCollection[_PlusTiling.InnerMaterialIndex].BrushKey)
            : base.InnerBrushKeys(param, point);

        #region public double Offset1 { get; set; }
        public double Offset1
        {
            get { return _Offset1; }
            set
            {
                if ((_Offset1 != value) && OnCanOffset1Change(value))
                {
                    _Offset1 = value;
                    DoPropertyChanged(@"Offset1");
                }
            }
        }
        protected virtual bool OnCanOffset1Change(double offset) => true;
        #endregion

        #region public double Offset2 { get; set; }
        public double Offset2
        {
            get { return _Offset2; }
            set
            {
                if ((_Offset2 != value) && OnCanOffset2Change(value))
                {
                    _Offset2 = value;
                    DoPropertyChanged(@"Offset2");
                }
            }
        }
        protected virtual bool OnCanOffset2Change(double offset) => true;
        #endregion

        public override string GetDescription(uint param)
            => $@"Wdg:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName}) [{Offset1};{Offset2}]";

        public override string GetParamText(uint param)
        {
            var _param = new WedgeParams(param);
            return $@"Axis={_param.Axis}, Flip={_param.FlipOffsets}, InvPri={_param.InvertPrimary}, InvSec={_param.InvertSecondary}";
        }

        public override bool IsShadeable(uint param)
            => true;

        public override void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            WedgeSpaceFaces.AddOuterSurface(param, this, group, z, y, x, face, effect, bump);
        }

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => WedgeSpaceFaces.OccludesFace(param, this, outwardFace);

        public override bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => WedgeSpaceFaces.ShowFace(param, this, outwardFace);

        #region public override uint FlipParameters(uint paramsIn, Axis flipAxis)
        public override uint FlipAxis(uint paramsIn, Axis flipAxis)
        {
            var _param = new WedgeParams(paramsIn);
            if (_param.Axis == flipAxis)
            {
                return paramsIn;
            }

            switch (_param.Axis)
            {
                case Axis.Z:
                    switch (flipAxis)
                    {
                        case Axis.X:
                            _param.InvertPrimary = !_param.InvertPrimary;
                            break;
                        default:
                            _param.InvertSecondary = !_param.InvertSecondary;
                            break;
                    }
                    break;
                case Axis.Y:
                    switch (flipAxis)
                    {
                        case Axis.Z:
                            _param.InvertPrimary = !_param.InvertPrimary;
                            break;
                        default:
                            _param.InvertSecondary = !_param.InvertSecondary;
                            break;
                    }
                    break;
                default:
                    switch (flipAxis)
                    {
                        case Axis.Y:
                            _param.InvertPrimary = !_param.InvertPrimary;
                            break;
                        default:
                            _param.InvertSecondary = !_param.InvertSecondary;
                            break;
                    }
                    break;
            }
            return _param.Value;
        }
        #endregion

        #region public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        {
            if (axis1 != axis2)
            {
                var _param = new WedgeParams(paramsIn);
                var _axis = _param.Axis;
                var _pri = _param.InvertPrimary;
                var _sec = _param.InvertSecondary;
                if ((_axis == axis1) || (_axis == axis2))
                {
                    _param.Axis = _axis == axis1 ? axis2 : axis1;
                    _param.FlipOffsets = !_param.FlipOffsets;
                    _param.InvertPrimary = _sec;
                    _param.InvertSecondary = _pri;
                }
                else
                {
                    // swapping primary and secondary, so flip offsets AND inversions
                    _param.FlipOffsets = !_param.FlipOffsets;
                    _param.InvertPrimary = _sec;
                    _param.InvertSecondary = _pri;
                }
                return _param.Value;
            }
            return paramsIn;
        }
        #endregion

        #region public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x)
        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            WedgeSpaceFaces.AddInnerStructures(param, this, addToGroup, z, y, x, effect);
        }
        #endregion

        /// <summary>Provides a sign comparable value to help indicate which side of a line defined by points A and B, test point C is on.</summary>
        private double UzoMeter(double xa, double ya, double xb, double yb, double xc, double yc)
            => (xb * ya - xa * yb) + (xc * yb - xb * yc) + (xa * yc - xc * ya);

        private bool Approx(double d1, double d2)
            => (Math.Abs(d1 - d2) < 0.0001d);

        #region public override bool BlocksDetect(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        /// <summary>Simplified (and generalized) detection blocking for wedges</summary>
        public override bool BlocksDetect(uint param, int z, int y, int x, Point3D entryPt, Point3D exitPt)
        {
            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = _param.SecondaryOffset(Offset1, Offset2);
            var _basePt = new Point3D(x * 5d, y * 5d, z * 5d);
            foreach (var _pt in new Point3D[] { entryPt, exitPt })
            {
                if ((CellMaterial.DetectBlockingThickness < Math.Abs(_primeOff))
                    && (CellMaterial.DetectBlockingThickness < Math.Abs(_secondOff)))
                {
                    switch (_axis)
                    {
                        case Axis.X:
                            if (WedgeCollision(param, _pt.Y, _pt.Z, _pt.X, _basePt.Y, _basePt.Z, _basePt.X))
                            {
                                return true;
                            }

                            break;

                        case Axis.Y:
                            if (WedgeCollision(param, _pt.Z, _pt.X, _pt.Y, _basePt.Z, _basePt.X, _basePt.Y))
                            {
                                return true;
                            }

                            break;

                        default:
                            if (WedgeCollision(param, _pt.X, _pt.Y, _pt.Z, _basePt.X, _basePt.Y, _basePt.Z))
                            {
                                return true;
                            }

                            break;
                    }
                }
                if ((PlusMaterial.DetectBlockingThickness < 5 - Math.Abs(_primeOff))
                    && (PlusMaterial.DetectBlockingThickness < 5 - Math.Abs(_secondOff)))
                {
                    switch (_axis)
                    {
                        case Axis.X:
                            if (PlusWedgeCollision(param, _pt.Y, _pt.Z, _pt.X, _basePt.Y, _basePt.Z, _basePt.X))
                            {
                                return true;
                            }

                            break;

                        case Axis.Y:
                            if (PlusWedgeCollision(param, _pt.Z, _pt.X, _pt.Y, _basePt.Z, _basePt.X, _basePt.Y))
                            {
                                return true;
                            }

                            break;

                        default:
                            if (PlusWedgeCollision(param, _pt.X, _pt.Y, _pt.Z, _basePt.X, _basePt.Y, _basePt.Z))
                            {
                                return true;
                            }

                            break;
                    }
                }
            }

            // nothing hit a solid, no blockings
            return false;
        }
        #endregion

        #region private bool WedgeCollision(uint param, double primePt, double secondPt, double testPt, double primeBase, double secondBase, double testBase)
        private bool WedgeCollision(uint param, double primePt, double secondPt, double testPt, double primeBase, double secondBase, double testBase)
        {
            var _param = new WedgeParams(param);
            var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = _param.SecondaryOffset(Offset1, Offset2);
            if (_primeOff > 0)
            {
                if (_secondOff > 0)
                {
                    if ((primePt <= primeBase + _primeOff) && Approx(secondPt, secondBase))
                    {
                        return true;
                    }

                    if ((secondPt <= secondBase + _secondOff) && Approx(primePt, primeBase))
                    {
                        return true;
                    }

                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        if (_CornerStyle)
                        {
                            // in the rectangle
                            if ((primePt <= primeBase + _primeOff) && (secondPt <= secondBase + _secondOff))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // check that corner and point are on the same side of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + _primeOff, secondBase, primeBase, secondBase + _secondOff, primeBase, secondBase);
                            var _pUzo = UzoMeter(primeBase + _primeOff, secondBase, primeBase, secondBase + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if ((primePt <= primeBase + _primeOff) && Approx(secondPt, secondBase + 5d))
                    {
                        return true;
                    }

                    if ((secondPt >= secondBase + 5d + _secondOff) && Approx(primePt, primeBase))
                    {
                        return true;
                    }
                    // bottom and top
                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        if (_CornerStyle)
                        {
                            // in the rectangle
                            if ((primePt <= primeBase + _primeOff) && (secondPt >= secondBase + 5d + _secondOff))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // check that corner and point are on the same side of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + _primeOff, secondBase + 5d, primeBase, secondBase + 5d + _secondOff, primeBase, secondBase + 5d);
                            var _pUzo = UzoMeter(primeBase + _primeOff, secondBase + 5d, primeBase, secondBase + 5d + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (_secondOff > 0)
                {
                    if ((primePt >= primeBase + 5d + _primeOff) && Approx(secondPt, secondBase))
                    {
                        return true;
                    }

                    if ((secondPt <= secondBase + _secondOff) && Approx(primePt, primeBase + 5d))
                    {
                        return true;
                    }
                    // bottom and top
                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        if (_CornerStyle)
                        {
                            // in the rectangle
                            if ((primePt >= primeBase + 5d + _primeOff) && (secondPt <= secondBase + _secondOff))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // check that corner and point are on the same side of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase, primeBase + 5d, secondBase + _secondOff, primeBase + 5d, secondBase);
                            var _pUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase, primeBase + 5d, secondBase + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if ((primePt >= primeBase + 5d + _primeOff) && Approx(secondPt, secondBase + 5d))
                    {
                        return true;
                    }

                    if ((secondPt >= secondBase + 5d + _secondOff) && Approx(primePt, primeBase + 5d))
                    {
                        return true;
                    }
                    // bottom and top
                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        if (_CornerStyle)
                        {
                            // in the rectangle
                            if ((primePt >= primeBase + 5d + _primeOff) && (secondPt >= secondBase + 5d + _secondOff))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            // check that corner and point are on the same side of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase + 5d, primeBase + 5d, secondBase + 5d + _secondOff, primeBase + 5d, secondBase + 5d);
                            var _pUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase + 5d, primeBase + 5d, secondBase + 5d + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region private bool PlusWedgeCollision(uint param, double primePt, double secondPt, double testPt, double primeBase, double secondBase, double testBase)
        private bool PlusWedgeCollision(uint param, double primePt, double secondPt, double testPt, double primeBase, double secondBase, double testBase)
        {
            var _param = new WedgeParams(param);
            var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = _param.SecondaryOffset(Offset1, Offset2);
            if (_primeOff > 0)
            {
                // far full prime face
                if (Approx(primePt, primeBase + 5d))
                {
                    return true;
                }
                // either test face in the PlusMaterial
                if ((Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    && (primePt >= primeBase + _primeOff))
                {
                    return true;
                }

                if (_secondOff > 0)
                {
                    // far full second face
                    if (Approx(secondPt, secondBase + 5d))
                    {
                        return true;
                    }
                    // near prime face, above the wedge
                    if (Approx(primePt, primeBase) && (secondPt >= secondBase + _secondOff))
                    {
                        return true;
                    }
                    // near second face, beyond the wedge
                    if (Approx(secondPt, secondBase) && (primePt >= primeBase + _primeOff))
                    {
                        return true;
                    }

                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        // either test face, above the wedge
                        if (secondPt >= secondBase + _secondOff)
                        {
                            return true;
                        }
                        else if (!_CornerStyle)
                        {
                            // see if the corner and point are on opposite sides of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + _primeOff, secondBase, primeBase, secondBase + _secondOff, primeBase, secondBase);
                            var _pUzo = UzoMeter(primeBase + _primeOff, secondBase, primeBase, secondBase + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) < 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    // near full second face
                    if (Approx(secondPt, secondBase))
                    {
                        return true;
                    }
                    // near prime face, below the wedge
                    if (Approx(primePt, primeBase) && (secondPt <= secondBase + 5d + _secondOff))
                    {
                        return true;
                    }
                    // far second face, past the wedge
                    if (Approx(secondPt, secondBase + 5d) && (primePt >= primeBase + _primeOff))
                    {
                        return true;
                    }

                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        // either second face, below the wedge
                        if (secondPt <= secondBase + 5d + _secondOff)
                        {
                            return true;
                        }
                        else if (!_CornerStyle)
                        {
                            // see if the corner and point are on opposite sides of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + _primeOff, secondBase + 5d, primeBase, secondBase + 5d + _secondOff, primeBase, secondBase + 5d);
                            var _pUzo = UzoMeter(primeBase + _primeOff, secondBase + 5d, primeBase, secondBase + 5d + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) < 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                // near full prime face
                if (Approx(primePt, primeBase))
                {
                    return true;
                }
                // either test face, before the wedge
                if ((Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    && (primePt <= primeBase + 5d + _primeOff))
                {
                    return true;
                }

                if (_secondOff > 0)
                {
                    // far full second face
                    if (Approx(secondPt, secondBase + 5d))
                    {
                        return true;
                    }
                    // far prime face, above the wedge
                    if (Approx(primePt, primeBase + 5d) && (secondPt >= secondBase + _secondOff))
                    {
                        return true;
                    }
                    // near second face, before the wedge
                    if (Approx(secondPt, secondBase) && (primePt <= primeBase + 5d + _primeOff))
                    {
                        return true;
                    }

                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        // either test face, above the wedge
                        if (secondPt >= secondBase + _secondOff)
                        {
                            return true;
                        }
                        else if (!_CornerStyle)
                        {
                            // see if the corner and point are on opposite sides of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase, primeBase + 5d, secondBase + _secondOff, primeBase + 5d, secondBase);
                            var _pUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase, primeBase + 5d, secondBase + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) < 0)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    // near full second face
                    if (Approx(secondPt, secondBase))
                    {
                        return true;
                    }
                    // far prime face, below the wedge
                    if (Approx(primePt, primeBase + 5d) && (secondPt <= secondBase + 5d + _secondOff))
                    {
                        return true;
                    }
                    // far second face, before the wedge
                    if (Approx(secondPt, secondBase + 5d) && (primePt <= primeBase + 5d + _primeOff))
                    {
                        return true;
                    }

                    if (Approx(testPt, testBase) || Approx(testPt, testBase + 5d))
                    {
                        // either test face, below the wedge
                        if (secondPt <= secondBase + 5d + _secondOff)
                        {
                            return true;
                        }
                        else if (!_CornerStyle)
                        {
                            // see if the corner and point are on opposite sides of the hypoteneuse
                            var _wUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase + 5d, primeBase + 5d, secondBase + 5d + _secondOff, primeBase + 5d, secondBase + 5d);
                            var _pUzo = UzoMeter(primeBase + 5d + _primeOff, secondBase + 5d, primeBase + 5d, secondBase + 5d + _secondOff, primePt, secondPt);
                            if ((_wUzo / _pUzo) < 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        /// <summary>Determine whether any material in the wedge blocks line of effect</summary>
        public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        {
            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            var _basePt = new Point3D(x * 5d, y * 5d, z * 5d);
            foreach (Point3D _pt in new Point3D[] { pt1, pt2 })
            {
                if (CellMaterial.BlocksEffect)
                {
                    switch (_axis)
                    {
                        case Axis.X:
                            if (WedgeCollision(param, _pt.Y, _pt.Z, _pt.X, _basePt.Y, _basePt.Z, _basePt.X))
                            {
                                return true;
                            }

                            break;

                        case Axis.Y:
                            if (WedgeCollision(param, _pt.Z, _pt.X, _pt.Y, _basePt.Z, _basePt.X, _basePt.Y))
                            {
                                return true;
                            }

                            break;

                        default:
                            if (WedgeCollision(param, _pt.X, _pt.Y, _pt.Z, _basePt.X, _basePt.Y, _basePt.Z))
                            {
                                return true;
                            }

                            break;
                    }
                }
                if (PlusMaterial.BlocksEffect)
                {
                    switch (_axis)
                    {
                        case Axis.X:
                            if (PlusWedgeCollision(param, _pt.Y, _pt.Z, _pt.X, _basePt.Y, _basePt.Z, _basePt.X))
                            {
                                return true;
                            }

                            break;

                        case Axis.Y:
                            if (PlusWedgeCollision(param, _pt.Z, _pt.X, _pt.Y, _basePt.Z, _basePt.X, _basePt.Y))
                            {
                                return true;
                            }

                            break;

                        default:
                            if (PlusWedgeCollision(param, _pt.X, _pt.Y, _pt.Z, _basePt.X, _basePt.Y, _basePt.Z))
                            {
                                return true;
                            }

                            break;
                    }
                }

            }

            // nothing hit a blocking material
            return false;
        }
        #endregion

        #region public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = _param.SecondaryOffset(Offset1, Offset2);
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock && _plusBlock)
            {
                return new HedralGrip(true);
            }
            else if (!_cellBlock && !_plusBlock)
            {
                return new HedralGrip(false);
            }

            var _surfaceAxis = surfaceFace.GetAxis();
            if (_surfaceAxis == _axis)
            {
                #region complex faces of parallel axis
                HedralGrip _orthoGrip()
                {
                    if (_cellBlock)
                    {
                        HedralGrip _cbPrime(AnchorFace high, AnchorFace low)
                            => new HedralGrip(_surfaceAxis, _primeOff < 0 ? high : low, Math.Abs(_primeOff));
                        HedralGrip _cbSecond(AnchorFace high, AnchorFace low)
                            => new HedralGrip(_surfaceAxis, _secondOff < 0 ? high : low, Math.Abs(_secondOff));

                        // AND
                        switch (_surfaceAxis)
                        {
                            case Axis.Z:
                                return _cbPrime(AnchorFace.XHigh, AnchorFace.XLow)
                                    .Intersect(_cbSecond(AnchorFace.YHigh, AnchorFace.YLow));

                            case Axis.Y:
                                return _cbPrime(AnchorFace.ZHigh, AnchorFace.ZLow)
                                    .Intersect(_cbSecond(AnchorFace.XHigh, AnchorFace.XLow));

                            case Axis.X:
                            default:
                                return _cbPrime(AnchorFace.YHigh, AnchorFace.YLow)
                                    .Intersect(_cbSecond(AnchorFace.ZHigh, AnchorFace.ZLow));
                        }
                    }
                    else // _plusBlock
                    {
                        HedralGrip _sbPrime(AnchorFace high, AnchorFace low)
                           => new HedralGrip(_surfaceAxis, _primeOff > 0 ? high : low, 5 - Math.Abs(_primeOff));
                        HedralGrip _sbSecond(AnchorFace high, AnchorFace low)
                            => new HedralGrip(_surfaceAxis, _secondOff > 0 ? high : low, 5 - Math.Abs(_secondOff));

                        // OR
                        switch (_surfaceAxis)
                        {
                            case Axis.Z:
                                return _sbPrime(AnchorFace.XHigh, AnchorFace.XLow)
                                    .Union(_sbSecond(AnchorFace.YHigh, AnchorFace.YLow));

                            case Axis.Y:
                                return _sbPrime(AnchorFace.ZHigh, AnchorFace.ZLow)
                                    .Union(_sbSecond(AnchorFace.XHigh, AnchorFace.XLow));

                            case Axis.X:
                            default:
                                return _sbPrime(AnchorFace.YHigh, AnchorFace.YLow)
                                    .Union(_sbSecond(AnchorFace.ZHigh, AnchorFace.ZLow));
                        }
                    }
                }

                if (!CornerStyle)
                {
                    // flipped if plus block
                    return new HedralGrip(_primeOff, _secondOff, _plusBlock);
                }
                else
                {
                    // only right angles
                    return _orthoGrip();
                }
                #endregion
            }
            else
            {
                // blocks or slivers
                switch (_surfaceAxis)
                {
                    case Axis.Z:
                        if (_axis == Axis.Y)
                        {
                            // Z is primary offset for Y Axis
                            if ((_primeOff < 0)         // if clipping from top
                               ? surfaceFace.IsLowFace()   // ... lowface is all plus
                               : !surfaceFace.IsLowFace()) // .. otherwise, hiface is all plus
                            {
                                return new HedralGrip(_plusBlock);
                            }
                            else if (_cellBlock)
                            {
                                // just _cell blocks (small corner)
                                return new HedralGrip(_surfaceAxis,
                                    _secondOff < 0 ? AnchorFace.XHigh : AnchorFace.XLow,
                                    Math.Abs(_secondOff));
                            }
                            else
                            {
                                // just plus blocks
                                return new HedralGrip(_surfaceAxis,
                                    _secondOff < 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    5 - Math.Abs(_secondOff));
                            }
                        }
                        else // Axis.X
                        {
                            // Z is secondary offset for X Axis
                            if ((_secondOff < 0)       // if clipping from top
                               ? surfaceFace.IsLowFace()   // ... lowface is all plus
                               : !surfaceFace.IsLowFace()) // .. otherwise, hiface is all plus
                            {
                                return new HedralGrip(_plusBlock);
                            }
                            else if (_cellBlock)
                            {
                                // just _cell blocks (small corner)
                                return new HedralGrip(_surfaceAxis,
                                    _primeOff < 0 ? AnchorFace.YHigh : AnchorFace.YLow,
                                    Math.Abs(_primeOff));
                            }
                            else
                            {
                                // just plus blocks
                                return new HedralGrip(_surfaceAxis,
                                    _primeOff < 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    5 - Math.Abs(_primeOff));
                            }
                        }

                    case Axis.Y:
                        if (_axis == Axis.X)
                        {
                            // Y is primary offset for X Axis
                            if ((_primeOff < 0)         // if clipping from top
                                ? surfaceFace.IsLowFace()   // ... lowface is all plus
                                : !surfaceFace.IsLowFace()) // .. otherwise, hiface is all plus
                            {
                                return new HedralGrip(_plusBlock);
                            }
                            else if (_cellBlock)
                            {
                                // just _cell blocks (small corner)
                                return new HedralGrip(_surfaceAxis,
                                    _secondOff < 0 ? AnchorFace.ZHigh : AnchorFace.ZLow,
                                    Math.Abs(_secondOff));
                            }
                            else
                            {
                                // just plus blocks
                                return new HedralGrip(_surfaceAxis,
                                    _secondOff < 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    5 - Math.Abs(_secondOff));
                            }
                        }
                        else // Axis.Z
                        {
                            // Y is seconday offset for Z Axis
                            if ((_secondOff < 0)       // if clipping from top
                                ? surfaceFace.IsLowFace()   // ... lowface is all plus
                                : !surfaceFace.IsLowFace()) // .. otherwise, hiface is all plus
                            {
                                return new HedralGrip(_plusBlock);
                            }
                            else if (_cellBlock)
                            {
                                // just _cell blocks (small corner)
                                return new HedralGrip(_surfaceAxis,
                                    _primeOff < 0 ? AnchorFace.XHigh : AnchorFace.XLow,
                                    Math.Abs(_primeOff));
                            }
                            else
                            {
                                // just plus blocks
                                return new HedralGrip(_surfaceAxis,
                                    _primeOff < 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    5 - Math.Abs(_primeOff));
                            }
                        }

                    case Axis.X:
                    default:
                        if (_axis == Axis.Z)
                        {
                            // X is primary offset for Z Axis
                            if ((_primeOff < 0)         // if clipping from top
                                ? surfaceFace.IsLowFace()   // ... lowface is all plus
                                : !surfaceFace.IsLowFace()) // .. otherwise, hiface is all plus
                            {
                                return new HedralGrip( _plusBlock );
                            }
                            else if (_cellBlock)
                            {
                                // just _cell blocks (small corner)
                                return new HedralGrip(_surfaceAxis,
                                    _secondOff < 0 ? AnchorFace.YHigh : AnchorFace.YLow,
                                    Math.Abs(_secondOff));
                            }
                            else
                            {
                                // just plus blocks
                                return new HedralGrip(_surfaceAxis,
                                    _secondOff < 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    5 - Math.Abs(_secondOff));
                            }
                        }
                        else // Axis.Y
                        {
                            // X is secondary offset for Y Axis
                            if ((_secondOff < 0)       // if clipping from top
                                ? surfaceFace.IsLowFace()   // ... lowface is all plus
                                : !surfaceFace.IsLowFace()) // .. otherwise, hiface is all plus
                            {
                                return new HedralGrip(_plusBlock);
                            }
                            else if (_cellBlock)
                            {
                                // just _cell blocks (small corner)
                                return new HedralGrip(_surfaceAxis,
                                    _primeOff < 0 ? AnchorFace.ZHigh : AnchorFace.ZLow,
                                    Math.Abs(_primeOff));
                            }
                            else
                            {
                                // just plus blocks
                                return new HedralGrip(_surfaceAxis,
                                    _primeOff < 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    5 - Math.Abs(_primeOff));
                            }
                        }
                }
            }
        }
        #endregion

        #region public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        {
            // TODO: may need to revisit with slope-like logic
            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = _param.SecondaryOffset(Offset1, Offset2);

            MovementOpening _opening(AnchorFace _face, double _off, double _blockage)
                => new MovementOpening(_face, _off, Math.Abs(_blockage) / 5);

            MovementOpening _plusOpening(AnchorFace _face, double _off, double _blockage)
                => new MovementOpening(_face, _off, (5 - Math.Abs(_blockage)) / 5);

            var _blocksPlus = !movement.CanMoveThrough(PlusMaterial);
            var _blocksCell = !movement.CanMoveThrough(CellMaterial);

            if (_blocksCell)
            {
                if (!_blocksPlus)
                {
                    // blocks the wedge, but not the filler
                    switch (_axis)
                    {
                        case Axis.Z:
                            if (_primeOff > 0)
                            {
                                yield return _opening(AnchorFace.XHigh, 5 - _primeOff, _secondOff);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.XLow, 5 + _primeOff, _secondOff);
                            }

                            if (_secondOff > 0)
                            {
                                yield return _opening(AnchorFace.YHigh, 5 - _secondOff, _primeOff);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.YLow, 5 + _secondOff, _primeOff);
                            }

                            break;

                        case Axis.Y:
                            if (_primeOff > 0)
                            {
                                yield return _opening(AnchorFace.ZHigh, 5 - _primeOff, _secondOff);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.ZLow, 5 + _primeOff, _secondOff);
                            }

                            if (_secondOff > 0)
                            {
                                yield return _opening(AnchorFace.XHigh, 5 - _secondOff, _primeOff);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.XLow, 5 + _secondOff, _primeOff);
                            }

                            break;

                        default: // X
                            if (_primeOff > 0)
                            {
                                yield return _opening(AnchorFace.YHigh, 5 - _primeOff, _secondOff);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.YLow, 5 + _primeOff, _secondOff);
                            }

                            if (_secondOff > 0)
                            {
                                yield return _opening(AnchorFace.ZHigh, 5 - _secondOff, _primeOff);
                            }
                            else
                            {
                                yield return _opening(AnchorFace.ZLow, 5 + _secondOff, _primeOff);
                            }

                            break;
                    }
                }
            }
            else if (_blocksPlus)
            {
                // blocks the filler, but not the wedge
                switch (_axis)
                {
                    case Axis.Z:
                        if (_primeOff > 0)
                        {
                            yield return _plusOpening(AnchorFace.XLow, _primeOff, _secondOff);
                        }
                        else
                        {
                            yield return _plusOpening(AnchorFace.XHigh, 0 - _primeOff, _secondOff);
                        }

                        if (_secondOff > 0)
                        {
                            yield return _plusOpening(AnchorFace.YLow, _secondOff, _primeOff);
                        }
                        else
                        {
                            yield return _plusOpening(AnchorFace.YHigh, 0 - _secondOff, _primeOff);
                        }

                        break;

                    case Axis.Y:
                        if (_primeOff > 0)
                        {
                            yield return _plusOpening(AnchorFace.ZLow, _primeOff, _secondOff);
                        }
                        else
                        {
                            yield return _plusOpening(AnchorFace.ZHigh, 0 - _primeOff, _secondOff);
                        }

                        if (_secondOff > 0)
                        {
                            yield return _plusOpening(AnchorFace.XLow, _secondOff, _primeOff);
                        }
                        else
                        {
                            yield return _plusOpening(AnchorFace.XHigh, 0 - _secondOff, _primeOff);
                        }

                        break;

                    default: // X
                        if (_primeOff > 0)
                        {
                            yield return _plusOpening(AnchorFace.YLow, _primeOff, _secondOff);
                        }
                        else
                        {
                            yield return _plusOpening(AnchorFace.YHigh, 0 - _primeOff, _secondOff);
                        }

                        if (_secondOff > 0)
                        {
                            yield return _plusOpening(AnchorFace.ZLow, _secondOff, _primeOff);
                        }
                        else
                        {
                            yield return _plusOpening(AnchorFace.ZHigh, 0 - _secondOff, _primeOff);
                        }

                        break;
                }
            }
            yield break;
        }
        #endregion

        public override IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        {
            foreach (var _open in OpensTowards(param, movement, upDirection.ReverseFace()).Where(_o => _o.Face == upDirection))
            {
                // TODO: !!!
                yield return new SlopeSegment
                {
                    Low = _open.Amount + baseElev,
                    High = _open.Amount + baseElev,
                    Run = _open.Blockage * 5
                };
            }
            yield break;
        }

        #region public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        public override Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
        {
            var _blocksPlus = !movement.CanMoveThrough(PlusMaterial);
            var _blocksCell = !movement.CanMoveThrough(CellMaterial);
            if (_blocksPlus ^ _blocksCell)
            {
                if (_blocksPlus)
                {
                    // double-down on offset
                    var _vec = new Vector3D();
                    foreach (var _open in OpensTowards(param, movement, gravity))
                    {
                        _vec += _open.OffsetVector3D;
                    }
                    return _vec;
                }
                else
                {
                    // only offset with most coverage
                    return OpensTowards(param, movement, gravity)
                        .Select(_mo => new { MoveOpen = _mo })
                        .OrderByDescending(_mo => _mo?.MoveOpen.Blockage)
                        .FirstOrDefault()?.MoveOpen.OffsetVector3D ?? new Vector3D();
                }
            }
            return new Vector3D();
        }
        #endregion

        /// <summary>Must be valid space for both materials</summary>
        public override bool ValidSpace(uint param, MovementBase movement)
            => base.ValidSpace(param, movement) && movement.CanMoveThrough(PlusMaterial);

        #region public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
        public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock && _plusBlock)
            {
                // both materials block
                return true;
            }
            else if (!_cellBlock && !_plusBlock)
            {
                // neither material blocks
                return false;
            }

            return BlocksExtension(param, movement, snap.ToFaceList(), _plusBlock);
        }
        #endregion

        #region private bool CornerTransit(uint param, IEnumerable<AnchorFace> faces, AnchorFace pLow, AnchorFace pHigh, AnchorFace sLow, AnchorFace sHigh)
        private bool CornerTransit(uint param, IEnumerable<AnchorFace> faces, AnchorFace pLow, AnchorFace pHigh, AnchorFace sLow, AnchorFace sHigh)
        {
            var _param = new WedgeParams(param);
            var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = _param.SecondaryOffset(Offset1, Offset2);
            if (_primeOff > 0)
            {
                if (_secondOff > 0)
                {
                    // XM blocks && YM blocks
                    return faces.Contains(pLow) && faces.Contains(sLow);
                }
                else
                {
                    // XM blocks && YP blocks
                    return faces.Contains(pLow) && faces.Contains(sHigh);
                }
            }
            else
            {
                if (_secondOff > 0)
                {
                    // XP blocks && YM blocks
                    return faces.Contains(pHigh) && faces.Contains(sLow);
                }
                else
                {
                    // XP blocks && YP blocks
                    return faces.Contains(pHigh) && faces.Contains(sHigh);
                }
            }
        }
        #endregion

        #region private bool BlocksExtension(uint param, MovementBase movement, IEnumerable<AnchorFace> faces)
        private bool BlocksExtension(uint param, MovementBase movement, IEnumerable<AnchorFace> faces, bool plus)
        {
            var _param = new WedgeParams(param);
            var _axis = _param.Axis;
            // one material blocks
            if (!plus)
            {
                // cell material blocks: if transit through the wedge/corner
                switch (_axis)
                {
                    case Axis.Z:
                        return CornerTransit(param, faces, AnchorFace.XLow, AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.YHigh);

                    case Axis.Y:
                        return CornerTransit(param, faces, AnchorFace.ZLow, AnchorFace.ZHigh, AnchorFace.XLow, AnchorFace.XHigh);

                    default:
                        return CornerTransit(param, faces, AnchorFace.YLow, AnchorFace.YHigh, AnchorFace.ZLow, AnchorFace.ZHigh);
                }
            }
            else
            {
                // plus material blocks: if *NOT* transit through the wedge/corner (?!?)
                switch (_axis)
                {
                    case Axis.Z:
                        return !CornerTransit(param, faces, AnchorFace.XLow, AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.YHigh);

                    case Axis.Y:
                        return !CornerTransit(param, faces, AnchorFace.ZLow, AnchorFace.ZHigh, AnchorFace.XLow, AnchorFace.XHigh);

                    default:
                        return !CornerTransit(param, faces, AnchorFace.YLow, AnchorFace.YHigh, AnchorFace.ZLow, AnchorFace.ZHigh);
                }
            }
        }
        #endregion

        /// <summary>True if the values have the specified number of 0s or 5s</summary>
        private bool HasExteriorValues(int mustHave, params double[] val)
            => val.Count(_v => _v == 0d || _v == 5d) >= mustHave;

        #region private bool RegularCorner(Axis axis, double primeOff, double secondoff, bool zLow, bool yLow, bool xLow)
        private bool RegularCorner(Axis axis, double primeOff, double secondoff, bool zLow, bool yLow, bool xLow)
        {
            switch (axis)
            {
                case Axis.Z:
                    if (((primeOff >= 0) && xLow) || ((primeOff < 0) && !xLow))
                    {
                        if (((secondoff >= 0) && yLow) || ((secondoff < 0) && !yLow))
                        {
                            return false;
                        }
                    }

                    return true;

                case Axis.Y:
                    if (((primeOff >= 0) && zLow) || ((primeOff < 0) && !zLow))
                    {
                        if (((secondoff >= 0) && xLow) || ((secondoff < 0) && !xLow))
                        {
                            return false;
                        }
                    }

                    return true;

                default:
                    if (((primeOff >= 0) && yLow) || ((primeOff < 0) && !yLow))
                    {
                        if (((secondoff >= 0) && zLow) || ((secondoff < 0) && !zLow))
                        {
                            return false;
                        }
                    }

                    return true;
            }
        }
        #endregion

        #region public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        public override IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock && _plusBlock)
            {
                // no points
            }
            else if (!_cellBlock && !_plusBlock)
            {
                // same as base
                foreach (var _pt in base.TacticalPoints(param, movement))
                {
                    yield return _pt;
                }
            }
            else
            {
                var _param = new WedgeParams(param);
                var _axis = _param.Axis;
                var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
                var _secondOff = _param.SecondaryOffset(Offset1, Offset2);

                // critical values
                var _lowZ = 0d;
                var _lowY = 0d;
                var _lowX = 0d;
                var _highZ = 5d;
                var _highY = 5d;
                var _highX = 5d;

                if (_cellBlock)
                {
                    #region cell blocked

                    #region regular corners
                    // regular corners (x6)
                    if (RegularCorner(_axis, _primeOff, _secondOff, true, true, true))
                    {
                        yield return new Point3D(0, 0, 0);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, true, true, false))
                    {
                        yield return new Point3D(5, 0, 0);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, true, false, true))
                    {
                        yield return new Point3D(0, 5, 0);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, true, false, false))
                    {
                        yield return new Point3D(5, 5, 0);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, true, true))
                    {
                        yield return new Point3D(0, 0, 5);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, true, false))
                    {
                        yield return new Point3D(5, 0, 5);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, false, true))
                    {
                        yield return new Point3D(0, 5, 5);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, false, false))
                    {
                        yield return new Point3D(5, 5, 5);
                    }
                    #endregion

                    switch (_axis)
                    {
                        case Axis.Z:
                            #region adjust X/Y
                            // adjust X
                            if (_primeOff > 0)
                            {
                                _lowX = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _highX = 5 + _primeOff;
                            }

                            // adjust Y
                            if (_secondOff > 0)
                            {
                                _lowY = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _highY = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.Y:
                            #region adjust Z/X
                            // adjust Z
                            if (_primeOff > 0)
                            {
                                _lowZ = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _highZ = 5 + _primeOff;
                            }

                            // adjust X
                            if (_secondOff > 0)
                            {
                                _lowX = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _highX = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.X:
                            #region adjust Y/Z
                            // adjust Y
                            if (_primeOff > 0)
                            {
                                _lowY = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _highY = 5 + _primeOff;
                            }

                            // adjust Z
                            if (_secondOff > 0)
                            {
                                _lowZ = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _highZ = 5 + _secondOff;
                            }

                            break;
                            #endregion
                    }

                    var _midX = (_lowX + _highX) / 2;
                    var _midY = (_lowY + _highY) / 2;
                    var _midZ = (_lowZ + _highZ) / 2;
                    var _zOff = _highZ - _lowZ;
                    var _yOff = _highY - _lowY;
                    var _xOff = _highX - _lowX;
                    var _pBump = (_primeOff >= 0) ? 0d : 5d;
                    var _sBump = (_secondOff >= 0) ? 0d : 5d;

                    switch (_axis)
                    {
                        case Axis.Z:
                            // regular edges
                            yield return new Point3D(5d - _pBump, _sBump, 2.5d);
                            yield return new Point3D(5d - _pBump, 5d - _sBump, 2.5d);
                            yield return new Point3D(_pBump, 5d - _sBump, 2.5d);

                            // prime offset edge
                            yield return new Point3D(_pBump + _primeOff, _sBump, 0);
                            yield return new Point3D(_pBump + _primeOff, _sBump, 2.5);
                            yield return new Point3D(_pBump + _primeOff, _sBump, 5);

                            // prime flush notch face
                            if (_xOff >= 2.5d)
                            {
                                yield return new Point3D(_midX, _sBump, 0d);
                                yield return new Point3D(_midX, _sBump, 2.5d);
                                yield return new Point3D(_midX, _sBump, 5d);
                            }

                            // prime full face
                            yield return new Point3D(5d - _pBump, 2.5d, 0d);     // bottom edge
                            yield return new Point3D(5d - _pBump, 2.5d, 2.5d);   // face
                            yield return new Point3D(5d - _pBump, 2.5d, 5d);     // top edge

                            // second offset edge
                            yield return new Point3D(_pBump, _sBump + _secondOff, 0);
                            yield return new Point3D(_pBump, _sBump + _secondOff, 2.5);
                            yield return new Point3D(_pBump, _sBump + _secondOff, 5);

                            // second flush notch face
                            if (_yOff >= 2.5d)
                            {
                                yield return new Point3D(_pBump, _midY, 0d);   // bottom edge
                                yield return new Point3D(_pBump, _midY, 2.5d); // face
                                yield return new Point3D(_pBump, _midY, 5d);   // top edge
                            }

                            // second full face
                            yield return new Point3D(2.5d, 5d - _sBump, 0d);
                            yield return new Point3D(2.5d, 5d - _sBump, 2.5d);
                            yield return new Point3D(2.5d, 5d - _sBump, 5d);

                            if (_CornerStyle)
                            {
                                #region corner style
                                // inner edge
                                yield return new Point3D(_pBump + _primeOff, _sBump + _secondOff, 0);
                                yield return new Point3D(_pBump + _primeOff, _sBump + _secondOff, 2.5d);
                                yield return new Point3D(_pBump + _primeOff, _sBump + _secondOff, 5);

                                // inner faces
                                if (Math.Abs(_primeOff) >= 2.5d)
                                {
                                    yield return new Point3D(_pBump + (_primeOff / 2), _sBump + _secondOff, 0d);
                                    yield return new Point3D(_pBump + (_primeOff / 2), _sBump + _secondOff, 2.5d);
                                    yield return new Point3D(_pBump + (_primeOff / 2), _sBump + _secondOff, 5d);
                                }

                                if (Math.Abs(_secondOff) >= 2.5d)
                                {
                                    yield return new Point3D(_pBump + _primeOff, _sBump + (_secondOff / 2), 0d);
                                    yield return new Point3D(_pBump + _primeOff, _sBump + (_secondOff / 2), 2.5d);
                                    yield return new Point3D(_pBump + _primeOff, _sBump + (_secondOff / 2), 5d);
                                }
                                #endregion
                            }
                            else
                            {
                                if ((Math.Abs(_primeOff) >= 2.5d) && (Math.Abs(_secondOff) >= 2.5d))
                                {
                                    yield return new Point3D(_pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d), 0d);
                                    yield return new Point3D(_pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d), 2.5d);
                                    yield return new Point3D(_pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d), 5d);
                                }
                            }
                            break;

                        case Axis.Y:
                            // regular edges
                            yield return new Point3D(_sBump, 2.5d, 5d - _pBump);
                            yield return new Point3D(5d - _sBump, 2.5d, 5d - _pBump);
                            yield return new Point3D(5d - _sBump, 2.5d, _pBump);

                            // prime offset edge
                            yield return new Point3D(_sBump, 0, _pBump + _primeOff);
                            yield return new Point3D(_sBump, 2.5, _pBump + _primeOff);
                            yield return new Point3D(_sBump, 5, _pBump + _primeOff);

                            // prime flush notch face
                            if (_zOff >= 2.5d)
                            {
                                yield return new Point3D(_sBump, 0d, _midZ);
                                yield return new Point3D(_sBump, 2.5d, _midZ);
                                yield return new Point3D(_sBump, 5d, _midZ);
                            }

                            // prime full face
                            yield return new Point3D(2.5d, 0d, 5d - _pBump);    // bottom edge
                            yield return new Point3D(2.5d, 2.5d, 5d - _pBump);  // face
                            yield return new Point3D(2.5d, 5d, 5d - _pBump);    // top edge

                            // second offset edge
                            yield return new Point3D(_sBump + _secondOff, 0, _pBump);
                            yield return new Point3D(_sBump + _secondOff, 2.5, _pBump);
                            yield return new Point3D(_sBump + _secondOff, 5, _pBump);

                            // second flush notch face
                            if (_xOff >= 2.5d)
                            {
                                yield return new Point3D(_midY, 0d, _pBump);    // bottom edge
                                yield return new Point3D(_midY, 2.5d, _pBump);  // face
                                yield return new Point3D(_midY, 5d, _pBump);    // top edge
                            }

                            // second full face
                            yield return new Point3D(5d - _sBump, 0d, 2.5d);
                            yield return new Point3D(5d - _sBump, 2.5d, 2.5d);
                            yield return new Point3D(5d - _sBump, 5d, 2.5d);

                            if (_CornerStyle)
                            {
                                #region corner style
                                // inner edge
                                yield return new Point3D(_sBump + _secondOff, 0, _pBump + _primeOff);
                                yield return new Point3D(_sBump + _secondOff, 2.5d, _pBump + _primeOff);
                                yield return new Point3D(_sBump + _secondOff, 5, _pBump + _primeOff);

                                // inner faces
                                if (Math.Abs(_primeOff) >= 2.5d)
                                {
                                    yield return new Point3D(_sBump + _secondOff, 0d, _pBump + (_primeOff / 2));
                                    yield return new Point3D(_sBump + _secondOff, 2.5d, _pBump + (_primeOff / 2));
                                    yield return new Point3D(_sBump + _secondOff, 5d, _pBump + (_primeOff / 2));
                                }

                                if (Math.Abs(_secondOff) >= 2.5d)
                                {
                                    yield return new Point3D(_sBump + (_secondOff / 2), 0d, _pBump + _primeOff);
                                    yield return new Point3D(_sBump + (_secondOff / 2), 2.5d, _pBump + _primeOff);
                                    yield return new Point3D(_sBump + (_secondOff / 2), 5d, _pBump + _primeOff);
                                }
                                #endregion
                            }
                            else
                            {
                                if ((Math.Abs(_primeOff) >= 2.5d) && (Math.Abs(_secondOff) >= 2.5d))
                                {
                                    yield return new Point3D(_sBump + (_secondOff / 2d), 0d, _pBump + (_primeOff / 2d));
                                    yield return new Point3D(_sBump + (_secondOff / 2d), 2.5d, _pBump + (_primeOff / 2d));
                                    yield return new Point3D(_sBump + (_secondOff / 2d), 5d, _pBump + (_primeOff / 2d));
                                }
                            }
                            break;

                        default:
                            // regular edges
                            yield return new Point3D(2.5d, 5d - _pBump, _sBump);
                            yield return new Point3D(2.5d, 5d - _pBump, 5d - _sBump);
                            yield return new Point3D(2.5d, _pBump, 5d - _sBump);

                            // prime offset edge
                            yield return new Point3D(0, _pBump + _primeOff, _sBump);
                            yield return new Point3D(2.5, _pBump + _primeOff, _sBump);
                            yield return new Point3D(5, _pBump + _primeOff, _sBump);

                            // prime flush notch face
                            if (_yOff >= 2.5d)
                            {
                                yield return new Point3D(0d, _midY, _sBump);
                                yield return new Point3D(2.5d, _midY, _sBump);
                                yield return new Point3D(5d, _midY, _sBump);
                            }

                            // prime full face
                            yield return new Point3D(0d, 5d - _pBump, 2.5d);     // bottom edge
                            yield return new Point3D(2.5d, 5d - _pBump, 2.5d);   // face
                            yield return new Point3D(5d, 5d - _pBump, 2.5d);     // top edge

                            // second offset edge
                            yield return new Point3D(0, _pBump, _sBump + _secondOff);
                            yield return new Point3D(2.5, _pBump, _sBump + _secondOff);
                            yield return new Point3D(5, _pBump, _sBump + _secondOff);

                            // second flush notch face
                            if (_zOff >= 2.5d)
                            {
                                yield return new Point3D(0d, _pBump, _midY);   // bottom edge
                                yield return new Point3D(2.5d, _pBump, _midY); // face
                                yield return new Point3D(5d, _pBump, _midY);   // top edge
                            }

                            // second full face
                            yield return new Point3D(0d, 2.5d, 5d - _sBump);
                            yield return new Point3D(2.5d, 2.5d, 5d - _sBump);
                            yield return new Point3D(5d, 2.5d, 5d - _sBump);

                            if (_CornerStyle)
                            {
                                #region corner style
                                // inner edge
                                yield return new Point3D(0, _pBump + _primeOff, _sBump + _secondOff);
                                yield return new Point3D(2.5d, _pBump + _primeOff, _sBump + _secondOff);
                                yield return new Point3D(5, _pBump + _primeOff, _sBump + _secondOff);

                                // inner faces
                                if (Math.Abs(_primeOff) >= 2.5d)
                                {
                                    yield return new Point3D(0, _pBump + (_primeOff / 2), _sBump + _secondOff);
                                    yield return new Point3D(2.5d, _pBump + (_primeOff / 2), _sBump + _secondOff);
                                    yield return new Point3D(5, _pBump + (_primeOff / 2), _sBump + _secondOff);
                                }

                                if (Math.Abs(_secondOff) >= 2.5d)
                                {
                                    yield return new Point3D(0d, _pBump + _primeOff, _sBump + (_secondOff / 2));
                                    yield return new Point3D(2.5d, _pBump + _primeOff, _sBump + (_secondOff / 2));
                                    yield return new Point3D(5d, _pBump + _primeOff, _sBump + (_secondOff / 2));
                                }
                                #endregion
                            }
                            else
                            {
                                if ((Math.Abs(_primeOff) >= 2.5d) && (Math.Abs(_secondOff) >= 2.5d))
                                {
                                    yield return new Point3D(0d, _pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d));
                                    yield return new Point3D(2.5d, _pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d));
                                    yield return new Point3D(5d, _pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d));
                                }
                            }
                            break;
                    }
                    #endregion
                }
                else
                {
                    #region plus blocked
                    switch (_axis)
                    {
                        case Axis.Z:
                            #region adjust X/Y
                            // adjust X
                            if (_primeOff > 0)
                            {
                                _highX = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _lowX = 5 + _primeOff;
                            }

                            // adjust Y
                            if (_secondOff > 0)
                            {
                                _highY = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _lowY = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.Y:
                            #region adjust Z/X
                            // adjust Z
                            if (_primeOff > 0)
                            {
                                _highZ = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _lowZ = 5 + _primeOff;
                            }

                            // adjust X
                            if (_secondOff > 0)
                            {
                                _highX = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _lowX = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.X:
                            #region adjust Y/Z
                            // adjust Y
                            if (_primeOff > 0)
                            {
                                _highY = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _lowY = 5 + _primeOff;
                            }

                            // adjust Z
                            if (_secondOff > 0)
                            {
                                _highZ = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _lowZ = 5 + _secondOff;
                            }

                            break;
                            #endregion
                    }

                    // corners (x8: corner, x6: wedge)
                    if (_CornerStyle || HasExteriorValues(2, _lowX, _lowY, _lowZ))
                    {
                        yield return new Point3D(_lowX, _lowY, _lowZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _lowY, _lowZ))
                    {
                        yield return new Point3D(_highX, _lowY, _lowZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _lowX, _highY, _lowZ))
                    {
                        yield return new Point3D(_lowX, _highY, _lowZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _highY, _lowZ))
                    {
                        yield return new Point3D(_highX, _highY, _lowZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _lowX, _lowY, _highZ))
                    {
                        yield return new Point3D(_lowX, _lowY, _highZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _lowY, _highZ))
                    {
                        yield return new Point3D(_highX, _lowY, _highZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _lowX, _highY, _highZ))
                    {
                        yield return new Point3D(_lowX, _highY, _highZ);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _highY, _highZ))
                    {
                        yield return new Point3D(_highX, _highY, _highZ);
                    }

                    var _midX = (_lowX + _highX) / 2;
                    var _midY = (_lowY + _highY) / 2;
                    var _midZ = (_lowZ + _highZ) / 2;
                    var _zOff = _highZ - _lowZ;
                    var _yOff = _highY - _lowY;
                    var _xOff = _highX - _lowX;
                    if (_CornerStyle)
                    {
                        // edges (<=11: corner)
                        #region corner style edges
                        if (_zOff >= 2.5)
                        {
                            if (HasExteriorValues(1, _lowX, _lowY))
                            {
                                yield return new Point3D(_lowX, _lowY, _midZ);
                            }

                            if (HasExteriorValues(1, _highX, _lowY))
                            {
                                yield return new Point3D(_highX, _lowY, _midZ);
                            }

                            if (HasExteriorValues(1, _lowX, _highY))
                            {
                                yield return new Point3D(_lowX, _highY, _midZ);
                            }

                            if (HasExteriorValues(1, _highX, _highY))
                            {
                                yield return new Point3D(_highX, _highY, _midZ);
                            }
                        }
                        if (_yOff >= 2.5)
                        {
                            if (HasExteriorValues(1, _lowX, _lowZ))
                            {
                                yield return new Point3D(_lowX, _midY, _lowZ);
                            }

                            if (HasExteriorValues(1, _highX, _lowZ))
                            {
                                yield return new Point3D(_highX, _midY, _lowZ);
                            }

                            if (HasExteriorValues(1, _lowX, _highZ))
                            {
                                yield return new Point3D(_lowX, _midY, _highZ);
                            }

                            if (HasExteriorValues(1, _highX, _highZ))
                            {
                                yield return new Point3D(_highX, _midY, _highZ);
                            }
                        }
                        if (_xOff >= 2.5)
                        {
                            if (HasExteriorValues(1, _lowY, _lowZ))
                            {
                                yield return new Point3D(_midX, _lowY, _lowZ);
                            }

                            if (HasExteriorValues(1, _highY, _lowZ))
                            {
                                yield return new Point3D(_midX, _highY, _lowZ);
                            }

                            if (HasExteriorValues(1, _lowY, _highZ))
                            {
                                yield return new Point3D(_midX, _lowY, _highZ);
                            }

                            if (HasExteriorValues(1, _highY, _highZ))
                            {
                                yield return new Point3D(_midX, _highY, _highZ);
                            }
                        }
                        #endregion

                        // faces (<=4)
                        #region corner style faces
                        if ((_xOff >= 2.5) && (_yOff >= 2.5))
                        {
                            if (_lowZ == 0)
                            {
                                yield return new Point3D(_midX, _midY, _lowZ);
                            }

                            if (_highZ == 5)
                            {
                                yield return new Point3D(_midX, _midY, _highZ);
                            }
                        }
                        if ((_xOff >= 2.5) && (_zOff >= 2.5))
                        {
                            if (_lowY == 0)
                            {
                                yield return new Point3D(_midX, _lowY, _midZ);
                            }

                            if (_highY == 5)
                            {
                                yield return new Point3D(_midX, _highY, _midZ);
                            }
                        }
                        if ((_yOff >= 2.5) && (_zOff >= 2.5))
                        {
                            if (_lowX == 0)
                            {
                                yield return new Point3D(_lowX, _midY, _midZ);
                            }

                            if (_highX == 5)
                            {
                                yield return new Point3D(_lowY, _midY, _midZ);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        // edges (<=9)
                        // faces (<=2)
                        switch (_axis)
                        {
                            case Axis.Z:
                                #region Parallel Z
                                if (_primeOff > 0)
                                {
                                    // all lowX edges
                                    yield return new Point3D(_lowX, _lowY, _midZ);
                                    yield return new Point3D(_lowX, _highY, _midZ);
                                    if (_yOff >= 2.5)
                                    {
                                        yield return new Point3D(_lowX, _midY, _lowZ);
                                        yield return new Point3D(_lowX, _midY, _highZ);

                                        // lowX face
                                        yield return new Point3D(_lowX, _midY, _midZ);
                                    }
                                    if (_secondOff > 0)
                                    {
                                        // all lowY edges (not already yielded)
                                        yield return new Point3D(_highX, _lowY, _midZ);
                                        if (_xOff >= 2.5)
                                        {
                                            yield return new Point3D(_midX, _lowY, _lowZ);
                                            yield return new Point3D(_midX, _lowY, _highZ);

                                            // lowY face
                                            yield return new Point3D(_midX, _lowY, _midZ);
                                        }
                                    }
                                    else
                                    {
                                        // all highY edges (not already yielded)
                                        yield return new Point3D(_highX, _highY, _midZ);
                                        if (_xOff >= 2.5)
                                        {
                                            yield return new Point3D(_midX, _highY, _lowZ);
                                            yield return new Point3D(_midX, _highY, _highZ);

                                            // highY face
                                            yield return new Point3D(_midX, _highY, _midZ);
                                        }
                                    }
                                }
                                else
                                {
                                    // all highX edges
                                    yield return new Point3D(_highX, _lowY, _midZ);
                                    yield return new Point3D(_highX, _highY, _midZ);
                                    if (_yOff >= 2.5)
                                    {
                                        yield return new Point3D(_highX, _midY, _lowZ);
                                        yield return new Point3D(_highX, _midY, _highZ);

                                        // lowX face
                                        yield return new Point3D(_highX, _midY, _midZ);
                                    }
                                    if (_secondOff > 0)
                                    {
                                        // all lowY edges (not already yielded)
                                        yield return new Point3D(_lowX, _lowY, _midZ);
                                        if (_xOff >= 2.5)
                                        {
                                            yield return new Point3D(_midX, _lowY, _lowZ);
                                            yield return new Point3D(_midX, _lowY, _highZ);

                                            // lowY face
                                            yield return new Point3D(_midX, _lowY, _midZ);
                                        }
                                    }
                                    else
                                    {
                                        // all highY edges (not already yielded)
                                        yield return new Point3D(_lowX, _highY, _midZ);
                                        if (_xOff >= 2.5)
                                        {
                                            yield return new Point3D(_midX, _highY, _lowZ);
                                            yield return new Point3D(_midX, _highY, _highZ);

                                            // highY face
                                            yield return new Point3D(_midX, _highY, _midZ);
                                        }
                                    }
                                }

                                // diagonal edges
                                yield return new Point3D(_midX, _midY, _lowZ);
                                yield return new Point3D(_midX, _midY, _highZ);
                                break;
                            #endregion

                            case Axis.Y:
                                #region Parallel Y
                                if (_primeOff > 0)
                                {
                                    // all lowZ edges
                                    yield return new Point3D(_lowX, _midY, _lowZ);
                                    yield return new Point3D(_highX, _midY, _lowZ);
                                    if (_xOff >= 2.5)
                                    {
                                        yield return new Point3D(_midX, _lowY, _lowZ);
                                        yield return new Point3D(_midX, _highY, _lowZ);

                                        // lowZ face
                                        yield return new Point3D(_midX, _midY, _lowZ);
                                    }
                                    if (_secondOff > 0)
                                    {
                                        // all lowX edges (not already yielded)
                                        yield return new Point3D(_lowX, _midY, _highZ);
                                        if (_zOff >= 2.5)
                                        {
                                            yield return new Point3D(_lowX, _lowY, _midZ);
                                            yield return new Point3D(_lowX, _highY, _midZ);

                                            // lowX face
                                            yield return new Point3D(_lowX, _midY, _midZ);
                                        }
                                    }
                                    else
                                    {
                                        // all highX edges (not already yielded)
                                        yield return new Point3D(_highX, _midY, _highZ);
                                        if (_zOff >= 2.5)
                                        {
                                            yield return new Point3D(_highX, _lowY, _midZ);
                                            yield return new Point3D(_highX, _highY, _midZ);

                                            // highX face
                                            yield return new Point3D(_highX, _midY, _midZ);
                                        }
                                    }
                                }
                                else
                                {
                                    // all highZ edges
                                    yield return new Point3D(_lowX, _midY, _highZ);
                                    yield return new Point3D(_highX, _midY, _highZ);
                                    if (_xOff >= 2.5)
                                    {
                                        yield return new Point3D(_midX, _lowY, _highZ);
                                        yield return new Point3D(_midX, _highY, _highZ);

                                        // highZ face
                                        yield return new Point3D(_midX, _midY, _highZ);
                                    }
                                    if (_secondOff > 0)
                                    {
                                        // all lowX edges (not already yielded)
                                        yield return new Point3D(_lowX, _midY, _lowZ);
                                        if (_zOff >= 2.5)
                                        {
                                            yield return new Point3D(_lowX, _lowY, _midZ);
                                            yield return new Point3D(_lowX, _highY, _midZ);

                                            // lowX face
                                            yield return new Point3D(_lowX, _midY, _midZ);
                                        }
                                    }
                                    else
                                    {
                                        // all highX edges (not already yielded)
                                        yield return new Point3D(_highX, _midY, _lowZ);
                                        if (_zOff >= 2.5)
                                        {
                                            yield return new Point3D(_highX, _lowY, _midZ);
                                            yield return new Point3D(_highX, _highY, _midZ);

                                            // lowX face
                                            yield return new Point3D(_highX, _midY, _midZ);
                                        }
                                    }
                                }

                                // diagonal edges
                                yield return new Point3D(_midX, _lowY, _midZ);
                                yield return new Point3D(_midX, _highY, _midZ);
                                break;
                            #endregion

                            case Axis.X:
                                #region Parallel X
                                if (_primeOff > 0)
                                {
                                    // all lowY edges
                                    yield return new Point3D(_midX, _lowY, _lowZ);
                                    yield return new Point3D(_midX, _lowY, _highZ);
                                    if (_zOff > 2.5)
                                    {
                                        yield return new Point3D(_lowX, _lowY, _midZ);
                                        yield return new Point3D(_highX, _lowY, _midZ);

                                        // lowY face
                                        yield return new Point3D(_midX, _lowY, _midZ);
                                    }
                                    if (_secondOff > 0)
                                    {
                                        // all lowZ edges (not already yielded)
                                        yield return new Point3D(_midX, _highY, _lowZ);
                                        if (_yOff >= 2.5)
                                        {
                                            yield return new Point3D(_lowX, _midY, _lowZ);
                                            yield return new Point3D(_highX, _midY, _lowZ);

                                            // lowZ face
                                            yield return new Point3D(_midX, _midY, _lowZ);
                                        }
                                    }
                                    else
                                    {
                                        // all highZ edges (not already yielded)
                                        yield return new Point3D(_midX, _highY, _highZ);
                                        if (_yOff >= 2.5)
                                        {
                                            yield return new Point3D(_lowX, _midY, _highZ);
                                            yield return new Point3D(_highX, _midY, _highZ);

                                            // highZ face
                                            yield return new Point3D(_midX, _midY, _highZ);
                                        }
                                    }
                                }
                                else
                                {
                                    // all highY edges
                                    yield return new Point3D(_midX, _highY, _lowZ);
                                    yield return new Point3D(_midX, _highY, _highZ);
                                    if (_zOff > 2.5)
                                    {
                                        yield return new Point3D(_lowX, _highY, _midZ);
                                        yield return new Point3D(_highX, _highY, _midZ);

                                        // highY face
                                        yield return new Point3D(_midX, _highY, _midZ);
                                    }
                                    if (_secondOff > 0)
                                    {
                                        // all lowZ edges (not already yielded)
                                        yield return new Point3D(_midX, _lowY, _lowZ);
                                        if (_yOff >= 2.5)
                                        {
                                            yield return new Point3D(_lowX, _midY, _lowZ);
                                            yield return new Point3D(_highX, _midY, _lowZ);

                                            // lowZ face
                                            yield return new Point3D(_midX, _midY, _lowZ);
                                        }
                                    }
                                    else
                                    {
                                        // all highZ edges (not already yielded)
                                        yield return new Point3D(_midX, _lowY, _highZ);
                                        if (_yOff >= 2.5)
                                        {
                                            yield return new Point3D(_lowX, _midY, _highZ);
                                            yield return new Point3D(_highX, _midY, _highZ);

                                            // highZ face
                                            yield return new Point3D(_midX, _midY, _highZ);
                                        }
                                    }
                                }

                                // diagonal edges
                                yield return new Point3D(_lowX, _midY, _midZ);
                                yield return new Point3D(_highX, _midY, _midZ);
                                break;
                                #endregion
                        }
                    }
                    #endregion
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        public override IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock && _plusBlock)
            {
                // no corners
            }
            else if (!_cellBlock && !_plusBlock)
            {
                // same as base
                foreach (var _pt in base.TargetCorners(param, movement))
                {
                    yield return _pt;
                }
            }
            else
            {
                var _param = new WedgeParams(param);
                var _axis = _param.Axis;
                var _primeOff = _param.PrimaryOffset(Offset1, Offset2);
                var _secondOff = _param.SecondaryOffset(Offset1, Offset2);

                // critical values
                var _lowZ = 0d;
                var _lowY = 0d;
                var _lowX = 0d;
                var _highZ = 5d;
                var _highY = 5d;
                var _highX = 5d;

                if (_cellBlock)
                {
                    #region cell blocked

                    #region regular corners
                    // regular corners (x6)
                    if (RegularCorner(_axis, _primeOff, _secondOff, true, true, true))
                    {
                        yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, true, true, false))
                    {
                        yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, true, false, true))
                    {
                        yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, true, false, false))
                    {
                        yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, true, true))
                    {
                        yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, true, false))
                    {
                        yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, false, true))
                    {
                        yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                    }

                    if (RegularCorner(_axis, _primeOff, _secondOff, false, false, false))
                    {
                        yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                    }
                    #endregion

                    switch (_axis)
                    {
                        case Axis.Z:
                            #region adjust X/Y
                            // adjust X
                            if (_primeOff > 0)
                            {
                                _lowX = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _highX = 5 + _primeOff;
                            }

                            // adjust Y
                            if (_secondOff > 0)
                            {
                                _lowY = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _highY = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.Y:
                            #region adjust Z/X
                            // adjust Z
                            if (_primeOff > 0)
                            {
                                _lowZ = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _highZ = 5 + _primeOff;
                            }

                            // adjust X
                            if (_secondOff > 0)
                            {
                                _lowX = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _highX = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.X:
                            #region adjust Y/Z
                            // adjust Y
                            if (_primeOff > 0)
                            {
                                _lowY = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _highY = 5 + _primeOff;
                            }

                            // adjust Z
                            if (_secondOff > 0)
                            {
                                _lowZ = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _highZ = 5 + _secondOff;
                            }

                            break;
                            #endregion
                    }

                    var _midX = (_lowX + _highX) / 2;
                    var _midY = (_lowY + _highY) / 2;
                    var _midZ = (_lowZ + _highZ) / 2;
                    var _zOff = _highZ - _lowZ;
                    var _yOff = _highY - _lowY;
                    var _xOff = _highX - _lowX;
                    var _pBump = (_primeOff >= 0) ? 0d : 5d;
                    var _sBump = (_secondOff >= 0) ? 0d : 5d;

                    switch (_axis)
                    {
                        case Axis.Z:
                            if (_CornerStyle)
                            {
                                // inner edge
                                yield return new TargetCorner(new Point3D(_pBump + _primeOff, _sBump + _secondOff, 0), AnchorFace.ZLow,
                                    _primeOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    _secondOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh);
                                yield return new TargetCorner(new Point3D(_pBump + _primeOff, _sBump + _secondOff, 5), AnchorFace.ZHigh,
                                    _primeOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    _secondOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh);
                            }
                            else
                            {
                                yield return new TargetCorner(new Point3D(_pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d), 0d), AnchorFace.ZLow,
                                    _primeOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    _secondOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh);
                                yield return new TargetCorner(new Point3D(_pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d), 5d), AnchorFace.ZHigh,
                                    _primeOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    _secondOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh);
                            }
                            break;

                        case Axis.Y:
                            if (_CornerStyle)
                            {
                                // inner edge
                                yield return new TargetCorner(new Point3D(_sBump + _secondOff, 0, _pBump + _primeOff), AnchorFace.YLow,
                                    _primeOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    _secondOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh);
                                yield return new TargetCorner(new Point3D(_sBump + _secondOff, 5, _pBump + _primeOff), AnchorFace.YHigh,
                                    _primeOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    _secondOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh);
                            }
                            else
                            {
                                yield return new TargetCorner(new Point3D(_sBump + (_secondOff / 2d), 0d, _pBump + (_primeOff / 2d)), AnchorFace.YLow,
                                    _primeOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    _secondOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh);
                                yield return new TargetCorner(new Point3D(_sBump + (_secondOff / 2d), 5d, _pBump + (_primeOff / 2d)), AnchorFace.YHigh,
                                    _primeOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    _secondOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh);
                            }
                            break;

                        default:
                            if (_CornerStyle)
                            {
                                // inner edge
                                yield return new TargetCorner(new Point3D(0, _pBump + _primeOff, _sBump + _secondOff), AnchorFace.XLow,
                                    _primeOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    _secondOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh);
                                yield return new TargetCorner(new Point3D(5, _pBump + _primeOff, _sBump + _secondOff), AnchorFace.XHigh,
                                    _primeOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    _secondOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh);
                            }
                            else
                            {
                                yield return new TargetCorner(new Point3D(0d, _pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d)), AnchorFace.XLow,
                                    _primeOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    _secondOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh);
                                yield return new TargetCorner(new Point3D(5d, _pBump + (_primeOff / 2d), _sBump + (_secondOff / 2d)), AnchorFace.XHigh,
                                    _primeOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    _secondOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh);
                            }
                            break;
                    }
                    #endregion
                }
                else
                {
                    #region plus blocked
                    switch (_axis)
                    {
                        case Axis.Z:
                            #region adjust X/Y
                            // adjust X
                            if (_primeOff > 0)
                            {
                                _highX = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _lowX = 5 + _primeOff;
                            }

                            // adjust Y
                            if (_secondOff > 0)
                            {
                                _highY = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _lowY = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.Y:
                            #region adjust Z/X
                            // adjust Z
                            if (_primeOff > 0)
                            {
                                _highZ = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _lowZ = 5 + _primeOff;
                            }

                            // adjust X
                            if (_secondOff > 0)
                            {
                                _highX = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _lowX = 5 + _secondOff;
                            }

                            break;
                        #endregion

                        case Axis.X:
                            #region adjust Y/Z
                            // adjust Y
                            if (_primeOff > 0)
                            {
                                _highY = _primeOff;
                            }
                            else if (_primeOff < 0)
                            {
                                _lowY = 5 + _primeOff;
                            }

                            // adjust Z
                            if (_secondOff > 0)
                            {
                                _highZ = _secondOff;
                            }
                            else if (_secondOff < 0)
                            {
                                _lowZ = 5 + _secondOff;
                            }

                            break;
                            #endregion
                    }

                    // corners (x8: corner, x6: wedge)
                    if (_CornerStyle || HasExteriorValues(2, _lowX, _lowY, _lowZ))
                    {
                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _lowZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _lowY, _lowZ))
                    {
                        yield return new TargetCorner(new Point3D(_highX, _lowY, _lowZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _lowX, _highY, _lowZ))
                    {
                        yield return new TargetCorner(new Point3D(_lowX, _highY, _lowZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _highY, _lowZ))
                    {
                        yield return new TargetCorner(new Point3D(_highX, _highY, _lowZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _lowX, _lowY, _highZ))
                    {
                        yield return new TargetCorner(new Point3D(_lowX, _lowY, _highZ), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _lowY, _highZ))
                    {
                        yield return new TargetCorner(new Point3D(_highX, _lowY, _highZ), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _lowX, _highY, _highZ))
                    {
                        yield return new TargetCorner(new Point3D(_lowX, _highY, _highZ), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                    }

                    if (_CornerStyle || HasExteriorValues(2, _highX, _highY, _highZ))
                    {
                        yield return new TargetCorner(new Point3D(_highX, _highY, _highZ), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
                    }

                    if (!_CornerStyle)
                    {
                        var _midX = (_lowX + _highX) / 2;
                        var _midY = (_lowY + _highY) / 2;
                        var _midZ = (_lowZ + _highZ) / 2;
                        var _zOff = _highZ - _lowZ;
                        var _yOff = _highY - _lowY;
                        var _xOff = _highX - _lowX;
                        switch (_axis)
                        {
                            case Axis.Z:
                                #region Parallel Z
                                // diagonal edges
                                yield return new TargetCorner(new Point3D(_midX, _midY, _lowZ), AnchorFace.ZLow,
                                    _primeOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    _secondOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh);
                                yield return new TargetCorner(new Point3D(_midX, _midY, _highZ), AnchorFace.ZHigh,
                                    _primeOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh,
                                    _secondOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh);
                                break;
                            #endregion

                            case Axis.Y:
                                #region Parallel Y
                                // diagonal edges
                                yield return new TargetCorner(new Point3D(_midX, _lowY, _midZ), AnchorFace.YLow,
                                    _primeOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    _secondOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh);
                                yield return new TargetCorner(new Point3D(_midX, _highY, _midZ), AnchorFace.YHigh,
                                    _primeOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh,
                                    _secondOff > 0 ? AnchorFace.XLow : AnchorFace.XHigh);
                                break;
                            #endregion

                            case Axis.X:
                                #region Parallel X
                                // diagonal edges
                                yield return new TargetCorner(new Point3D(_lowX, _midY, _midZ), AnchorFace.XLow,
                                    _primeOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    _secondOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh);
                                yield return new TargetCorner(new Point3D(_highX, _midY, _midZ), AnchorFace.XHigh,
                                    _primeOff > 0 ? AnchorFace.YLow : AnchorFace.YHigh,
                                    _secondOff > 0 ? AnchorFace.ZLow : AnchorFace.ZHigh);
                                break;
                                #endregion
                        }
                    }
                    #endregion
                }
            }
            yield break;
        }
        #endregion

        #region IWedgeSpace Members

        public bool IsPlusGas => PlusMaterial is GasCellMaterial;
        public bool IsPlusLiquid => PlusMaterial is LiquidCellMaterial;
        public bool IsPlusInvisible => IsPlusGas && (PlusMaterial as GasCellMaterial).IsInvisible;
        public bool IsPlusSolid => PlusMaterial is SolidCellMaterial;
        public bool CornerStyle => _CornerStyle;
        public bool HasTiling => Tiling != null;
        public bool HasPlusTiling => PlusTiling != null;

        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (PlusTiling == null)
            {
                return new BuildableMaterial { Material = null, IsAlpha = false };
            }

            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = PlusTiling.ZPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    }

                    return new BuildableMaterial { Material = PlusTiling.ZMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = PlusTiling.YPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    }

                    return new BuildableMaterial { Material = PlusTiling.YMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = PlusTiling.XPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    }

                    return new BuildableMaterial { Material = PlusTiling.XMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }

        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
            => (PlusTiling == null)
            ? new BuildableMaterial { Material = null, IsAlpha = false }
            : new BuildableMaterial { Material = PlusTiling.WedgeMaterial(effect), IsAlpha = PlusTiling.GetWedgeAlpha() };

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
        {
            return new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = PlusTiling?.BrushCollectionKey ?? string.Empty,
                BrushIndex = PlusTiling?.GetAnchorFaceMaterialIndex(face) ?? 0
            };
        }

        #endregion

        public override CellSpaceInfo ToCellSpaceInfo()
            => new CornerSpaceInfo(this);

        public string PlusMaterialName
            => PlusMaterial?.Name ?? string.Empty;

        public string PlusTilingName
            => PlusTiling?.Name ?? string.Empty;

        #region public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        public override CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock ^ _plusBlock)
            {
                // only 1 material can block
                var _gAxis = gravity.GetAxis();
                var _param = new WedgeParams(param);
                var _axis = _param.Axis;
                var _faces = InnerGripBaseFace(_param, _cellBlock, _plusBlock, _axis);
                if (_gAxis == _axis)
                {
                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerBase(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
                else
                {
                    // if either ONLY the edge is on the gravity face OR-ONLY the plus material blocks...
                    if (WedgeSpaceFaces.GetWedgeEdge(param, Offset1, Offset2).Contains(gravity) ^ _plusBlock)
                    {
                        // inner ledge (handled by the base?)
                        return new CellGripResult
                        {
                            Difficulty = GripRules.GetInnerLedge(_plusBlock),
                            Faces = _faces,
                            InnerFaces = _faces
                        };
                    }

                    return new CellGripResult
                    {
                        Difficulty = GripRules.GetInnerDangling(_plusBlock),
                        Faces = _faces,
                        InnerFaces = _faces
                    };
                }
            }

            // default
            return new CellGripResult
            {
                Difficulty = null,
                Faces = AnchorFaceList.None,
                InnerFaces = AnchorFaceList.None
            };
        }
        #endregion

        #region private AnchorFaceList InnerGripBaseFace(WedgeParams param, bool cellBlock, bool plusBlock, Axis axis)
        private AnchorFaceList InnerGripBaseFace(WedgeParams param, bool cellBlock, bool plusBlock, Axis axis)
        {
            // TODO: may need to revisit with slope-like logic
            var _primeOff = param.PrimaryOffset(Offset1, Offset2);
            var _secondOff = param.SecondaryOffset(Offset1, Offset2);

            if (cellBlock)
            {
                if (!plusBlock)
                {
                    // blocks the wedge, but not the filler
                    switch (axis)
                    {
                        case Axis.Z:
                            if (Math.Abs(_primeOff) > Math.Abs(_secondOff))
                            {
                                if (_primeOff > 0)
                                {
                                    return AnchorFaceList.XHigh;
                                }
                                else
                                {
                                    return AnchorFaceList.XLow;
                                }
                            }
                            else
                            {
                                if (_secondOff > 0)
                                {
                                    return AnchorFaceList.YHigh;
                                }
                                else
                                {
                                    return AnchorFaceList.YLow;
                                }
                            }

                        case Axis.Y:
                            if (Math.Abs(_primeOff) > Math.Abs(_secondOff))
                            {
                                if (_primeOff > 0)
                                {
                                    return AnchorFaceList.ZHigh;
                                }
                                else
                                {
                                    return AnchorFaceList.ZLow;
                                }
                            }
                            else
                            {
                                if (_secondOff > 0)
                                {
                                    return AnchorFaceList.XHigh;
                                }
                                else
                                {
                                    return AnchorFaceList.XLow;
                                }
                            }

                        default: // X
                            if (Math.Abs(_primeOff) > Math.Abs(_secondOff))
                            {
                                if (_primeOff > 0)
                                {
                                    return AnchorFaceList.YHigh;
                                }
                                else
                                {
                                    return AnchorFaceList.YLow;
                                }
                            }
                            else
                            {
                                if (_secondOff > 0)
                                {
                                    return AnchorFaceList.ZHigh;
                                }
                                else
                                {
                                    return AnchorFaceList.ZLow;
                                }
                            }
                    }
                }
            }
            else if (plusBlock)
            {
                // blocks the filler, but not the wedge
                switch (axis)
                {
                    case Axis.Z:
                        if (Math.Abs(_primeOff) > Math.Abs(_secondOff))
                        {
                            if (_primeOff > 0)
                            {
                                return AnchorFaceList.XLow;
                            }
                            else
                            {
                                return AnchorFaceList.XHigh;
                            }
                        }
                        else
                        {
                            if (_secondOff > 0)
                            {
                                return AnchorFaceList.YLow;
                            }
                            else
                            {
                                return AnchorFaceList.YHigh;
                            }
                        }

                    case Axis.Y:
                        if (Math.Abs(_primeOff) > Math.Abs(_secondOff))
                        {
                            if (_primeOff > 0)
                            {
                                return AnchorFaceList.ZLow;
                            }
                            else
                            {
                                return AnchorFaceList.ZHigh;
                            }
                        }
                        else
                        {
                            if (_secondOff > 0)
                            {
                                return AnchorFaceList.XLow;
                            }
                            else
                            {
                                return AnchorFaceList.XHigh;
                            }
                        }

                    default: // X
                        if (Math.Abs(_primeOff) > Math.Abs(_secondOff))
                        {
                            if (_primeOff > 0)
                            {
                                return AnchorFaceList.YLow;
                            }
                            else
                            {
                                return AnchorFaceList.YHigh;
                            }
                        }
                        else
                        {
                            if (_secondOff > 0)
                            {
                                return AnchorFaceList.ZLow;
                            }
                            else
                            {
                                return AnchorFaceList.ZHigh;
                            }
                        }
                }
            }

            // default
            return AnchorFaceList.None;
        }
        #endregion

        #region public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        public override int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            var _cellBlock = !movement.CanMoveThrough(CellMaterial);
            var _plusBlock = !movement.CanMoveThrough(PlusMaterial);
            if (_cellBlock || _plusBlock)
            {
                // disposition (what type of face is being gripped?)
                var _param = new WedgeParams(param);
                var _axis = _param.Axis;
                var _edge = WedgeSpaceFaces.GetWedgeEdge(param, Offset1, Offset2);
                var _gripAxis = gripFace.GetAxis();
                var _disposition = _gripAxis == _axis ? GripDisposition.Irregular
                    : _edge.Contains(gripFace) ? GripDisposition.Rectangular
                    : GripDisposition.Full; // always cell material!

                if (gravity == gripFace)
                {
                    // dangling...
                    if (_disposition == GripDisposition.Full)
                    {
                        return GripRules.GetOuterDangling(false);
                    }
                    return GripRules.GetOuterDangling(_disposition);
                }

                // full face grip
                if (_disposition == GripDisposition.Full)
                {
                    return GripRules.GetOuterBase(false);
                }

                // ledge if the gravity is towards blocking wedge or blocking remainder ...
                // ... but only if a single material blocks
                if ((_plusBlock ^ _cellBlock) &&
                    ((_edge.Contains(gravity) && _plusBlock) || (!_edge.Contains(gravity) && _cellBlock)))
                {
                    // using plus material if the plus was the blocker
                    return GripRules.GetOuterLedge(_plusBlock);
                }

                // mixed face grip without ledge
                return GripRules.GetOuterBase(_disposition);
            }
            return null;
        }
        #endregion

        public override int? InnerSwimDifficulty(uint param)
            => (new[] { base.InnerSwimDifficulty(param), MaterialSwimDifficulty(PlusMaterial) }).Max();

        public override bool SuppliesBreathableAir(uint param)
            => (new[] { CellMaterial, PlusMaterial }).OfType<GasCellMaterial>()
            .Any(_gas => _gas.AirBreathe);

        public override bool SuppliesBreathableWater(uint param)
            => (new[] { CellMaterial, PlusMaterial }).OfType<LiquidCellMaterial>()
            .Any(_liquid => _liquid.AquaticBreathe);
    }
}