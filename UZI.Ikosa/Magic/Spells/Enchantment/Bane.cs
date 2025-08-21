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
    public class Bane : SpellDef, ISpellMode, IDurableCapable, IRegionCapable, IBurstCaptureCapable, ISaveCapable,
        ICounterDispelCapable, IGeometryCapable<SpellSource>, IPowerDeliverVisualize
    {
        public override string DisplayName
            => @"Bane";

        public override string Description
            => @"-1 penalty on attack and saves versus fear.";

        public override MagicStyle MagicStyle
            => new Enchantment(Enchantment.SubEnchantment.Compulsion);

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Fear();
                yield return new MindAffecting();
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
            var _bane = new BaneEffect(source);
            target.AddAdjunct(_bane);
            return _bane;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            target.RemoveAdjunct((BaneEffect)source.ActiveAdjunctObject);
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new PersonalStartAim(@"Self", @"Self", actor);
            yield break;
        }

        public bool AllowsSpellResistance
            => true;

        public bool IsHarmless
            => true;

        #region public void ActivateSpell(PowerDeliveryStep<SpellSource> activation)
        public void ActivateSpell(PowerActivationStep<SpellSource> activation)
        {
            // get burst geometry
            var _target = activation.TargetingProcess.GetFirstTarget<LocationTarget>(@"Self");
            var _sphere = new Geometry(GetDeliveryGeometry(activation), new Intersection(_target.Location), true);

            // multiple targets needs a multi-next step from deliver
            SpellDef.DeliverBurstToMultipleSteps(activation, new Intersection(_target.Location), _sphere, null);
        }
        #endregion

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return 50;
            yield break;
        }
        #endregion

        #region IBurstCapture Members

        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                var _actor = _spellBurst.Activation.Actor as Creature;

                // everything directly on the locator (for now)
                foreach (var _step in SpellDef.DeliverDurableDirectFromBurst(_spellBurst, locator,
                    delegate (Locator loc, ICore core)
                    {
                        return (core is Creature) && _actor.IsUnfriendly(core.ID);
                    }, delegate (CoreStep step) { return true; }, 0))
                {
                    yield return _step;
                }
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

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, 
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region ICounterDispelMode Members
        public IEnumerable<Type> CounterableSpells
        {
            get
            {
                yield return typeof(Bless);
                yield break;
            }
        }

        public IEnumerable<Type> DescriptorTypes { get { yield break; } }
        #endregion

        // IGeometryMode Members
        public IGeometryBuilder GetBuilder(IPowerUse<SpellSource> powerUse, CoreActor actor)
        {
            // get burst geometry
            var _reg = powerUse.CapabilityRoot.GetCapability<IRegionCapable>();
            return new SphereBuilder(Convert.ToInt32(_reg.Dimensions(actor,
                powerUse.PowerActionSource.CasterLevel).FirstOrDefault() / 5));
        }

        #region IPowerDeliverVisualize

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.SurgeFrom;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#C0FF00FF";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Drain;
        public string GetSplashMaterialKey() => @"#C0FF00FF|#80FF00FF|#C0FF00FF";

        #endregion
    }

    [Serializable]
    public class BaneEffect : Adjunct, IQualifyDelta
    {
        #region Constructor
        public BaneEffect(object source)
            : base(source)
        {
            // NOTE: delta is not from Morale, it is from Bane
            _Bane = new Delta(-1, typeof(Uzi.Ikosa.Magic.Spells.Bane));
            _Term = new TerminateController(this);
        }
        #endregion

        private Delta _Bane;

        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter?.MeleeDeltable.Deltas.Add(_Bane);
            _critter?.RangedDeltable.Deltas.Add(_Bane);
            _critter?.OpposedDeltable.Deltas.Add(_Bane);

            // NOTE: probably only the will is relevant, seriously doubt there are fear-fortitude or fear-reflex saves...
            _critter?.WillSave.Deltas.Add((IQualifyDelta)this);
            _critter?.FortitudeSave.Deltas.Add((IQualifyDelta)this);
            _critter?.ReflexSave.Deltas.Add((IQualifyDelta)this);

            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            // terminate attack bonus
            _Bane.DoTerminate();

            // terminate saves versus fear
            DoTerminate();

            base.OnDeactivate(source);
        }

        #region IQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // NOTE: what besides SpellTransit can carry a fear effect?
            if (qualify is Interaction _interact)
            {
                if ((_interact.InteractData is PowerActionTransit<PowerActionSource> _transit)
                    && _transit.PowerSource.PowerDef.Descriptors.OfType<Fear>().Any())
                {
                    yield return _Bane;
                }
            }
            yield break;
        }
        #endregion

        #region IControlTerminate Members

        private readonly TerminateController _Term;

        public void DoTerminate()
            => _Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        public override object Clone()
            => new BaneEffect(Source);
    }
}
