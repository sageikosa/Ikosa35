using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class StunnedEffect : PredispositionBase, ITrackTime, IActionSkip
    {
        public StunnedEffect(object source, double endTime, double resolution)
            : base(source)
        {
            _EndTime = endTime;
            _TimeRes = resolution;
        }

        #region data
        private double _EndTime;
        private double _TimeRes;
        #endregion

        public override string Description
            => @"Stunned";

        // drop held items...
        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;

            // drop held items
            foreach (var _slot in _critter.Body.ItemSlots.AllSlots
                .OfType<HoldingSlot>().Where(_is => _is.SlottedItem != null))
            {
                // go right to the holding slots...
                var _act = new CoreActivity(_critter, new DropHeldObject(_slot, string.Empty), null);
                _critter.ProcessManager?.StartProcess(_act);
            }

            // condition effects
            // NOTE: attack roll modifications are handled by ConditionAttackHandler, so we add no modifiers here...
            _critter.Conditions.Add(new Condition(Condition.Stunned, this));
            _critter.Actions.Filters.Add(this, (IActionFilter)this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter.Conditions.Remove(_critter.Conditions[Condition.Stunned, this]);
            _critter.Actions.Filters.Remove(this);
            base.OnDeactivate(source);
        }

        #region ITrackTime Members
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _EndTime) && (direction == TimeValTransition.Entering))
            {
                Anchor.RemoveAdjunct(this);
            }
        }

        public double EndTime => _EndTime;
        public double Resolution => _TimeRes;
        #endregion

        #region IActionFilter Members
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // unconscious creatures get no actions
            if ((Creature)budget.Actor == (Creature)Anchor)
            {
                // TODO: may allow self-stabilizing action...
                return true;
            }
            return false;
        }
        #endregion

        public override object Clone()
            => new StunnedEffect(Source, EndTime, Resolution);
    }
}
