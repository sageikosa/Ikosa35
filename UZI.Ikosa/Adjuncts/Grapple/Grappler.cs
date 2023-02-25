using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Grappler : GroupParticipantAdjunct, IActionProvider, IActionFilter
    {
        public Grappler(GrappleGroup group)
            : base(typeof(GrappleGroup), group)
        {
            _AtkDelta = new Delta(-4, typeof(Grappler), @"Grappling");
        }

        #region state
        private Delta _AtkDelta;
        #endregion

        public GrappleGroup GrappleGroup => Group as GrappleGroup;

        public Creature Creature => Anchor as Creature;

        public bool IsGrappling(Guid id)
            => GrappleGroup.Grapplers.Any(_g => _g.ID == id);

        public override object Clone()
            => new Grappler(GrappleGroup);

        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // no opportunistic attacks (no threatened cells)
                Anchor.AddAdjunct(new UnpreparedForOpportunities(this));
                _critter.Conditions.Add(new Condition(Condition.Grappling, this));

                // melee ATK penalty (when used)
                _critter.MeleeDeltable.Deltas.Add(_AtkDelta);
            }
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // remove opportunistic attack block
                Anchor.Adjuncts.OfType<UnpreparedForOpportunities>().FirstOrDefault(_ufo => _ufo.Source == this)?.Eject();
                _critter.Conditions.Remove(_critter.Conditions[Condition.Grappling, this]);

                // remove melee ATK penalty
                _AtkDelta.DoTerminate();
            }
            base.OnDeactivate(source);
        }

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (Anchor is Creature _critter)
            {
                // TODO: draw light weapon as move action with successful grapple check
                // TODO: opposed grapple for unarmed damge (non-lethal or lethal @ -4)
                // TODO: escape grapple with opposed check beating all; move into space
                // TODO: retrieve spell component (full-round action, no check)
                // TODO: spell-casting (1 regular action or less, no somatic; must make concentration check)
                // TODO: move grapple at half your speed with opposed check beating all (regular action)
                // TODO: pin with opposed check
                // TODO: break pin of third-party with opposed check of pinner
                // TODO: use opponent's (light) weapon: opposed check, then -4 ATK
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Grappling", ID);

        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // do not suppress attack actions, they handle grappling themselves
            // do not suppress most magic item activations 
            
            // TODO: suppress spell completions (ie, scroll) and potions...

            // TODO: suppress practically anything else (non-mental) that is not sourced from grappling
            return false;
        }
    }
}
