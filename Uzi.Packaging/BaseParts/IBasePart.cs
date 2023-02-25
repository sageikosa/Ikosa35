using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace Uzi.Packaging
{
    /// <summary>Part that is storable in a package.</summary>
    public interface IBasePart : ICorePart
    {
        /// <summary>Name suitable for binding via WPF (includes a setter)</summary>
        string BindableName { get; set; }

        /// <summary>Parent part that allows this part to be found by name</summary>
        ICorePartNameManager NameManager { get; set; }

        /// <summary>Gets the OPC PackagePart from which this IBasePart was loaded (if defined)</summary>
        PackagePart Part { get; }

        /// <summary>Save IBasePart to Package</summary>
        void Save(Package parent);

        /// <summary>Save IBasePart to PackagePart</summary>
        void Save(PackagePart parent, Uri baseUri);

        /// <summary>Reconstitutes an IBasePart with the contents of the PackagePart</summary>
        void RefreshPart(PackagePart part);

        /// <summary>Close resources</summary>
        void Close();
    }
}
