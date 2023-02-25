using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>ICore supports Imagery</summary>
    public interface ICoreImagery : ICore
    {
        /// <summary>Gets the image key strings in the order they are preferred</summary>
        IEnumerable<string> ImageKeys { get; }
    }
}
