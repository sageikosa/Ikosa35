using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class BurningHands : SpellDef, ISpellMode, ISaveCapable, IDamageCapable, IRegionCapable, IBurstCaptureCapable, IGeneralSubMode
    {
        public override string DisplayName => @"Burning Hands";
        public override string Description => @"Cone deals 1d4/level fire damage (max 5d4).  May ignite flammable materials.";
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
            var _reg = mode.GetCapability<IRegionCapable>();
            var _range = _reg.Dimensions(null, 0).FirstOrDefault();
            yield return new PersonalConicAim(@"Cone", @"Directional cone", new FixedRange(_range), actor);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _cone = deliver.TargetingProcess.Targets.Where(_t => _t.Key.Equals(@"Cone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as GeometricTarget;

            // multiple targets needs a multi-next step from deliver
            var _workset = new Interaction(deliver.Actor, null, null, null);
            SpellDef.DeliverBurstToMultipleSteps(deliver, _cone.Origin, _cone.Geometry, SpellDef.GetDamageModePreRequisites(deliver, _workset, new int[] { 0 }, 1, 1, false));
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
            yield return new EnergyDamageRule(@"Fire.Damage", new DiceRange(@"Fire", DisplayName, 5, new DieRoller(4), 1), @"Fire Damage", EnergyType.Fire);
            yield break;
        }

        public string DamageSaveKey(Interaction workSet, int subMode)
        {
            // NOTE: shared damage amount (IDamageMode), unique saves (IGeneralSubMode)
            // NOTE: do not want the damage mode prerequisites to pick up a save, damage is handled in the multi-step
            // NOTE: saves are handled in the per-target deliveries as general mode saves
            // NOTE: same as fireball
            return string.Empty;
        }

        public bool CriticalFailDamagesItems(int subMode) => true;
        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return 15;
            yield break;
        }
        #endregion

        #region IBurstCapture Members
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                // NOTE: not trying to deliver damage, as the multi-step gathers damage prerequisites, and the general mode will gather save prerequisites
                CoreActor _actor = _spellBurst.Activation.Actor;

                // skip actor's locator
                var _actLoc = _actor.GetLocated();
                if ((_actLoc == null) || (_actLoc.Locator != locator))
                {
                    // everything directly on the locator (for now)
                    foreach (var _step in DeliverDirectFromBurst(_spellBurst, locator,
                        delegate (Locator loc, ICore core) { return true; },
                        delegate (CoreStep step) { return true; }, 0))
                        yield return _step;
                }
            }
            yield break;
        }

        public void PostInitialize(BurstCapture burst) { return; }
        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection) { return selection; }
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
