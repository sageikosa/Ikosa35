using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Core.Dice;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class UnknownToUndead : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Unknown to Undead";
        public override string Description => @"Undead cannot observe the warded subjects.";
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

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Warded Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, new PowerLevelRange(1), new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance
            => true;

        public bool IsHarmless
            => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);

            // get save information
            var _spellSave = deliver.PowerUse.CapabilityRoot.GetCapability<ISaveCapable>();
            var _save = _spellSave.GetSaveMode(deliver.Actor, deliver.PowerUse?.PowerActionSource,
                new Interaction(deliver.Actor, deliver.PowerUse.PowerActionSource, null, null), @"Save.Will");

            // bind the group master adjunct to the actor, track in activity targets for application
            var _master = new UnknownToUndeadGroup(deliver.PowerUse.PowerActionSource, new ImmunityTracker(),
                _save.Difficulty);
            deliver.TargetingProcess.Targets.Add(new ValueTarget<UnknownToUndeadGroup>(@"Master", _master));
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // make sure the SpellEffect has all the target information (master adjunct)
            var _feedback = apply.DeliveryInteraction.Feedback.OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
            ((MagicPowerEffectTransit<SpellSource>)_feedback.PowerTransit).MagicPowerEffects.First().AllTargets.Add(apply.TargetingProcess.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Master")));

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
                // add the member adjunct
                var _group = _spellEffect.GetTargetValue<UnknownToUndeadGroup>(@"Master");
                if (_group != null)
                {
                    // create the member adjunct
                    var _unknown = new UnknownToUndeadAdjunct(_spellEffect.MagicPowerActionSource, _group);
                    target.AddAdjunct(_unknown);
                    return _unknown;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // remove the member adjunct
            if (source.ActiveAdjunctObject is UnknownToUndeadAdjunct _unknown)
            {
                // remove member adjunct
                _unknown.Eject();
            }
        }

        public bool IsDismissable(int subMode)
            => true;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
        {
            // targets do not need to save, but undead trying to observe the warded creature do
            return string.Empty;
        }

        #endregion

        #region ISpellSaveMode Members

        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            // intelligent undead save according to this rule (add as a target to the durable mode)
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }

        #endregion

    }

    [Serializable]
    public class UnknownToUndeadGroup : ReactorSideEffectGroup
    {
        #region construction
        public UnknownToUndeadGroup(object source, ImmunityTracker tracker, DeltaCalcInfo difficulty)
            : base(source)
        {
            _Tracker = tracker;
            _Difficulty = difficulty;
        }
        #endregion

        #region state
        private ImmunityTracker _Tracker;
        private DeltaCalcInfo _Difficulty;
        #endregion

        public ImmunityTracker Tracker => _Tracker;
        public DeltaCalcInfo Difficulty => _Difficulty;
    }


    [Serializable]
    public class UnknownToUndeadAdjunct : GroupParticipantAdjunct, IInteractHandler, ICanReactBySideEffect
    {
        public UnknownToUndeadAdjunct(MagicPowerActionSource source, UnknownToUndeadGroup group)
            : base(source, group)
        {
        }

        public UnknownToUndeadGroup UnknownToUndeadGroup
            => Group as UnknownToUndeadGroup;

        public SpellSource SpellSource
            => Source as SpellSource;

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

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet.InteractData is Observe _obs
                && _obs.Viewer is Creature _viewer
                && _viewer.CreatureType is UndeadType)  // undead observer...?
            {
                if (workSet.Target is ICoreObject _target)
                {
                    if (_viewer.Abilities.Intelligence.IsNonAbility)
                    {
                        // non-intelligent undead always affected
                        workSet.Feedback.Add(new UnderstoodFeedback(this));
                        return;
                    }
                    else
                    {
                        // determine whether already tracking this intelligent undead
                        if (!UnknownToUndeadGroup.Tracker.IsTracking(_viewer.ID))
                        {
                            // check whether possible to use effect by planar compatibility and distance
                            var _tPlanar = _obs.GetTargetLocator(_target)?.PlanarPresence ?? PlanarPresence.None;
                            var _distance = _obs.GetDistance(_target);

                            // could senses detect?
                            if (_viewer.Senses.AllSenses.Any(_s
                                => _s.IsActive
                                && (_distance <= _s.Range)
                                && _s.PlanarPresence.HasOverlappingPresence(_tPlanar)))
                            {
                                // sense could (possibly) detect, track immunity/affect
                                var _check = Deltable.GetCheckNotify(_viewer.ID, @"Will Save", Guid.Empty, @"Difficulty");
                                _check.OpposedInfo = UnknownToUndeadGroup.Difficulty;

                                // requires an auto-save to proceed
                                if ((_viewer.WillSave.QualifiedValue(
                                    new Interaction(workSet.Actor, Source as SpellSource, null, null), _check.CheckInfo)
                                    + DieRoller.RollDie(_viewer.ID, 20, $@"Will {_viewer.Name}", @"Unknown to Undead"))
                                    >= UnknownToUndeadGroup.Difficulty.Result)
                                {
                                    // save exceeds difficulty
                                    UnknownToUndeadGroup.Tracker.TrackImmunity(_viewer.ID);
                                }
                                else
                                {
                                    // affected viewer cannot observe target
                                    UnknownToUndeadGroup.Tracker.TrackAffect(_viewer.ID);
                                    workSet.Feedback.Add(new UnderstoodFeedback(this));
                                    return;
                                }
                            }
                        }
                        else if (!UnknownToUndeadGroup.Tracker.Info(_viewer.ID).Immune)
                        {
                            // affected viewer cannot observe target
                            workSet.Feedback.Add(new UnderstoodFeedback(this));
                            return;
                        }
                    }
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(Observe);
            yield return typeof(SearchData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // this gets added to the top of the chain, so we don't waste time transitting if we don't need to...
            return true;
        }

        #endregion

        #region ICanReact Members

        private void EndHideFromUndead()
        {
            // snapshot member adjuncts
            var _allMembers = UnknownToUndeadGroup.Members.ToArray();
            foreach (var _memb in _allMembers)
            {
                // for each, get the durable spell effect enabling it on the anchor
                (from _adj in _memb.Anchor.Adjuncts.OfType<DurableMagicEffect>()
                 where _adj.Source == _memb.Source
                 select _adj).FirstOrDefault()?.Eject();
            }
        }

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            // see if this is an activity performed by the Anchor
            if ((process is CoreActivity _activity) && (_activity.Actor == Anchor))
            {
                if (_activity.Action is ActionBase _action)
                {
                    // TODO: filter out "harmful" to an unattended object (as it doesn't count)
                    if (!_action.IsHarmless)
                    {
                        // harmful action (not just targeting undead) ends the adjunct
                        EndHideFromUndead();
                    }
                    else if (_action is DriveCreature)
                    {
                        // derived from drive undead (repel, reinforce or dispel repel) ends the adjunct
                        var _drive = _action as DriveCreature;
                        // TODO: must be keyed to undead...
                        EndHideFromUndead();
                    }
                    else if ((from _atk in _activity.Targets.OfType<AttackTarget>()
                              let _critter = _atk.Target as Creature
                              where (_critter != null)
                              && _critter.CreatureType is UndeadType
                              && _atk.Attack is MeleeAttackData
                              select _atk).Any())
                    {
                        // melee touching an undead ends the adjunct (even if the action is "harmless")
                        EndHideFromUndead();
                    }
                }
            }
        }

        public bool IsFunctional => IsActive;

        #endregion

        public override object Clone()
            => new UnknownToUndeadAdjunct(SpellSource, UnknownToUndeadGroup);
    }
}
