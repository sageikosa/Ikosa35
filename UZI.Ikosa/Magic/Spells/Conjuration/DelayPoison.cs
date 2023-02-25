using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class DelayPoison : SpellDef, ISpellMode, ISaveCapable, IDurableCapable
    {
        public override MagicStyle MagicStyle => new Conjuration(Conjuration.SubConjure.Healing);
        public override string DisplayName => @"Delay Poison";
        public override string Description => @"The effects of poison are held in abatement until the spell ends, including the need to make saving rolls.  Initial exposures are released serially, once per round in order of exposure.  Secondary effect time countdowns are paused for the duration of the spell.";

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

        #region ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, new FixedRange(1), new FixedRange(1), new MeleeRange(), new CreatureTargetType());
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

        #region ISaveMode
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _delay = new DelayPoisonEffect(source);
            (target as Creature)?.AddAdjunct(_delay);
            return _delay;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source?.ActiveAdjunctObject as DelayPoisonEffect)?.Eject();
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
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Fortitude";

        DurationRule IDurableCapable.DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Hour(), 1));
        #endregion
    }

    [Serializable]
    public class DelayPoisonEffect : Adjunct, IInteractHandler
    {
        public DelayPoisonEffect(object source)
            : base(source)
        {
            _Poisons = new List<Poisoned>();
        }

        #region data
        private List<Poisoned> _Poisons;
        private double _DelayBaseTime;
        #endregion

        public List<Poisoned> Poisons => _Poisons;
        public double DelayBaseTime => _DelayBaseTime;

        public override object Clone()
            => new DelayPoisonEffect(Source);

        #region OnActivate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // intercept future poisons
                _critter.AddIInteractHandler(this);

                // get delay time
                _DelayBaseTime = _critter?.GetCurrentTime() ?? 0d;

                // abate current poisons
                foreach (var _poisoned in _critter.Adjuncts.OfType<Poisoned>().ToList())
                {
                    _poisoned.Eject();
                    _Poisons.Add(_poisoned);
                }
            }
            base.OnActivate(source);
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // unhook capture (don't want it to interfere with the rest of this)
                _critter.RemoveIInteractHandler(this);

                // release poisons already progressed to secondary
                var _time = _critter?.GetCurrentTime() ?? 0d;
                foreach (var _poisoned in _Poisons.Where(_p => _p.PrimaryDone).ToList())
                {
                    // take out of list
                    _Poisons.Remove(_poisoned);

                    // update secondary time
                    _poisoned.SecondaryTime += _time - _DelayBaseTime;

                    // reapply
                    _critter.AddAdjunct(_poisoned);
                }

                if (_Poisons.Any())
                {
                    // first (or only) gets released immediately
                    var _first = _Poisons.FirstOrDefault();
                    _Poisons.Remove(_first);
                    _critter.AddAdjunct(_first);

                    if (_Poisons.Any())
                    {
                        // more than 1, release in subsequent rounds, one by one
                        _critter.AddAdjunct(new DelayPoisonRelease(_Poisons.Select(_p => _p), _time + Round.UnitFactor));
                        _Poisons.Clear();
                    }
                }
            }
            base.OnDeactivate(source);
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is AddAdjunctData _addAdjunct
                && _addAdjunct.Adjunct is Poisoned _poison)
            {
                // intercept poisoned for later
                _Poisons.Add(_poison);

                // all done processing
                workSet.Feedback.Add(new UnderstoodFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // blockers only block, so they might keep poisoned from even getting here
            if (interactType.Equals(typeof(MultiAdjunctBlocker)))
                return false;
            return true;
        }
    }

    [Serializable]
    public class DelayPoisonRelease : Adjunct, ITrackTime
    {
        public DelayPoisonRelease(IEnumerable<Poisoned> poisons, double nextTime)
            : base(typeof(DelayPoison))
        {
            _NextTime = nextTime;
            _Poisons = poisons.ToList();
        }

        #region data
        private double _NextTime;
        private List<Poisoned> _Poisons;
        #endregion

        public double NextTime => _NextTime;
        public List<Poisoned> Poisons
            => _Poisons.ToList();

        public override object Clone()
            => new DelayPoisonRelease(Poisons, _NextTime);

        public double Resolution => Round.UnitFactor;

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextTime) && (direction == TimeValTransition.Entering))
            {
                // release
                var _poison = _Poisons.FirstOrDefault();
                _Poisons.Remove(_poison);
                (Anchor as Creature)?.AddAdjunct(_poison);

                // next time
                _NextTime += Round.UnitFactor;

                if (!_Poisons.Any())
                {
                    // eject when done
                    Eject();
                }
            }
        }
    }
}
