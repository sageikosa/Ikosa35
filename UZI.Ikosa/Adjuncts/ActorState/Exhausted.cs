using System;
using Uzi.Core;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Exhausted : Adjunct, IActionFilter
    {
        /// <summary>Use typeof(Fatigued) as source for free-floating exhaustion</summary>
        public Exhausted(object source)
            : base(source)
        {
            // NOTE: using same source as Fatigued so both do not apply
            _Penalty = new Delta(-6, typeof(Fatigued), @"Exhausted");
            _MoveCost = new Delta(1, this);
        }

        #region private data
        private Delta _Penalty;
        private Delta _MoveCost;
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                // condition add
                _critter.Conditions.Add(new Condition(Condition.Exhausted, this));

                // action filter (run/charge blocked)
                _critter.Actions.Filters.Add(this, (IActionFilter)this);

                // add penalty
                _critter.Abilities.Strength.Deltas.Add(_Penalty);
                _critter.Abilities.Dexterity.Deltas.Add(_Penalty);

                // move cost
                _critter.MoveHalfing.Deltas.Add(_MoveCost);
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

                // remove move penalties
                _MoveCost.DoTerminate();

                // condition remove
                _critter.Conditions.Remove(_critter.Conditions[Condition.Exhausted, this]);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
        {
            return new Exhausted(Source);
        }

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
