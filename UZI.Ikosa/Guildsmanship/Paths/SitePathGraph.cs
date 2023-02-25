using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Guildsmanship.Overland;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class SitePathGraph : ModuleNode
    {
        /// <summary>Relationship type to identify a site path graph object part (http://pack.guildsmanship.com/ikosa/sitepathgraph)</summary>
        public const string SitePathGraphRelation = @"http://pack.guildsmanship.com/ikosa/sitepathgraph";

        private readonly List<EncounterTableLink> _Encounters;
        private readonly List<OverRegion> _Regions;

        private readonly List<SitePath> _Segments;
        private readonly Dictionary<Guid, SiteLink> _Links;

        public SitePathGraph(Description description, SitePath initialSegment)
            : base(description)
        {
            _Regions = new List<OverRegion>();
            _Segments = new List<SitePath> { initialSegment };
            _Links = new Dictionary<Guid, SiteLink>();
            _Encounters = new List<EncounterTableLink>();
        }

        public static SitePathGraph GetSitePathGraph(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _spgraph = (SitePathGraph)_fmt.Deserialize(_ctxStream);
                _spgraph.SetPackagePart(part);

                // return
                return _spgraph;
            }
            return null;
        }

        // --- what might happen here
        /// <summary>Get SitePaths where the SiteNavigation is a source or target</summary>
        public IEnumerable<SitePath> GetSitePaths(SiteLink siteLink)
            => _Segments.Where(_s => _s.HasSiteLink(siteLink.ID));

        protected bool HasLinkedNode(Guid siteNodeID, SitePath path)
            => GetSiteLink(path.SourceLinkID)?.LinkedNodeID == siteNodeID
            || GetSiteLink(path.TargetLinkID)?.LinkedNodeID == siteNodeID;

        /// <summary>Get SitePaths where the NavigationID of the source or target matches navigationID</summary>
        public IEnumerable<SitePath> GetSitePaths(Guid siteNodeID)
            => _Segments.Where(_s => HasLinkedNode(siteNodeID, _s));

        public override string GroupName => @"Site Paths";

        public override string TypeName => typeof(SitePathGraph).FullName;

        public override IEnumerable<ICorePart> Relationships { get { yield break; } }
        protected override void OnSetPackagePart() { }

        protected override string NodeExtension => @"sitepathgraph";
        protected override string ContentType => @"ikosa/sitepathgraph";
        protected override string RelationshipType => SitePathGraphRelation;

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

        public List<EncounterTableLink> Encounters => _Encounters;
        public List<OverRegion> OverRegions => _Regions;

        public bool AddSiteLink(SiteLink siteLink)
        {
            if (!_Links.ContainsKey(siteLink.ID))
            {
                _Links.Add(siteLink.ID, siteLink);
                return true;
            }
            return false;
        }

        public bool RemoveSiteLink(SiteLink siteLink)
        {
            if (_Links.Remove(siteLink.ID))
            {
                foreach (var _segment in _Segments.Where(_s => _s.HasSiteLink(siteLink.ID)).ToList())
                {
                    _Segments.Remove(_segment);
                }
                return true;
            }
            return false;
        }

        public SiteLink GetSiteLink(Guid siteLinkID)
            => _Links[siteLinkID];

        public IEnumerable<SiteLink> SiteLinks
            => _Links.Select(_sl => _sl.Value);

        public IEnumerable<SitePath> Segments => _Segments;

        /// <summary>During construction, paths must connect to existing site links</summary>
        public bool AddSegment(SitePath segment)
        {
            if (_Links.ContainsKey(segment.SourceLinkID)
                && _Links.ContainsKey(segment.TargetLinkID))
            {
                // source and target are already being tracked
                _Segments.Add(segment);
                return true;
            }

            // could not add
            return false;
        }

        public bool Remove(SitePath segment)
            => _Segments.Remove(segment);

    }
}
