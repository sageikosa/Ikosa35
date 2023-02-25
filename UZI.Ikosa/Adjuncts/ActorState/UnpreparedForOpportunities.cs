using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class UnpreparedForOpportunities : Adjunct
    {
        public UnpreparedForOpportunities(object source)
            : base(source)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.Conditions.Add(new Condition(Condition.UnpreparedForOpportunities, this));
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Remove(_critter.Conditions[Condition.UnpreparedForOpportunities, this]);
            }
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new UnpreparedForOpportunities(Source);
    }
}
