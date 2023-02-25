using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public readonly struct CellStructure
    {
        public CellStructure(IParamCellSpace cellSpace, uint paramData)
        {
            CellSpace = cellSpace;
            ParamData = paramData;
        }

        public readonly IParamCellSpace CellSpace;
        public readonly uint ParamData;

        private static readonly CellStructure _Default = new CellStructure(null, 0);
        public static ref readonly CellStructure Default => ref _Default;

        public CellStructure FlipAxis(Axis flipAxis)
            => (CellSpace != null)
            ? new CellStructure(CellSpace, CellSpace.FlipAxis(ParamData, flipAxis))
            : new CellStructure(null, ParamData);

        public CellStructure SwapAxis(Axis axis1, Axis axis2)
            => (CellSpace != null)
            ? new CellStructure(CellSpace, CellSpace.SwapAxis(ParamData, axis1, axis2))
            : new CellStructure(null, ParamData);

        // non-parameterized parts

        public string Name => CellSpace?.Name ?? string.Empty;
        public string Description => CellSpace?.GetDescription(ParamData);
        public string ParamText => CellSpace?.GetParamText(ParamData);
        public LocalMap Map => CellSpace?.Map;
        public uint Index => CellSpace?.Index ?? uint.MaxValue;
        public bool IsTemplate => CellSpace?.IsTemplate ?? true;
        public CellSpace Template => CellSpace?.Template ?? null;
        public bool IsShadeable => CellSpace?.IsShadeable(ParamData) ?? false;
        public CellSpace Parent => CellSpace?.Parent ?? null;
        public ulong ID
            => (new IndexStruct
            {
                Index = Index,
                StateInfo = ParamData
            }).ID;

        /// <summary>Item1 = collection key, Item 2 - brush key</summary>
        public (string collectionKey, string brushKey) InnerBrushKeys(Point3D point)
            => CellSpace?.InnerBrushKeys(ParamData, point) ?? default;

        public Brush InnerBrush(VisualEffect effect)
            => CellSpace?.InnerBrush(ParamData, effect);

        #region public void AddOuterSurface(BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, System.Windows.Media.Media3D.Transform3D bump)
        public void AddOuterSurface(BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            // should show?
            var _show = CellSpace.ShowCubicFace(ParamData, face);
            if (_show ?? true)
            {
                // yes or maybe show
                var _occluded = CellSpace.NeighborOccludes(z, y, x, face, currentGroup);
                if (!_show.HasValue)
                {
                    // maybe show
                    if (_occluded.HasValue && !_occluded.Value)
                    {
                        // neighbor definitely not occluding, so show
                        CellSpace.AddOuterSurface(ParamData, group, z, y, x, face, effect, bump, currentGroup);
                    }
                }
                else if (!(_occluded ?? false))
                {
                    // show, and neighbor is not strongly occluding
                    CellSpace.AddOuterSurface(ParamData, group, z, y, x, face, effect, bump, currentGroup);
                }
            }
        }
        #endregion

        public void AddExteriorSurface(BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Cubic currentGroup)
        {
            // should show?
            if (CellSpace.ShowDirectionalFace(ParamData, face) && (CellSpace.NeighborOccludes(z, y, x, face, currentGroup) ?? true))
            {
                // show, and neighbor is not strongly occluding
                CellSpace.AddOuterSurface(ParamData, group, z, y, x, face, effect, new Vector3D(), currentGroup);
            }
        }

        public void AddInnerStructures(BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            CellSpace.AddInnerStructures(ParamData, addToGroup, z, y, x, effect);
        }

        public BuildableGroup GenerateModel(int z, int y, int x, Cubic currentGroup)
            => CellSpace.GenerateModel(ParamData, z, y, x, currentGroup);

        public bool? OccludesFace(AnchorFace outwardFace)
            => CellSpace.OccludesFace(ParamData, outwardFace);

        public bool BlocksDetect(int z, int y, int x, Point3D entryPt, Point3D exitPt)
            => CellSpace.BlocksDetect(ParamData, z, y, x, entryPt, exitPt);

        public bool BlocksPath(int z, int y, int x, Point3D pt1, Point3D pt2)
            => CellSpace.BlocksPath(ParamData, z, y, x, pt1, pt2);

        public HedralGrip HedralGripping(MovementBase movement, AnchorFace surfaceFace)
            => CellSpace.HedralGripping(ParamData, movement, surfaceFace);

        public bool ValidSpace(MovementBase movement)
            => CellSpace.ValidSpace(ParamData, movement);

        public IEnumerable<MovementOpening> OpensTowards(MovementBase movement, AnchorFace baseFace)
            => CellSpace.OpensTowards(ParamData, movement, baseFace);

        public IEnumerable<SlopeSegment> InnerSlopes(MovementBase movement, AnchorFace upDirection, double baseElev)
            => CellSpace.InnerSlopes(ParamData, movement, upDirection, baseElev);

        public Vector3D OrthoOffset(MovementBase movement, AnchorFace gravity)
            => CellSpace.OrthoOffset(ParamData, movement, gravity);

        public bool CanPlummetAcross(MovementBase movement, AnchorFace plummetFace)
            => CellSpace.CanPlummetAcross(ParamData, movement, plummetFace);

        public IEnumerable<AnchorFace> PlummetDeflection(MovementBase movement, AnchorFace gravity)
            => CellSpace.PlummetDeflection(ParamData, movement, gravity);

        public bool BlockedAt(MovementBase movement, CellSnap snap)
            => CellSpace.BlockedAt(ParamData, movement, snap);

        public static bool operator ==(CellStructure x, CellStructure y)
        {
            return (x.ParamData == y.ParamData) && (x.CellSpace == y.CellSpace);
        }

        public static bool operator !=(CellStructure x, CellStructure y)
        {
            return (x.ParamData != y.ParamData) || (x.CellSpace != y.CellSpace);
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
                return false;

            var _struc = (CellStructure)obj;
            return (_struc.ParamData == ParamData) && (_struc.CellSpace == CellSpace);
        }

        public override int GetHashCode()
        {
            return ParamData.GetHashCode() ^ CellSpace.GetHashCode();
        }

        public Vector3D InteractionOffset3D()
            => CellSpace.InteractionOffset3D(ParamData);

        public IEnumerable<Point3D> TacticalPoints(MovementBase movement)
            => CellSpace.TacticalPoints(ParamData, movement);

        public IEnumerable<TargetCorner> TargetCorners(MovementBase movement)
            => CellSpace.TargetCorners(ParamData, movement);

        // material and geometry absolutes

        public int? OuterGripDifficulty(AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
            => CellSpace.OuterGripDifficulty(ParamData, gripFace, gravity, movement, sourceStruc);

        public int? OuterCornerGripDifficulty(AnchorFaceList edgeFaces, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
            => CellSpace.OuterCornerGripDifficulty(ParamData, edgeFaces, gravity, movement, sourceStruc);

        public CellGripResult InnerGripResult(AnchorFace gravity, MovementBase movement)
            => CellSpace.InnerGripResult(ParamData, gravity, movement);

        public int? InnerSwimDifficulty()
            => CellSpace.InnerSwimDifficulty(ParamData);

        // body-relative grip

        /// <summary>Extra difficulty to grip the cell for climbing while leaving</summary>
        public int? InnerGripMoveOutDelta(double maxReach, AnchorFaceList moveTowards, AnchorFace gravity, MovementBase movement)
        {
            // TODO: calculate from opens-towards/inner-slopes, gravity and move direction
            return null;
        }

        /// <summary>Extra difficulty to grip the cell for climbing while entering</summary>
        public int? InnerGripMoveInDelta(double maxReach, AnchorFaceList moveFrom, AnchorFace gravity, MovementBase movement)
        {
            // TODO: calculate from opens-towards/inner-slopes, gravity and move direction
            return null;
        }

        public int? OuterGripMoveOutDelta(double maxReach, AnchorFace gripSurface, AnchorFaceList moveTowards,
            AnchorFace gravity, MovementBase movement)
        {
            // TODO: used blocked-at and outer opening
            return null;
        }

        public int? OuterGripMoveInDelta(double maxReach, AnchorFace gripSurface, AnchorFaceList moveFrom,
            AnchorFace gravity, MovementBase movement)
        {
            // TODO: used blocked-at and outer openings
            return null;
        }

        public bool SuppliesBreathableWater
            => CellSpace.SuppliesBreathableWater(ParamData);

        public bool SuppliesBreathableAir
            => CellSpace.SuppliesBreathableAir(ParamData);
    }
}