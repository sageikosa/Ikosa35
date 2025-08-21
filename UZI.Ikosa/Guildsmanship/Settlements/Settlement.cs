using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class Settlement : Site, IPlaceNames
    {
        /// <summary>Relationship type to identify a settlement object part (http://pack.guildsmanship.com/ikosa/settlement)</summary>
        public const string SettlementRelation = @"http://pack.guildsmanship.com/ikosa/settlement";

        private int _Population;
        // TODO: species mix

        // largest cost items
        private decimal _TradeLimit;

        // maximum goods buy and sell per month
        private decimal _Fluidity;

        // track of available fluidity
        private decimal _FluidityTracker;

        // when the fluidity was last updated in world time
        private double _LastUpdated;

        // TODO: automatic NPC capacity (and generation if needed)

        private List<NonPlayerService> _Services;           // embed/reference/both ???
        private List<PlaceName> _Names;
        private List<SettlementSubDivision> _SubDivisions;  // embed/reference/both ???

        // TODO: terrain/vision/lighting/climate

        public Settlement(Description description)
            : base(description)
        {
            _Services = [];
            _Names = [];
            _SubDivisions = [];
        }

        public static Settlement GetSettlement(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _settlement = (Settlement)_fmt.Deserialize(_ctxStream);
                _settlement.SetPackagePart(part);

                // return
                return _settlement;
            }
            return null;
        }

        public override string GroupName => @"Settlements";

        // --- general identity and connectedness
        public List<PlaceName> Names => _Names;

        // --- what the game master can offer
        public List<NonPlayerService> NonPlayerServices => _Services;

        public List<SettlementSubDivision> SubDivisions => _SubDivisions;

        public override string TypeName => typeof(Settlement).FullName;

        protected override void OnSetPackagePart() { }

        public override void Close()
        {
            // TODO: only if settlement subdivisions are packaged directly under this
        }


        protected override void OnRefreshPart()
        {
            // TODO: only if settlement subdivisions are packaged directly under this
        }

        protected override string NodeExtension => @"settlement";
        protected override string ContentType => @"ikosa/settlement";
        protected override string RelationshipType => SettlementRelation;

        public override void Save(Package parent)
        {
            // TODO: if folder, then save folder
            DoSaveFile(parent);
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // TODO: if folder, then save folder
            DoSaveFile(parent, baseUri);
        }

        protected override void OnDoSave(Uri baseUri)
        {
            // TODO: only if settlement subdivisions are packaged directly under this
        }
    }
}
