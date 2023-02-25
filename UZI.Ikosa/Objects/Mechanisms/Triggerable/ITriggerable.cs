using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    public interface ITriggerable : IActivatableObject
    {
        PostTriggerState PostTriggerState { get; }
        void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators);
    }
}
