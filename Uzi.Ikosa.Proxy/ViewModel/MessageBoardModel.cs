using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class MessageBoardModel : ViewModelBase
    {
        public MessageBoardModel(ProxyModel proxies)
        {
            _Proxies = proxies;
        }

        private ProxyModel _Proxies;

        public ProxyModel Proxies => _Proxies;
    }
}
