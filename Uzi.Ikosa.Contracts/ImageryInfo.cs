using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ImageryInfo : ICloneable
    {
        public ImageryInfo()
        {
            IconRef = new IconReferenceInfo
            {
                IconScale = 1
            };
        }

        [DataMember]
        /// <summary>Keys ordered by preference</summary>
        public string[] Keys { get; set; }

        [DataMember]
        public IconReferenceInfo IconRef { get; set; }

        /// <summary>This image represents the "topmost" one of a possible set.</summary>
        [DataMember]
        public bool IsImageSet { get; set; }

        // ICloneable Members
        public object Clone()
            => new ImageryInfo
            {
                Keys = (string[])Keys.Clone(),
                IconRef = new IconReferenceInfo
                {
                    IconColorMap = IconRef?.IconColorMap?.ToDictionary(_cm => _cm.Key, _cm => _cm.Value),
                    IconAngle = IconRef?.IconAngle ?? 0,
                    IconScale = IconRef?.IconScale ?? 1
                },
                IsImageSet = IsImageSet
            };
    }
}
