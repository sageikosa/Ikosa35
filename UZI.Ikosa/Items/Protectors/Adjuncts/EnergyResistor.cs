using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public abstract class EnergyResistor : ProtectorAdjunct
    {
        #region construction
        protected EnergyResistor(object source, EnergyType energy, int amount)
            : base(source, 0, ((amount <= 10) ? 18000 : ((amount >= 30) ? 66000 : 42000)))
        {
            _Energy = energy;
            _Amt = ((amount <= 10) ? 10 : (amount >= 30) ? 30 : 20);
            _Amount = null;
        }
        #endregion

        #region private data
        private int _Amt;
        private EnergyType _Energy;
        private Delta _Amount;
        #endregion

        public EnergyType EnergyType => _Energy;
        public Delta Amount => _Amount;
        public int AmountVal => _Amt;

        protected override void OnSlottedActivate()
            => Protector?.CreaturePossessor?.EnergyResistances[EnergyType].Deltas.Add(_Amount);

        protected override void OnSlottedDeActivate()
            => _Amount?.DoTerminate();

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);
            if (Protector != null)
            {
                _Amount = new Delta(_Amt, typeof(EnergyResistance),
                    $"Energy Resistor via {((ICoreObject)Protector).Name}");
            }
        }

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = $"Resist {EnergyType} {AmountVal}" };
                yield break;
            }
        }
    }
}
