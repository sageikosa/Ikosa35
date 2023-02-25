using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.UI
{
    public class PresentableContainerObjectVM : PresentableThingVM<ContainerObject>
    {
        public IEnumerable<PresentableContext> ContextualContents
            => from _c in Thing?.Connected
               select _c.GetPresentableObjectVM(VisualResources, Possessor);

        public void DoChangedContents()
            => DoPropertyChanged(nameof(ContextualContents));
    }
}
