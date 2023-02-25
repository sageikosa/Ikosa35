using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ProtectionFromArrows : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override MagicStyle MagicStyle => new Abjuration();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
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

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region ISpellMode

        public override string DisplayName => @"Protection from Arrows";
        public override string Description => @"DR 10/magic against ranged weapons";
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        #region IDurableMode

        public IEnumerable<int> DurableSubModes => (0).ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                // ensure battery
                var _battery = _spellEffect.EnsureBattery(() => Math.Min(_spellEffect.MagicPowerActionSource.CasterLevel * 10, 100));
                var _reduce = new ProtectionFromArrowsDR(_spellEffect, _battery, 10);
                (target as Creature)?.DamageReductions.Add(_reduce);
                return _reduce;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (target as Creature)?.DamageReductions.Remove(source.ActiveAdjunctObject as ProtectionFromArrowsDR);
        }

        public bool IsDismissable(int subMode) => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Hour(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class ProtectionFromArrowsDR : IDamageReduction
    {
        public ProtectionFromArrowsDR(MagicPowerEffect source, PowerBattery battery, int maxUse)
        {
            _Source = source;
            _Battery = battery;
            _MaxUse = maxUse;
        }

        #region data
        private MagicPowerEffect _Source;
        private PowerBattery _Battery;
        private int _MaxUse;
        #endregion

        public MagicPowerEffect MagicPowerEffect => _Source;
        public PowerBattery PowerBattery => _Battery;
        public int MaxUse => _MaxUse;

        public int Amount => Math.Min(PowerBattery.AvailableCharges, MaxUse);

        public string Name => @"DR 10/magic against ranged";

        public object Source => _Source;

        public bool WeaponIgnoresReduction(IWeaponHead weaponHead)
        {
            if (weaponHead?.ContainingWeapon is IProjectileWeapon)
            {
                return weaponHead.IsMagicalDamage;
            }

            return true;
        }

        public void HasReduced(int amount)
        {
            PowerBattery.UseCharges(amount);
            if (PowerBattery.AvailableCharges <= 0)
                MagicPowerEffect.Eject();
        }
    }
}
