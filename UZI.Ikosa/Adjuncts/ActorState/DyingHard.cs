using System;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class DyingHard : Adjunct, IMonitorChange<CoreActivity>
    {
        public DyingHard(object source)
            : base(source)
        {
            _SingleAct = new SingleActionsOnly(this);
        }

        private SingleActionsOnly _SingleAct;
        private Delta _MoveCost;

        #region Activate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // add condition
                _critter.Conditions.Add(new Condition(Condition.DyingHard, this));

                // add action filter
                _critter.AddAdjunct(_SingleAct);

                // add CoreActivity change monitor
                _critter.AddChangeMonitor(this);

                // move cost
                _MoveCost = new Delta(1, this);
                _critter.MoveHalfing.Deltas.Add(_MoveCost);
            }
            base.OnActivate(source);
        }
        #endregion

        #region DeActivate
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (Anchor is Creature _critter)
            {
                // remove CoreActivity change monitor
                _critter.RemoveChangeMonitor(this);

                // remove action filter
                _SingleAct.Eject();

                // remove condition
                _critter.Conditions.Remove(_critter.Conditions[Condition.DyingHard, this]);

                // remove move penalties
                _MoveCost.DoTerminate();
            }
        }
        #endregion

        public override object Clone()
            => new DyingHard(Source);

        #region IMonitorChange<CoreActivity> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<CoreActivity> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<CoreActivity> args)
        {
            if ((args != null) && (args.NewValue != null)
                && (Anchor is Creature _critter) && (args.NewValue.Actor == _critter))
            {
                if ((args.NewValue.Action is ActionBase _action)
                    && (args.Action == @"Stop")
                    && _action.IsStrenuous)
                {
                    // lose a health point
                    _critter.HealthPoints.CurrentValue--;
                }
            }
        }

        #endregion
    }
}
