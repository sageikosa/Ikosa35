using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Ikosa.Packaging
{
    /// <summary>Part that is storable in a package.</summary>
    public interface IStorablePart : IRetrievablePart
    {
        /// <summary>Name suitable for binding via WPF (includes a setter)</summary>
        string MutableName { get; set; }

        /// <summary>Parent part that allows this part to be found by name</summary>
        IRetrievablePartNameManager PartNameManager { get; set; }

        /// <summary>Save IStorablePart to PackagePart</summary>
        void StorePart(ZipArchive archive, string parentPath);

        /// <summary>Reconstitutes an IStorablePart with the contents of the PackagePart</summary>
        void ReloadPart(ZipArchive archive, string parentPath);

        /// <summary>Close resources</summary>
        void ClosePart();
    }
}
