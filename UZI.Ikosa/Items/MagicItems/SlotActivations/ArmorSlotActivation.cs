using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class ArmorSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        #region ctor
        public ArmorSlotActivation(object source, int amount, bool isForce, bool affinity)
            : base(source, affinity)
        {
            _Amt = (amount < 1) ? 1 : amount;
            _Amount = null;
            _Force = isForce;
        }
        #endregion

        #region state
        private bool _Force;
        private int _Amt;
        private Delta _Amount;
        #endregion

        public Delta Amount => _Amount;
        public int AmountVal => _Amt;
        public bool IsForce => _Force;

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info
                {
                    Message = $"Armor +{AmountVal}"
                };
                yield break;
            }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (SlottedItem != null)
            {
                _Amount = new Delta(_Amt, typeof(IArmor),
                    $"Armor via {((ICoreObject)SlottedItem).Name}");
            }
        }

        public bool RequiresMasterwork
            => false;

        public decimal StandardCost
            => (AmountVal * AmountVal) * 1000m;

        protected override void OnSlottedActivate()
        {
            var _critter = SlottedItem?.CreaturePossessor;
            if (_critter != null)
            {
                _critter.NormalArmorRating.Deltas.Add(_Amount);
                if (IsForce)
                {
                    _critter.IncorporealArmorRating.Deltas.Add(_Amount);
                }
            }
        }

        protected override void OnSlottedDeActivate()
        {
            _Amount?.DoTerminate();
        }

        public override object Clone()
            => new ArmorSlotActivation(Source, AmountVal, IsForce, Affinity);

        public static MagicAugment CreateArmorAugment(ICasterClass caster, Func<ISpellDef> spellDef, int bonus, bool isForce, bool affinity)
            => new MagicAugment(
                new SpellSource(caster, 1, 1, false, spellDef()),
                new ArmorSlotActivation(typeof(IArmor), bonus, isForce, affinity));
    }
}
