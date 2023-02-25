using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class CylinderSpace : CellSpace, IPlusCellSpace
    {
        #region ctor()
        public CylinderSpace(SolidCellMaterial solid, TileSet solidTiling, CellMaterial plus, TileSet plusTiling)
            : this(solid, solidTiling, plus, plusTiling, true)
        {
        }

        protected CylinderSpace(SolidCellMaterial solid, TileSet solidTiling, CellMaterial plusMaterial, TileSet plusTiles,
            bool defer)
            : base(solid, solidTiling, defer)
        {
            _PlusMaterial = plusMaterial;
            _PlusTiling = plusTiles;
        }
        #endregion

        #region data
        private CellMaterial _PlusMaterial;
        private TileSet _PlusTiling;
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

        protected virtual bool OnCanPlusMaterialChange(CellMaterial material)
            => !(material is SolidCellMaterial);
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

        protected override bool OnCanCellMaterialChange(CellMaterial material)
            => CellMaterial is SolidCellMaterial;

        // TODO: pediment material...

        //public override bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentGroup)
        //    => false;

        public override bool? OccludesFace(uint param, AnchorFace outwardFace)
            => false;

        private bool CircleLineIntersection(Point point1, Point point2)
        {
            double _d1, _d2, A, B, C, det;

            _d1 = point2.X - point1.X;
            _d2 = point2.Y - point1.Y;

            A = _d1 * _d1 + _d2 * _d2;
            B = 2 * (_d1 * (point1.X) + _d2 * (point1.Y));
            C = (point1.X) * (point1.X) + (point1.Y) * (point1.Y) - 6.25d;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                return false;
            }
            else if (det == 0)
            {
                // One solution.
                return true;
            }
            else
            {
                // Two solutions.
                return true;
            }
        }

        public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
        {
            if (CellMaterial.BlocksEffect)
            {
                var _loc = new CellPosition(z, y, x);
                var _centroid = _loc.GetPoint3D();
                var _v1 = (pt1 - _centroid);
                var _v2 = (pt2 - _centroid);
                var _param = new CylinderParams(param);
                switch (_param.AnchorFace.GetAxis())
                {
                    case Axis.X:
                        return CircleLineIntersection(new Point(_v1.Y, _v1.Z), new Point(_v2.Y, _v2.Z));

                    case Axis.Y:
                        return CircleLineIntersection(new Point(_v1.Z, _v1.X), new Point(_v2.Z, _v2.X));

                    case Axis.Z:
                    default:
                        return CircleLineIntersection(new Point(_v1.X, _v1.Y), new Point(_v2.X, _v2.Y));
                }
            }
            return false;
        }

        public override bool IsShadeable(uint param)
            => true; // always shadeable based on its structure

        public override bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => outwardFace.GetAxis() != new CylinderParams(param).AnchorFace.GetAxis();

        public override void AddOuterSurface(uint param, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            CylinderSpaceFaces.AddCylindricalQuarterFace(param, this, buildable, z, y, x, face, effect, bump);
            if (!IsPlusInvisible)
                CellSpaceFaces.AddPlusOuterSurface(param, this, buildable, z, y, x, face, effect, bump);
            // TODO: pediment? cornice?
        }

        public override uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
        {
            var _param = new CylinderParams(paramsIn);
            if (axis1 == _param.AnchorFace.GetAxis())
                _param.AnchorFace = _param.AnchorFace.IsLowFace() ? axis2.GetLowFace() : axis2.GetHighFace();
            else if (axis2 == _param.AnchorFace.GetAxis())
                _param.AnchorFace = _param.AnchorFace.IsLowFace() ? axis1.GetLowFace() : axis2.GetHighFace();
            return _param.Value;
        }

        public override string GetDescription(uint param)
            => $@"Cyl:{Name} ({CellMaterialName};{TilingName}),({PlusMaterialName};{PlusTilingName})";

        public override string GetParamText(uint param)
        {
            var _param = new CylinderParams(param);
            return $@"Face={_param.AnchorFace}, Style={_param.Style}, Segs={_param.SegmentCount}";
        }

        #region IPlusSpace members
        public string PlusMaterialName
            => PlusMaterial?.Name ?? string.Empty;

        public string PlusTilingName
            => PlusTiling?.Name ?? string.Empty;

        public bool IsPlusGas => PlusMaterial is GasCellMaterial;
        public bool IsPlusLiquid => PlusMaterial is LiquidCellMaterial;
        public bool IsPlusSolid => false;
        public bool IsPlusInvisible => (PlusMaterial as GasCellMaterial)?.IsInvisible ?? false;

        #region public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (PlusTiling == null)
                return new BuildableMaterial { Material = null, IsAlpha = false };
            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                        return new BuildableMaterial { Material = PlusTiling.ZPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    return new BuildableMaterial { Material = PlusTiling.ZMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                        return new BuildableMaterial { Material = PlusTiling.YPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    return new BuildableMaterial { Material = PlusTiling.YMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                        return new BuildableMaterial { Material = PlusTiling.XPlusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    return new BuildableMaterial { Material = PlusTiling.XMinusMaterial(effect), IsAlpha = PlusTiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }
        #endregion

        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
            => new BuildableMaterial { Material = null, IsAlpha = false };

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
            => new CylinderSpaceInfo(this);

        // overrides for plus material considerations
        public override bool SuppliesBreathableAir(uint param)
            => (new[] { CellMaterial, PlusMaterial }).OfType<GasCellMaterial>()
            .Any(_gas => _gas.AirBreathe);

        public override bool SuppliesBreathableWater(uint param)
            => (new[] { CellMaterial, PlusMaterial }).OfType<LiquidCellMaterial>()
            .Any(_liquid => _liquid.AquaticBreathe);
    }
}
