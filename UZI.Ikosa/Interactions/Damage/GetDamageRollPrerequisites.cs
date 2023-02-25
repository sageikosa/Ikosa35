using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>Feedback should be <see cref="ValueFeedback{T}"/>(<see cref="DamageRollPrerequisite"/>)</summary>
    [Serializable]
    public class GetDamageRollPrerequisites : InteractData
    {
        /// <summary>Feedback should be <see cref="ValueFeedback{T}"/>(<see cref="DamageRollPrerequisite"/>)</summary>
        public GetDamageRollPrerequisites(CoreActor actor, Interaction attack)
            : base(actor)
        {
            _Attack = attack;
        }

        private Interaction _Attack;

        public Interaction Interaction => _Attack;
        public AttackData AttackData => _Attack?.InteractData as AttackData;

        /// <summary>Provides damage roll prerequisites from the actor based on the attack situation</summary>
        public static IEnumerable<DamageRollPrerequisite> GetDamageRollFeedback(CoreActor actor, Interaction attack)
        {
            var _dmgRoll = new Interaction(actor, typeof(GetDamageRollPrerequisites), actor,
                new GetDamageRollPrerequisites(actor, attack));
            actor?.HandleInteraction(_dmgRoll);
            return _dmgRoll.Feedback.OfType<ValueFeedback<DamageRollPrerequisite>>()
                .Select(_drb => _drb.Value);
        }
    }
}
