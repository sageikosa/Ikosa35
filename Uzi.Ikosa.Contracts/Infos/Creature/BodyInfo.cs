using System;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class BodyInfo : CoreInfo
    {
        [DataMember]
        public DeltableInfo NaturalArmor { get; set; }
        [DataMember]
        public MaterialInfo Material { get; set; }
        [DataMember]
        public bool UseExtraCarryingFactor { get; set; }
        [DataMember]
        public DeltableInfo ReachSquares { get; set; }
        [DataMember]
        public SizeInfo Size { get; set; }
        [DataMember]
        public double BaseHeight { get; set; }
        [DataMember]
        public double BaseWeight { get; set; }
        [DataMember]
        public double BaseLength { get; set; }
        [DataMember]
        public double BaseWidth { get; set; }
        [DataMember]
        public double Height { get; set; }
        [DataMember]
        public double Weight { get; set; }
        [DataMember]
        public double Length { get; set; }
        [DataMember]
        public double Width { get; set; }
        [DataMember]
        public string[] Features { get; set; }

        // TODO: provide DataContract with IsMajor (Key? and Source?) instead of just string[]
        public override object Clone()
        {
            return new BodyInfo
            {
                Message = Message,
                ID = ID,
                NaturalArmor = NaturalArmor.Clone() as DeltableInfo,
                Material = Material.Clone() as MaterialInfo,
                UseExtraCarryingFactor = UseExtraCarryingFactor,
                ReachSquares = ReachSquares.Clone() as DeltableInfo,
                Size = Size.Clone() as SizeInfo,
                BaseHeight = BaseHeight,
                BaseLength = BaseLength,
                BaseWeight = BaseWeight,
                BaseWidth = BaseWidth,
                Features = Features.ToArray(),
                Height = Height,
                Length = Length,
                Weight = Weight,
                Width = Width
            };
        }
    }
}
