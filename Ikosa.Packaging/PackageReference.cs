using System;
using System.Collections.Generic;
using System.Text;

namespace Ikosa.Packaging
{
    public struct PackageReference
    {
        /// <summary>Display name</summary>
        public string ReferenceName { get; set; }

        /// <summary>PackageType to generate</summary>
        public string PackageType { get; set; }

        /// <summary>from campaigns and packages</summary>
        public string PackageSet { get; set; }

        /// <summary>filename that implements the package</summary>
        public string FileName { get; set; }

        /// <summary>Universal resource name for package, if needed to refresh from source</summary>
        public string PackageUrn { get; set; }

        /// <summary>
        /// True if not part of a packageSet
        /// </summary>
        public bool IsResource
            => string.IsNullOrWhiteSpace(PackageSet);

        /// <summary>
        /// Usable as an identifier
        /// </summary>
        public string PackagePath =>
            !string.IsNullOrWhiteSpace(PackageSet)
            ? $@"/{PackageSet}/{FileName}".ToLowerInvariant()
            : $@"/{FileName}".ToLowerInvariant();
    }
}
