using System;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Entangled : Adjunct, IActionFilter
    {
        public Entangled(object source)
            : base(source)
        {
            _Attack = new Delta(-2, typeof(Entangled));
            _Ability = new Delta(-4, typeof(Entangled));
            _MoveCost = new Delta(1, this);
        }

        #region private data
        private Delta _Attack;
        private Delta _Ability;
        private Distracted _Distract;
        private Delta _MoveCost;
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;

            // deltas
            _critter.MeleeDeltable.Deltas.Add(_Attack);
            _critter.RangedDeltable.Deltas.Add(_Attack);
            _critter.OpposedDeltable.Deltas.Add(_Attack);
            _critter.Abilities.Dexterity.Deltas.Add(_Ability);

            // distraction
            _Distract = new Distracted(new Interaction(null, this, _critter, null), 15);
            _critter.AddAdjunct(_Distract);
            _critter.Actions.Filters.Add(this, this);

            // move cost
            _critter.MoveHalfing.Deltas.Add(_MoveCost);

            // conditions
            _critter.Conditions.Add(new Condition(Condition.Entangled, this));
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // condition
            var _critter = Anchor as Creature;
            _critter.Conditions.Remove(_critter.Conditions[Condition.Entangled, this]);

            // penalties
            _Attack.DoTerminate();
            _Ability.DoTerminate();

            // distraction
            if (_Distract != null)
            {
                _Distract.Eject();
            }

            _critter.Actions.Filters.Remove(this);

            // remove move penalties
            _MoveCost.DoTerminate();

            base.OnDeactivate(source);
        }
        #endregion

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

        public override object Clone()
        {
            return new Entangled(Source);
        }
    }
}
