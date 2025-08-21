using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SleepEffect : ActorStateBase, IActionSkip, IInteractHandler, IMonitorChange<Activation>
    {
        #region Construction
        public SleepEffect(object source)
            : this(source, true)
        {
        }

        protected SleepEffect(object source, bool handleDamage)
            : base(source)
        {
            _Handling = handleDamage;
            _Senses = [];
        }
        #endregion

        #region private data
        // indicates handling damage to wake up
        private bool _Handling;
        private List<SensoryBase> _Senses;
        #endregion

        #region Activate
        protected override void OnActivate(object source)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                // add stuff
                _critter.AddAdjunct(new Immobilized(this, true));
                _critter.Actions.Filters.Add(this, (IActionFilter)this);
                if (_Handling)
                {
                    _critter.AddIInteractHandler(this);
                }

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
            NotifyStateChange(false, false, false);
            base.OnActivate(source);
        }
        #endregion

        #region DeActivate
        protected override void OnDeactivate(object source)
        {
            var _critter = Critter;
            if (_critter != null)
            {
                // remove stuff
                _critter.Adjuncts.OfType<Immobilized>().FirstOrDefault(_i => _i.Source == this)?.Eject();
                _critter.Actions.Filters.Remove(this);
                if (_Handling)
                {
                    _critter.RemoveIInteractHandler(this);
                }

                // remove programmed sleep action
                if (_critter.GetLocalActionBudget() is LocalActionBudget _budget
                    && _budget.NextActivity?.Action is Sleep)
                {
                    _budget.NextActivity = null;
                }

                // reactivate senses
                foreach (var _sense in _Senses)
                {
                    _sense.RemoveChangeMonitor(this);
                    _sense.IsActive = true;
                }
                _Senses.Clear();

                // reactivate awarenesses
                _critter.Awarenesses.RecalculateAwareness(_critter);

                // notify
                NotifyStateChange(true, false, false);
            }
            base.OnDeactivate(source);
        }
        #endregion

        /// <summary>Called when awoken by a standard action or taking damage.</summary>
        public virtual void Awaken()
        {
            if ((Source is MagicPowerEffect _effect) && (Anchor != null))
            {
                Anchor.RemoveAdjunct(_effect);
            }
            if (Anchor != null)
            {
                Anchor.RemoveAdjunct(this);
            }
        }

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

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is IDeliverDamage _damage) && (_damage.GetTotal() > 0))
            {
                Awaken();
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler switch
            {
                DamageReductionHandler _ => true,
                EnergyResistanceHandler _ => true,
                EvasionHandler _ => true,
                _ => false,
            };
        #endregion

        public override object Clone()
            => new SleepEffect(Source, _Handling);

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            // attempting to reactivate a sense will fail
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
