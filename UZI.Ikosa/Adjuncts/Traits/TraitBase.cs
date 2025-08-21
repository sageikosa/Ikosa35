using System;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public abstract class TraitBase : Adjunct
    {
        protected TraitBase(ITraitSource traitSource, string name, string benefit, TraitCategory category, TraitEffect trait)
            : base(traitSource)
        {
            _Name = name;
            _Benefit = benefit;
            _Category = category;
            _Trait = trait;
        }

        #region private data
        private TraitEffect _Trait;
        private string _Name;
        private string _Benefit;
        private TraitCategory _Category;
        #endregion

        public ITraitSource TraitSource => Source as ITraitSource;
        public TraitEffect Trait => _Trait;
        public string Name => (Trait as IPowerDef)?.DisplayName ?? _Name;
        public string Benefit => (Trait as IPowerDef)?.Description ?? _Benefit;
        public TraitCategory TraitCategory => _Category;
        public abstract string TraitNature { get; }

        #region public override bool CanAnchor(IAdjunctable newAnchor)
        public override bool CanAnchor(IAdjunctable newAnchor)
        {
            if (!Trait.CanAnchor(newAnchor))
            {
                return false;
            }

            return true;
        }
        #endregion

        #region public override bool CanUnAnchor()
        public override bool CanUnAnchor()
        {
            return Trait.CanUnAnchor();
        }
        #endregion

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            base.OnAnchorSet(oldAnchor, oldSetting);

            if (oldAnchor != null)
            {
                Trait.Eject();
            }

            if (Anchor != null)
            {
                Trait.InitialActive = false;
                Anchor.AddAdjunct(Trait);
            }
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Trait.IsActive = true;
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            Trait.IsActive = false;
        }

        public abstract TraitBase Clone(ITraitSource species);
    }
}
