using System;
using System.Collections.Generic;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Services
{
    public class VisualizationCallbackTracker
    {
        public string UserName { get; set; }
        public List<Guid> IDs { get; set; }
        public IVisualizationCallback Callback { get; set; }
    }
}
