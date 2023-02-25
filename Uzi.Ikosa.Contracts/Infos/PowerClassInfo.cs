using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts.Infos;
using Uzi.Visualize;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class PowerClassInfo : CoreInfo, IIconInfo
    {
        public PowerClassInfo()
        {
        }

        protected PowerClassInfo(PowerClassInfo copySource)
        {
            Message = copySource.Message;
            ClassPowerLevel = copySource.ClassPowerLevel.Clone() as DeltableInfo;
            OwnerID = copySource.OwnerID;
            IsPowerClassActive = copySource.IsPowerClassActive;
            Key = copySource.Key;
            Icon = copySource.Icon?.Clone() as ImageryInfo;
        }

        [DataMember]
        public ImageryInfo Icon { get; set; }
        [DataMember]
        public VolatileValueInfo ClassPowerLevel { get; set; }
        [DataMember]
        public string OwnerID { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public bool IsPowerClassActive { get; set; }

        /// <summary>
        /// Added by client-side View Model, tracked here for convenience in data-binding of client controls
        /// </summary>
        public IResolveIcon IconResolver { get; set; }

        public override object Clone()
        {
            return new PowerClassInfo(this);
        }
    }
}
