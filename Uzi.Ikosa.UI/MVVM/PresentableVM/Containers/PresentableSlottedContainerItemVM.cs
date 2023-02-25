using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.UI
{
    public class PresentableSlottedContainerItemVM : PresentableThingVM<SlottedContainerItemBase>
    {
        public PresentableContext ContainerObjectContext
            => Thing?.Container.GetPresentableObjectVM(VisualResources, Possessor);
    }
}
