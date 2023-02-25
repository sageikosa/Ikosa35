using System;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using System.Collections.Generic;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Opportunistic attacks must be triggered by ILocatorCapture, or by ICanReact when a process starts.
    /// </summary>
    [Serializable]
    public class OpportunityBudget : IResetBudgetItem, ICreatureBound
    {
        #region construction
        public OpportunityBudget(Creature critter, Deltable capacity)
        {
            _Critter = critter;
            _Capacity = capacity;
            _Activities = new Collection<CoreActivity>();
        }
        #endregion

        #region private data
        private Creature _Critter;
        private Deltable _Capacity;
        private Collection<CoreActivity> _Activities;
        #endregion

        /// <summary>Activities (since last turn reset) for which an opportunity has been taken.</summary>
        public Collection<CoreActivity> Activities => _Activities;

        /// <summary>Returns the number of available opportunities in the budget (capacity - count)</summary>
        public int Available
            => _Capacity.EffectiveValue - Activities.Count;

        /// <summary>Number of opportunistic attacks that can be made between turn resets.</summary>
        public Deltable Capacity => _Capacity;

        public Creature Creature => _Critter;

        #region public bool Reset()
        /// <summary>Resets the number used and the list of activities responded </summary>
        public bool Reset()
        {
            _Activities.Clear();
            return false;
        }
        #endregion

        public object Source => typeof(OpportunityBudget);

        #region IBudgetItem Members

        public void Added(CoreActionBudget budget) { }
        public void Removed() { }
        public string Name => @"Opportunities";
        public string Description => $@"{Available} of {Capacity.EffectiveValue} remaining";

        #endregion

        #region public void RegisterOpportunity(CoreActivity opportunity)
        /// <summary>Register the opportunity</summary>
        public void RegisterOpportunity(CoreActivity opportunity)
        {
            if (opportunity?.Action is ActionBase _action)
            {
                // register for activity if provokes target and creature is a target
                if (_action.IsProvocableTarget(opportunity, Creature))
                {
                    Activities.Add(opportunity);
                }
                else
                {
                    if (_action?.Budget?.HasActivity ?? false)
                    {
                        // parent activity?
                        var _parent = _action.Budget.TopActivity;
                        Activities.Add(_parent);
                    }
                    else
                    {
                        Activities.Add(opportunity);
                    }
                }
            }
        }
        #endregion

        #region public bool CanTakeOpportunity(CoreActivity opportunity)
        /// <summary>True if the activity can lead to an opportunistic attack</summary>
        public bool CanTakeOpportunity(CoreActivity opportunity)
        {
            // none available?
            if (Available <= 0)
                return false;

            // already taken for this opportunity?
            if (Activities.Contains(opportunity))
            {
                // only get one chance per opportunity...
                return false;
            }

            // *only* provokes from target?
            var _action = opportunity?.Action as ActionBase;
            if (!_action.ProvokesMelee)
            {
                // these can always be taken if the creature is a target
                // and cannot if the creature is not a target
                return _action.IsProvocableTarget(opportunity, Creature);
            }

            // opportunity for a sub-action?
            if (_action?.Budget?.HasActivity ?? false)
            {
                // get parent opportunity
                var _parent = _action.Budget.TopActivity;
                if (!Activities.Contains(_parent))
                {
                    // didn't use an opportunistic attack for the parent either
                    return true;
                }

                // used an opportunistic attack for parent ...
                // ... however, if the action provokes for a target
                // ... still might take the opportunity
                return (_action.IsProvocableTarget(opportunity, Creature));
            }

            // wasn't a sub-action, so good to take the opportunity
            return true;
        }
        #endregion

        public CapacityBudgetInfo ToCapacityBudgetInfo()
        {
            var _info = this.ToBudgetItemInfo<CapacityBudgetInfo>();
            _info.Available = Available;
            _info.Capacity = Capacity.EffectiveValue;
            return _info;
        }
    }
}