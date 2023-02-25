using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public interface IPrerequisiteProxy
    {
        ProxyModel Proxies { get; }
        RelayCommand DoSendPrerequisites { get; }
        Guid FulfillerID { get; }
        IEnumerable<AwarenessInfo> GetCoreInfoAwarenesses(IEnumerable<PrerequisiteInfo> preReqs);
    }
}
