using System;
using System.Collections.Generic;
using System.ServiceModel;
using Uzi.Core.Contracts.Faults;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Contracts.Host
{
    // NOTE: If you change the interface name "ILoginService" here, you must also update the reference to "ILoginService" in App.config.
    [ServiceContract(Namespace = Statics.ServiceNamespace, CallbackContract = typeof(ILoginCallback))]
    [ServiceKnownType(@"KnownTypes", typeof(IkosaKnownTypes))]
    public interface ILoginService
    {
        // boot-strapping
        [OperationContract]
        List<UserInfo> GetUserList(bool allUsers);
        [OperationContract]
        List<CreatureLoginInfo> GetAvailableCreatures();
        [OperationContract]
        List<CreatureLoginInfo> GetAllCreatures();
        [OperationContract]
        List<BitmapImageInfo> GetPortraits(List<Guid> ids);

        // user-centric
        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        void SendMessage(string user, string message);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        List<UserMessage> GetMessages();

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        [FaultContract(typeof(InvalidStateFault))]
        UserInfo Login(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        [FaultContract(typeof(InvalidStateFault))]
        UserInfo Logout(Guid id);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        [FaultContract(typeof(InvalidStateFault))]
        void LogoutUser();

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        RollerLog RollDice(string key, string description, string expression, Guid notify);

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        FlowState GetFlowState();

        [OperationContract]
        [FaultContract(typeof(SecurityFault))]
        bool GetPauseState();
    }
}