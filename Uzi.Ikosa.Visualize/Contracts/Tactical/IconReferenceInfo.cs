using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

namespace Uzi.Visualize.Contracts.Tactical
{
    [Serializable]
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class IconReferenceInfo : ICloneable, IIconReference
    {
        [DataMember]
        /// <summary>Keys ordered by preference</summary>
        public string[] Keys { get; set; }

        [DataMember]
        public IDictionary<string, string> IconColorMap { get; set; }

        [DataMember]
        public double IconAngle { get; set; }

        [DataMember]
        public double IconScale { get; set; }

        /// <summary>This image represents the "topmost" one of a possible set.</summary>
        [DataMember]
        public bool IsImageSet { get; set; }

        public object Clone()
            => new IconReferenceInfo
            {
                Keys = (string[])Keys.Clone(),
                IconColorMap = IconColorMap?.ToDictionary(_cm => _cm.Key, _cm => _cm.Value),
                IconAngle = IconAngle,
                IconScale = IconScale,
                IsImageSet = IsImageSet
            };
    }
}
