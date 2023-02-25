using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    [KnownType(typeof(Description))]
    public class NaturalWeaponInfo : ObjectInfo
    {
        #region construction
        public NaturalWeaponInfo()
            : base()
        {
        }

        protected NaturalWeaponInfo(NaturalWeaponInfo copySource)
            : base(copySource)
        {
            IsAlwaysOn=copySource.IsAlwaysOn;
            IsPrimary = copySource.IsPrimary;
            TreatAsSoleWeapon = copySource.TreatAsSoleWeapon;
            IsReachWeapon = copySource.IsReachWeapon;
            SlotType = copySource.SlotType;
            SlotSubType = copySource.SlotSubType;
            WeaponHead = copySource.WeaponHead.Clone() as WeaponHeadInfo;
            IsFinessable = copySource.IsFinessable;
            WieldTemplate = copySource.WieldTemplate;
        }
        #endregion

        [DataMember]
        public bool IsAlwaysOn { get; set; }
        [DataMember]
        public bool IsPrimary { get; set; }
        [DataMember]
        public bool TreatAsSoleWeapon { get; set; }
        [DataMember]
        public bool IsReachWeapon { get; set; }
        [DataMember]
        public string SlotType { get; set; }
        [DataMember]
        public string SlotSubType { get; set; }
        [DataMember]
        public WeaponHeadInfo WeaponHead { get; set; }
        [DataMember]
        public bool IsFinessable { get; set; }
        [DataMember]
        public WieldTemplate WieldTemplate { get; set; }

        public string WieldSummary
            => $@"{(IsPrimary ? "primary" : "secondary")} {WieldTemplate.GetWieldString()}{(TreatAsSoleWeapon ? " main" : string.Empty)}{(IsReachWeapon ? " reach" : string.Empty)}{(IsFinessable ? " finessable" : string.Empty)}";

        public override object Clone()
        {
            return new NaturalWeaponInfo(this);
        }
    }
}