using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SkillBonusSlotActivation<SType> : SlotActivation, IItemRequirements, IAugmentationCost
        where SType : SkillBase
    {
        #region ctor
        protected SkillBonusSlotActivation(object source, int amount, bool affinity)
            : base(source, affinity)
        {
            _Amt = (amount < 5) ? 5 : (amount > 20) ? 20 : amount;
            _Amount = null;
        }
        #endregion

        #region private data
        private int _Amt;
        private Delta _Amount;
        #endregion

        public Delta Amount => _Amount;
        public int AmountVal => _Amt;

        public Info Info => IdentificationInfos.FirstOrDefault();

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info
                {
                    Message = $"{SkillBase.SkillInfo(typeof(SType)).Name} +{AmountVal}"
                };
                yield break;
            }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (SlottedItem != null)
            {
                _Amount = new Delta(_Amt, typeof(Competence),
                    $"Competence via {((ICoreObject)SlottedItem).Name}");
            }
        }

        public bool RequiresMasterwork
            => false;

        public decimal StandardCost
            => (AmountVal * AmountVal) * 100m;

        protected override void OnSlottedActivate()
        {
            var _critter = SlottedItem?.CreaturePossessor;
            if (_critter != null)
            {
                _critter.Skills.Skill<SType>().Deltas.Add(_Amount);
            }
        }

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        public override object Clone()
            => new SkillBonusSlotActivation<SType>(Source, AmountVal, Affinity);

        public static MagicAugment CreateSkillBonusAugment(ICasterClass caster, int bonus, bool affinity)
            => new MagicAugment(
                MagicAugmentationPowerSource.CreateItemPowerSource(caster, 5, new Transformation(),
                @"Competence", $"+5 Competence on {SkillBase.SkillInfo(typeof(SType))?.Name ?? typeof(SType).Name}",
                typeof(SkillBonusSlotActivation<SType>).FullName),
                new SkillBonusSlotActivation<SType>(typeof(Competence), bonus, affinity));
    }
}
