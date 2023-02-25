using System.Collections.Generic;

namespace Uzi.Core
{
    public interface IObjectContainer : 
        //ICollection<ICoreObject>, 
        IActionProvider, ILoadedObjects, IActionSource
    {
        double MaximumLoadWeight { get; set; }
        bool CanHold(ICoreObject obj);

        // ICollection replacements
        IEnumerable<ICoreObject> Objects { get; }
        int Count { get; }
        void Add(ICoreObject item);
        bool Contains(ICoreObject item);
        bool Remove(ICoreObject item);
   }
}
