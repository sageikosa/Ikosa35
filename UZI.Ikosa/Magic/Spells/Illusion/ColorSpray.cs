using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class ColorSpray : SpellDef, ISpellMode, ISaveCapable, IRegionCapable, IGeneralSubMode, IBurstCaptureCapable
    {
        public override string DisplayName => @"Color Spray";
        public override string Description => @"Stun, blind and knock creatures unconscious";
        public override MagicStyle MagicStyle => new Illusion(Illusion.SubIllusion.Pattern);

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
            IRegionCapable _reg = mode.GetCapability<IRegionCapable>();
            var _range = _reg.Dimensions(null, 0).FirstOrDefault();
            yield return new PersonalConicAim(@"Cone", @"Directional cone", new FixedRange(_range), actor);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _cone = deliver.TargetingProcess.Targets.Where(_t => _t.Key.Equals(@"Cone", StringComparison.OrdinalIgnoreCase)).FirstOrDefault() as GeometricTarget;
            DeliverBurstToMultipleSteps(deliver, _cone.Origin, _cone.Geometry, null);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // get the prerequisite for the expected save, 
            SavePrerequisite _savePre = apply.AllPrerequisites<SavePrerequisite>(@"Save.Will").FirstOrDefault();
            if (!_savePre.Success)
            {
                // determine which effects to apply
                var _critter = apply.DeliveryInteraction.Target as Creature;
                StunnedEffect _stunned = null;
                BlindEffect _blinded = null;
                UnconsciousEffect _knockedOut = null;
                RollPrerequisite _unconRounds = apply.AllPrerequisites<RollPrerequisite>(@"Roll.Unconscious").FirstOrDefault();
                RollPrerequisite _blindRounds = apply.AllPrerequisites<RollPrerequisite>(@"Roll.Blind").FirstOrDefault();
                var _rnd = new Round();
                switch (_critter.AdvancementLog.NumberPowerDice)
                {
                    case 0:
                    case 1:
                    case 2:
                        // unconscious, blind and stun
                        {
                            var _unconEnd = (_critter?.GetCurrentTime() ?? 0d) + (_unconRounds.RollValue * Round.UnitFactor);
                            _knockedOut = new UnconsciousEffect(apply.PowerUse.PowerActionSource, _unconEnd, _rnd.BaseUnitFactor);

                            var _blindEnd = _unconEnd + (_blindRounds.RollValue * Round.UnitFactor);
                            _blinded = new BlindEffect(apply.PowerUse.PowerActionSource, _blindEnd, _rnd.BaseUnitFactor);

                            var _stunEnd = _blindRounds.RollValue + Round.UnitFactor;
                            _stunned = new StunnedEffect(apply.PowerUse.PowerActionSource, _stunEnd, _rnd.BaseUnitFactor);
                        }
                        break;

                    case 3:
                    case 4:
                        // blind and stun
                        {
                            var _blindEnd = (_critter?.GetCurrentTime() ?? 0d) + (_blindRounds.RollValue * Round.UnitFactor);
                            _blinded = new BlindEffect(apply.PowerUse.PowerActionSource, _blindEnd, _rnd.BaseUnitFactor);

                            var _stunEnd = _blindRounds.RollValue + Round.UnitFactor;
                            _stunned = new StunnedEffect(apply.PowerUse.PowerActionSource, _stunEnd, _rnd.BaseUnitFactor);
                        }
                        break;

                    default:
                        // stun
                        _stunned = new StunnedEffect(apply.PowerUse.PowerActionSource, 
                            (_critter?.GetCurrentTime() ?? 0d) + Round.UnitFactor, _rnd.BaseUnitFactor);
                        break;
                }
                if (_knockedOut != null)
                    _critter.AddAdjunct(_knockedOut);
                if (_blinded != null)
                    _critter.AddAdjunct(_blinded);
                if (_stunned != null)
                    _critter.AddAdjunct(_stunned);
            }
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource,Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion

        #region IRegionMode Members
        public IEnumerable<double> Dimensions(CoreActor actor, int casterLevel)
        {
            yield return 15;
            yield break;
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

        public string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode) { return @"Save.Will"; }

        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact)
        {
            var _critter = interact.Target as Creature;
            switch (_critter.AdvancementLog.NumberPowerDice)
            {
                case 0:
                case 1:
                case 2:
                    yield return new RollPrerequisite(interact.Source, interact, interact.Actor,
                        @"Roll.Unconscious", @"Unconscious Rounds", new DiceRoller(2, 4), false);
                    yield return new RollPrerequisite(interact.Source, interact, interact.Actor,
                        @"Roll.Blind", @"Blinded Rounds", new DieRoller(4), false);
                    break;

                case 3:
                case 4:
                    yield return new RollPrerequisite(interact.Source, interact, interact.Actor,
                        @"Roll.Blind", @"Blinded Rounds", new DieRoller(4), false);
                    break;

                default:
                    break;
            }
            yield break;
        }
        #endregion

        #region IBurstCapture Members
        public IEnumerable<CoreStep> Capture(BurstCapture burst, Locator locator)
        {
            if (burst is PowerBurstCapture<SpellSource> _spellBurst)
            {
                CoreActor _actor = _spellBurst.Activation.Actor;

                // skip actor's locator
                var _actLoc = _actor.GetLocated();
                if ((_actLoc == null) || (_actLoc.Locator != locator))
                    // everything directly on the locator (for now)
                    foreach (var _step in DeliverDirectFromBurst(_spellBurst, locator,
                        delegate (Locator loc, ICore core) { return true; },
                        delegate (CoreStep step) { return true; }, 0))
                        yield return _step;
            }
            yield break;
        }

        public void PostInitialize(BurstCapture burst)
        {
            return;
        }

        public IEnumerable<Locator> ProcessOrder(BurstCapture burst, IEnumerable<Locator> selection)
        {
            return selection;
        }

        #endregion
    }
}
