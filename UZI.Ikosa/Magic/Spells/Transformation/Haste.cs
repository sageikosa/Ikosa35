using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Haste : SpellDef, ISpellMode, IDurableCapable, IPowerDeliverVisualize, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Haste";
        public override string Description => @"Creatures move faster.  Extra attack in full attack.  +1 attack.  +1 dodge to armor rating and reflex.";
        public override MagicStyle MagicStyle => new Transformation();

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
            // 1 per level, no max
            var _max = new PowerLevelRange(1);
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, _max, new NearRange(), new CreatureTargetType()) { AllowDuplicates = false };
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // get "Creature" keyed targets of which the creature is truly aware
            var _critter = deliver.Actor as Creature;
            var _targets = (from _t in deliver.TargetingProcess.Targets
                            where _t.Key.Equals(@"Creature", StringComparison.OrdinalIgnoreCase)
                            && (_critter.Awarenesses.GetAwarenessLevel(_t.Target.ID) == AwarenessLevel.Aware)
                            select _t).ToList();

            SpellDef.DeliverDurableInCluster(deliver, _targets, true, 30, 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
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
            var _haste = new HasteEffect(source);
            target.AddAdjunct(_haste);
            return _haste;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as HasteEffect)?.Eject();
        }

        public bool IsDismissable(int subMode)
            => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Fortitude";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.FullSurge;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0FFFFB0";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#C0FFFFB0|#80FFFFB0|#C0FFFFB0";

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(Slow);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Fortitude, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class HasteEffect : Adjunct, IQualifyDelta, IMonitorChange<MovementBase>
    {
        public HasteEffect(object source)
            : base(source)
        {
            _TermCtrl = new TerminateController(this);
            _Factory = new ExtraAttackBudgetFactory(this);
            _Haste = new Delta(1, typeof(HasteEffect), @"Haste");
            _SpeedUps = new Dictionary<MovementBase, Delta>();
        }

        #region state
        private ExtraAttackBudgetFactory _Factory;
        private Delta _Haste;
        private readonly TerminateController _TermCtrl;
        private readonly Dictionary<MovementBase, Delta> _SpeedUps;
        #endregion

        public Creature Creature => Anchor as Creature;

        #region OnActivate
        protected override void OnActivate(object source)
        {
            var _critter = Creature;
            _critter?.AddAdjunct(_Factory);

            // atk +1
            _critter?.MeleeDeltable.Deltas.Add(_Haste);
            _critter?.RangedDeltable.Deltas.Add(_Haste);
            _critter?.OpposedDeltable.Deltas.Add(_Haste);

            // AR/reflex +1
            _critter?.IncorporealArmorRating.Deltas.Add(this);
            _critter?.TouchArmorRating.Deltas.Add(this);
            _critter?.NormalArmorRating.Deltas.Add(this);
            _critter?.ReflexSave.Deltas.Add(this);

            // watch for new natural movements
            _SpeedUps.Clear();
            _critter?.Movements.AddChangeMonitor(this);
            var _natMoves = _critter?.Movements.AllMovements.Where(_m => _m.IsNativeMovement).ToList();
            foreach (var _nm in _natMoves)
            {
                var _delta = new Delta(Math.Min(_nm.BaseValue, 30), typeof(Enhancement), @"Haste");
                _nm.Deltas.Add(_delta);
                _SpeedUps.Add(_nm, _delta);
            }
            base.OnActivate(source);
        }
        #endregion

        #region OnDeactivate
        protected override void OnDeactivate(object source)
        {
            _Factory?.Eject();
            _Haste.DoTerminate();
            DoTerminate();

            // stop watching for new natural movements
            Creature?.Movements.RemoveChangeMonitor(this);
            foreach (var _mv in _SpeedUps)
            {
                // terminate speed up
                _mv.Value.DoTerminate();
            }
            _SpeedUps.Clear();
            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new HasteEffect(Source);

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => (IsActive
            && (Creature?.CanDodge(qualify) ?? false)
                ? _Haste
                : null).ToEnumerable().Where(_d => _d != null);

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _TermCtrl.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _TermCtrl.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _TermCtrl.RemoveTerminateDependent(subscriber);
        }
        public int TerminateSubscriberCount => _TermCtrl.TerminateSubscriberCount;

        #endregion

        #region IMonitorChange<MovementBase>

        public void PreTestChange(object sender, AbortableChangeEventArgs<MovementBase> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<MovementBase> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<MovementBase> args)
        {
            switch (args.Action)
            {
                case @"Add":
                    if (args.NewValue.IsNativeMovement
                        && !_SpeedUps.TryGetValue(args.NewValue, out var _delta))
                    {
                        var _nm = args.NewValue;
                        _delta = new Delta(Math.Min(_nm.BaseValue, 30), typeof(Enhancement), @"Haste");
                        _nm.Deltas.Add(_delta);
                        _SpeedUps.Add(_nm, _delta);
                    }
                    break;

                case @"Remove":
                    _SpeedUps.Remove(args.NewValue);
                    break;
            }
        }

        #endregion
    }
}
