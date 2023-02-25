using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Uzi.Ikosa.Contracts
{
    [ServiceContract(Namespace = Statics.ServiceNamespace)]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface IMasterControlCallback
    {
        [OperationContract(IsOneWay = true)]
        void Stub();
    }
}
