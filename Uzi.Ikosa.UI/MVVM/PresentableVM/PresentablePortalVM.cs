using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.UI
{
    public class PresentablePortalVM : PresentableThingVM<PortalBase>
    {
        public FlatObjectSideVM PortalSideA
            => Thing.PortalledObjectA.GetPresentableObjectVM(VisualResources, Possessor) as FlatObjectSideVM;

        public FlatObjectSideVM PortalSideB
            => Thing.PortalledObjectB.GetPresentableObjectVM(VisualResources, Possessor) as FlatObjectSideVM;

        public double OpenState
        {
            get => Thing.OpenState.Value;
            set
            {
                Thing.OpenState = new OpenStatus(value);
            }
        }
    }
}
