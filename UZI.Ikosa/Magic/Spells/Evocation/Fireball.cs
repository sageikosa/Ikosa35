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

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class Fireball : SpellDef, ISpellMode, ISaveCapable, IDamageCapable, IRegionCapable, IBurstCaptureCapable, IGeneralSubMode
    {
        public override string DisplayName => @"Fireball";
        public override string Description => @"Fire damage in burst at a distance";
        public override MagicStyle MagicStyle => new Evocation();

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new Fire();
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
            yield return new LocationAim(@"Origin", @"Point of Burst", LocationAimMode.Any, FixedRange.One, FixedRange.One, new FarRange());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // get burst geometry
            var _target = deliver.TargetingProcess.Targets[0] as LocationTarget;
            var _sphere = new Geometry(new SphereBuilder(Convert.ToInt32(
                deliver.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>()
                .Dimensions(deliver.Actor, deliver.PowerUse.PowerActionSource.CasterLevel)
                .FirstOrDefault() / 5)), new Intersection(_target.Location), true);
            var _workset = new Interaction(deliver.Actor, null, null, null);
            SpellDef.DeliverBurstToMultipleSteps(deliver, new Intersection(_target.Location), _sphere, SpellDef.GetDamageModePreRequisites(deliver, _workset, new int[] { 0 }, 1, 1, false));
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

        public IEnumerable<int> DamageSubModes => 0.ToEnumerable();

        public IEnumerable<DamageRule> GetDamageRules(int subMode, bool isCriticalHit)
        {
            yield return new EnergyDamageRule(@"Fire.Damage", new DiceRange(@"Fire", DisplayName, 10, new DieRoller(6), 1), @"Fire Damage", EnergyType.Fire);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            // NOTE: shared damage amount (IDamageMode), unique saves (IGeneralSubMode)
            // NOTE: do not want the damage mode prerequisites to pick up a save, damage is handled in the multi-step
            // NOTE: saves are handled in the per-target deliveries as general mode saves
            // NOTE: same as burning hands...
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => true;
    
        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            // 20 foot radius
            yield return 20d;
            yield break;
        }
        #endregion

        #region IBurstCapture Members

        public void PostInitialize(BurstCapture burst) { return; }
        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection) { return selection; }

        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            // get the burst
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
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
        {
            return @"Save.Reflex";
        }

        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }
        #endregion
    }
}
