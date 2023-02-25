using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class ParalysisTrait : TraitEffect, ISpecialAttack
    {
        public ParalysisTrait(ITraitSource traitSource, IParalysisProvider paralysisProvider)
            : base(traitSource)
        {
            _Provider = paralysisProvider;
        }

        #region data
        private IParalysisProvider _Provider;
        #endregion

        public IParalysisProvider ParalysisProvider => _Provider;

        public override object Clone()
            => new ParalysisTrait(TraitSource, ParalysisProvider);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new ParalysisTrait(traitSource, ParalysisProvider);

        public string Key => @"ParalysisTrait";
        public string DisplayName => @"Paralysis";
        public bool AllowsSave => ParalysisProvider?.SaveType >= Interactions.SaveType.Fortitude;
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
                var _difficulty = ParalysisProvider.Difficulty;
                var _duration =
                    (_Provider.TimeUnits != null)
                    ? $@"; {_Provider.TimeUnits.RollString} {_Provider.UnitFactor.PluralName}"
                    : @"; Permanent";
                if (_difficulty != null)
                    return $@"Difficulty {_difficulty.EffectiveValue} {_Provider.SaveType}{_duration}";
                return $@"No Save{_duration}";
            }
        }

        public void ApplySpecialAttack(StepInteraction deliverDamageInteraction)
        {
            if ((deliverDamageInteraction.Target is Creature _critter)
                && ParalysisProvider.WillAffect(_critter))
            {
                deliverDamageInteraction.Step.StartNewProcess(
                    new ParalyzeStep((CoreProcess)null, ParalysisProvider, _critter), @"Paralyze");
            }
        }
    }
}
