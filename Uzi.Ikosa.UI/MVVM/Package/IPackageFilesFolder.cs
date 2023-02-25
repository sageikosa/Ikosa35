using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.UI.MVVM.Package
{
    public interface IPackageFilesFolder
    {
        PackagePathEntry PackagePathEntry { get; }
        IList<PackageFileVM> PackageFiles { get; }
        IRefreshPackages RefreshHost { get; }
    }
}
