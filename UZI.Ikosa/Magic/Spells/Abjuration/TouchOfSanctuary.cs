using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class TouchOfSanctuary : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Touch of Santuary";
        public override string Description => @"Cannot be directly attacked, but cannot attack either.";
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

        #region public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal, 20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }
        #endregion

        public bool AllowsSpellResistance
            => false;

        public bool IsHarmless
            => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // save mode used difficulty as target information for sanctuary adjunct
            var _spellSave = apply.PowerUse.CapabilityRoot.GetCapability<ISaveCapable>();
            var _save = _spellSave.GetSaveMode(apply.Actor, apply.PowerUse?.PowerActionSource,
                apply.DeliveryInteraction, @"Save.Will");
            var _difficulty = new ValueTarget<DeltaCalcInfo>(@"Difficulty", _save.Difficulty);

            // tracker to persist across deactivation/re-activation cycles (pockets of anti-magic)
            var _tracker = new ValueTarget<ImmunityTracker>(@"Tracker", new ImmunityTracker());

            // make sure the SpellEffect has all the target information
            var _feedback = apply.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
            ((MagicPowerEffectTransit<SpellSource>)_feedback.PowerTransit).MagicPowerEffects.First().AllTargets.Add(_difficulty);
            ((MagicPowerEffectTransit<SpellSource>)_feedback.PowerTransit).MagicPowerEffects.First().AllTargets.Add(_tracker);

            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        #region IDurableMode Members

        #region public IEnumerable<int> DurableSubModes { get; }
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                // get calculated or persistent info
                var _difficulty = _spellEffect.GetTargetValue<DeltaCalcInfo>(@"Difficulty");
                var _tracker = _spellEffect.GetTargetValue<ImmunityTracker>(@"Tracker");

                // setup and anchor the adjunct
                var _located = target.GetLocated();
                if (_located != null)
                {
                    var _group = new ReactorSideEffectGroup(_spellEffect.MagicPowerActionSource);
                    var _sanct = new SanctuaryAdjunct(_spellEffect.MagicPowerActionSource, _group, _difficulty, _tracker);
                    target.AddAdjunct(_sanct);
                    return _sanct;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as SanctuaryAdjunct)?.Eject();
        }

        public bool IsDismissable(int subMode)
            => false;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
        {
            // will save for creatures trying to attack the warded creature
            return string.Empty;
        }

        #endregion

        #region ISpellSaveMode Members

        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
            => new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));

        #endregion
    }

    [Serializable]
    public class SanctuaryAdjunct : GroupParticipantAdjunct, ICanReactBySuppress, ICanReactBySideEffect, IMonitorChange<bool>
    {
        #region ctor()
        public SanctuaryAdjunct(object source, AdjunctGroup group, DeltaCalcInfo difficulty, ImmunityTracker tracker)
            : base(source, group)
        {
            _Difficulty = difficulty;
            _Tracker = tracker;
        }
        #endregion

        #region data
        private ImmunityTracker _Tracker;
        private DeltaCalcInfo _Difficulty;
        #endregion

        public DeltaCalcInfo Difficulty => _Difficulty;
        public ImmunityTracker Tracker => _Tracker;

        #region ICanReact Members

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            if (process is CoreActivity _activity)
            {
                // TODO: filter out "harmful" to an unattended object (as it doesn't count)
                if (!(_activity.Action as ActionBase)?.IsHarmless ?? true)
                {
                    if (_activity.Actor == Anchor)
                    {
                        // activity initiated by the anchor of the adjunct
                        (from _dme in Anchor.Adjuncts.OfType<DurableMagicEffect>()
                         where _dme.Source == Source
                         select _dme).FirstOrDefault()?.Eject();
                    }
                }
            }
        }

        public void ReactToProcessBySuppress(CoreProcess process)
        {
            if (process is CoreActivity _activity)
            {
                if ((_activity.Action is ActionBase _act)
                    && !_act.IsHarmless)
                {
                    // harmful activity explicitly targetting the anchor
                    if (_activity.Targets.Any(_t => _t.Target == Anchor))
                    {
                        // see if the actor is already in the tracker list ...
                        if (Tracker.IsTracking(_activity.Actor.ID))
                        {
                            if (!Tracker.Info(_activity.Actor.ID).Immune)
                            {
                                // if not immune (has been affected), terminate
                                _activity.IsActive = false;
                            }
                        }
                        else
                        {
                            // otherwise, inject reactive step with a save prerequisite, targetting the actor
                            // NOTE: the bool being monitored is the ReactivePrerequisiteStep success flag
                            var _save = new SavePrerequisite(this,
                                new Interaction(null, Source, _activity.Actor, null), @"Save.Will", @"Will Save",
                                new SaveMode(SaveType.Will, SaveEffect.Negates, Difficulty));
                            var _react = new ReactivePrerequisiteStep(process, _save);
                            _react.AddChangeMonitor(this);
                        }
                    }
                }
            }
        }

        public bool IsFunctional => true;

        #endregion

        #region IMonitorChange<bool> Members
        // NOTE: the bool being monitored is the ReactivePrerequisiteStep success flag
        public void PreTestChange(object sender, AbortableChangeEventArgs<bool> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<bool> args) { }
        public void ValueChanged(object sender, ChangeValueEventArgs<bool> args)
        {
            if (sender is ReactivePrerequisiteStep _step)
            {
                var _id = _step.DispensedPrerequisites.First().Qualification.Target.ID;
                if (args.NewValue)
                {
                    // if the save failed, track it (ReactivePrerequisiteStep has already failed the process)
                    Tracker.TrackAffect(_id);
                }
                else
                {
                    // if the save succeeded, track it
                    Tracker.TrackImmunity(_id);
                }
            }
        }
        #endregion

        public override object Clone()
            => new SanctuaryAdjunct(Source, Group, Difficulty, Tracker);
    }
}
