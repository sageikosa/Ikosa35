using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    [KnownType(typeof(Description))]
    public class MeleeWeaponInfo : ObjectInfo
    {
        #region construction
        public MeleeWeaponInfo()
            : base()
        {
        }

        protected MeleeWeaponInfo(MeleeWeaponInfo copySource)
            : base(copySource)
        {
            WeaponHeads = copySource.WeaponHeads.Select(_w => _w.Clone() as WeaponHeadInfo).ToArray();
            ProficiencyType = copySource.ProficiencyType;
            WieldTemplate = copySource.WieldTemplate;
            IsFinessable = copySource.IsFinessable;
            IsReachWeapon = copySource.IsReachWeapon;
            AllowsOpportunisticAttacks = copySource.AllowsOpportunisticAttacks;
            IsThrowable = copySource.IsThrowable;
            RangeIncrement = copySource.RangeIncrement;
            MaxRange = copySource.MaxRange;
        }
        #endregion

        [DataMember]
        public WeaponHeadInfo[] WeaponHeads { get; set; }
        [DataMember]
        public WeaponProficiencyType ProficiencyType { get; set; }
        [DataMember]
        public WieldTemplate WieldTemplate { get; set; }
        [DataMember]
        public bool IsFinessable { get; set; }
        [DataMember]
        public bool IsReachWeapon { get; set; }
        [DataMember]
        public bool AllowsOpportunisticAttacks { get; set; }
        [DataMember]
        public bool IsThrowable { get; set; }
        [DataMember]
        public int RangeIncrement { get; set; }
        [DataMember]
        public int MaxRange { get; set; }

        public IEnumerable<Info> AllInfos => AdjunctInfos.Union(WeaponHeads);

        public string WieldSummary
            => $@"{WieldTemplate.GetWieldString()} {ProficiencyType.ToString().ToLower()}{(IsReachWeapon ? " reach" : string.Empty)}{(IsFinessable ? " finessable" : string.Empty)}{(IsThrowable ? " throwable" : string.Empty)}";

        public override object Clone()
        {
            return new MeleeWeaponInfo(this);
        }
    }
}