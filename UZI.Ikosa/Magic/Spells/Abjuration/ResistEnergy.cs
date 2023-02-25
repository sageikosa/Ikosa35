using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ResistEnergy : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName { get { return @"Resist Energy"; } }
        public override string Description { get { return @"Resist 10/20/30 energy damage of one type"; } }
        public override MagicStyle MagicStyle { get { return new Abjuration(); } }

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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield return new OptionAim(@"Energy", @"Energy", true, FixedRange.One, FixedRange.One, Energies());
            yield break;
        }

        public bool AllowsSpellResistance { get { return true; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            CopyActivityTargetsToSpellEffects(apply);

            // add the fully targetted effect
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                // determine amount
                var _amount = ((_spellEffect.MagicPowerActionSource.CasterLevel + 1) / 4) * 10;
                if (_amount > 30)
                    _amount = 30;
                if (_amount < 10)
                    _amount = 10;

                // build delta
                var _energy = ((_spellEffect.FirstTarget(@"Energy") as OptionTarget).Option) as OptionAimValue<EnergyType>;
                var _resist = new Delta(_amount, typeof(EnergyResistance), string.Format(@"{0} from Resist Energy", _amount));
                (target as Creature)?.EnergyResistances[_energy.Value].Deltas.Add(_resist);
                return _resist;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource) 
            => (source.ActiveAdjunctObject as Delta)?.DoTerminate();

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) { return @"Save.Fort"; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1)); }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }
}
