using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Bless : SpellDef, IDurableCapable, ISpellMode, IRegionCapable, IBurstCaptureCapable, ICounterDispelCapable,
        IGeometryCapable<SpellSource>, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Bless";
        public override string Description => @"+1 Moral on attack and saves versus fear.";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);
        public override IEnumerable<Descriptor> Descriptors => new MindAffecting().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

        public override IEnumerable<SpellComponent> DivineComponents
            => YieldComponents(true, true, false, false, true);

        public override IEnumerable<SpellComponent> ArcaneComponents
            => YieldComponents(true, true, false, true, false);

        // IDurableMode
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _bless = new BlessEffect(source);
            target.AddAdjunct(_bless);
            return _bless;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as BlessEffect)?.Eject();

        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        // ISpellMode
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new PersonalStartAim(@"Self", @"Self", actor).ToEnumerable();

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        #region public void ActivateSpell(PowerDeliveryStep<SpellSource> activation)
        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // get burst geometry
            var _target = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Self");
            var _intersect = new Intersection(_target.Location);
            var _sphere = new Geometry(GetDeliveryGeometry(activation), _intersect, true);
            SpellDef.DeliverBurstToMultipleSteps(activation, _intersect, _sphere, null);
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }

        // IRegionCapable Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
            => 50d.ToEnumerable();

        #region IBurstCapture Members
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                var _actor = _spellBurst.Activation.Actor as Creature;

                // everything directly on the locator (for now)
                foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                    (Locator loc, ICore core) => (core is Creature) && _actor.IsFriendly(core.ID),
                    (CoreStep step) => true, 0))
                    yield return _step;
            }
            yield break;
        }

        public void PostInitialize(BurstCapture burst)
        {
            return;
        }

        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
            => selection;

        #endregion

        // ICounterDispelCapable Members
        public IEnumerable<Type> CounterableSpells
            => typeof(Bane).ToEnumerable();

        public IEnumerable<Type> DescriptorTypes
            => Enumerable.Empty<Type>();

        // IGeometryCapable Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get burst geometry
            var _reg = powerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            return new SphereBuilder(Convert.ToInt32(_reg.Dimensions(actor,
                powerUse.PowerActionSource.CasterLevel).FirstOrDefault() / 5));
        }

        // IPowerDeliverVisualize
        public VisualizeTransferType GetTransferType() => VisualizeTransferType.FullSurge;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0FFFFB0";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Pulse;
        public string GetSplashMaterialKey() => @"#C0FFFFB0|#80FFFFB0|#C0FFFFB0";

    }

    [Serializable]
    public class BlessEffect : Adjunct, IQualifyDelta
    {
        #region ctor()
        public BlessEffect(object source)
            : base(source)
        {
            _Bless = new Delta(1, typeof(Uzi.Ikosa.Deltas.Morale));
            _Term = new TerminateController(this);
        }
        #endregion

        #region data
        private Delta _Bless;
        private TerminateController _Term;
        #endregion

        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter?.MeleeDeltable.Deltas.Add(_Bless);
            _critter?.RangedDeltable.Deltas.Add(_Bless);
            _critter?.OpposedDeltable.Deltas.Add(_Bless);

            // NOTE: probably only the will is relevant, seriously doubt there are fear-fortitude or fear-reflex saves...
            _critter?.WillSave.Deltas.Add((IQualifyDelta)this);
            _critter?.FortitudeSave.Deltas.Add((IQualifyDelta)this);
            _critter?.ReflexSave.Deltas.Add((IQualifyDelta)this);

            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            // terminate attack bonus
            _Bless.DoTerminate();

            // terminate saves versus fear
            DoTerminate();

            base.OnDeactivate(source);
        }

        #region IQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // NOTE: what besides SpellTransit can carry a fear effect?
            if (((qualify as Interaction)?.InteractData is PowerActionTransit<PowerActionSource> _transit)
                && _transit.PowerSource.PowerDef.Descriptors.OfType<Fear>().Any())
            {
                yield return _Bless;
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
            => new BlessEffect(Source);
    }
}
