using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RemoveFear : SpellDef, ISpellMode, IDurableCapable, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Remove Fear";
        public override string Description => @"+4 Morale versus fear; suppresses fear effect";
        public override MagicStyle MagicStyle => new Abjuration();

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
            var _max = new LinearRange(1, 1, 4);
            yield return new AwarenessAim(@"Creature", @"Creature",
                FixedRange.One, _max, new NearRange(), new CreatureTargetType());
            yield break;
        }
        #endregion

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _targets = (from _aware in deliver.TargetingProcess.Targets.OfType<AwarenessTarget>()
                            where _aware.Key.Equals(@"Creature", StringComparison.OrdinalIgnoreCase)
                            select _aware).Cast<AimTarget>().ToList();
            if (_targets.Any())
            {
                SpellDef.DeliverDurableInCluster(deliver, _targets, true, 30, 0);
            }
            else
            {
                deliver.Notify(@"No targets within spell capacity.", @"Failed", false);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
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
            => new RemoveFearEffect(this);

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as RemoveFearEffect)?.Eject();

        public bool IsDismissable(int subMode) => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
        {
            // NOTE: 10 minute effect...
            return new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute()));
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(CauseFear);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion
    }

    [Serializable]
    public class RemoveFearEffect : Adjunct, IQualifyDelta, IMonitorChange<Activation>
    {
        #region ctor()
        public RemoveFearEffect(object source)
            : base(source)
        {
            _RmvFear = new QualifyingDelta(4, typeof(Deltas.Morale));
            _Term = new TerminateController(this);
        }
        #endregion

        #region data
        private IDelta _RmvFear;
        private TerminateController _Term;
        private List<ShakenEffect> _Fears;
        #endregion

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if ((oldAnchor == null) && (Anchor is Creature _critter))
            {
                // get all active fear effects (these are all derived from ShakenEffect)
                _Fears = _critter.Adjuncts.OfType<ShakenEffect>()
                    .Where(_f => _f.IsActive)
                    .ToList();
            }

            base.OnAnchorSet(oldAnchor, oldSetting);
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;

            // deactivate fears found during anchor set
            foreach (var _fear in _Fears.ToList())
            {
                // try to deactivate
                _fear.Activation = new Activation(this, false);
                if (!_fear.IsActive)
                {
                    // block attempts to re-activate it
                    _fear.AddChangeMonitor(this);
                }
                else
                {
                    // otherwise, we don't get to control it
                    _Fears.Remove(_fear);
                }
            }

            // NOTE: probably only the will is relevant, seriously doubt there are fear-fortitude or fear-reflex saves...
            _critter?.WillSave.Deltas.Add((IQualifyDelta)this);
            _critter?.FortitudeSave.Deltas.Add((IQualifyDelta)this);
            _critter?.ReflexSave.Deltas.Add((IQualifyDelta)this);
            base.OnActivate(source);
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // terminate saves versus fear
            DoTerminate();

            // reactivate any fears still anchored
            foreach (var _fear in _Fears.ToList())
            {
                // no longer suppressing it
                _fear.RemoveChangeMonitor(this);
                if (_fear.Anchor != null)
                {
                    // try to reactivate those still anchored
                    _fear.Activation = new Activation(this, true);
                }
                else
                {
                    // if it isn't anchored, forget about it
                    _Fears.Remove(_fear);
                }
            }

            base.OnDeactivate(source);
        }
        #endregion

        #region IQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // NOTE: what besides SpellTransit can carry a fear effect?
            if (((qualify as Interaction)?.InteractData is PowerActionTransit<PowerActionSource> _transit)
                && _transit.PowerSource.PowerDef.Descriptors.OfType<Fear>().Any())
            {
                yield return _RmvFear;
            }
            yield break;
        }
        #endregion

        #region IControlTerminate Members
        public void DoTerminate()
            => _Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;
        #endregion

        public override object Clone()
            => new RemoveFearEffect(Source);

        #region IMonitorChange<Activation>
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            if (_Fears.Contains(sender as ShakenEffect) && args.NewValue.IsActive)
            {
                args.DoAbort(@"Suppressed", this);
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }
        #endregion
    }
}
