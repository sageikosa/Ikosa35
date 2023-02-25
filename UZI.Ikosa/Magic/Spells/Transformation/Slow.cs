using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Slow : SpellDef, ISpellMode, IDurableCapable, IPowerDeliverVisualize, ISaveCapable, ICounterDispelCapable
    {
        public override string DisplayName => @"Slow";
        public override string Description => @"Creatures move slower.  Only make a single action per turn.  -1 attack.  -1 to armor rating and reflex.";
        public override MagicStyle MagicStyle => new Transformation();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

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

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            // 1 per level, no max
            var _max = new PowerLevelRange(1);
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, _max, new NearRange(), new CreatureTargetType()) { AllowDuplicates = false };
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

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
            // tuck save mode difficulty and type into SlowEffect
            var _dMode = apply.PowerUse.CapabilityRoot.GetCapability<IDurableCapable>();
            var _spellSave = apply.PowerUse.CapabilityRoot.GetCapability<ISaveCapable>();
            var _save = _spellSave.GetSaveMode(apply.Actor, 
                (apply.PowerUse as PowerUse<SpellSource>)?.PowerActionSource, 
                apply.DeliveryInteraction,
                _dMode.DurableSaveKey(apply.TargetingProcess.Targets, apply.DeliveryInteraction, 0));
            var _sMode = new ValueTarget<SaveMode>(@"SaveMode", _save);

            // must pass the save mode along
            var _feedback = apply.DeliveryInteraction.Feedback
                .OfType<PowerActionTransitFeedback<SpellSource>>().FirstOrDefault();
            if (_feedback.PowerTransit is MagicPowerEffectTransit<SpellSource> _transit)
            {
                _transit.MagicPowerEffects.FirstOrDefault()?.AllTargets.Add(_sMode);
            }
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IDurableMode Members
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _sMode = _spellEffect.AllTargets.Where(_t => _t.Key.Equals(@"SaveMode")).First() as ValueTarget<SaveMode>;
                var _slow = new SlowEffect(source, _sMode.Value);
                target.AddAdjunct(_slow);
                return _slow;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as SlowEffect)?.Eject();
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

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(Haste);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.SurgeFrom;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0B000B0";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Drain;
        public string GetSplashMaterialKey() => @"#C0B000B0|#80FF00FF|#C0B000B0";

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class SlowEffect : Adjunct
    {
        public SlowEffect(object source, SaveMode saveMode)
            : base(source)
        {
            _SaveMode = saveMode;
            _Slow = new Delta(-1, typeof(SlowEffect), @"Slow");
            _Single = new SingleActionsOnly(this);
            _Move = new Delta(1, typeof(SlowEffect), @"Slow");
        }

        #region state
        private SaveMode _SaveMode;
        private Delta _Slow;
        private SingleActionsOnly _Single;
        private Delta _Move;
        #endregion

        public Creature Creature => Anchor as Creature;
        public SaveMode SaveMode => _SaveMode;

        protected override void OnActivate(object source)
        {
            var _critter = Creature;
            _critter?.AddAdjunct(_Single);

            // atk -1
            _critter?.MeleeDeltable.Deltas.Add(_Slow);
            _critter?.RangedDeltable.Deltas.Add(_Slow);
            _critter?.OpposedDeltable.Deltas.Add(_Slow);

            // AR/reflex -1
            _critter?.IncorporealArmorRating.Deltas.Add(_Slow);
            _critter?.TouchArmorRating.Deltas.Add(_Slow);
            _critter?.NormalArmorRating.Deltas.Add(_Slow);
            _critter?.ReflexSave.Deltas.Add(_Slow);

            _critter?.MoveHalfing.Deltas.Add(_Move);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            _Single?.Eject();
            _Slow.DoTerminate();
            _Move.DoTerminate();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new SlowEffect(Source, SaveMode);
    }
}
