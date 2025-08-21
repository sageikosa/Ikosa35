using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public abstract class ProtectAgainstAlignmentBase : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override MagicStyle MagicStyle { get { return new Abjuration(); } }

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

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield return new AutoSpellResistanceAim(@"SR");
            yield break;
        }

        public bool AllowsSpellResistance { get { return false; } }
        public bool IsHarmless { get { return true; } }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // make sure the SpellEffect has all the target information
            var _feedback = apply.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
            var _targets = ((MagicPowerEffectTransit<SpellSource>)_feedback.PowerTransit).MagicPowerEffects.First().AllTargets;
            _targets.Add(apply.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"SR")));

            // tracker to persist across deactivation/re-activation cycles
            var _tracker = new ValueTarget<ImmunityTracker>(@"Tracker", new ImmunityTracker());
            _targets.Add(_tracker);

            // other bits of information: blocking and alignment
            _targets.Add(new ValueTarget<bool>(@"Blocking", true));
            _targets.Add(new ValueTarget<Alignment>(@"Alignment", WardedAlignment()));

            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        protected abstract Alignment WardedAlignment();

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
                var _targets = _spellEffect.AllTargets;
                // get calculated or persistent info
                var _tracker = _spellEffect.GetTargetValue<ImmunityTracker>(@"Tracker");
                var _sr = _spellEffect.GetTargetValue<int>(@"SR", 0);

                // setup and anchor the adjunct
                var _located = target.GetLocated();
                if (_located != null)
                {
                    var _group = new ReactorSideEffectGroup(_spellEffect.MagicPowerActionSource);
                    var _align = _targets.FirstOrDefault(_t => _t.Key.Equals(@"Alignment")) as ValueTarget<Alignment>;
                    var _block = _targets.FirstOrDefault(_t => _t.Key.Equals(@"Blocking")) as ValueTarget<bool>;

                    // create, bind, return
                    var _protect = new AlignmentProtectionAdjunct(source, _group, _align.Value, _tracker, _sr, _block);
                    target.AddAdjunct(_protect);
                    return _protect;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if (source.ActiveAdjunctObject is AlignmentProtectionAdjunct _protect)
            {
                _protect.Eject();
            }
        }

        public bool IsDismissable(int subMode) { return true; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) { return @"Save.Will"; }
        public DurationRule DurationRule(int subMode) { return new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1)); }

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

    /// <summary>
    /// Applies all effects for protection against alignment
    /// </summary>
    [Serializable]
    public class AlignmentProtectionAdjunct : GroupParticipantAdjunct, ICanReactBySuppress, ICanReactBySideEffect
    {
        #region construction
        public AlignmentProtectionAdjunct(object source, ReactorSideEffectGroup group, Alignment align, ImmunityTracker tracker, int magicResistance,
            ValueTarget<bool> blocking)
            : base(source, group)
        {
            _Align = align;
            _Deflect = new DeflectionAgainstAlignment(align);
            _Resist = new ResistanceAgainstAlignment(align);
            _Tracker = tracker;
            _SR = magicResistance;
            _Blocking = blocking;
        }
        #endregion

        #region state
        private ValueTarget<bool> _Blocking;
        private Alignment _Align;
        private int _SR;
        private IQualifyDelta _Deflect;
        private IQualifyDelta _Resist;
        private ImmunityTracker _Tracker;
        #endregion

        public ImmunityTracker Tracker => _Tracker;
        public int SpellResistance => _SR;
        public bool Blocking => _Blocking.Value;

        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                _critter.NormalArmorRating.Deltas.Add(_Deflect);
                _critter.TouchArmorRating.Deltas.Add(_Deflect);
                _critter.IncorporealArmorRating.Deltas.Add(_Deflect);
                _critter.WillSave.Deltas.Add(_Resist);
                _critter.FortitudeSave.Deltas.Add(_Resist);
                _critter.ReflexSave.Deltas.Add(_Resist);
                // TODO: suppress mental possession and control
            }
        }

        protected override void OnDeactivate(object source)
        {
            _Deflect.DoTerminate();
            _Resist.DoTerminate();
            // TODO: deactivate mental possession and control suppression
        }

        #region ICanReact Members

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            if (!Blocking)
            {
                return;
            }

            if (process is CoreActivity _activity)
            {
                if (!((_activity.Action as ActionBase)?.IsHarmless ?? true))
                {
                    if (_activity.Actor == Anchor)
                    {
                        // if targetting an aligned summoned creature, turn the blocking off
                        if ((from _crt in _activity.Targets.OfType<Creature>()
                             where _crt.HasAdjunct<Summoned>() && _crt.Alignment.IsMatchingAxial(_Align)
                             select _crt).Any())
                        {
                            // deflection and resistance continue, summoned blocking stops
                            _Blocking.Value = false;
                        }
                    }
                }
            }
        }

        public bool IsFunctional
            => IsActive;

        public void ReactToProcessBySuppress(CoreProcess process)
        {
            if (!Blocking)
            {
                return;
            }

            if (process is CoreActivity _activity)
            {
                if ((_activity.Action is ActionBase _act)
                    && !_act.IsHarmless)
                {
                    // harmful activity explicitly targetting the anchor
                    if (_activity.Targets.Any(_t => _t.Target == Anchor))
                    {
                        // summoned creature alignable with the warded effect
                        if ((_activity.Actor is Creature _critter)
                            && _critter.HasAdjunct<Summoned>()
                            && _critter.Alignment.IsMatchingAxial(_Align))
                        {
                            // melee strike or melee touch with a natural weapon
                            if ((_act is MeleeStrike)
                                && (_act as ISupplyAttackAction)?.Attack?.Weapon is NaturalWeapon)
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
                                    // if the previously deteremined check equals or exceeds the critters'
                                    if (SpellResistance >= _critter.SpellResistance.EffectiveValue)
                                    {
                                        // critter is affected (and activity terminates)
                                        Tracker.TrackAffect(_critter.ID);
                                        _activity.IsActive = false;
                                    }
                                    else
                                    {
                                        // otherwise, not
                                        Tracker.TrackImmunity(_critter.ID);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        public override object Clone()
            => new AlignmentProtectionAdjunct(Source, Group as ReactorSideEffectGroup, _Align, Tracker, _SR, _Blocking);
    }

    /// <summary>
    /// Deflection bonus
    /// </summary>
    [Serializable]
    public class DeflectionAgainstAlignment : IQualifyDelta
    {
        public DeflectionAgainstAlignment(Alignment align)
        {
            _Terminator = new TerminateController(this);
            _Align = align;
            _Delta = new QualifyingDelta(2, typeof(Deflection));
        }

        private Alignment _Align;
        private IDelta _Delta;

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify.Actor is Creature _critter)
                && _critter.Alignment.IsMatchingAxial(_Align))
            {
                yield return _Delta;
            }
            yield break;
        }

        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }

    /// <summary>
    /// Resistance bonus
    /// </summary>
    [Serializable]
    public class ResistanceAgainstAlignment : IQualifyDelta
    {
        public ResistanceAgainstAlignment(Alignment align)
        {
            _Terminator = new TerminateController(this);
            _Align = align;
            _Delta = new QualifyingDelta(2, typeof(Uzi.Ikosa.Deltas.Resistance));
        }

        private Alignment _Align;
        private IDelta _Delta;

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if ((qualify.Actor is Creature _critter)
                && _critter.Alignment.IsMatchingAxial(_Align))
            {
                yield return _Delta;
            }
            yield break;
        }

        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }
}
