using System.Runtime.Serialization;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class PointEffect
    {
        /// <summary>Name of the BrushSet that contains the InnerBrush</summary>
        [DataMember]
        public string BrushSet { get; set; }

        /// <summary>Name of the BrushKey within the BrushSet</summary>
        [DataMember]
        public string BrushKey { get; set; }

        /// <summary>VisualEffect to apply when observing from this point</summary>
        [DataMember]
        public VisualEffect Effect { get; set; }
    }
}
