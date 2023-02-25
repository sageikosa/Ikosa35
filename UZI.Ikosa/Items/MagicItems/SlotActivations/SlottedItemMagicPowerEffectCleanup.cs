using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Items
{
    /// <summary>
    /// Provides cleanup of magic power effects activated by slotted magic items when the magic item is unslotted
    /// </summary>
    [Serializable]
    public class _SlottedItemMagicPowerEffectCleanup : SlotActivation
    {
        /// <summary>
        /// Provides cleanup of magic power effects activated by slotted magic items when the magic item is unslotted
        /// </summary>
        public _SlottedItemMagicPowerEffectCleanup(MagicPowerActionSource source)
            : base(source, true)
        {
        }

        public override bool IsProtected => true;

        public override IEnumerable<Info> IdentificationInfos { get { yield break; } }

        public MagicPowerActionSource MagicPowerActionSource => Source as MagicPowerActionSource;

        public override object Clone()
            => new _SlottedItemMagicPowerEffectCleanup(MagicPowerActionSource);

        protected override void OnSlottedActivate()
        {
            // nothing to do here
        }

        protected override void OnSlottedDeActivate()
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
    }
}
