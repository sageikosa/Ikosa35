using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    // TODO: make a version that targets a creature for a shorter duration (such as to suppress aurification...)
    [Serializable]
    public class MagicalAura : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Magical Aura";
        public override string Description => @"Make an object appear magical, or hide its magical character.";
        public override MagicStyle MagicStyle => new Illusion(Illusion.SubIllusion.Glamer);

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new FocusComponent();
            }
        }
        #endregion

        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        #region ISpellMode Members

        #region public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Target", @"Item to Aurify", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One,
                new MeleeRange(), new ObjectTargetType());
            yield return new OptionAim(@"Style", @"Aura Style", true, FixedRange.One, FixedRange.One, MagicStyleOptions);
            yield return new OptionAim(@"Strength", @"Aura Strength", true, FixedRange.One, FixedRange.One, AuraStrengthOptions);
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> MagicStyleOptions { get; }
        private IEnumerable<OptionAimOption> MagicStyleOptions
        {
            get
            {
                yield return new OptionAimOption() { Key = @"Evocation", Description = @"Energy manipulation", Name = @"Evocation" };
                yield return new OptionAimOption() { Key = @"Conjuration", Description = @"Materializations", Name = @"Conjuration" };
                yield return new OptionAimOption() { Key = @"Enchantment", Description = @"Mental influence", Name = @"Enchantment" };
                yield return new OptionAimOption() { Key = @"General", Description = @"General", Name = @"General" };
                yield return new OptionAimOption() { Key = @"Illusion", Description = @"Illusions", Name = @"Illusion" };
                yield return new OptionAimOption() { Key = @"Necromancy", Description = @"Life and death manipulation", Name = @"Necromancy" };
                yield return new OptionAimOption() { Key = @"Divination", Description = @"Mystic knowledge", Name = @"Divination" };
                yield return new OptionAimOption() { Key = @"Transformation", Description = @"Alterations", Name = @"Transformation" };
                yield return new OptionAimOption() { Key = @"Abjuration", Description = @"Protection and defense", Name = @"Abjuration" };
                yield break;
            }
        }
        #endregion

        #region private IEnumerable<OptionAimOption> AuraStrengthOptions { get; }
        private IEnumerable<OptionAimOption> AuraStrengthOptions
        {
            get
            {
                yield return new OptionAimValue<AuraStrength>() { Value = AuraStrength.None, Key = @"None", Description = @"Hides auras", Name = @"None" };
                yield return new OptionAimValue<AuraStrength>() { Value = AuraStrength.Faint, Key = @"Faint", Description = @"Just a twinge", Name = @"Faint" };
                yield return new OptionAimValue<AuraStrength>() { Value = AuraStrength.Moderate, Key = @"Moderate", Description = @"Mid-range power level", Name = @"Moderate" };
                yield return new OptionAimValue<AuraStrength>() { Value = AuraStrength.Strong, Key = @"Strong", Description = @"Seems to be high-level", Name = @"Strong" };
                yield return new OptionAimValue<AuraStrength>() { Value = AuraStrength.Overwhelming, Key = @"Overwhelming", Description = @"Overwhelming", Name = @"Overwhelming" };
                yield break;
            }
        }
        #endregion

        public bool AllowsSpellResistance => false;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Target", 0);
        }

        #region public void ApplySpell(PowerApplyStep<SpellSource> apply)
        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            CopyActivityTargetsToSpellEffects(apply);

            // apply fully targetted spell effect
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #endregion

        #region IDurableMode Members

        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        #region public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                if ((_spellEffect.AllTargets.Where(_t => _t.Key.Equals(@"Style")).FirstOrDefault() is OptionTarget _style)
                    && (_spellEffect.AllTargets.Where(_t => _t.Key.Equals(@"Strength")).FirstOrDefault() is OptionTarget _strength))
                {
                    AuraStrength _aStr = ((OptionAimValue<AuraStrength>)_strength.Option).Value;
                    MagicStyle _mStyle = null;
                    switch (_style.Option.Key)
                    {
                        case @"Evocation":
                            _mStyle = new Evocation();
                            break;

                        case @"Conjuration":
                            _mStyle = new Conjuration(Conjuration.SubConjure.Summoning);
                            break;

                        case @"Enchantment":
                            _mStyle = new Enchantment(Enchantment.SubEnchantment.Charm);
                            break;

                        case @"General":
                            _mStyle = new General();
                            break;

                        case @"Illusion":
                            _mStyle = new Illusion(Illusion.SubIllusion.Figment);
                            break;

                        case @"Necromancy":
                            _mStyle = new Necromancy();
                            break;

                        case @"Divination":
                            _mStyle = new Divination(Divination.SubDivination.RemoteSensing);
                            break;

                        case @"Transformation":
                            _mStyle = new Transformation();
                            break;

                        case @"Abjuration":
                            _mStyle = new Abjuration();
                            break;
                    }
                    if (_mStyle != null)
                    {
                        var _mAura = new MagicalAuraAdjuct(_spellEffect.MagicPowerActionSource, _mStyle, _aStr);
                        target.AddAdjunct(_mAura);
                        return _mAura;
                    }
                }
            }
            return null;
        }
        #endregion

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is MagicSourceAuraAdjunct _mAdj)
            {
                target.RemoveAdjunct(_mAdj);
            }
        }

        public bool IsDismissable(int subMode) => true;
        public DurationRule DurationRule(int subMode) => new DurationRule(DurationType.Span, new SpanRulePart(1, new Day(), 1));
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) => string.Empty;
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        #endregion
    }

    /// <summary>
    /// Used to express an aura based on parameters (rather than a specific spell-source)
    /// </summary>
    [Serializable]
    public class MagicalAuraAdjuct : MagicSourceAuraAdjunct, IMagicAura
    {
        public MagicalAuraAdjuct(MagicPowerActionSource source, MagicStyle magicStyle, AuraStrength strength)
            : base(source)
        {
            _MStyle = magicStyle;
            _Strength = strength;
        }

        #region data
        private MagicStyle _MStyle;
        private AuraStrength _Strength;
        #endregion

        public override AuraStrength MagicStrength => _Strength;
        public override MagicStyle MagicStyle => _MStyle;

    }
}
