using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Feats
{
    [
        Serializable,
        FeatInfo(@"Combat Reflexes", true),
        FighterBonusFeat
    ]
    public class CombatReflexesFeat : FeatBase, IInteractHandler
    {
        public CombatReflexesFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
        }

        #region data
        private BetterDelta _Boost;
        #endregion

        public BetterDelta Boost => _Boost;

        public override string Benefit => @"Additional opportunistic attacks based on Dexterity bonus. Opportunistic attacks while flat-footed.";

        protected override void OnActivate()
        {
            base.OnActivate();
            _Boost = new BetterDelta(Creature.Abilities.Dexterity, new ConstDelta(0, typeof(Dexterity)));
            Creature.Opportunities.Deltas.Add(_Boost);
            Creature.AddIInteractHandler(this);
        }

        protected override void OnDeactivate()
        {
            Creature.RemoveIInteractHandler(this);
            _Boost.DoTerminate();
            base.OnDeactivate();
        }

        public void HandleInteraction(Interaction workSet)
        {
            // when starting initiative, opportunities are possible
            if ((workSet?.InteractData as AddAdjunctData)?.Adjunct is UnpreparedForOpportunities _unprep
                && (_unprep.Source as Type) == typeof(LocalTurnTracker))
            {
                workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            return true;
        }
    }
}
