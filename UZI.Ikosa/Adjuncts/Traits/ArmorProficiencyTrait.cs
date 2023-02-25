using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items.Armor;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ArmorProficiencyTrait : TraitEffect, IArmorProficiency
    {
        public ArmorProficiencyTrait(ITraitSource traitSource, ArmorProficiencyType profType)
            : base(traitSource)
        {
            _ProfType = profType;
        }

        private ArmorProficiencyType _ProfType;

        public ArmorProficiencyType ProficiencyType => _ProfType;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as Creature)?.Proficiencies.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.Proficiencies.Remove(this);
            base.OnDeactivate(source);
        }

        public string Description
            => $@"{ProficiencyType} Armor";

        public bool IsProficientWith(ArmorBase armor, int powerLevel)
            => IsProficientWith(armor?.ProficiencyType ?? ArmorProficiencyType.Heavy, powerLevel);

        public bool IsProficientWith(ArmorProficiencyType profType, int powerLevel)
            => profType <= ProficiencyType;

        public override object Clone()
            => new ArmorProficiencyTrait(TraitSource, ProficiencyType);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new ArmorProficiencyTrait(traitSource, ProficiencyType);
    }
}
