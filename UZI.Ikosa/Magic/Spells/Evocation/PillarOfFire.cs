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
    public class PillarOfFire : SpellDef, ISpellMode, ISaveCapable, IDamageCapable, IRegionCapable, IBurstCaptureCapable, IGeneralSubMode
    {
        public override string DisplayName => @"Pillar of Fire";
        public override string Description => @"Fire damage in cylinder raining dpwn";
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
            yield return new LocationAim(@"Origin", @"Top of Pillar", LocationAimMode.Any, FixedRange.One, FixedRange.One, new MediumRange());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // get burst geometry
            var _target = deliver.TargetingProcess.Targets[0] as LocationTarget;
            var _dimensions = deliver.PowerUse.CapabilityRoot.GetCapability<IRegionCapable>().Dimensions(deliver.Actor, deliver.PowerUse.PowerActionSource.CasterLevel).ToList();
            var _pillar = new Geometry(new CylinderBuilder((int)(_dimensions[0] / 5d), (int)(_dimensions[1] / 5d)),
                new Intersection(_target.Location), true);
            var _workset = new Interaction(deliver.Actor, null, null, null);
            SpellDef.DeliverBurstToMultipleSteps(deliver, new Intersection(_target.Location), _pillar, SpellDef.GetDamageModePreRequisites(deliver, _workset, new int[] { 0 }, 1, 1, false));
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
            yield return new EnergyDamageRule(@"Fire.Damage", new DiceRange(@"Fire/Divine", DisplayName, 15, new DieRoller(6), 1), @"Fire & Divine Damage", EnergyType.Fire, EnergyType.Unresistable);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            // NOTE: shared damage amount (IDamageMode), unique saves (IGeneralSubMode)
            // NOTE: do not want the damage mode prerequisites to pick up a save, damage is handled in the multi-step
            // NOTE: saves are handled in the per-target deliveries as general mode saves
            // NOTE: same as burning hands and fireball...
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => true;

        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            // 10 foot radius
            yield return 10d;
            // 40 foot high
            yield return 40d;
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
                    yield return _step;
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
