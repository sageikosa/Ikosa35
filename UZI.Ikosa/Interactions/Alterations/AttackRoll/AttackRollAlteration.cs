using System;
using Uzi.Core;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public abstract class AttackRollAlteration : InteractionAlteration, IMonitorChange<InteractionAlteration>
    {
        protected AttackRollAlteration(AttackData attackData, object source, int modifier, bool informAttacker)
            : base(attackData, source)
        {
            Modifier = modifier;
            InformAttacker = informAttacker;

            // JDO: 2008-12-11, think I missed this line earlier, necessary to see that we have really been added as an alteration and can add our deltas...
            attackData.Alterations.AddChangeMonitor(this);
            // JDO: 2008-12-11
        }

        public AttackData AttackData => InteractData as AttackData;
        public int Modifier { get; set; }
        public bool InformAttacker { get; set; }

        #region IMonitorChange<InteractionAlteration> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<InteractionAlteration> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
            // indicates we got past change control filtering
            if (args.NewValue == this)
            {
                var _atk = (args.NewValue as AttackRollAlteration)?.AttackData;
                if (_atk != null)
                {
                    var _delta = new Delta(Modifier, Source);
                    _atk.AttackScore.Deltas.Add(_delta);
                    _atk.CriticalConfirmation?.Deltas.Add(_delta);
                }
            }
        }
        #endregion
    }
}
