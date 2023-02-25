using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Packaging
{
    public interface IPackagePartReference
    {
        string Name { get; }

        /// <summary>Package set for package</summary>
        string PackageSet { get; }

        /// <summary>file-path | PackageID</summary>
        string PackageID { get; }

        /// <summary>Part path in package</summary>
        string InternalPath { get; }

        (string packageSet, string packageID) GetRefKey();

        IBasePart Part { get; }
        CorePackage ResolvePackage();
    }
}
