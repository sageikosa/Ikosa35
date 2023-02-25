using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Items
{
    /// <summary>Blocks poisoned from being added if the Poison.SourceID resolves to a creature with the specified species</summary>
    [Serializable]
    public class SpeciesPoisonImmunity<CritterSpecies> : AdjunctBlocker<Poisoned>
        where CritterSpecies : Species
    {
        /// <summary>Blocks poisoned from being added if the Poison.SourceID resolves to a creature with the specified species</summary>
        public SpeciesPoisonImmunity(CritterSpecies species)
            : base(species)
        {
        }

        public CritterSpecies Species => Source as CritterSpecies;

        protected override bool WillBlock(Poisoned adjunct)
            => IkosaStatics.CreatureProvider
            .GetCreature(adjunct.Poison.SourceID ?? Guid.Empty)?.Species is CritterSpecies;

        public override object Clone()
            => new SpeciesPoisonImmunity<CritterSpecies>(Species);
    }
}
