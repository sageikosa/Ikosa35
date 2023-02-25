using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.MasterSvc
{
    public class MasterServicesClient : ServiceClient<IMasterControl, IMasterControlCallback>
    {
        public MasterServicesClient(string address, IMasterControlCallback callback, string userName, string password) 
            : base(address, callback, userName, password)
        {
        }
    }
}
