using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Movement;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Helpless
    /// </summary>
    [Serializable]
    public class Immobilized : ActorStateBase, IActionFilter
    {
        /// <summary>
        /// Helpless
        /// </summary>
        public Immobilized(object source, bool prone)
            : base(source)
        {
            _Prone = prone;
        }

        #region data 
        private bool _Prone;
        #endregion

        public bool FallsProne => _Prone;

        public override object Clone()
            => new Immobilized(Source, FallsProne);

        protected override void OnActivate(object source)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                // add stuff
                _critter.Conditions.Add(new Condition(Condition.Helpless, this));
                _critter.Abilities.Dexterity.SetZeroHold(this, true);
                _critter.Actions.Filters.Add(this, (IActionFilter)this);
                _critter.AddAdjunct(new UnpreparedForOpportunities(this));
                if (FallsProne)
                {
                    _critter.AddAdjunct(new ProneEffect(Source));
                }
            }

            // notify
            NotifyStateChange(false, true, true);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                // remove stuff
                _critter.Conditions.Remove(_critter.Conditions[Condition.Helpless, this]);
                _critter.Abilities.Dexterity.SetZeroHold(this, false);
                _critter.Actions.Filters.Remove(this);
                var _un = _critter.Adjuncts.OfType<UnpreparedForOpportunities>().FirstOrDefault(_u => _u.Source == this);
                if (_un != null)
                {
                    _critter.RemoveAdjunct(_un);
                }

                // notify
                NotifyStateChange(true, true, true);
            }
        }

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if ((budget.Actor as Creature) == (Anchor as Creature))
            {
                return (action as ActionBase)?.IsMental ?? false;
            }
            return false;
        }
    }
}
