using System;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Bonus feat as a trait.  Use for automatic species bonus feats.  Use requirements for selectable bonus feats.</summary>
    [Serializable]
    public class BonusFeatTrait : TraitEffect
    {
        /// <summary>Bonus feat as a trait.  Use for automatic species bonus feats.  Use requirements for selectable bonus feats.</summary>
        public BonusFeatTrait(ITraitSource traitSource, FeatBase feat)
            : base(traitSource)
        {
            _Feat = feat;
        }

        #region private data
        private FeatBase _Feat;
        #endregion

        public FeatBase Feat => _Feat;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                _Feat.BindTo(_critter);
            }
        }

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _Feat.UnbindFromCreature();
            }

            base.OnDeactivate(source);
        }

        public override object Clone()
        {
            // TODO: clone feats
            return new BonusFeatTrait(TraitSource, _Feat);
        }

        public override TraitEffect Clone(ITraitSource traitSource)
        {
            // TODO: clone feats
            return new BonusFeatTrait(traitSource, _Feat);
        }

        public static ExtraordinaryTrait GetExtraordinaryBonusFeat(
            ITraitSource source, TraitCategory category, FeatBase feat)
            => new ExtraordinaryTrait(source, feat.Name, feat.Benefit, category,
                new BonusFeatTrait(source, feat));
    }
}