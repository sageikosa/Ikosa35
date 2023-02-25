using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Uzi.Ikosa.Contracts
{
    [ServiceContract(
        Namespace = Statics.ServiceNamespace,
        CallbackContract = typeof(IIkosaCallback)
        )]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface IIkosaCombinedServices : IIkosaServices, IIkosaAdvancement
    {
    }
}
