using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Descriptions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ReadMagic : SpellDef, ISpellMode, IDurableCapable
    {
        public override string DisplayName => @"Read Magic";
        public override string Description => @"Unlock magical writings.";
        public override MagicStyle MagicStyle => new Divination(Divination.SubDivination.Illumination);

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
            yield return new PersonalAim(@"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance
            => false;

        public bool IsHarmless
            => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.Targets[0], 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableSpellMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _effect = new ReadMagicEffect((source as DurableMagicEffect).MagicPowerActionSource);
            if (target is Creature _critter)
            {
                _critter.AddAdjunct(_effect);
            }
            return _effect;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if ((((DurableMagicEffect)source).ActiveAdjunctObject is ReadMagicEffect _effect)
                && (target is Creature _critter))
            {
                _critter.RemoveAdjunct(_effect);
            }
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
    public class ReadMagicEffect : Adjunct, IActionProvider, ICore
    {
        public ReadMagicEffect(IActionSource source)
            : base(source)
        {
        }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Add(this, this);
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.Actions.Providers.Remove(this);
            }
        }
        #endregion

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            var _canSeeSelf = true;
            if (budget.Actor is Creature _critter)
            {
                var _loc = (_critter.GetLocated().Locator.Map.ContextSet.FindAll(_critter).FirstOrDefault() as Locator);
                LightRange _level = _loc.LightLevel;
                _canSeeSelf = _critter.Senses.AllSenses
                    .Any(_s => _s.IsActive && _s.UsesSight && _s.WorksInLightLevel(_level));
            }

            if (_canSeeSelf && _budget.CanPerformTotal)
            {
                // if insufficient lighting for the creature to read, the first two must be suppressed
                yield return new ReadMagicSpellBook(Source as IActionSource, @"100");
                yield return new ReadMagicInObjectLoad(Source as IActionSource, @"101");
            }

            // NOTE: this last depends on the creature's awareness, which automatically handles lighting
            if (_budget.CanPerformRegular)
            {
                yield return new ReadMagicMark(Source as IActionSource, @"102");
            }

            yield break;
        }
        #endregion

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Read Magic", ID);

        public override object Clone()
            => new ReadMagicEffect(Source as IActionSource);
    }

    #region public class ReadMagicSpellBook : ActionBase { ... }
    /// <summary>
    /// Identify a spell written in another spellbook [ActionBase (Span: Minute)]
    /// </summary>
    public class ReadMagicSpellBook : ActionBase
    {
        /// <summary>
        /// Identify a spell written in another spellbook [ActionBase (Span: Minute)]
        /// </summary>
        public ReadMagicSpellBook(IActionSource source, string orderKey)
            : base(source, new ActionTime(Minute.UnitFactor), true, false, orderKey)
        {
        }

        public override string Key => @"ReadMagic.SpellBook";
        public override string DisplayName(CoreActor actor) => @"Read spell from spell book";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Read", activity.Actor, observer);

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            if ((activity.Targets[0] as OptionTarget).Option is OptionAimValue<BookSpell> _spell)
            {
                _spell.Value.Decipher(activity.Actor.ID);
                return activity.GetActivityResultNotifyStep(_spell.Value.SpellDef.DisplayName);
            }
            else
            {
                activity.IsActive = false;
                return null;
            }
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"BookSpell", @"Spell", true, FixedRange.One, FixedRange.One, AvailableSpells(activity));
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> AvailableSpells(CoreActivity activity)
        private IEnumerable<OptionAimOption> AvailableSpells(CoreActivity activity)
        {
            // get all spell books within object load
            foreach (var _book in activity.Actor.ObjectLoad.OfType<SpellBook>())
            {
                // get all spells
                foreach (var _spell in _book.UnDecipheredSpells(activity.Actor.ID))
                {
                    yield return new OptionAimValue<BookSpell>()
                    {
                        Value = _spell,
                        Key = _spell.SpellDef.DisplayName,
                        Description = $@"Description: {_spell.SpellDef.Description}",
                        Name = $@"Spell {_spell.SpellDef.DisplayName} (Level {_spell.Level})"
                    };
                }
            }
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
    #endregion

    #region public class ReadMagicInObjectLoad : ActionBase { ... }
    /// <summary>
    /// Identify decipherable magic writing [ActionBase (Total)]
    /// </summary>
    public class ReadMagicInObjectLoad : ActionBase
    {
        #region Construction
        /// <summary>
        /// Identify decipherable magic writing [ActionBase (Total)]
        /// </summary>
        public ReadMagicInObjectLoad(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Total), true, false, orderKey)
        {
        }
        #endregion

        public override string Key => @"ReadMagic.SpellCompletion";
        public override string DisplayName(CoreActor actor) => @"Read magic from spell completion item";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Read", activity.Actor, observer);

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        /// <summary>
        /// Deciphers decipherable magic
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _decipher = ((activity.Targets[0] as OptionTarget).Option as OptionAimValue<IDecipherable>).Value;
            return _decipher.Decipher(activity);
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"DecipherMagic", @"Magic", true, FixedRange.One, FixedRange.One, AvailableMagic(activity));
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> AvailableMagic(Activity activity)
        private IEnumerable<OptionAimOption> AvailableMagic(CoreActivity activity)
        {
            // in cases dealing with invisible magic marks
            var _seeInvis = false;
            if (activity.Actor is Creature _critter)
            {
                var _loc = _critter.GetLocated().Locator;
                var _level = _loc.LightLevel;
                _seeInvis = _critter.Senses.AllSenses
                    .Any(_s => _s.IgnoresInvisibility && _s.IsActive && _s.UsesSight && _s.WorksInLightLevel(_level));
            }

            // get all spell completions on all objects within load 
            var _sx = 0;
            foreach (var _lister in from _obj in activity.Actor.ObjectLoad
                                    from _adj in _obj.GetDecipherables()
                                    where !_adj.HasDeciphered(activity.Actor.ID)
                                    select new { Anchor = _obj, Ciphered = _adj as IDecipherable })
            {
                if (!(_lister.Ciphered is IVisible _visible)
                    || _visible.IsVisible
                    || _seeInvis)
                {
                    yield return new OptionAimValue<IDecipherable>()
                    {
                        Value = _lister.Ciphered,
                        Key = _sx.ToString(),
                        Description = $@"{_lister.Anchor.Name}: {_sx}",
                        Name = $@"{_lister.Anchor.Name}: {_sx}"
                    };
                }
                _sx++;
            }
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
    #endregion

    #region public class ReadMagicMark : ActionBase, ITargetType
    /// <summary>
    /// Read short symbols, glyphs or marks [ActionBase (Regular)]
    /// </summary>
    public class ReadMagicMark : ActionBase, ITargetType
    {
        /// <summary>
        /// Read short symbols, glyphs or marks [ActionBase (Regular)]
        /// </summary>
        public ReadMagicMark(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
        }

        public override string Key => @"ReadMagic.Rune";
        public override string DisplayName(CoreActor actor) => @"Read magic rune";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Read", activity.Actor, observer);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _decipher = ((activity.Targets[0] as OptionTarget).Option as OptionAimValue<IDecipherable>).Value;
            return _decipher.Decipher(activity);
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AwarenessAim(@"Mark", @"Mark to Decipher", FixedRange.One, FixedRange.One, new MediumRange(), (ITargetType)this);
            yield break;
        }

        #region ITargetType Members
        public bool ValidTarget(ICore iCore)
            => (iCore as IDecipherable) != null;

        public TargetTypeInfo ToTargetTypeInfo()
            => new TargetTypeInfo();
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
    #endregion
}
