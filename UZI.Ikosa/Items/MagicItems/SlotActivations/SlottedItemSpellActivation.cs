using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SlottedItemSpellActivation : SlottedItemActionProvider, IAugmentationCost, IItemRequirements
    {
        private bool _Deactivation;
        private bool _MasterWork;

        public SlottedItemSpellActivation(
            SpellActivation spellCommandWord,
            bool masterwork,
            bool unslotDeactivation
            ) : base(spellCommandWord)
        {
            _MasterWork = masterwork;
            _Deactivation = unslotDeactivation;
        }

        public bool UnslottingDeactivates => _Deactivation;
        public bool RequiresMasterwork => _MasterWork;

        public SpellActivation SpellActivation => Provider as SpellActivation;
        public decimal StandardCost => SpellActivation.StandardCost;

        public override object Clone()
            => new SlottedItemSpellActivation(SpellActivation, RequiresMasterwork, UnslottingDeactivates);

        protected override void OnSlottedDeActivate()
        {
            if (_Deactivation)
            {
                // all spell activations defined on the item
                var _activations = SlottedItem.Adjuncts.OfType<SpellActivation>().ToList();

                // all magic power effects in force on the creature that are sources by an activation
                foreach (var _effect in (from _eff in SlottedItem.CreaturePossessor.Adjuncts.OfType<MagicPowerEffect>()
                                         where _activations.Any(_act => _act.SpellSource == _eff.MagicPowerActionSource)
                                         select _eff).ToList())
                {
                    // get rid of magic powers sourced by the spell activation
                    _effect.Eject();
                }
            }

            base.OnSlottedDeActivate();
        }
    }
}
