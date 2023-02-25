using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public interface IActivatable : IControlChange<Activation>
    {
        Activation Activation { get; set; }
        IEnumerable<IActivatable> Dependents { get; }
    }

    public interface IActivatableObject : IActivatable, ICoreObject
    {
    }

    public static class IActivatableHelper
    {
        /// <summary>True if either the target is self, or self has a (recursive) dependent that is the target.</summary>
        public static bool HasDepenent(this IActivatable self, IActivatable target)
        {
            if (self == target)
                return true;
            if (self.Dependents.Any(_d => _d.HasDepenent(target)))
                return true;
            return false;

        }
    }
}
