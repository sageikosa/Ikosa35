using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ShadingInfo : CubicInfo
    {
        [DataMember]
        public byte[] EffectBytes { get; set; }

        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public int Face { get; set; }

        [DataMember]
        public List<PanelShadingInfo> PanelShadings { get; set; }

        /// <summary>Provides VisualEffect relative to the cubic so that [0,0,0] is first effect</summary>
        public VisualEffect this[int z, int y, int x]
        {
            get
            {
                if ((z < ZHeight) && (y < YLength) && (x < XLength) && (z >= 0) && (y >= 0) && (x >= 0))
                    return (VisualEffect)EffectBytes[x + (y * XLength) + (z * XLength * YLength)];
                return VisualEffect.Skip;
            }
        }

        public AnchorFaceList AnchorFaces
            => (OptionalAnchorFace == OptionalAnchorFace.None)
                ? AnchorFaceList.All
                : AnchorFaceListHelper.Create(OptionalAnchorFace.ToAnchorFace());

        public OptionalAnchorFace OptionalAnchorFace
            => (OptionalAnchorFace)Face;

        public IEnumerable<VisualEffect> VisualEffects
            => EffectBytes.Select(_b => (VisualEffect)_b);
    }
}
