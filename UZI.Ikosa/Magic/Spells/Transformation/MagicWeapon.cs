using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class MagicWeapon : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Magic Weapon";
        public override string Description => @"Provides +1 enhancment to attack and damage for a weapon.";
        public override MagicStyle MagicStyle => new Transformation();

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

        public override IEnumerable<ISpellMode> SpellModes
            => this.ToEnumerable();

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Weapon", @"Weapon to Enhance", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Weapon", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if (apply.DeliveryInteraction.Target is IWeapon _weapon)
            {
                if (!(_weapon is NaturalWeapon _natrl))
                {
                    SpellDef.ApplyDurableMagicEffects(apply);
                }
            }
        }
        #endregion

        #region IDurableCapable Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            // apply the delta to each weapon head, do not apply the enhanced adjunct
            return new MagicWeaponAdjunct(source as MagicPowerEffect, 1, DisplayName);
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as Adjunct)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
            => Enumerable.Empty<StepPrerequisite>();

        public IEnumerable<int> DurableSubModes
            => 0.ToEnumerable();

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class MagicWeaponAdjunct : Adjunct
    {
        public MagicWeaponAdjunct(MagicPowerEffect source, int delta, string name)
            : base(source)
        {
            _Enhanced = new ConstDelta(delta, typeof(Deltas.Enhancement), name);
        }

        #region state
        private readonly ConstDelta _Enhanced;
        #endregion

        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;
        public ConstDelta Enhanced => _Enhanced;

        public override object Clone()
            => new MagicWeaponAdjunct(MagicPowerEffect, _Enhanced.Value, _Enhanced.Name);

        protected override void OnActivate(object source)
        {
            if (Anchor is IMeleeWeapon _melee)
            {
                foreach (var _head in _melee.AllHeads)
                {
                    _head.AttackBonus.Deltas.Add(_Enhanced);
                    _head.DamageBonus.Deltas.Add(_Enhanced);
                }
            }
            else if (Anchor is IProjectileWeapon _project)
            {
                _project.AttackBonus.Deltas.Add(_Enhanced);
                _project.DamageBonus.Deltas.Add(_Enhanced);
            }

            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            _Enhanced.DoTerminate();
            base.OnDeactivate(source);
        }
    }

    public static class MagicWeaponAdjunctHelper
    {
        public static bool IsMagicWeaponActive(this IAdjunctSet self)
            => self.HasActiveAdjunct<MagicWeaponAdjunct>();
    }
}
