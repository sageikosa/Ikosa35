using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;

namespace Uzi.Packaging
{
    public class RelatedPackagePart
    {
        public string RelationshipType { get; set; }
        public string ID { get; set; }
        public PackagePart Part { get; set; }
    }
}
