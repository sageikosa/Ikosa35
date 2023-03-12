using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    public class VisualResourcePackage : IkosaPackage
    {
        public VisualResourcePackage()
        {
        }

        public VisualResourcePackage(FileInfo fileInfo, ZipArchive zipArchive) : base(fileInfo, zipArchive)
        {
        }

        protected override IStorablePart CreateStorablePart() 
            => new VisualResourcePart(this, @"VisualResources");

        protected override IStorablePart GetStorablePart()
        {
            var _part = new VisualResourcePart(this, @"VisualResources");
            _part.ReloadPart(Archive, string.Empty);
            return _part;
        }

        public VisualResourcePart VisualResources 
            => _Root as VisualResourcePart;
    }
}
