using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class EnergyResistorSlotActivation : SlotActivation, IItemRequirements, IAugmentationCost
    {
        protected EnergyResistorSlotActivation(EnergyType energy, int amount, bool affinity)
            : base(typeof(EnergyResistorSlotActivation), affinity)
        {
            _Energy = energy;
            _Amt = ((amount <= 10) ? 10 : (amount >= 30) ? 30 : 20);
            _Amount = null;
        }

        #region private data
        private int _Amt;
        private EnergyType _Energy;
        private Delta _Amount;
        #endregion

        public EnergyType EnergyType => _Energy;
        public Delta Amount => _Amount;
        public int AmountVal => _Amt;

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = $"Resist {EnergyType} {AmountVal}" };
                yield break;
            }
        }

        public bool RequiresMasterwork
            => false;

        public decimal StandardCost
            => ((AmountVal <= 10) ? 12000 : ((AmountVal >= 30) ? 44000 : 28000));

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (SlottedItem != null)
            {
                _Amount = new Delta(_Amt, typeof(EnergyResistance),
                    $"Energy Resistor via {((ICoreObject)SlottedItem).Name}");
            }
        }

        protected override void OnSlottedActivate()
            => SlottedItem?.CreaturePossessor?.EnergyResistances[EnergyType].Deltas.Add(_Amount);

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        public override object Clone()
            => new EnergyResistorSlotActivation(EnergyType, AmountVal, Affinity);

        public static MagicAugment CreateEnergyResistorAugment(ICasterClass caster, EnergyType energyType, int amount, bool affinity)
            => new MagicAugment(
                new SpellSource(caster, 2, 2, false, new ResistEnergy()),
                new EnergyResistorSlotActivation(energyType, amount, affinity));
    }
}
