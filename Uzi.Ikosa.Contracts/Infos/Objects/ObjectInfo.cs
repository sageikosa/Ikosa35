using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts.Infos;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    /// <summary>Object info</summary>
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class ObjectInfo : CoreInfo, IIconInfo
    {
        #region construction
        public ObjectInfo()
        {
        }

        protected ObjectInfo(ObjectInfo copySource)
            : base(copySource)
        {
            Weight = copySource.Weight;
            Size = copySource.Size.Clone() as SizeInfo;
            CreatureSize = copySource.CreatureSize.Clone() as SizeInfo;
            Hardness = copySource.Hardness.Clone() as DeltableInfo;
            Material = copySource.Material.Clone() as MaterialInfo;
            StructurePercent = copySource.StructurePercent;
            AdjunctInfos = copySource.AdjunctInfos.Select(_a => _a.Clone() as Info).ToArray();
            Icon = copySource.Icon?.Clone() as ImageryInfo;
        }
        #endregion

        [DataMember]
        public double? Weight { get; set; }
        [DataMember]
        public SizeInfo Size { get; set; }
        [DataMember]
        public SizeInfo CreatureSize { get; set; }
        [DataMember]
        public DeltableInfo Hardness { get; set; }
        [DataMember]
        public MaterialInfo Material { get; set; }
        [DataMember]
        public double? StructurePercent { get; set; }
        [DataMember]
        public Info[] AdjunctInfos { get; set; }
        [DataMember]
        public ImageryInfo Icon { get; set; }

        public Visibility WeightVisibility
            => Weight.HasValue ? Visibility.Visible : Visibility.Collapsed;

        public Visibility AdjunctVisibility 
            => (AdjunctInfos?.Any() ?? false) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Added by client-side View Model, tracked here for convenience in data-binding of client controls
        /// </summary>
        public IResolveIcon IconResolver { get; set; }

        public override object Clone()
        {
            return new ObjectInfo(this);
        }
    }
}