using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>Indicates the object has contents which may also contribute to load.  The object must signal changes to its loaded contents.</summary>
    public interface ILoadedObjects : ICoreObject, IControlChange<ICoreObject>, IMonitorChange<Physical>
    {
        /// <summary>If a container does not expose loaded objects to the ObjectLoad, this yields nothing</summary>
        IEnumerable<ICoreObject> AllLoadedObjects();
        /// <summary>Must return true if at least some objects contribute to the object load</summary>
        bool ContentsAddToLoad { get; }
        double LoadWeight { get; }
        double TareWeight { get; set; }
    }
}
