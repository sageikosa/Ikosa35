using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa
{
    public class PackagePathEntry : PackageEntry
    {
        public PackageSetPathEntry PackageSetPathEntry { get; set; }

        public IEnumerable<PackageFileEntry> GetPackages()
            => from _p in Path.ToEnumerable()
               let _dir = new DirectoryInfo(_p)
               where _dir.Exists
               from _pck in _dir.GetFiles(@"*.ikosa")
               select new PackageFileEntry
               {
                   Name = _pck.Name,
                   Path = _pck.FullName
               };

        public IList<PackageFileEntry> PackageFiles => GetPackages().ToList();
    }
}
