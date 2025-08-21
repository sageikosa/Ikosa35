using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class PoisonTrait : TraitEffect, ISpecialAttack
    {
        public PoisonTrait(ITraitSource traitSource, Poisonous poisonous)
            : base(traitSource)
        {
            _Poisonous = poisonous;
        }

        #region data
        private Poisonous _Poisonous;
        #endregion

        public override object Clone()
            => new PoisonTrait(TraitSource, Poisonous);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new PoisonTrait(traitSource, Poisonous);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Anchor.AddAdjunct(Poisonous);
        }

        protected override void OnDeactivate(object source)
        {
            Poisonous.Eject();
            base.OnDeactivate(source);
        }

        public Poisonous Poisonous => _Poisonous;
        public string Key => @"PoisonTrait";
        public string DisplayName => @"Poison";
        public bool AllowsSave => Poisonous?.Poison.Difficulty != null;
        public IPowerDef ForPowerSource() => this;

        public PowerDefInfo ToPowerDefInfo()
            => this.GetPowerDefInfo();

        public IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = Description };
                yield break;
            }
        }

        public string Description
        {
            get
            {
                var _p = Poisonous.Poison;
                return $@"{_p.Name}: {_p.PrimaryDamage.Name}/{_p.SecondaryDamage.Name} ({_p.Activation}); Difficulty={_p.Difficulty.EffectiveValue}";
            }
        }

        public IEnumerable<Descriptor> Descriptors
        {
            get { yield break; }
        }

        public void ApplySpecialAttack(StepInteraction deliverDamageInteraction)
        {
            (deliverDamageInteraction?.Target as IAdjunctable)?.AddAdjunct(new Poisoned(Poisonous.Poison));
        }

        /// <summary>Default: true if overlapping, or source-material to target-ethereal with force descriptor</summary>
        public virtual bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target)
        {
            if (source.HasOverlappingPresence(target))
            {
                return true;
            }

            if (source.HasMaterialPresence() && target.HasEtherealPresence() && Descriptors.OfType<Force>().Any())
            {
                return true;
            }

            return false;
        }
    }
}
