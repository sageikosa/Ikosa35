using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Indicates that the creature is dead (or destroyed if undead)</summary>
    [Serializable]
    public class DeadEffect : ActorStateBase, IActionSkip, IMonitorChange<Activation>
    {
        /// <summary>Indicates that the creature is dead (or destroyed if undead)</summary>
        public DeadEffect(object source, double timeOfDeath)
            : base(source)
        {
            _Time = timeOfDeath;
            _Senses = [];
        }

        #region private data
        private double _Time;
        private List<SensoryBase> _Senses;
        #endregion

        public double TimeOfDeath { get { return _Time; } }

        #region IActionFilter Members

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            if ((Creature)budget.Actor == (Creature)Anchor)
            {
                return true;
            }
            return false;
        }

        #endregion

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                // TODO: other things for a dead creature (like interaction handlers and abilitiy deltas)?
                _critter.Conditions.Add(new Condition(Condition.Dead, this));
                _critter.Actions.Filters.Add(this, (IActionFilter)this);
                _critter.AddAdjunct(new Immobilized(this, true));

                // clear senses
                foreach (var _sense in from _s in _critter.Senses.AllSenses
                                       where _s.IsActive
                                       select _s)
                {
                    _sense.IsActive = false;
                    if (!_sense.IsActive)
                    {
                        _sense.AddChangeMonitor(this);
                        _Senses.Add(_sense);
                    }
                }

                // clear awarenesses
                _critter.Awarenesses.RecalculateAwareness(_critter);
            }

            // notify
            NotifyStateChange();
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // TODO: other things for a dead creature (like interaction handlers and abilitiy deltas)?
                _critter.Conditions.Remove(_critter.Conditions[Condition.Dead, this]);
                _critter.Actions.Filters.Remove(this);
                _critter.Adjuncts.OfType<Immobilized>().FirstOrDefault(_u => _u.Source == this)?.Eject();

                // reactivate senses
                foreach (var _sense in _Senses)
                {
                    _sense.RemoveChangeMonitor(this);
                    _sense.IsActive = true;
                }
                _Senses.Clear();

                // reactivate awarenesses
                _critter.Awarenesses.RecalculateAwareness(_critter);

            }
            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            return new DeadEffect(Source, TimeOfDeath);
        }

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            if (args.NewValue.IsActive)
            {
                args.DoAbort();
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }
        #endregion
    }
}
