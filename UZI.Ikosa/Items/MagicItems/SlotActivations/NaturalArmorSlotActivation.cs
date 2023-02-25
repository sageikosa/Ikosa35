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
    public class NaturalArmorSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        #region ctor
        public NaturalArmorSlotActivation(object source, int amount, bool affinity)
            : base(source, affinity)
        {
            _Amt = (amount < 1) ? 1 : amount;
            _Amount = null;
        }
        #endregion

        #region private data
        private int _Amt;
        private Delta _Amount;
        #endregion

        public Delta Amount => _Amount;
        public int AmountVal => _Amt;

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info
                {
                    Message = $"Natural Armor Enhancement +{AmountVal}"
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
            => (AmountVal * AmountVal) * 2000m;

        protected override void OnSlottedActivate()
        {
            var _critter = SlottedItem?.CreaturePossessor;
            if (_critter != null)
            {
                _critter.Body.NaturalArmor.Deltas.Add(_Amount);
            }
        }

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        public override object Clone()
            => new NaturalArmorSlotActivation(Source, AmountVal, Affinity);

        public static MagicAugment CreateNaturalArmorAugment(ICasterClass caster, Func<ISpellDef> spellDef, int bonus, bool affinity)
            => new MagicAugment(
                new SpellSource(caster, 1, 1, false, spellDef()),
                new NaturalArmorSlotActivation(typeof(Enhancement), bonus, affinity));
    }
}
