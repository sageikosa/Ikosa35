using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.IkosaSvc
{
    public class IkosaServicesClient : ServiceClient<IIkosaCombinedServices, IIkosaCallback>
    {
        public IkosaServicesClient(string address, IIkosaCallback callback, string userName, string password) :
            base(address, callback, userName, password)
        {
        }
    }
}
