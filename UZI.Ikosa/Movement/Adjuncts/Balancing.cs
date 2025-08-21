using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Balancing : Adjunct, IInteractHandler
    {
        // TODO: consider this as a template for general check roll adjuncts (where one roll may be needed for a duration)

        #region construction
        public Balancing(SuccessCheckTarget target)
            : base(typeof(Balancing))
        {
            _Target = target;
            _Expiry = false;
        }
        #endregion

        #region private data
        private SuccessCheckTarget _Target;
        private bool _Expiry;
        #endregion

        public enum BalanceOutcome : byte { Successful, Immobilized, Falling }

        public Creature Creature => Anchor as Creature;
        public SuccessCheckTarget SuccessCheckTarget => _Target;
        public override bool IsProtected => true;

        /// <summary>
        /// True if this balance check has expired (IInteractHandler and Condition may still be in effect, though).
        /// An expired balancing check requires a new check to be made, but this adjunct will continue to provide balancing
        /// mechanics until replaced.
        /// </summary>
        public bool IsCheckExpired { get { return _Expiry; } set { _Expiry = value; } }

        public BalanceOutcome Outcome
            => SuccessCheckTarget.Success ? BalanceOutcome.Successful
            : SuccessCheckTarget.SoftFail(4) ? BalanceOutcome.Immobilized
            : BalanceOutcome.Falling;

        public bool IsAccelerated
            => SuccessCheckTarget.IsUsingPenalty;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Creature != null)
            {
                // remove all Balancings that are not this
                foreach (var _balancing in Creature.Adjuncts.OfType<Balancing>().Where(_b => this != _b).ToList())
                {
                    _balancing.Eject();
                }

                var _balance = Creature.Skills.Skill<BalanceSkill>();

                // if you have less than five ranks, you are unprepared to dodge while balancing
                if (_balance.BaseValue < 5)
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
            => new Balancing(SuccessCheckTarget);

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet != null)
                && (workSet.InteractData is DeliverDamageData || workSet.InteractData is SaveableDamageData))
            {
                Creature?.StartNewProcess(
                    new BalanceDamageStep(
                        new Interaction(workSet.Actor, this, Creature, workSet.InteractData),
                        SuccessCheckTarget.Check),
                    @"Balance due to Damage");
            }
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
