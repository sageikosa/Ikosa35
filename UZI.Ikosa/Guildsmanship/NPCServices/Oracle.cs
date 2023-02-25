using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class Oracle : NonPlayerService
    {
        /// <summary>Relationship type to identify a part (http://pack.guildsmanship.com/ikosa/)</summary>
        public const string Relation = @"http://pack.guildsmanship.com/ikosa/oracle";

        // TODO: divinations

        public Oracle(Description description)
            : base(description)
        {
        }

        protected override string NodeExtension => @"oracle";
        protected override string ContentType => @"ikosa/oracle";
        protected override string RelationshipType => Relation;

        public override string TypeName => typeof(Oracle).FullName;
        public override IEnumerable<ICorePart> Relationships { get { yield break; } }

        protected override void OnSetPackagePart() { }
        public override void Close() { }
        protected override void OnRefreshPart() { }
        protected override void OnDoSave(Uri baseUri) { }

        public override void Save(Package parent)
        {
            DoSaveFile(parent);
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            DoSaveFile(parent, baseUri);
        }
    }
}
