using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class TouchOfIdiocy : SpellDef, IDurableCapable, ISpellMode, IPowerDeliverVisualize
    {
        public override string DisplayName => @"Touch of Idiocy";
        public override string Description => @"1d6 penalty to INT, WIS, CHA of creature touched";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);
        public override IEnumerable<Descriptor> Descriptors => new MindAffecting().ToEnumerable();
        public override IEnumerable<ISpellMode> SpellModes => this.ToEnumerable();

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

        // ISpellMode
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
            => new TouchAim(@"Creature", @"Creature", Lethality.AlwaysLethal,
                ImprovedCriticalTouchFeat.CriticalThreatStart(actor as Creature),
                this, FixedRange.One, FixedRange.One, new MeleeRange(), new LivingCreatureTargetType())
            .ToEnumerable();

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
            => SpellDef.DeliverDurableToTouch(deliver, @"Creature", 0);

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            // get roll prerequisite, use to indicate maximum penalty
            RollPrerequisite _roll = apply.AllPrerequisites<RollPrerequisite>(@"Mental.Penalty").FirstOrDefault();
            if (_roll != null)
            {
                var _transit = apply.DeliveryInteraction.InteractData as MagicPowerEffectTransit<SpellSource>;

                // setup AimTargets for the roll
                _transit.MagicPowerEffects.First().AllTargets.Add(new PrerequisiteTarget(_roll));
                SpellDef.ApplyDurableMagicEffects(apply);
            }
        }

        // IDurableMode
        public IEnumerable<int> DurableSubModes => 0.ToEnumerable();
        public bool IsDismissable(int subMode) => false;
        public string DurableSaveKey(IEnumerable<AimTarget> targets,Interaction workSet, int subMode) => string.Empty;

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(10, new Minute(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield return new RollPrerequisite(interact.Source, interact, interact.Actor,
                @"Mental.Penalty", @"Mental Penalty", new DieRoller(6), false);
            yield break;
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            var _idiocy = new IdiocyPenalties(source);
            target.AddAdjunct(_idiocy);
            return _idiocy;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
            => (source.ActiveAdjunctObject as IdiocyPenalties)?.Eject();

        #region IPowerDeliverVisualize

        public VisualizeTransferType GetTransferType() => VisualizeTransferType.Cone;
        public VisualizeTransferSize GetTransferSize() => VisualizeTransferSize.Small;
        public string GetTransferMaterialKey() => @"#C0FF40FF";
        public VisualizeSplashType GetSplashType() => VisualizeSplashType.Drain;
        public string GetSplashMaterialKey() => @"#C0FF00FF|#80FF00FF|#C0FF00FF";

        #endregion
    }

    [Serializable]
    public class IdiocyPenalties : Adjunct
    {
        public IdiocyPenalties(object source)
            : base(source)
        {
        }

        #region state
        private Delta _Int;
        private Delta _Wis;
        private Delta _Cha;
        #endregion

        public override object Clone()
            => new IdiocyPenalties(Source);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter
                && source is MagicPowerEffect _spellEffect)
            {
                var _val = ((_spellEffect.AllTargets[0] as PrerequisiteTarget).PreRequisite as RollPrerequisite).RollValue;

                _Int = new Delta(0 - _val, typeof(IdiocyPenalties), @"Idiocy");
                ManagePenalty(_critter.Abilities.Intelligence, _val, _Int);
                _Wis = new Delta(0 - _val, typeof(IdiocyPenalties), @"Idiocy");
                ManagePenalty(_critter.Abilities.Wisdom, _val, _Wis);
                _Cha = new Delta(0 - _val, typeof(IdiocyPenalties), @"Idiocy");
                ManagePenalty(_critter.Abilities.Charisma, _val, _Cha);
            }
        }

        private void ManagePenalty(AbilityBase ability, int posPenalty, Delta penaltyDelta)
        {
            ability.Deltas.Add(penaltyDelta);
            if (ability.EffectiveValue < 1)
            {
                posPenalty += ability.EffectiveValue - 1;
                if (posPenalty > 0)
                {
                    // still calculating a penalty, so alter the delta
                    penaltyDelta.Value = 0 - posPenalty;
                }
                else
                {
                    // no penalty anymore
                    penaltyDelta.DoTerminate();
                }
            }
        }

        protected override void OnDeactivate(object source)
        {
            // terminate penalties
            _Int?.DoTerminate();
            _Wis?.DoTerminate();
            _Cha?.DoTerminate();
            base.OnDeactivate(source);
        }
    }
}
