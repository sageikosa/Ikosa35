using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SpiderClimb : SpellDef, ISpellMode, IDurableCapable, ISaveCapable
    {
        public override string DisplayName => @"Spider Climb";
        public override string Description => @"Creature gains natural climb speed of 20 feet if hands are free.  Retain dexterity bonus to armor.  No running.";
        public override MagicStyle MagicStyle => new Transformation();

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
            yield return new TouchAim(@"Creature", @"Creature", Lethality.AlwaysNonLethal,
                20, this, FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            if (apply.DeliveryInteraction.Target is Creature)
            {
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }
        #endregion

        #region IDurableMode Members
        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (target is Creature _critter)
            {
                // define a spider climb effect, add to creature
                var _climb = new SpiderClimbEffect(source);
                _critter.AddAdjunct(_climb);
                return _climb;
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            // terminate spider climb effect
            (source.ActiveAdjunctObject as SpiderClimbEffect)?.Eject();
        }

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }

        public bool IsDismissable(int subMode)
            => false;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class SpiderClimbEffect : Adjunct
    {
        public SpiderClimbEffect(object source)
            : base(source)
        {
        }

        #region state
        private BetterDelta _Better;
        private Delta _Skill;
        private SpiderClimbMovement _Climb;
        #endregion

        private Creature _Critter => Anchor as Creature;

        #region void OnActivate()
        protected override void OnActivate(object source)
        {
            var _critter = _Critter;
            if (_critter != null)
            {
                // define a spider climb movement, add to creature
                _Climb = new SpiderClimbMovement(20, _critter, Source);
                _critter.Movements.Add(_Climb);

                // +8 climb (for reactions)
                _Skill = new Delta(8, typeof(Racial));
                _critter.Skills.Skill<ClimbSkill>().Deltas.Add(_Skill);

                // dexterity or strength for climb reactions
                _Better = new BetterDelta(_critter.Abilities.Ability<Dexterity>(), _critter.Abilities.Ability<Strength>());
                _critter.Skills.Skill<ClimbSkill>().Deltas.Add(_Better);
            }

            // done
            base.OnActivate(source);
        }
        #endregion

        #region void OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            // remove spider climb movement
            _Critter?.Movements.Remove(_Climb);
            _Skill?.DoTerminate();
            _Better?.DoTerminate();

            // last active movement?
            if ((_Critter?.GetLocated()?.Locator is Locator _loc)
                && _Critter.HasActiveAdjunct<Climbing>()
                && (_loc.ActiveMovement == _Climb))
            {
                // look for another climb movement
                var _fallback = _Critter.Movements.AllMovements.OfType<ClimbMovement>()
                    .OrderByDescending(_cm => _cm.EffectiveValue)
                    .FirstOrDefault();
                if (_fallback != null)
                {
                    var _face = _loc.BaseFace;
                    _loc.ActiveMovement = _fallback;
                    var _difficulty = _loc.Map.GripDifficulty(_loc.GeometricRegion, _fallback, _face).Difficulty ?? 0;

                    // make a check
                    _Critter?.StartNewProcess(
                        new ClimbSustainStep(
                            new Qualifier(_Critter, this, _Critter), 
                            new SuccessCheck(_Critter.Skills.Skill<ClimbSkill>(), _difficulty, -5), 
                            @"Climb check"),
                        @"Check on spider-climb ending");
                }
                else
                {
                    if (_loc.IsGravityEffective)
                    {
                        FallingStartStep.StartFall(_loc, 500, 500, @"Spider-climb ended", 1);    // TODO: fall reduce?
                    }
                }
            }

            base.OnDeactivate(source);
        }
        #endregion

        public override object Clone()
            => new SpiderClimbEffect(Source);
    }

    [Serializable]
    public class SpiderClimbMovement : ClimbMovement
    {
        public SpiderClimbMovement(int speed, Creature creature, object source)
            : base(speed, creature, source, true, null)
        {
        }

        public override string Name => @"Spider-Climb";

        #region public override bool IsUsable { get; }
        /// <summary>two free holding slots, or no holding slots at all</summary>
        public override bool IsUsable
        {
            get
            {
                var _locator = CoreObject.GetLocated()?.Locator;
                if (_locator != null)
                {
                    if (!_locator.PlanarPresence.HasMaterialPresence())
                    {
                        return false;
                    }

                    if (CoreObject is Creature _critter)
                    {
                        if (_critter.Body.ItemSlots.AllSlots
                            .OfType<HoldingSlot>()
                            .Where(_hold => _hold.SlottedItem == null)
                            .Count() >= 2)
                        {
                            // more than one empty holding slot
                            return true;
                        }
                        else if (!_critter.Body.ItemSlots.AllSlots
                            .OfType<HoldingSlot>()
                            .Any())
                        {
                            // no holding slots at all (multiple feet...)
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        #endregion

        public override bool IsAccelerated { get => base.IsAccelerated; set { /* cannot accelerate */ } }

        public override bool IsCheckRequired => false;

        public override MovementBase Clone(Creature forCreature, object source)
            => new SpiderClimbMovement(BaseValue, forCreature, source);
    }
}
