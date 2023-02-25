using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class EnergyDrainTrait : TraitEffect, ISpecialAttack
    {
        public EnergyDrainTrait(ITraitSource traitSource, IEnergyDrainProvider energyDrainProvider)
            : base(traitSource)
        {
            _Provider = energyDrainProvider;
        }

        #region data
        private IEnergyDrainProvider _Provider;
        #endregion

        public IEnergyDrainProvider EnergyDrainProvider => _Provider;

        public override object Clone()
            => new EnergyDrainTrait(TraitSource, EnergyDrainProvider);

        public override TraitEffect Clone(ITraitSource traitSource)
            => new EnergyDrainTrait(traitSource, EnergyDrainProvider);

        public PowerDefInfo ToPowerDefInfo()
            => this.GetPowerDefInfo();

        public string Key => @"EnergyDrain";
        public string DisplayName => @"Energy Drain";
        public virtual bool AllowsSave => false;
        public IPowerDef ForPowerSource() => this;

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

        public string Description => @"Negative level(s) bestowed";

        /// <summary>Default: true if overlapping, or source-material to target-ethereal with force descriptor</summary>
        public virtual bool HasPlanarCompatibility(PlanarPresence source, PlanarPresence target)
        {
            if (source.HasOverlappingPresence(target))
                return true;
            if (source.HasMaterialPresence() && target.HasEtherealPresence() && Descriptors.OfType<Force>().Any())
                return true;
            return false;
        }

        public void ApplySpecialAttack(StepInteraction deliverDamageInteraction)
        {
            if (deliverDamageInteraction.Target is Creature _critter)
            {
                var _critical = (deliverDamageInteraction.InteractData as IDeliverDamage)?.IsCriticalHit ?? false;
                deliverDamageInteraction.Step.StartNewProcess(
                    new EnergyDrainStep((CoreProcess)null, EnergyDrainProvider, _critter, _critical), @"EnergyDrain");
            }
        }
    }

    public interface IEnergyDrainProvider
    {
        IVolatileValue Difficulty { get; }
        Roller Levels { get; }
        Creature Drainer { get; }
    }

    [Serializable]
    public class EnergyDrainStep : PreReqListStepBase
    {
        #region ctor()
        public EnergyDrainStep(CoreProcess process, IEnergyDrainProvider provider, Creature target, bool critical)
            : base(process)
        {
            _Provider = provider;
            _Target = target;
            _Critical = critical;

            var _levels = provider.Levels;
            if (!((_levels is ConstantRoller) || (_levels == null)))
            {
                _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Levels", @"Level Damage", _levels, false));
                if (critical)
                    _PendingPreRequisites.Enqueue(new RollPrerequisite(this, @"Levels.Critical", @"Critical Level Damage", _levels, false));

            }
        }
        #endregion

        #region data
        private IEnergyDrainProvider _Provider;
        private Creature _Target;
        private bool _Critical;
        #endregion

        public IEnergyDrainProvider EnergyDrainProvider => _Provider;
        public Creature Target => _Target;
        public bool IsCritical => _Critical;

        protected override bool OnDoStep()
        {
            if (IsComplete)
                return true;

            var _rolls = AllPrerequisites<RollPrerequisite>().ToList();

            // get levels
            var _levels = 1;
            if (_rolls.Any())
            {
                if (_rolls.All(_r => _r.IsReady))
                    _levels = _rolls.Sum(_r => _r.RollValue);
                else
                    return false;
            }
            else
            {
                // check for constant roller...
                var _units = EnergyDrainProvider.Levels;
                if (_units is ConstantRoller)
                    _levels = _units.RollValue(Guid.Empty, @"Drain", @"Levels") * (IsCritical ? 2 : 1);
            }

            // drainer creature and riser type
            var _drainer = EnergyDrainProvider.Drainer;
            var _riser = typeof(Wight);
            if (_drainer.Species is IReplaceCreature)
            {
                _riser = _drainer.Species.GetType();
            }

            // difficulty
            var _calc = EnergyDrainProvider.Difficulty
                .GetDeltaCalcInfo(new Qualifier(_drainer, EnergyDrainProvider, Target), @"Drain Save Difficulty");

            // add negative levels
            var _now = Target?.GetCurrentTime() ?? 0d;
            var _next = _now + Day.UnitFactor;
            for (var _lx = 0; _lx < _levels; _lx++)
            {
                // persistent negative level
                Target.AddAdjunct(new PersistentNegativeLevel(_riser, _next, Day.UnitFactor, _calc));
            }

            // drainer gets 5 temp points per drained level for 1 hour
            _drainer.AddAdjunct(new TempHPAdjunct(this, _levels * 5, _now + Hour.UnitFactor));
            return true;
        }
    }
}
