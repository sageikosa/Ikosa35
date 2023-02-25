using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Uzi.Visualize;
using Uzi.Ikosa.Movement;
using System.Windows.Media;
using Uzi.Visualize.Contracts.Tactical;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Represents a single &quot;damageable&quot; instance of a CellSpace.
    /// NOT currently being used...
    /// </summary>
    public class CellSpaceUnit : IParamCellSpace
    {
        public CellSpaceUnit(CellSpace template)
        {
            _Template = template;
        }

        #region private data
        private CellSpace _Template;
        private int _Struc;
        #endregion

        public string Name => _Template.Name;
        public string GetDescription(uint param) => _Template.GetDescription(param);
        public string GetParamText(uint param) => _Template.GetParamText(param);
        public LocalMap Map => _Template.Map;
        public uint Index => _Template.Index;
        public uint ID => _Template.ID;
        public bool IsTemplate => false;
        public CellSpace Template => _Template;
        public bool IsShadeable(uint param) => _Template.IsShadeable(param);
        public CellSpace Parent => _Template.Parent;

        public BuildableGroup GenerateModel(uint param, int z, int y, int x, Cubic currentGroup)
            => _Template.GenerateModel(param, z, y, x, currentGroup);

        public uint FlipAxis(uint paramsIn, Axis flipAxis)
            => _Template.FlipAxis(paramsIn, flipAxis);

        public uint SwapAxis(uint paramsIn, Axis axis1, Axis axis2)
            => _Template.SwapAxis(paramsIn, axis1, axis2);

        public void AddOuterSurface(uint param, BuildableGroup group, int z, int y, int x, AnchorFace face, VisualEffect effect, Vector3D bump, Cubic currentGroup)
        {
            _Template.AddOuterSurface(param, group, z, y, x, face, effect, bump, currentGroup);
        }

        public void AddInnerStructures(uint param, BuildableGroup addToGroup, int z, int y, int x, VisualEffect effect)
        {
            _Template.AddInnerStructures(param, addToGroup, z, y, x, effect);
        }

        public Brush InnerBrush(uint param, VisualEffect effect)
            => _Template.InnerBrush(param, effect);

        public (string collectionKey, string brushKey) InnerBrushKeys(uint param, Point3D point)
            => _Template?.InnerBrushKeys(param, point) ?? default;

        public bool BlocksDetect(uint param, int z, int y, int x, Point3D entryPt, Point3D exitPt)
            => _Template.BlocksDetect(param, z, y, x, entryPt, exitPt);

        public bool BlocksPath(uint param, int z, int y, int x, Point3D pt1, Point3D pt2)
            => _Template.BlocksPath(param, z, y, x, pt1, pt2);

        public HedralGrip HedralGripping(uint param, MovementBase movement, AnchorFace surfaceFace)
            => _Template.HedralGripping(param, movement, surfaceFace);

        public bool ValidSpace(uint param, MovementBase movement)
            => _Template.ValidSpace(param, movement);

        public IEnumerable<MovementOpening> OpensTowards(uint param, MovementBase movement, AnchorFace baseFace)
            => _Template.OpensTowards(param, movement, baseFace);

        public bool CanPlummetAcross(uint param, MovementBase movement, AnchorFace plummetFace)
            => _Template.CanPlummetAcross(param, movement, plummetFace);

        public IEnumerable<AnchorFace> PlummetDeflection(uint param, MovementBase movement, AnchorFace plummetFace)
            => _Template.PlummetDeflection(param, movement, plummetFace);

        public bool BlockedAt(uint param, MovementBase movement, CellSnap snap)
            => _Template.BlockedAt(param, movement, snap);

        /// <summary>Allows a cell structure to take damage</summary>
        public int StructurePoints { get { return _Struc; } set { _Struc = value; } }

        public Vector3D InteractionOffset3D(uint param)
            => _Template.InteractionOffset3D(param);

        public IEnumerable<Point3D> TacticalPoints(uint param, MovementBase movement)
            => _Template.TacticalPoints(param, movement);

        public IEnumerable<TargetCorner> TargetCorners(uint param, MovementBase movement)
            => _Template.TargetCorners(param, movement);

        public bool? NeighborOccludes(int z, int y, int x, AnchorFace neighborFace, IGeometricRegion currentRegion)
            => _Template.NeighborOccludes(z, y, x, neighborFace, currentRegion);

        public bool? OccludesFace(uint param, AnchorFace outwardFace)
            => _Template.OccludesFace(param, outwardFace);

        public bool? ShowCubicFace(uint param, AnchorFace outwardFace)
            => _Template.ShowCubicFace(param, outwardFace);

        public bool ShowDirectionalFace(uint param, AnchorFace outwardFace)
            => _Template.ShowDirectionalFace(param, outwardFace);

        [field:NonSerialized, JsonIgnore]
        public event EventHandler NeedsRedraw;

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        public IEnumerable<SlopeSegment> InnerSlopes(uint param, MovementBase movement, AnchorFace upDirection, double baseElev)
            => _Template.InnerSlopes(param, movement, upDirection, baseElev);

        public Vector3D OrthoOffset(uint param, MovementBase movement, AnchorFace gravity)
            => _Template.OrthoOffset(param, movement, gravity);

        public CellSpaceInfo ToCellSpaceInfo()
            => _Template.ToCellSpaceInfo();

        public int? OuterGripDifficulty(uint param, AnchorFace gripFace, AnchorFace gravity, MovementBase movement, CellStructure sourceStruc)
            => _Template.OuterGripDifficulty(param, gripFace, gravity, movement, sourceStruc);

        public CellGripResult InnerGripResult(uint param, AnchorFace gravity, MovementBase movement)
            => _Template.InnerGripResult(param, gravity, movement);

        public int? OuterGripMoveOutDeltas(uint param, Size size, AnchorFace gripFace, AnchorFaceList moveTowards, MovementBase movement)
            => _Template.OuterGripMoveOutDeltas(param, size, gripFace, moveTowards, movement);

        public int? OuterGripMoveInDeltas(uint param, Size size, AnchorFace gripFace, AnchorFaceList moveFrom, MovementBase movement)
            => _Template.OuterGripMoveInDeltas(param, size, gripFace, moveFrom, movement);

        public bool SuppliesBreathableAir(uint param)
            => _Template.SuppliesBreathableAir(param);

        public bool SuppliesBreathableWater(uint param)
            => _Template.SuppliesBreathableWater(param);

        public int? InnerSwimDifficulty(uint param)
            => _Template.InnerSwimDifficulty(param);

        public int MaxStructurePoints(uint param)
            => _Template.MaxStructurePoints(param);
    }
}