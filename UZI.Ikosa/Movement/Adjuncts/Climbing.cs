using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Climbing : Adjunct, IInteractHandler
    {
        #region construction
        public Climbing(SuccessCheckTarget target)
            : base(typeof(Climbing))
        {
            _Target = target;
            _Expiry = false;
        }
        #endregion

        #region private data
        private SuccessCheckTarget _Target;
        private bool _Expiry;
        #endregion

        public enum ClimbOutcome : byte { Successful, Immobilized, Falling }

        public Creature Creature => Anchor as Creature;
        public SuccessCheckTarget SuccessCheckTarget => _Target;
        public override bool IsProtected => true;

        /// <summary>
        /// True if this climb check has expired (IInteractHandler and Condition may still be in effect, though).
        /// An expired climbing check requires a new check to be made, but this adjunct will continue to provide climbing
        /// mechanics until replaced.
        /// </summary>
        public bool IsCheckExpired { get => _Expiry; set => _Expiry = value; }

        #region public ClimbOutcome Outcome { get; }
        public ClimbOutcome Outcome
        {
            get
            {
                if (SuccessCheckTarget.Success)
                {
                    return ClimbOutcome.Successful;
                }
                else if (SuccessCheckTarget.SoftFail(4))
                {
                    return ClimbOutcome.Immobilized;
                }
                else
                {
                    return ClimbOutcome.Falling;
                }
            }
        }
        #endregion

        public bool IsAccelerated => SuccessCheckTarget.IsUsingPenalty;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Creature != null)
            {
                // remove all climbs that are not this
                foreach (var _climb in Creature.Adjuncts.OfType<Climbing>().Where(_c => this != _c).ToList())
                {
                    _climb.Eject();
                }

                // creatures without natural climb speed are not prepared to dodge
                if (!Creature.Movements.AllMovements.OfType<ClimbMovement>().Any(_c => _c.IsNaturalClimber))
                {
                    Creature.AddAdjunct(new UnpreparedToDodge(this));
                }

                Creature.AddIInteractHandler(this);
            }
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Creature != null)
            {
                Creature.Adjuncts.OfType<UnpreparedToDodge>().FirstOrDefault(_d => _d.Source == this)?.Eject();
                Creature.RemoveIInteractHandler(this);
            }
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new Climbing(SuccessCheckTarget);

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            Creature?.StartNewProcess(
                new ClimbSustainStep(
                    new Interaction(workSet.Actor, this, Creature, workSet.InteractData),
                    SuccessCheckTarget.Check, @"Climb Check from Damage"),
                @"Climb due to Damage");
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            if (existingHandler is TempHPDamageHandler)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
