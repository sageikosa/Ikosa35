using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.UI
{
    public class PresentableMechanismMountVM : PresentableThingVM<MechanismMount>
    {
        public IEnumerable<PresentableContext> AnchoredVMs
            => Thing.Anchored.Select(_a => _a.GetPresentableObjectVM(VisualResources, Possessor));
    }
}
