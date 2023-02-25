using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Cannot dodge, does not get Max Dexterity to Armor Rating</summary>
    [Serializable]
    public class UnpreparedToDodge : Adjunct
    {
        /// <summary>Cannot dodge, does not get Max Dexterity to Armor Rating</summary>
        public UnpreparedToDodge(object source)
            : base(source)
        {
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.Conditions.Add(new Condition(Condition.UnpreparedToDodge, this));
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Conditions.Remove(_critter.Conditions[Condition.UnpreparedToDodge, this]);
            }
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new UnpreparedToDodge(Source);
    }
}
