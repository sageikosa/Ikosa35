using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Core.Dice;
using Uzi.Visualize;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class LightningBolt : SpellDef, ISpellMode, ISaveCapable, IDamageCapable, IBurstCaptureCapable, IGeneralSubMode,
        IRegionCapable, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Lightning Bolt";
        public override string Description => @"Line of electricity out to a distance point";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Electricity();
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
                yield return new MaterialComponent();
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
            yield return new LocationAim(@"Target", @"Final point of bolt", LocationAimMode.Any, FixedRange.One, FixedRange.One, new FixedRange(120));
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // TODO: consider this for "all" line like powers

            // get burst geometry
            var _target = deliver.TargetingProcess.Targets[0] as LocationTarget;

            if (deliver.Actor is Creature _actor)
            {
                // corner of actor as start
                var _start = new Intersection(_actor.AimPoint);
                var _startPt = _start.Point3D();

                // using end point to line up vector
                var _endPt = _target.SupplyPoint3D();
                var _vector = (_endPt - _startPt);
                _vector.Normalize();

                // lines can be lengthened via widen spell
                var _length = deliver.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                    .Dimensions(deliver.Actor, deliver.PowerUse.PowerActionSource.CasterLevel)
                    .FirstOrDefault();

                // true end point, and effective offset
                _endPt = _startPt + (_vector * _length);
                var _end = new Intersection(_endPt);
                var _offset = _start.Subtract(_end);

                // now make the builder
                var _actorLoc = _actor.GetLocated()?.Locator;
                var _actorRegion = _actorLoc?.GeometricRegion;
                var _segSet = new SegmentSetBuilder(_actor.Setting as LocalMap, _actorRegion,
                    _target.LocationAimMode == LocationAimMode.Cell ? _target.Location.ToCellPosition() : (IGeometricRegion)null,
                    _offset, _target.LocationAimMode, SegmentSetProcess.Effect,
                    _actorLoc?.PlanarPresence ?? PlanarPresence.Material);

                // snapshot line
                var _line = new Geometry(_segSet, _start, true);
                var _workset = new Interaction(deliver.Actor, null, null, null);
                SpellDef.DeliverBurstToMultipleSteps(deliver, _start, _line,
                    SpellDef.GetDamageModePreRequisites(deliver, _workset, new int[] { 0 }, 1, 1, false));
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // TODO: ignite flammable materials...
            var _multi = apply.MasterStep as MultiNextStep;
            SpellDef.ApplyDamage(apply, _multi, 0);
        }

        #endregion

        #region ISpellSaveMode Members

        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Reflex, SaveEffect.Half, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }

        #endregion

        #region IDamageMode Members

        public IEnumerable<int> DamageSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Electric.Damage", new DiceRange(@"Electric", DisplayName, 10, new DieRoller(6), 1), @"Electric Damage", EnergyType.Electric);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            // NOTE: shared damage amount (IDamageMode), unique saves (IGeneralSubMode)
            // NOTE: do not want the damage mode prerequisites to pick up a save, damage is handled in the multi-step
            // NOTE: saves are handled in the per-target deliveries as general mode saves
            // NOTE: same as burning hands, fireball, pillar of fire...
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => true;

        #endregion

        #region IBurstCapture Members

        public void PostInitialize(BurstCapture burst) { return; }

        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection) { return selection; }

        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // NOTE: not trying to deliver damage, as the multi-step gathers damage prerequisites, and the general mode will gather save prerequisites
                var _actor = _spellBurst.Activation.Actor;

                // skip actor's locator
                var _actLoc = _actor.GetLocated();
                if (_actLoc?.Locator != locator)
                {
                    // everything directly on the locator (for now)
                    foreach (var _step in DeliverDirectFromBurst(_spellBurst, locator,
                    delegate (Locator loc, ICore core) { return true; },
                    delegate (CoreStep step) { return true; }, 0))
                    {
                        yield return _step;
                    }
                }
            }
        }

        #endregion

        #region IGeneralSubMode Members
        public IEnumerable<int> GeneralSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode)
            => @"Save.Reflex";

        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion

        #region IRegionMode
        public IEnumerable<double> Dimensions(CoreActor actor, int powerLevel)
        {
            yield return 120d;
            yield break;
        }
        #endregion

        #region IPowerDeliverVisualize Members

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.Beam;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Medium;
        public string GetTransferMaterialKey() => @"#E000FFFF";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Uniform;
        public string GetSplashMaterialKey() => @"#C000FFFF|#8040FFFF|#C000FFFF";

        #endregion
    }
}
