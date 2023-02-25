using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa
{
    public class IkosaPackageManagerConfig
    {
        /// <summary>List of folders where general packages (resources) are kept</summary>
        public List<PackagePathEntry> Packages { get; set; }

        /// <summary>List of folders where general packages (resources) are kept</summary>
        public List<PackagePathEntry> TeamTracking { get; set; }

        /// <summary>List of folders where campaign folders are kept</summary>
        public List<PackageSetPathEntry> Campaigns { get; set; }
    }
}
