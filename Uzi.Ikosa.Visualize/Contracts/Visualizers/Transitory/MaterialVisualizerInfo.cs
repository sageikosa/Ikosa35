using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MaterialVisualizerInfo : TransientVisualizerInfo
    {
        [DataMember]
        public string MaterialKey { get; set; }

        protected MVisualizer GetMaterialTransientVisualizer<MVisualizer>() 
            where MVisualizer : MaterialVisualizer, new()
        {
            var _mVisualizer = GetRootTransientVisualizer<MVisualizer>();
            _mVisualizer.MaterialKey = MaterialKey;
            return _mVisualizer;
        }
    }
}
