using System.Collections.Generic;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Core
{
    /// <summary>ICore supports Iconography</summary>
    public interface ICoreIconic : IIconReference, ICore
    {
        /// <summary>Gets the icon key strings in the order they are preferred</summary>
        IEnumerable<string> IconKeys { get; }

        /// <summary>Gets the icon key strings to use when building a visualized presentation</summary>
        IEnumerable<string> PresentationKeys { get; }
    }
}
