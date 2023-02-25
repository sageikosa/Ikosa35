using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class AbilityEnhanceSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        public AbilityEnhanceSlotActivation(object source, string mnemonic, int amount, bool affinity)
            : base(source, affinity)
        {
            _Mnemonic = mnemonic;
            _Amt = amount;
            _Amount = null;
        }

        #region private data
        private int _Amt;
        private Delta _Amount;
        private string _Mnemonic;
        #endregion

        public Delta Amount => _Amount;
        public int AmountVal => _Amt;
        public string Mnemonic => _Mnemonic;

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info
                {
                    Message = $"{Mnemonic} Enhancement +{AmountVal}"
                };
                yield break;
            }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (SlottedItem != null)
            {
                _Amount = new Delta(_Amt, typeof(Enhancement),
                    $"Enhancement via {((ICoreObject)SlottedItem).Name}");
            }
        }

        public bool RequiresMasterwork
            => false;

        public decimal StandardCost
            => (AmountVal * AmountVal) * 1000m;

        protected override void OnSlottedActivate()
        {
            SlottedItem?.CreaturePossessor?.Abilities[Mnemonic].Deltas.Add(_Amount);
        }

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        public override object Clone()
            => new AbilityEnhanceSlotActivation(Source, Mnemonic, AmountVal, Affinity);

        public static MagicAugment CreateAbilityEnhanceAugment(ICasterClass caster, string mnemonic, Func<ISpellDef> spellDef, int bonus, bool affinity)
            => new MagicAugment(
                new SpellSource(caster, 1, 1, false, spellDef()),
                new AbilityEnhanceSlotActivation(typeof(Enhancement), mnemonic, bonus, affinity));
    }
}
