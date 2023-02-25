using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace=Statics.Namespace)]
    public class TransientVisualizerInfo
    {
        [DataMember]
        public string MyName { get; set; }
        [DataMember]
        public Point3D Source { get; set; }
        [DataMember]
        public long Duration { get; set; }
        [DataMember]
        public List<TransientVisualizerInfo> Followers { get; set; }

        public virtual TransientVisualizer GetTransientVisualizer()
        {
            return null;
        }

        protected TVisualizer GetRootTransientVisualizer<TVisualizer>()
            where TVisualizer : TransientVisualizer, new()
        {
            var _tVisualizer = new TVisualizer
            {
                Source = Source,
                Duration = new TimeSpan(Duration)
            };
            _tVisualizer.Followers.AddRange(Followers.Select(_f => _f.GetTransientVisualizer()));
            return _tVisualizer;
        }
    }
}
