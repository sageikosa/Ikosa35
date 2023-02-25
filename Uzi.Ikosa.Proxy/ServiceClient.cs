using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Uzi.Ikosa.Proxy
{
    public abstract class ServiceClient<ServiceType, CallbackType> : IDisposable
    {
        protected ServiceClient(string address, CallbackType callback, string userName, string password)
        {
            _Callback = callback;
            _Endpoint = new EndpointAddress(address);
            _Binding = new CustomBinding(@"IkosaClearTCPBinding");
            _Disposed = false;
            _UserName = userName;
            _Password = password;
        }

        #region private data
        private ServiceType _Channel;
        private CallbackType _Callback;

        private string _UserName;
        private string _Password;
        private CustomBinding _Binding;
        private DuplexChannelFactory<ServiceType> _Factory;
        private readonly EndpointAddress _Endpoint;
        private bool _Disposed;
        private readonly object _LockObject = new object();
        #endregion

        public void Abort()
            => (Service as ICommunicationObject)?.Abort();

        public CommunicationState State => (Service as ICommunicationObject)?.State ?? CommunicationState.Faulted;

        public ServiceType Service
        {
            get
            {
                if (_Disposed)
                {
                    throw new ObjectDisposedException($@"Uzi.Ikosa.Proxy.ServiceClient<{typeof(ServiceType).Name},{typeof(CallbackType).Name}>", @"Already disposed");
                }

                // TODO: revisit this, since I just slapped this in from a website...
                lock (_LockObject)
                {
                    if (_Factory == null)
                    {
                        _Factory = new DuplexChannelFactory<ServiceType>(_Callback, _Binding, _Endpoint);
                        _Factory.Credentials.UserName.UserName = _UserName;
                        _Factory.Credentials.UserName.Password = _Password;
                        _Channel = _Factory.CreateChannel();
                    }
                }
                return _Channel;
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    lock (_LockObject)
                    {
                        if (_Channel != null)
                        {
                            ((IClientChannel)_Channel).Close();
                        }
                        if (_Factory != null)
                        {
                            _Factory.Close();
                        }
                    }

                    _Factory = null;
                    _Disposed = true;
                }
            }
        }
        #endregion
    }
}
