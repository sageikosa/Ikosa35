using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.ComponentModel;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using Uzi.Packaging;
using System.Windows.Media;
using Uzi.Visualize.Contracts.Tactical;
using Newtonsoft.Json;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class CellSpace : IParamCellSpace, ICellSpace, ICorePart
    {
        #region Construction
        public CellSpace(CellMaterial material, TileSet tileSet)
            : this(material, tileSet, true)
        {
            _Rules = new GripRules();
            _Rules.InitializeUniform(material);
        }

        protected CellSpace(CellMaterial material, TileSet tileSet, bool defer)
        {
            if ((tileSet == null) || material.AvailableTilings.Any(_m => _m?.Name.Equals(tileSet.Name) ?? false))
            {
                _CellMaterial = material;
                _Tiling = tileSet;
                _Name = string.Empty;
                _Rules = new GripRules();
            }
            else
            {
                throw new ArgumentException(@"TileSet must be an available tiling of the CellMaterial");
            }
        }
        #endregion

        #region data
        private CellMaterial _CellMaterial;
        private TileSet _Tiling;
        private string _Name;
        private CellSpace _Parent = null;
        private uint _Index;
        private GripRules _Rules = null;
        #endregion

        public uint Index { get { return _Index; } set { _Index = value; } }

        public virtual LocalMap Map => CellMaterial?.LocalMap;
        public virtual uint ID => _Index;
        public CellSpace Template => this;

        public virtual IEnumerable<CellMaterial> AllMaterials
            => CellMaterial.ToEnumerable();

        public GripRules GripRules
            => _Rules ??= new GripRules();

        #region public CellMaterial CellMaterial { get; set; }
        public CellMaterial CellMaterial
        {
            get { return _CellMaterial; }
            set
            {
                if ((CellMaterial != value) && OnCanCellMaterialChange(value))
                {
                    _CellMaterial = value;
                    OnCellMaterialChanged();
                    DoPropertyChanged(nameof(CellMaterial));
                }
            }
        }

        protected virtual bool OnCanCellMaterialChange(CellMaterial material) { return true; }

        /// <summary>
        /// base initializes grip rules for the new cell material
        /// </summary>
        protected virtual void OnCellMaterialChanged() { GripRules.InitializeUniform(CellMaterial); }
        #endregion

        #region public TileSet Tiling { get; set; }
        public TileSet Tiling
        {
            get { return _Tiling; }
            set
            {
                if (_Tiling != value)
                {
                    _Tiling = value;
                    OnTilingChanged();
                    DoPropertyChanged(nameof(Tiling));
                }
            }
        }

        protected virtual void OnTilingChanged() { }
        #endregion

        #region public string Name { get; set; }
        public string Name
        {
            get { return _Name; }
            set
            {
                _Name = value;
                DoPropertyChanged(nameof(Name));
            }
        }
        #endregion

        public virtual string GetDescription(uint param)
            => $@"U:{Name} ({CellMaterialName};{TilingName})";

        public virtual string GetParamText(uint param)
            => string.Empty;

        /// <summary>Cell is shadeable purely on its own structure</summary>
        public virtual bool IsShadeable(uint param)
            => !(CellMaterial is SolidCellMaterial);

        /// <summary>Used when the CellSpace is part of a CompositeCellSpace</summary>
        public CellSpace Parent { get { return _Parent; } set { _Parent = value; } }

        #region public virtual void AddOuterSurface(uint param, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Transform3D bump)
        public virtual void AddOuterSurface(uint param, BuildableGroup buildable, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            CellSpaceFaces.AddOuterSurface(param, this, buildable, z, y, x, face, effect, bump);
        }
        #endregion

        /// <summary>Generates Model3DGroup for any part of the cell that is not flush to the surface</summary>
        public virtual void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            return;
        }

        public virtual uint FlipAxis(uint paramsIn, Axis flipAxis) => paramsIn;
        public virtual uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2) => paramsIn;

        #region public BuildableGroup GenerateModel(uint param, int z, int y, int x)
        /// <summary>Generates the model for a uniform fill</summary>
        public BuildableGroup GenerateModel(uint param, int z, int y, int x, Cubic currentGroup)
        {
            // setup
            var _opaque = new Model3DGroup();
            var _alpha = new Model3DGroup();
            var _context = new BuildableContext();
            var _group = new BuildableGroup { Context = _context, Opaque = _opaque, Alpha = _alpha };
            var _move = new TranslateTransform3D(x * 5d, y * 5d, z * 5d);

            // add stuff
            AddOuterSurface(param, _group, z, y, x, AnchorFace.ZLow, VisualEffect.Normal, new Vector3D(), currentGroup);
            AddOuterSurface(param, _group, z, y, x, AnchorFace.ZHigh, VisualEffect.Normal, new Vector3D(), currentGroup);
            AddOuterSurface(param, _group, z, y, x, AnchorFace.YLow, VisualEffect.Normal, new Vector3D(), currentGroup);
            AddOuterSurface(param, _group, z, y, x, AnchorFace.YHigh, VisualEffect.Normal, new Vector3D(), currentGroup);
            AddOuterSurface(param, _group, z, y, x, AnchorFace.XLow, VisualEffect.Normal, new Vector3D(), currentGroup);
            AddOuterSurface(param, _group, z, y, x, AnchorFace.XHigh, VisualEffect.Normal, new Vector3D(), currentGroup);
            AddInnerStructures(param, _group, z, y, x, VisualEffect.Normal);

            // merge buildables
            Model3DGroup _getFinal(bool alpha, Model3DGroup gathered)
            {
                var _final = new Model3DGroup();
                foreach (var _m in _context.GetModel3D(alpha))
                {
                    _final.Children.Add(_m);
                }

                if (gathered.Children.Count > 0)
                {
                    gathered.Transform = _move;
                    _final.Children.Add(gathered);
                }
                if (_final.Children.Count > 0)
                {
                    _final.Freeze();
                    return _final;
                }
                return null;
            }

            return new BuildableGroup
            {
                Alpha = _getFinal(true, _alpha),
                Opaque = _getFinal(false, _opaque)
            };
        }
        #endregion

        // TODO: affect on specific interactions...

        public virtual bool BlocksDetect(uint param, int z, int y, int x, Point3D entryPt, Point3D exitPt)
            => (CellMaterial.DetectBlockingThickness <= ((Vector3D)(exitPt - entryPt)).Length);

        public virtual bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
            => CellMaterial.BlocksEffect;

        /// <summary>Provides a number in range 0 to 5 for the linear amount of blockage of a face from another face</summary>
        public virtual double HedralTransitBlocking(uint param, MovementBase movement, AnchorFace transitFace, AnchorFace fromFace)
            => !movement.CanMoveThrough(CellMaterial) ? 5 : 0;

        /// <summary>Used to indicate that the space is completely valid (totally clear)</summary>
        public virtual bool ValidSpace(uint param, MovementBase movement)
            => movement.CanMoveThrough(CellMaterial);

        #region public virtual IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace gravity)
        /// <summary>
        /// Boundaries are used to provide hints and data on how this cell can connect to others, 
        /// and that it will require some squeezing if used as a boundary.
        /// Must still use HedralTransitBlocking to confirm suitability for particular configurations.
        /// </summary>
        public virtual IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
        {
            // the base cell space cannot be a boundary for anything (its either valid or invalid)
            yield break;
        }
        #endregion

        /// <summary>True if this cell can be entered during a plummet (including being blown or swept away)</summary>
        public virtual bool CanPlummetAcross(uint param, MovementBase movement, AnchorFace gravity)
            => HedralGripping(param, movement, gravity).GripCount() < 24;

        #region public IEnumerable<AnchorFace> PlummetDeflection(uint param, MovementBase movement, AnchorFace plummetFace)
        /// <summary>
        /// Offset faces for deflecting on plummet.  
        /// Based on IEnumerable&lt;MovementOpening&gt; retrieved from OpensTowards
        /// </summary>
        public IEnumerable<AnchorFace> PlummetDeflection(uint param, MovementBase movement, AnchorFace gravity)
        {
            // default implementation is to return all opening faces that are not axial with plummet face
            var _revPlummet = gravity.ReverseFace();
            return OpensTowards(param, movement, gravity)
                .Where(_mo => _mo.Face != gravity && _mo.Face != _revPlummet)
                .Select(_mo => _mo.Face);
        }
        #endregion

        // TODO: need a little more hint-control of offsetVector? especially for wedge, composite
        // TODO: such as, support on a ledge...cling to wall (conditional offsetvectors)

        /// <summary>True if this cellspace is blocked at the specified interior (all highs set to null), face, edge, or corner</summary>
        public virtual bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
            // NOTE: used for squeezing, extension, and larger creature movement ability
            => !movement.CanMoveThrough(CellMaterial);

        // NOTE: not going to try to determine odd matchups for transit and blocking
        // NOTE: if odd matchups are needed, they may need to be patched with blocking movement zones

        #region INotifyPropertyChanged Members

        protected void DoPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IParamCellSpace Members

        public bool IsGas => CellMaterial is GasCellMaterial;
        public bool IsLiquid => CellMaterial is LiquidCellMaterial;
        public bool IsInvisible => IsGas && (CellMaterial as GasCellMaterial).IsInvisible;

        public BuildableMeshKey GetBuildableMeshKey(AnchorFace face, VisualEffect effect)
            => new BuildableMeshKey
            {
                Effect = effect,
                BrushKey = (Tiling != null) ? Tiling.BrushCollectionKey : string.Empty,
                BrushIndex = (Tiling != null) ? Tiling.GetAnchorFaceMaterialIndex(face) : 0
            };

        #region public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlus, VisualEffect effect)
        {
            if (Tiling == null)
            {
                return new BuildableMaterial { Material = null, IsAlpha = false };
            }

            switch (axis)
            {
                case Axis.Z:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = Tiling.ZPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.ZHigh) };
                    }

                    return new BuildableMaterial { Material = Tiling.ZMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.ZLow) };
                case Axis.Y:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = Tiling.YPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.YHigh) };
                    }

                    return new BuildableMaterial { Material = Tiling.YMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.YLow) };
                default:
                    if (isPlus)
                    {
                        return new BuildableMaterial { Material = Tiling.XPlusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.XHigh) };
                    }

                    return new BuildableMaterial { Material = Tiling.XMinusMaterial(effect), IsAlpha = Tiling.GetAnchorFaceAlpha(AnchorFace.XLow) };
            }
        }
        #endregion

        public BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect)
            => new BuildableMaterial
            {
                Material = Tiling?.WedgeMaterial(effect),
                IsAlpha = Tiling?.GetWedgeAlpha() ?? false
            };

        public virtual bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentRegion)
            => Map[z, y, x, neighborFace, currentRegion].OccludesFace(neighborFace.ReverseFace());

        public virtual bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => (CellMaterial is SolidCellMaterial)
                ? true
                : (IsInvisible ? (bool?)false : null);

        public virtual bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => false;

        public virtual bool? OccludesFace(uint param, AnchorFace outwardFace)
            => (CellMaterial is SolidCellMaterial)
                ? true
                : (IsInvisible ? (bool?)false : null);

        public virtual Brush InnerBrush(uint param, VisualEffect effect)
            => _Tiling?.InnerBrush(effect);

        public virtual (string collectionKey, string brushKey) InnerBrushKeys(uint param, Point3D point)
            => (_Tiling?.BrushCollectionKey, _Tiling?.BrushCollection[_Tiling.InnerMaterialIndex].BrushKey);

        public string CellMaterialName => CellMaterial?.Name ?? string.Empty;
        public string TilingName => Tiling?.Name ?? string.Empty;
        public bool IsSolid => CellMaterial is SolidCellMaterial;

        public virtual int MaxStructurePoints(uint param)
            => 60 * ((_CellMaterial as SolidCellMaterial)?.SolidMaterial.StructurePerInch ?? 0);

        #endregion

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships
        {
            get { yield break; }
        }

        public string TypeName => GetType().FullName;

        #endregion

        public bool IsTemplate => true;

        public virtual Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
            => new Vector3D();

        #region public virtual IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection)
        public virtual IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
        {
            yield break;
        }
        #endregion

        #region protected IEnumerable<Point3D> RawTacticalPoints { get; }
        protected IEnumerable<Point3D> RawTacticalPoints
        {
            get
            {
                // corners (x8)
                yield return new Point3D(0, 0, 0);
                yield return new Point3D(5, 0, 0);
                yield return new Point3D(0, 5, 0);
                yield return new Point3D(5, 5, 0);

                yield return new Point3D(0, 0, 5);
                yield return new Point3D(5, 0, 5);
                yield return new Point3D(0, 5, 5);
                yield return new Point3D(5, 5, 5);

                // edges (x12)
                yield return new Point3D(0, 2.5, 0);
                yield return new Point3D(5, 2.5, 0);
                yield return new Point3D(2.5, 0, 0);
                yield return new Point3D(2.5, 5, 0);

                yield return new Point3D(0, 0, 2.5);
                yield return new Point3D(5, 0, 2.5);
                yield return new Point3D(0, 5, 2.5);
                yield return new Point3D(5, 5, 2.5);

                yield return new Point3D(0, 2.5, 5);
                yield return new Point3D(5, 2.5, 5);
                yield return new Point3D(2.5, 0, 5);
                yield return new Point3D(2.5, 5, 5);

                // faces (x6)
                yield return new Point3D(2.5, 2.5, 0);
                yield return new Point3D(2.5, 2.5, 5);
                yield return new Point3D(2.5, 0, 2.5);
                yield return new Point3D(2.5, 5, 2.5);
                yield return new Point3D(0, 2.5, 2.5);
                yield return new Point3D(5, 2.5, 2.5);

                // center
                yield return new Point3D(2.5, 2.5, 2.5);
                yield break;
            }
        }
        #endregion

        public virtual Vector3D InteractionOffset3D(uint param)
            => new Vector3D();

        #region public virtual IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        public virtual IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
        {
            if (movement.CanMoveThrough(CellMaterial))
            {
                foreach (var _pt in RawTacticalPoints)
                {
                    yield return _pt;
                }
            }
            yield break;
        }
        #endregion

        #region public virtual IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        public virtual IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
        {
            if (movement.CanMoveThrough(CellMaterial))
            {
                // corners (x8)
                yield return new TargetCorner(new Point3D(0, 0, 0), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZLow);
                yield return new TargetCorner(new Point3D(5, 0, 0), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZLow);
                yield return new TargetCorner(new Point3D(0, 5, 0), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZLow);
                yield return new TargetCorner(new Point3D(5, 5, 0), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZLow);

                yield return new TargetCorner(new Point3D(0, 0, 5), AnchorFace.XLow, AnchorFace.YLow, AnchorFace.ZHigh);
                yield return new TargetCorner(new Point3D(5, 0, 5), AnchorFace.XHigh, AnchorFace.YLow, AnchorFace.ZHigh);
                yield return new TargetCorner(new Point3D(0, 5, 5), AnchorFace.XLow, AnchorFace.YHigh, AnchorFace.ZHigh);
                yield return new TargetCorner(new Point3D(5, 5, 5), AnchorFace.XHigh, AnchorFace.YHigh, AnchorFace.ZHigh);
            }
            yield break;
        }
        #endregion

        public virtual CellSpaceInfo ToCellSpaceInfo()
            => new CellSpaceInfo(this);

        #region ICellSpaceTacticalMovement Members

        public virtual int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
        {
            if (movement.CanMoveThrough(CellMaterial))
            {
                return null;
            }

            if (gripFace == gravity)
            {
                var _grip = GripRules.Rules.OfType<AnchorFaceGripRule>()
                    .FirstOrDefault(_gr => _gr.AppliesTo.Contains(gripFace) && _gr.Dangling.HasValue);
                if (_grip != null)
                {
                    return _grip.Dangling;
                }
            }
            else if (gripFace == gravity.ReverseFace())
            {
                return -5;
            }
            else
            {
                var _grip = GripRules.Rules.OfType<AnchorFaceGripRule>()
                    .FirstOrDefault(_gr => _gr.AppliesTo.Contains(gripFace) && _gr.Base.HasValue);
                if (_grip != null)
                {
                    return _grip.Base;
                }
            }
            return null;
        }

        public virtual int? OuterGripMoveOutDeltas(uint param, Size size, AnchorFace gripFace, AnchorFaceList moveTowards, MovementBase movement)
            => movement.CanMoveThrough(CellMaterial)
            ? (int?)null
            : 0;

        public virtual int? OuterGripMoveInDeltas(uint param, Size size, AnchorFace gripFace, AnchorFaceList moveFrom, MovementBase movement)
            => movement.CanMoveThrough(CellMaterial)
            ? (int?)null
            : 0;

        public virtual CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
            => new CellGripResult
            {
                Difficulty = null,
                InnerFaces = AnchorFaceList.None,
                Faces = AnchorFaceList.None
            };

        protected int? MaterialSwimDifficulty(CellMaterial material)
            => (material as LiquidCellMaterial)?.SwimDifficulty ?? null;

        public virtual int? InnerSwimDifficulty(uint param)
            => MaterialSwimDifficulty(CellMaterial);

        public virtual HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
            => (movement.CanMoveThrough(CellMaterial))
            ? new Tactical.HedralGrip(false) // no grip
            : new Tactical.HedralGrip(true);

        #endregion

        public virtual bool SuppliesBreathableAir(uint param)
            => (CellMaterial as GasCellMaterial)?.AirBreathe ?? false;

        public virtual bool SuppliesBreathableWater(uint param)
            => (CellMaterial as LiquidCellMaterial)?.AquaticBreathe ?? false;
    }
}