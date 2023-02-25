using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class LocalMapSite : Site, IPlaceNames
    {
        /// <summary>Relationship type to identify a local map site object part (http://pack.guildsmanship.com/ikosa/mapsite)</summary>
        public const string LocalMapSiteRelation = @"http://pack.guildsmanship.com/ikosa/mapsite";

        private List<PlaceName> _Names;
        private List<LocalPlace> _LocalPlaces;

        [field: NonSerialized]
        private LocalMap _Map;

        public LocalMapSite(Description description)
            : base(description)
        {
            _Names = new List<PlaceName>();
            _LocalPlaces = new List<LocalPlace>();
            _Map = new LocalMap
            {
                BindableName = @"Map"
            };
        }

        public static LocalMapSite GetLocalMapSite(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _mapSite = (LocalMapSite)_fmt.Deserialize(_ctxStream);
                _mapSite.SetPackagePart(part);

                // return
                return _mapSite;
            }
            return null;
        }

        public override string GroupName => @"Local Maps";

        // --- general identity and connectedness
        public List<PlaceName> Names => _Names;

        public List<LocalPlace> LocalPlaces => _LocalPlaces;
        public LocalMap Map => _Map;

        public override string TypeName => typeof(LocalMapSite).FullName;

        protected override void OnSetPackagePart() { }

        public ModuleUse CurrentUse
            => Part == null ? ModuleUse.New
            : _Map == null ? ModuleUse.Referenced
            : ModuleUse.Open;

        public void Open()
        {
            if (CurrentUse == ModuleUse.Referenced)
            {
                // parts
                _Map = Part.GetRelationships().RelatedBaseParts(this).OfType<LocalMap>().FirstOrDefault()
                    ?? new LocalMap { BindableName = @"Map" };
            }
        }

        public override void Close()
        {
            if (CurrentUse == ModuleUse.Open)
            {
                // after this, CurrentUse will be Referenced
                _Map?.Close();
                _Map = null;
            }
        }

        protected override void OnRefreshPart()
        {
            if (CurrentUse != ModuleUse.Referenced)
            {
                var _part = Part.GetRelationships().RelatedPackageParts()
                    .FirstOrDefault(_p => _p.RelationshipType == LocalMap.IkosaMapRelation);
                if (_part != null)
                {
                    _Map.RefreshPart(_part.Part);
                }
            }
        }

        protected override string NodeExtension => @"mapsite";
        protected override string ContentType => @"ikosa/mapsite";
        protected override string RelationshipType => LocalMapSiteRelation;

        public override void Save(Package parent)
        {
            // ensure local map is fully loaded
            if (CurrentUse == ModuleUse.Referenced)
            {
                Open();
            }
            DoSaveFolder(parent, @"mapsite.ikosa");
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // ensure local map is fully loaded
            if (CurrentUse == ModuleUse.Referenced)
            {
                Open();
            }
            DoSaveFolder(parent, baseUri, @"mapsite.ikosa");
        }

        protected override void OnDoSave(Uri baseUri)
        {
            _Map.Save(Part, baseUri);
        }
    }
}
