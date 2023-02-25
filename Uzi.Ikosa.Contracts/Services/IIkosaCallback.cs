using System;
using System.Collections.Generic;
using System.ServiceModel;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [ServiceContract(Namespace = Statics.ServiceNamespace)]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface IIkosaCallback
    {
        [OperationContract(IsOneWay = true)]
        void SerialStateChanged();

        [OperationContract(IsOneWay = true)]
        void SystemNotifications(Notification[] sysInfos);

        [OperationContract(IsOneWay = true)]
        void WaitingOnUsers(List<string> waitList);
    }
}
