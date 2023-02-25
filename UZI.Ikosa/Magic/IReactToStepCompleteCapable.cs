using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic
{
    public interface IReactToStepCompleteCapable : ICapability
    {
        bool WillReactToStepComplete(Creature actor, CoreStep step);
    }
}
