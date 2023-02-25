using System;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class SkillInfo : DeltableInfo
    {
        #region construction
        public SkillInfo()
        {
        }
        #endregion

        [DataMember]
        public bool IsClassSkill { get; set; }
        [DataMember]
        public string KeyAbilityMnemonic { get; set; }
        [DataMember]
        public bool UseUntrained { get; set; }
        [DataMember]
        public bool IsTrained { get; set; }
        [DataMember]
        public double CheckFactor { get; set; }
        [DataMember]
        public Take10Info Take10 { get; set; }

        public string SkillName => Message;

        public override object Clone()
        {
            return new SkillInfo
            {
                EffectiveValue = EffectiveValue,
                DeltaDescriptions = DeltaDescriptions,
                BaseDoubleValue = BaseDoubleValue,
                BaseValue = BaseValue,
                IsClassSkill = IsClassSkill,
                KeyAbilityMnemonic = KeyAbilityMnemonic,
                UseUntrained = UseUntrained,
                IsTrained = IsTrained,
                CheckFactor = CheckFactor,
                Message = Message,
                Take10 = Take10
            };
        }
    }
}
