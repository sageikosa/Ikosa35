using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class MagicItemCrafter : NonPlayerService
    {
        /// <summary>Relationship type to identify a part (http://pack.guildsmanship.com/ikosa/)</summary>
        public const string Relation = @"http://pack.guildsmanship.com/ikosa/magicitemcrafter";

        // TODO: item type(s): caster type and level, available spells

        // potion
        // scroll
        // weapons
        // armor
        // rod
        // staff
        // wand
        // ring
        // wondrous items

        public MagicItemCrafter(Description description)
            : base(description)
        {
        }

        protected override string NodeExtension => @"magicitemcrafter";
        protected override string ContentType => @"ikosa/magicitemcrafter";
        protected override string RelationshipType => Relation;

        public override string TypeName => typeof(MagicItemCrafter).FullName;
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
