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
    public class ResistanceSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        #region ctor
        public ResistanceSlotActivation(object source, int amount, bool affinity)
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
                    Message = $"Resistance +{AmountVal}"
                };
                yield break;
            }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (SlottedItem != null)
            {
                _Amount = new Delta(_Amt, typeof(Resistance),
                    $"Resistance via {((ICoreObject)SlottedItem).Name}");
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
                _critter.FortitudeSave.Deltas.Add(_Amount);
                _critter.ReflexSave.Deltas.Add(_Amount);
                _critter.WillSave.Deltas.Add(_Amount);
            }
        }

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        public override object Clone()
            => new ResistanceSlotActivation(Source, AmountVal, Affinity);

        public static MagicAugment CreateResistanceAugment(ICasterClass caster, int bonus, bool affinity)
            => new MagicAugment(
                new SpellSource(caster, 1, 1, false, new Magic.Spells.Resistance()),
                new ResistanceSlotActivation(typeof(Resistance), bonus, affinity));
    }
}
