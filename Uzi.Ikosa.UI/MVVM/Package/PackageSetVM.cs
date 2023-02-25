using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI.MVVM.Package
{
    public class PackageSetVM
    {
        public PackageSetPathEntry PackageSetPathEntry { get; set; }
        public object CommandHost { get; set; }
        public bool IsNodeExpanded { get; set; }

        public IList<PackageCampaignVM> Campaigns
            => PackageSetPathEntry?.GetPackagePaths()
            .Select(_pp =>
            new PackageCampaignVM
            {
                PackagePathEntry = _pp,
                CommandHost = CommandHost
            }).ToList();
    }
}
