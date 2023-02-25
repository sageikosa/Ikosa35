using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class UncannyDodge : Adjunct, IInteractHandler, IMonitorChange<InteractionAlteration>
    {
        public UncannyDodge(IPowerClass powerClass)
            : base(powerClass)
        {
        }

        public IPowerClass PowerClass
            => Source as IPowerClass;

        public override object Clone()
            => new UncannyDodge(PowerClass);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            var _critter = Anchor as Creature;
            _critter?.AddIInteractHandler(this);
            _critter?.AddChangeMonitor(this);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter?.RemoveIInteractHandler(this);
            _critter?.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if ((((workSet?.InteractData as AddAdjunctData)?.Adjunct as UnpreparedToDodge)?.Source as Type)
                == typeof(LocalTurnTracker))
            {
                // block initiative-based unprepared to dodge
                workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        #endregion

        // IMonitorChange<InteractionAlteration> members
        public void PreTestChange(object sender, AbortableChangeEventArgs<InteractionAlteration> args)
        {
            // block max dexterity alterations due to awareness loss
            if (args.Action.Equals(AlterationSet.TargetAction))
            {
                if ((args.NewValue as MaxDexterityToARAlteration)?.UnawarenessLoss ?? false)
                {
                    args.DoAbort(@"Uncanny Dodge", this);
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<InteractionAlteration> args)
        {
        }
    }
}
