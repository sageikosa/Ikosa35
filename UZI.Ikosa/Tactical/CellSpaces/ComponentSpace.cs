using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class ComponentSpace : CellSpace, IPlusCellSpace
    {
        // TODO: IPlusCellSpace : Plus stuff...
        protected ComponentSpace(CellMaterial material, TileSet tileSet, CellMaterial plusMaterial, TileSet plusTiles)
            : base(material, tileSet)
        {
            _PlusMaterial = plusMaterial;
            _PlusTiling = plusTiles;
        }

        #region private data
        private CellMaterial _PlusMaterial;
        private TileSet _PlusTiling;
        #endregion

        public override bool IsShadeable(uint param)
            => true;

        #region public CellMaterial PlusMaterial { get; set; }
        public CellMaterial PlusMaterial
        {
            get { return _PlusMaterial; }
            set
            {
                if (_PlusMaterial != value)
                {
                    _PlusMaterial = value;
                    DoPropertyChanged(@"PlusMaterial");
                }
            }
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
                    DoPropertyChanged(@"PlusTiling");
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

        protected abstract IEnumerable<CellStructure> Components(uint param);

        #region public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        public override IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        {
            // all boundaries
            foreach (var _opening in YieldBoundaries((from _c in Components(param)
                                                      from _b in _c.OpensTowards(movement, baseFace)
                                                      select _b).ToList()))
            {
                yield return _opening;
            }

            yield break;
        }
        #endregion

        #region protected IEnumerable<MovementOpening> YieldBoundaries(List<MovementOpening> source)
        protected IEnumerable<MovementOpening> YieldBoundaries(List<MovementOpening> source)
        {
            if ((source.Any(_k => _k.Face == AnchorFace.ZHigh) && source.Any(_k => _k.Face == AnchorFace.ZLow)) ||
               (source.Any(_k => _k.Face == AnchorFace.YHigh) && source.Any(_k => _k.Face == AnchorFace.YLow)) ||
               (source.Any(_k => _k.Face == AnchorFace.XHigh) && source.Any(_k => _k.Face == AnchorFace.XLow)))
            {
                // nothing if directly opposed boundaries
            }
            else
            {
                // ZBoundaries
                if (source.Any(_k => _k.Face == AnchorFace.ZHigh))
                {
                    foreach (var _k in source.Where(_k => _k.Face == AnchorFace.ZHigh))
                    {
                        yield return _k;
                    }
                }
                else if (source.Any(_k => _k.Face == AnchorFace.ZLow))
                {
                    foreach (var _k in source.Where(_k => _k.Face == AnchorFace.ZLow))
                    {
                        yield return _k;
                    }
                }

                // YBoundaries
                if (source.Any(_k => _k.Face == AnchorFace.YHigh))
                {
                    foreach (var _k in source.Where(_k => _k.Face == AnchorFace.YHigh))
                    {
                        yield return _k;
                    }
                }
                else if (source.Any(_k => _k.Face == AnchorFace.YLow))
                {
                    foreach (var _k in source.Where(_k => _k.Face == AnchorFace.YLow))
                    {
                        yield return _k;
                    }
                }

                // XBoundaries
                if (source.Any(_k => _k.Face == AnchorFace.XHigh))
                {
                    foreach (var _k in source.Where(_k => _k.Face == AnchorFace.XHigh))
                    {
                        yield return _k;
                    }
                }
                else if (source.Any(_k => _k.Face == AnchorFace.XLow))
                {
                    foreach (var _k in source.Where(_k => _k.Face == AnchorFace.XLow))
                    {
                        yield return _k;
                    }
                }
            }
            yield break;
        }
        #endregion

        public override bool ValidSpace(uint param, MovementBase movement)
            => Components(param).All(_c => _c.ValidSpace(movement));

        /// <summary>True if this cell can be entered during a plummet (including being blown or swept away)</summary>
        public override bool CanPlummetAcross(uint param, MovementBase movement, AnchorFace plummetFace)
            => Components(param).All(_c => _c.CanPlummetAcross(movement, plummetFace));

        #region public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        public override void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            foreach (var _cSpace in Components(param))
            {
                _cSpace.AddInnerStructures(addToGroup, z, y, x, effect);
            }
        }
        #endregion

        #region public override void AddOuterSurface(uint param, BuildablePair pair, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump)
        public override void AddOuterSurface(uint param, BuildableGroup pair, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            foreach (var _cSpace in Components(param))
            {
                _cSpace.AddOuterSurface(pair, z, y, x, face, effect, bump, currentGroup);
            }
        }
        #endregion

        public override bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
            => Components(param).Any(_c => _c.BlockedAt(movement, snap));

        public override bool BlocksDetect(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
            => Components(param).Any(_c => _c.BlocksDetect(z, y, x, pt1, pt2));

        public override bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
            => Components(param).Any(_c => _c.BlocksPath(z, y, x, pt1, pt2));

        public override HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
        {
            var _grip = new HedralGrip(false);
            foreach (var _c in Components(param))
            {
                _grip = _grip.Union(_c.HedralGripping(movement, surfaceFace));
            }

            return _grip;
        }

        #region IPlusCellSpace Members

        public bool IsPlusGas => PlusMaterial is GasCellMaterial;
        public bool IsPlusLiquid => PlusMaterial is LiquidCellMaterial;
        public bool IsPlusInvisible => IsPlusGas && (PlusMaterial as GasCellMaterial).IsInvisible;
        public bool IsPlusSolid => PlusMaterial is SolidCellMaterial;

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
            => new BuildableMaterial
            {
                Material = PlusTiling?.WedgeMaterial(effect),
                IsAlpha = PlusTiling?.GetWedgeAlpha() ?? false
            };

        public BuildableMeshKey GetPlusBuildableMeshKey(AnchorFace face, VisualEffect effect)
        {
            return new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = PlusTiling?.BrushCollectionKey ?? string.Empty,
                BrushIndex = PlusTiling?.GetAnchorFaceMaterialIndex(face) ?? 0
            };
        }

        public string PlusMaterialName => PlusMaterial?.Name ?? string.Empty;
        public string PlusTilingName => PlusTiling?.Name ?? string.Empty;

        #endregion

        public override bool SuppliesBreathableAir(uint param)
            => Components(param).Any(_c => _c.SuppliesBreathableAir);

        public override bool SuppliesBreathableWater(uint param)
            => Components(param).Any(_c => _c.SuppliesBreathableWater);
    }
}
