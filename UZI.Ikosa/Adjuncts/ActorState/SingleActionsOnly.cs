using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>NOTE: this adjunct enables special processing in LocalActionBudget</summary>
    [Serializable]
    public class SingleActionsOnly : Adjunct
    {
        /// <summary>NOTE: this adjunct enables special processing in LocalActionBudget</summary>
        public SingleActionsOnly(object source)
            : base(source)
        {
        }

        #region Activate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // add condition
                _critter.Conditions.Add(new Condition(Condition.SingleAction, this));
            }
            base.OnActivate(source);
        }
        #endregion

        #region DeActivate
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (Anchor is Creature _critter)
            {
                // remove condition
                _critter.Conditions.Remove(_critter.Conditions[Condition.SingleAction, this]);
            }
        }
        #endregion

        public override object Clone()
            => new SingleActionsOnly(Source);
    }
}
