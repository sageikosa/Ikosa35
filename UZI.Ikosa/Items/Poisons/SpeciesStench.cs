using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class SpeciesStench<CritterSpecies> : Stench
        where CritterSpecies : Species, IPoisonProvider
    {
        public SpeciesStench(CritterSpecies species, IStenchGeometryBuilderFactory builder) // TODO: range by calculation
            : base(species, builder)
        {
            _Immunity = new SpeciesPoisonImmunity<CritterSpecies>(species);
        }

        #region immunity
        private SpeciesPoisonImmunity<CritterSpecies> _Immunity;
        #endregion

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // as long as species stench is attached, so is immunity
            if (oldAnchor == null)
            {
                Anchor.AddAdjunct(_Immunity);
            }
            else
            {
                _Immunity.Eject();
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }

        public CritterSpecies Species => Source as CritterSpecies;

        public override object Clone()
            => new SpeciesStench<CritterSpecies>(Species, Factory);

        protected override IGeometricSize SourceSize(Locator locator)
            => (locator.Chief == Species.Creature
            ? Species.Creature.Body.Sizer.Size.CubeSize()
            : locator.NormalSize as IGeometricSize);
    }
}
