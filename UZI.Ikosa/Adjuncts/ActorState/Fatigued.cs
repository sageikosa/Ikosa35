using System;
using Uzi.Core;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Fatigued : Adjunct, IActionFilter
    {
        public Fatigued(object source)
            : base(source)
        {
            _Penalty = new Delta(-2, typeof(Fatigued), @"Fatigued");
        }

        private Delta _Penalty;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                // condition add
                _critter.Conditions.Add(new Condition(Condition.Fatigued, this));

                // action filter (run/charge blocked)
                _critter.Actions.Filters.Add(this, (IActionFilter)this);

                // add penalty
                _critter.Abilities.Strength.Deltas.Add(_Penalty);
                _critter.Abilities.Dexterity.Deltas.Add(_Penalty);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // remove penalty
                _Penalty.DoTerminate();

                // action filter (run/charge no longer blocked)
                _critter.Actions.Filters.Remove(this);

                // condition remove
                _critter.Conditions.Remove(_critter.Conditions[Condition.Fatigued, this]);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new Fatigued(Source);

        #region IActionFilter Members

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if (budget != null)
            {
                var _actor = budget.Actor as Creature;
                if (_actor == (Anchor as Creature))
                {
                    // suppress start of charge or run
                    return (action is StartCharge) || (action is StartRun);
                }
            }
            return false;
        }

        #endregion
    }
}
