using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class FlameArrow : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Flame Arrow";
        public override string Description => @"Ammunition gains flaming power delivering 1d6 fire damage.";
        public override MagicStyle MagicStyle => new Transformation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Fire();
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

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
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
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Weapon", @"Ammunition to Enhance",
                FixedRange.One, FixedRange.One, new NearRange(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if ((apply.DeliveryInteraction.Target is IAmmunitionBundle _bundle)
                && (_bundle.Count <= 50))
            {
                SpellDef.ApplySpellEffectAmmunitionBundle(apply);
            }
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // apply the delta to each weapon head, do not apply the enhanced adjunct
            if (source is MagicPowerEffect _spellEffect)
            {
                if (target is IAmmunitionBase _ammoBase)
                {
                    var _flame = new FlameArrowPower(_spellEffect);
                    _ammoBase.AddAdjunct(_flame);
                    return _flame;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as FlameArrowPower)?.Eject();
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
            => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
        #endregion

    }

    [Serializable]
    public class FlameArrowPower : Adjunct, IWeaponExtraDamage
    {
        public FlameArrowPower(MagicPowerEffect magicPowerEffect)
            : base(magicPowerEffect)
        {
        }

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public bool PoweredUp { get => true; set { } }

        public override object Clone()
            => new FlameArrowPower(MagicPowerEffect);

        public IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            yield return
                new EnergyDamageRollPrerequisite(typeof(FlameArrowPower), workSet, @"Fire", @"Flame Arrow", 
                new DieRoller(6), false, @"Flame Arrow", 0, EnergyType.Fire);
            yield break;
        }
    }
}
