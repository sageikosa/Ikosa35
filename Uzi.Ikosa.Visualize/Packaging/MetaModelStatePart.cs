using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packaging
{
    public class MetaModelStatePart : StorablePart
    {
        protected MetaModelState _State;

        public MetaModelStatePart(IRetrievablePartNameManager manager, MetaModelState metaModelState, string name) 
            : base(manager, name)
        {
            _State = metaModelState;
        }

        public MetaModelStatePart(IRetrievablePartNameManager manager, ZipArchiveEntry entry) 
            : base(manager, entry)
        {
        }

        public MetaModelState MetaModelState => _State;

        public override IEnumerable<IRetrievablePart> Parts => Enumerable.Empty<IRetrievablePart>();

        public override string PartType => typeof(MetaModelStatePart).FullName;

        public override void ClosePart() { }

        protected override void OnRefreshStoragePart() { }

        protected override void OnStorePart(ZipArchiveEntry lastEntry)
        {
            using var _stream = _Entry.Open();
            using var _writer = new StreamWriter(_stream);
            XamlWriter.Save(_State, _writer);
        }
    }
}
