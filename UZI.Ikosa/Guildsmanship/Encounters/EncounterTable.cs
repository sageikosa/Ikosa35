using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class EncounterTable : ModuleNode
    {
        /// <summary>Relationship type to identify an encoutner table object part (http://pack.guildsmanship.com/ikosa/encountertable)</summary>
        public const string EncounterTableRelation = @"http://pack.guildsmanship.com/ikosa/encountertable";

        private Roller _Roller;
        private List<EncounterEntry> _Entries;

        public EncounterTable(Description description, Roller roller) 
            : base(description)
        {
            _Roller = roller;
            _Entries = [];
        }

        public static EncounterTable GetEncounterTable(PackagePart part)
        {
            if (part != null)
            {
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _tbl = (EncounterTable)_fmt.Deserialize(_ctxStream);
                _tbl.SetPackagePart(part);

                // return
                return _tbl;
            }
            return null;
        }

        public Roller Roller { get => _Roller; set => _Roller = value; }
        public List<EncounterEntry> Entries => _Entries;

        protected override void OnSetPackagePart() { }

        public override string GroupName => @"Encounters";

        public override string TypeName => typeof(EncounterTable).FullName;
        public override IEnumerable<ICorePart> Relationships { get { yield break; } }

        protected override string NodeExtension => @"encounters";
        protected override string ContentType => @"ikosa/encountertable";
        protected override string RelationshipType => EncounterTableRelation;

        public override void Close() { }
        protected override void OnRefreshPart() { }
        protected override void OnDoSave(Uri baseUri) { }

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
    }
}
