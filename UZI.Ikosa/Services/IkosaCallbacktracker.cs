using System;
using System.Collections.Generic;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Services
{
    public class IkosaCallbackTracker
    {
        public string UserName { get; set; }
        public List<Guid> IDs { get; set; }
        public IIkosaCallback Callback { get; set; }
    }
}
