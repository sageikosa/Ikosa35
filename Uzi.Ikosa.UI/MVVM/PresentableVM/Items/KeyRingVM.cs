using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.UI
{
    public class KeyRingVM : PresentableThingVM<KeyRing>
    {
        public IEnumerable<PresentableContext> ContextualObjects
            => from _c in Thing?.Objects
               select _c.GetPresentableObjectVM(VisualResources, Possessor);
    }
}
