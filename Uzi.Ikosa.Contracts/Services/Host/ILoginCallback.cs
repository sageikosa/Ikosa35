using System.ServiceModel;

namespace Uzi.Ikosa.Contracts.Host
{
    [ServiceContract(Namespace = Statics.ServiceNamespace)]
    public interface ILoginCallback
    {
        [OperationContract(IsOneWay = true)]
        void NewMessage();
        [OperationContract(IsOneWay = true)]
        void UserListChanged();
        [OperationContract(IsOneWay = true)]
        void FlowStateChanged();
        [OperationContract(IsOneWay = true)]
        void PauseChanged(bool isPaused);
        [OperationContract(IsOneWay = true)]
        void UserLogout(string userName);
    }
}
