using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship.Overland
{
    [Serializable]
    public class Region : ModuleNode, IPlaceNames
    {
        /// <summary>Relationship type to identify a region object part (http://pack.guildsmanship.com/ikosa/region)</summary>
        public const string RegionRelation = @"http://pack.guildsmanship.com/ikosa/region";

        private Vector3D _Base;
        private decimal _CellScale;
        private int _Columns;
        private int _Rows;
        private int _Order;

        private readonly OverRegion _OverRegion;
        private readonly StoryInformation _StoryInfo;
        private readonly List<SubRegion> _SubRegions;
        private readonly List<SiteLink> _Sites;    // references
        private readonly List<PlaceName> _Names;
        private readonly List<SitePathGraphOverlay> _Overlays;

        private readonly Dictionary<byte, ITerrain> _TerrainIndex;
        private readonly RegionLayer<ITerrain> _Terrain;             // terrain set resource
        private readonly RegionLayer<TileRef> _Tiles;                // tile ref resource

        public Region(Description description)
            : base(description)
        {
            _OverRegion = new OverRegion(description.Clone() as Description);
            _StoryInfo = new StoryInformation();
            _SubRegions = [];
            _Sites = [];
            _Names = [];
            _Overlays = [];

            // NOTE: 6000 is the value for "mile" being used here
            //       each cell is a 16 mile square (one day at base 20')
            _CellScale = 96000;

            // NOTE: 40x30 grid represents graph-paper @ 10"x7.5" with 1/4" gridding (and 1/2" margins)
            //       approx 640 mile width by 480 mile height
            _Columns = 40;
            _Rows = 30;

            _TerrainIndex = [];
            _Terrain = new RegionLayer<ITerrain>(this,
                (r, c, i) => _TerrainIndex.TryGetValue(i, out var _t) ? _t : null,
                (r, c, i) => _TerrainIndex.ContainsKey(i));

            _Tiles = new RegionLayer<TileRef>(this,
                (r, c, i) => TerrainLayer[r, c]?.GetTileRef(i),
                (r, c, i) => TerrainLayer[r, c]?.HasTileRef(i) ?? false);

            _Order = 0;
        }

        public static Region GetRegion(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _region = (Region)_fmt.Deserialize(_ctxStream);
                _region.SetPackagePart(part);

                // return
                return _region;
            }
            return null;
        }

        public override string GroupName => @"Regions";

        public OverRegion OverRegion => _OverRegion;
        public Vector3D CoordinateBase { get => _Base; set => _Base = value; }
        public Vector3D Size => new Vector3D((double)(_CellScale * Columns), (double)(_CellScale * Rows), 0);

        public decimal CellScale { get => _CellScale; set => _CellScale = value; }

        public int Columns { get => _Columns; set => _Columns = value; }
        public int Rows { get => _Rows; set => _Rows = value; }

        public RegionLayer<ITerrain> TerrainLayer => _Terrain;
        public RegionLayer<TileRef> Tiles => _Tiles;

        // --- general identity and connectedness
        public List<PlaceName> Names => _Names;

        public StoryInformation StoryInformation => _StoryInfo;

        public List<SubRegion> SubRegions => _SubRegions;
        public List<SitePathGraphOverlay> SitePathGraphOverlays => _Overlays;

        /// <summary>If needing to resolve positioning overlap conflicts, highest order wins</summary>
        public int Order { get => _Order; set => _Order = value; }

        public bool Contains(Point3D point)
        {
            var _size = Size;
            return (point.X >= _Base.X) && (point.X <= _Base.X + _size.X)
                && (point.Y >= _Base.Y) && (point.Y <= _Base.Y + _size.Y)
                && (point.Z >= _Base.Z) && (point.Z <= _Base.Z + _size.Z);
        }

        public List<SiteLink> Sites => _Sites;

        public SiteLink GetSite(Guid navigationID)
            => _Sites.FirstOrDefault(_sn => _sn.LinkedNodeID == navigationID);

        public override string TypeName => typeof(Region).FullName;

        public override IEnumerable<ICorePart> Relationships { get { yield break; } }
        protected override void OnSetPackagePart() { }

        protected override string NodeExtension => @"region";
        protected override string ContentType => @"ikosa/region";
        protected override string RelationshipType => RegionRelation;

        public override void Close() { }
        protected override void OnRefreshPart() { }
        protected override void OnDoSave(Uri baseUri) { }

        public override void Save(Package parent)
        {
            // TODO: folder?
            DoSaveFile(parent);
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // TODO: folder?
            DoSaveFile(parent, baseUri);
        }
    }
}
