using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa
{
    public class PackageSetPathEntry : PackageEntry
    {
        public IEnumerable<PackagePathEntry> GetPackagePaths()
            => from _c in Path.ToEnumerable()
               let _dir = new DirectoryInfo(_c)
               from _f in _dir.GetDirectories()
               select new PackagePathEntry
               {
                   PackageSetPathEntry = this,
                   Name = _f.Name,
                   Path = _f.FullName
               };

        public IList<PackagePathEntry> PackagePaths => GetPackagePaths().ToList();
    }
}
