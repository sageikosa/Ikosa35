using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items.Shields;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ShieldProficiencyTrait : TraitEffect, IShieldProficiency
    {
        public ShieldProficiencyTrait(ITraitSource traitSource, bool tower)
            : base(traitSource)
        {
            _IsTower = tower;
        }

        private bool _IsTower;

        public bool IsTower => _IsTower;

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

        public override TraitEffect Clone(ITraitSource traitSource)
            => new ShieldProficiencyTrait(traitSource, IsTower);

        public override object Clone()
            => new ShieldProficiencyTrait(TraitSource, IsTower);

        public string Description
            => IsTower ? @"All shields" : @"All shields except tower)";

        public bool IsProficientWithShield(bool tower, int powerLevel)
            => tower ? IsTower : true;

        public bool IsProficientWith(ShieldBase shield, int powerLevel)
            => IsProficientWithShield(shield?.Tower ?? false, powerLevel);
    }
}
