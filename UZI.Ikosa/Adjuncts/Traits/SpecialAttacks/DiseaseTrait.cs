using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class DiseaseTrait : TraitEffect, ISpecialAttack
    {
        public DiseaseTrait(ITraitSource traitSource, IDiseaseProvider provider)
            : base(traitSource)
        {
            _Provider = provider;
        }

        #region data
        private IDiseaseProvider _Provider;
        #endregion

        public IDiseaseProvider DiseaseProvider => _Provider;

        public override object Clone()
            => new DiseaseTrait(TraitSource, DiseaseProvider);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new DiseaseTrait(traitSource, DiseaseProvider);

        public string Key => @"DiseaseTrait";
        public string DisplayName => @"Disease";
        public bool AllowsSave => true;
        public IPowerDef ForPowerSource() => this;

        public PowerDefInfo ToPowerDefInfo()
            => this.GetPowerDefInfo();

        public IEnumerable<Descriptor> Descriptors
        {
            get { yield break; }
        }

        public IEnumerable<Info> IdentificationInfos
        {
            get
            {
                yield return new Info { Message = Description };
                yield break;
            }
        }

        /// <summary>Default: true if overlapping, or source-material to target-ethereal with force descriptor</summary>
        public virtual bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target)
        {
            if (source.HasOverlappingPresence(target))
                return true;
            if (source.HasMaterialPresence() && target.HasEtherealPresence() && Descriptors.OfType<Force>().Any())
                return true;
            return false;
        }

        public string Description
        {
            get
            {
                var _d = DiseaseProvider.GetDisease();
                return $@"{_d.Name}: {_d.Damage.Name} ({_d.Infection}); Difficulty={_d.Difficulty.EffectiveValue}";
            }
        }

        public void ApplySpecialAttack(StepInteraction deliverDamageInteraction)
        {
            if (deliverDamageInteraction.Target is Creature)
            {
                var _disease = DiseaseProvider.GetDisease();
                var _critter = deliverDamageInteraction.Target as Creature;
                new DiseasedSaveStep(deliverDamageInteraction, _disease, _critter);
            }
        }
    }
}
