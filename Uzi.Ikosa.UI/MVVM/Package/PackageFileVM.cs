using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI.MVVM.Package
{
    public class PackageFileVM
    {
        public IPackageFilesFolder Folder { get; set; }
        public PackageFileEntry PackageFileEntry { get; set; }
        public object CommandHost { get; set; }
        public bool IsNodeExpanded { get => false; set { } }

        public bool InPackageSet => Folder is PackageSetVM;
    }
}
