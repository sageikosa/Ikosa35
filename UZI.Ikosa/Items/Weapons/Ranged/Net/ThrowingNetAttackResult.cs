using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    /// <summary>Allows a net to entangle on a successful attack</summary>
    [Serializable]
    public class ThrowingNetAttackResult : Adjunct, ISecondaryAttackResult
    {
        /// <summary>Allows a net to entangle on a successful attack</summary>
        public ThrowingNetAttackResult(ThrowingNet net)
            : base(net)
        {
        }

        public ThrowingNet ThrowingNet => Source as ThrowingNet;
        public override bool IsProtected => true;

        public override object Clone()
            => new ThrowingNetAttackResult(ThrowingNet);

        #region ISecondaryAttackResult Members

        public object AttackResultSource => Source;

        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        {
            yield break;
        }

        public void AttackResult(StepInteraction deliverDamageInteraction)
        {
            if (deliverDamageInteraction != null)
            {
                // bind target to this group
                if (deliverDamageInteraction.Target is Creature _target)
                {
                    if (Math.Abs(_target.Sizer.Size.Order - ThrowingNet.ItemSizer.ExpectedCreatureSize.Order) <= 1)
                    {
                        // remove from thrower's hands
                        ThrowingNet.ClearSlots();

                        // use it as a covering wrapper
                        // TODO: block pickup of ICanCover object?
                        new CoveringWrapper(_target, ThrowingNet);
                    }
                }
            }
            return;
        }

        public bool PoweredUp
        {
            // always powered up
            get { return true; }
            set { }
        }

        public bool IsDamageSufficient(StepInteraction final)
        {
            // damage doesn't matter, the entangler always finds a hit sufficient
            return true;
        }

        #endregion
    }

}
