using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>
    /// Anything that can be used as an anchorage (such as a container target) for other IAdjunctables.  
    /// This should only be called by the ObjectBound adjunct(?)
    /// </summary>
    public interface IAnchorage: ILoadedObjects
    {
        bool CanAcceptAnchor(IAdjunctable newAnchor);
        bool CanEjectAnchor(IAdjunctable existingAnchor);
        void AcceptAnchor(IAdjunctable newAnchor);
        void EjectAnchor(IAdjunctable existingAnchor);

        /// <summary>Objects expressly appearing as anchored on target (as opposed to simply connected)</summary>
        IEnumerable<ICoreObject> Anchored { get; }
   }
}
