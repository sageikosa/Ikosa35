using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ProtectionFromEnergy : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Protection from Energy";
        public override string Description => @"Protective shell of energy with the capacity negate 12 points/level of the chosen energy; lasts until the capacity is consumed or after 10 minutes/level if capacity not fully used.";
        public override MagicStyle MagicStyle => new Abjuration();

        #region public override IEnumerable<SpellComponent> DivineComponents { get; }
        public override IEnumerable<SpellComponent> DivineComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new DivineFocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode Members

        #region private IEnumerable<OptionAimOption> Energies()
        private IEnumerable<OptionAimOption> Energies()
        {
            yield return new OptionAimValue<EnergyType>
            {
                Description = @"Acid",
                Key = @"Acid",
                Name = @"Acid",
                Value = EnergyType.Acid
            };
            yield return new OptionAimValue<EnergyType>
            {
                Description = @"Cold",
                Key = @"Cold",
                Name = @"Cold",
                Value = EnergyType.Cold
            };
            yield return new OptionAimValue<EnergyType>
            {
                Description = @"Electric",
                Key = @"Electric",
                Name = @"Electric",
                Value = EnergyType.Electric
            };
            yield return new OptionAimValue<EnergyType>
            {
                Description = @"Fire",
                Key = @"Fire",
                Name = @"Fire",
                Value = EnergyType.Fire
            };
            yield return new OptionAimValue<EnergyType>
            {
                Description = @"Sonic",
                Key = @"Sonic",
                Name = @"Sonic",
                Value = EnergyType.Sonic
            };
            yield break;
        }
        #endregion

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            // TODO: energy type
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield return new OptionAim(@"Energy", @"Energy", true, FixedRange.One, FixedRange.One, Energies());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if (apply.DeliveryInteraction.Target is Creature)
            {
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }

        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if ((source is MagicPowerEffect _spellEffect)
                && (target is Creature _critter))
            {
                // ensure battery
                var _battery = _spellEffect.EnsureBattery(() => Math.Min(_spellEffect.MagicPowerActionSource.CasterLevel * 12, 120));
                var _energy = ((_spellEffect.FirstTarget(@"Energy") as OptionTarget).Option) as OptionAimValue<EnergyType>;
                var _protection = new ProtectionFromEnergyEffect(source, _energy.Value, _battery);
                _critter.AddAdjunct(_protection);
                return _protection;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as ProtectionFromEnergyEffect)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Fortitude";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class ProtectionFromEnergyEffect : Adjunct, IInteractHandler
    {
        public ProtectionFromEnergyEffect(object source, EnergyType energy, PowerBattery battery)
            : base(source)
        {
            _Energy = energy;
            _Battery = battery;
        }

        #region state
        private PowerBattery _Battery;
        private readonly EnergyType _Energy;
        #endregion

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public EnergyType Energy => _Energy;
        public PowerBattery Battery => _Battery;

        public override object Clone()
            => new ProtectionFromEnergyEffect(Source, Energy, Battery);

        protected override void OnActivate(object source)
        {
            (Anchor as Creature)?.AddIInteractHandler(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as Creature)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        public void HandleInteraction(Interaction workSet)
        {
            if ((workSet.InteractData is IDeliverDamage _deliverDamage)
                && (workSet.Target is Creature _critter))
            {
                var _damage = _deliverDamage.GetEnergy(Energy);
                if (_damage > 0)
                {
                    var _avail = Battery.AvailableCharges;
                    if (_avail > _damage)
                    {
                        // cancel damage, subtract from battery and eject if exhausted
                        _deliverDamage.Damages.Add(new EnergyDamageData(0 - _damage, Energy, $@"Protection from {Energy}", -1));
                        Battery.UseCharges(_damage);
                    }
                    else
                    {
                        // subtract from damage and remove battery
                        _deliverDamage.Damages.Add(new EnergyDamageData(0 - _avail, Energy, $@"Protection from {Energy}", -1));
                        MagicPowerEffect.Eject();
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(DeliverDamageData);
            yield return typeof(SaveableDamageData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => existingHandler switch
            {
                DamageReductionHandler _ => true,
                EnergyResistanceHandler _ => true,
                EvasionHandler _ => true,
                _ => false,
            };
    }
}
