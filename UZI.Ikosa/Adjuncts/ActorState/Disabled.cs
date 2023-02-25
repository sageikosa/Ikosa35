using System;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Disabled : ActorStateBase, IMonitorChange<CoreActivity>, IHealthActivityLimiter
    {
        public Disabled(object source)
            : base(source)
        {
            _SingleAct = new SingleActionsOnly(this);
            _MoveCost = new Delta(1, this);
        }

        #region private data
        private SingleActionsOnly _SingleAct;
        private Delta _MoveCost;
        #endregion

        #region Activate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // add condition
                _critter.Conditions.Add(new Condition(Condition.Disabled, this));

                // add action filter
                _critter.AddAdjunct(_SingleAct);

                // add CoreActivity change monitor
                _critter.AddChangeMonitor(this);

                // move cost
                _critter.MoveHalfing.Deltas.Add(_MoveCost);

                // notify
                NotifyStateChange();
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
                _critter.Conditions.Remove(_critter.Conditions[Condition.Disabled, this]);

                // remove move penalties
                _MoveCost.DoTerminate();
            }
        }
        #endregion

        public override object Clone()
        {
            return new Disabled(Source);
        }

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
                    if (Source is NotHealing)
                    {
                        (Source as NotHealing).Eject();
                    }
                    else
                    {
                        Eject();
                    }

                    // lose a health point
                    _critter.HealthPoints.CurrentValue--;

                    // add dying
                    _critter.HealthPoints.DoDamage();
                }
            }
        }

        #endregion
    }
}
