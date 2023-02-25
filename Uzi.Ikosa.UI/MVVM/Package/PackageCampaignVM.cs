using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI.MVVM.Package
{
    public class PackageCampaignVM : IPackageFilesFolder
    {
        public PackagePathEntry PackagePathEntry { get; set; }
        public object CommandHost { get; set; }
        public bool IsNodeExpanded { get; set; }
        public IRefreshPackages RefreshHost => CommandHost as IRefreshPackages;

        public IList<PackageFileVM> PackageFiles
            => PackagePathEntry?.PackageFiles.Select(_pf =>
            new PackageFileVM
            {
                Folder = this,
                PackageFileEntry = _pf,
                CommandHost = CommandHost
            }).ToList();
    }
}
