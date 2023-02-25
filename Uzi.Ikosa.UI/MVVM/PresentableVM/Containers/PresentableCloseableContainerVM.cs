using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.UI
{
    public class PresentableCloseableContainerVM : PresentableThingVM<CloseableContainerObject>
    {
        public PresentableContainerObjectVM ContainerObjectVM
            => Thing?.Container.GetPresentableObjectVM(VisualResources, Possessor) as PresentableContainerObjectVM;
    }
}
