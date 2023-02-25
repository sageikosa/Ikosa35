using System;
using System.Collections.Generic;
using Uzi.Visualize;
using Uzi.Packaging;
using Uzi.Visualize.Contracts.Tactical;
using System.Runtime.Serialization;
using System.Linq;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    /// <summary>Used by the local map for anything not in a room</summary>
    public class BackgroundCellGroup : LocalCellGroup, ICorePart, ISerializable
    {
        #region construction
        public BackgroundCellGroup(CellStructure templateCell, ICellLocation start, IGeometricSize size,
            LocalMap map, string name, bool deepShadows)
            : base(start, size, map, name, deepShadows)
        {
            _TemplateCell = templateCell;
        }

        protected BackgroundCellGroup(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _TemplateCell = (CellStructure)info.GetValue(nameof(_TemplateCell), typeof(CellStructure));
        }
        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(_TemplateCell), _TemplateCell);
        }

        #region state
        private CellStructure _TemplateCell = default;
        #endregion

        public ref CellStructure TemplateCell => ref _TemplateCell;

        #region public override IEnumerable<ICellLocation> AllCellLocations()
        public override IEnumerable<ICellLocation> AllCellLocations()
        {
            // NOTE: background cell sets are intended to be massive, not going to accidentally allow this
            yield break;
        }
        #endregion

        /// <summary>Gets a GeoCell using map coordinates</summary>
        public override ref readonly CellStructure GetCellSpace(ICellLocation location)
            => ref GetCellSpace(location.Z, location.Y, location.X);

        #region public override IBaseSpace GetCellSpace(ICellLocation location)
        public override ref readonly CellStructure GetCellSpace(int z, int y, int x)
        {
            if (ContainsCell(z, y, x))
                return ref _TemplateCell;
            return ref CellStructure.Default;
        }
        #endregion

        //public override CellStructure? GetContainedCellSpace(int z, int y, int x)
        //    => ContainsCell(z, y, x) ? _TemplateCell : (CellStructure?)null;

        // ICorePart Members
        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public string TypeName => GetType().FullName;

        #region public override void RefreshTerrainShading()
        public override void RefreshTerrainShading()
        {
            Map.ShadingZones.ReshadeBackground();
        }
        #endregion

        public override LightRange GetLightLevel(ICellLocation location) => LightRange.OutOfRange;
        public override bool IsInMagicDarkness(ICellLocation location) => false;
        public override bool IsPartOfBackground => true;

        public BackgroundCellGroupInfo ToBackgroundCellGroupInfo()
        {
            var _info = ToInfo<BackgroundCellGroupInfo>();
            _info.TemplateSpace = TemplateCell.CellSpace.ToCellSpaceInfo();
            _info.ParamData = TemplateCell.ParamData;
            return _info;
        }
    }
}
