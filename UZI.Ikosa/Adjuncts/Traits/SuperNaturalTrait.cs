using System;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SuperNaturalTrait : TraitBase, IMagicAura
    {
        public SuperNaturalTrait(ITraitSource traitSource, ISuperNaturalPowerSource powerSource, TraitCategory category, TraitEffect trait)
            : base(traitSource, powerSource.PowerDef.DisplayName, powerSource.PowerDef.Description, category, trait)
        {
            _PowerSource = powerSource;
        }

        private ISuperNaturalPowerSource _PowerSource;

        public ISuperNaturalPowerSource SuperNaturalPowerSource 
            => _PowerSource;

        public override string TraitNature 
            => @"SuperNatural";

        public override object Clone() 
            => new SuperNaturalTrait(TraitSource, SuperNaturalPowerSource, TraitCategory, Trait.Clone(TraitSource));

        public override TraitBase Clone(ITraitSource traitSource) 
            => new SuperNaturalTrait(traitSource, SuperNaturalPowerSource, TraitCategory, Trait.Clone(traitSource));

        #region IMagicAura Members

        public Core.Contracts.AuraStrength MagicStrength
        {
            get { return SuperNaturalPowerSource.MagicStrength; }
        }

        public MagicStyle MagicStyle
        {
            get { return SuperNaturalPowerSource.MagicStyle; }
        }

        public int PowerLevel
        {
            get { return ((IMagicAura)SuperNaturalPowerSource).PowerLevel; }
        }

        public int CasterLevel
        {
            get { return SuperNaturalPowerSource.CasterLevel; }
        }

        #endregion

        #region IAura Members

        public Core.Contracts.AuraStrength AuraStrength
        {
            get { return SuperNaturalPowerSource.AuraStrength; }
        }

        #endregion
    }
}
