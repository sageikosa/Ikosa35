using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class DeflectionSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        #region ctor
        public DeflectionSlotActivation(object source, int amount, bool affinity)
            : base(source, affinity)
        {
            _Amt = (amount < 1) ? 1 : (amount > 5) ? 5 : amount;
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
                yield return new Info { Message = $"Protection +{AmountVal}" };
                yield break;
            }
        }

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (SlottedItem != null)
            {
                _Amount = new Delta(_Amt, typeof(Deflection),
                    $"Deflection via {((ICoreObject)SlottedItem).Name}");
            }
        }

        public bool RequiresMasterwork
            => false;

        public decimal StandardCost
            => (AmountVal * AmountVal) * 2000m;

        public override object Clone()
            => new DeflectionSlotActivation(Source, AmountVal, Affinity);

        protected override void OnSlottedActivate()
        {
            var _critter = SlottedItem?.CreaturePossessor;
            if (_critter != null)
            {
                _critter.NormalArmorRating.Deltas.Add(_Amount);
                _critter.TouchArmorRating.Deltas.Add(_Amount);
                _critter.IncorporealArmorRating.Deltas.Add(_Amount);
            }
        }

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        public static MagicAugment CreateDeflectionAugment(ICasterClass caster, int bonus, bool affinity)
            => new MagicAugment(
                new SpellSource(caster, 1, 1, false, new ShieldOfGrace()),
                new DeflectionSlotActivation(typeof(Deflection), bonus, affinity));
    }
}
