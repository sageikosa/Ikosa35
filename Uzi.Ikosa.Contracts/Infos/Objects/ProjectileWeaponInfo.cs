using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    [KnownType(typeof(Description))]
    public class ProjectileWeaponInfo : ObjectInfo
    {
        #region construction
        public ProjectileWeaponInfo()
            : base()
        {
        }

        protected ProjectileWeaponInfo(ProjectileWeaponInfo copySource)
            : base(copySource)
        {
            MaxStrengthBonus = copySource.MaxStrengthBonus;
            VirtualHead = copySource.VirtualHead.Clone() as WeaponHeadInfo;
            RangeIncrement = copySource.RangeIncrement;
            MaxRange = copySource.MaxRange;
            ProficiencyType = copySource.ProficiencyType;
            WieldTemplate = copySource.WieldTemplate;
        }
        #endregion

        [DataMember]
        public int MaxStrengthBonus { get; set; }
        [DataMember]
        public WeaponHeadInfo VirtualHead { get; set; }
        [DataMember]
        public int RangeIncrement { get; set; }
        [DataMember]
        public int MaxRange { get; set; }
        [DataMember]
        public WeaponProficiencyType ProficiencyType { get; set; }
        [DataMember]
        public WieldTemplate WieldTemplate { get; set; }

        public IEnumerable<Info> AllInfos
        {
            get
            {
                foreach (var _inf in AdjunctInfos)
                    yield return _inf;
                yield return VirtualHead;
                yield break;
            }
        }

        public string WieldSummary
            => $@"{WieldTemplate.GetWieldString()} {ProficiencyType.ToString().ToLower()}";

        public override object Clone()
        {
            return new ProjectileWeaponInfo(this);
        }
    }
}