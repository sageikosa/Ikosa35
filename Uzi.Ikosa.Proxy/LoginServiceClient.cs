using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Proxy
{
    public class LoginServiceClient : ServiceClient<ILoginService, ILoginCallback>
    {
        public LoginServiceClient(string address, ILoginCallback callback, string userName, string password)
            : base(address, callback, userName, password)
        {
        }
    }
}
