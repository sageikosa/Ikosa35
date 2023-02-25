using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    public interface IRegionCapable : ICapability
    {
        /// <summary>
        /// Actor may be passed as null.  
        /// Implementers must expect this if they are not level dependent.
        /// </summary>
        IEnumerable<double> Dimensions(CoreActor actor, int powerLevel);
        // TODO: this helps widen, but not shapes
    }
}
