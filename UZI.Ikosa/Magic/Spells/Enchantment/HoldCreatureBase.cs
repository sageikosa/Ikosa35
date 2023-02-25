using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public abstract class HoldCreatureBase : SpellDef, IDurableCapable, ISpellMode, ISaveCapable
    {
        public override string Description => @"Paralyze subject.  May attempt new save as full-round action on its turn to end.";
        public override MagicStyle MagicStyle => new Enchantment(Enchantment.SubEnchantment.Compulsion);

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

        #region public override IEnumerable<Descriptor> Descriptors { get; }
        public override IEnumerable<Descriptor> Descriptors
        {
            get
            {
                yield return new MindAffecting();
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

        protected abstract string AimName { get; }
        protected abstract TargetType TargetType { get; }

        #region ISpellMode Members
        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", AimName, FixedRange.One, FixedRange.One, new MediumRange(), TargetType);
            yield break;
        }

        public bool AllowsSpellResistance => true;
        public bool IsHarmless => false;

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            // TODO: validate target type...
            SpellDef.DeliverDurable(deliver, deliver.TargetingProcess.GetFirstTarget<AwarenessTarget>(@"Creature"), 0);
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _savePre = apply.AllPrerequisites<SavePrerequisite>(@"Save.Will").FirstOrDefault();
            var _effect = apply.DurableMagicEffects.FirstOrDefault();
            if (_savePre?.Success ?? false)
            {
                // add save mode for future attempts to break the hold
                _effect.AllTargets.Add(new ValueTarget<SaveMode>(@"Save.Will", _savePre.SaveMode));
            }
            SpellDef.ApplyDurableMagicEffects(apply);
        }
        #endregion

        #region ISpellSaveMode Members
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates, SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
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

        public bool IsDismissable(int subMode) => true;

        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode)
            => @"Save.Will";

        public DurationRule DurationRule(int subMode)
            => new DurationRule(DurationType.Span, new SpanRulePart(1, new Round(), 1));

        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if (source is MagicPowerEffect _spellEffect)
            {
                var _saveMode = _spellEffect.GetTargetValue<SaveMode>(@"Save.Will");
                if (_spellEffect != null)
                {
                    // hold creature may be broken later with a future save
                    var _hold = new HoldEffect(_spellEffect, _saveMode);
                    target.AddAdjunct(_hold);
                    return _hold;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            (source.ActiveAdjunctObject as HoldEffect)?.Eject();
        }
        #endregion
    }

    [Serializable]
    public class HoldAnimal : HoldCreatureBase
    {
        protected override string AimName => @"Animal";
        protected override TargetType TargetType => new CreatureTypeTargetType<AnimalType>();

        public override string DisplayName => @"Hold Animal";
    }

    [Serializable]
    public class HoldPerson : HoldCreatureBase
    {
        protected override string AimName => @"Person";
        protected override TargetType TargetType => new CreatureTypeTargetType<HumanoidType>();

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

        #region public override IEnumerable<SpellComponent> DivineComponents
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

        public override string DisplayName => @"Hold Person";
    }

    [Serializable]
    public class HoldMonster : HoldCreatureBase
    {
        protected override string AimName => @"Living Creature";
        protected override TargetType TargetType => new LivingCreatureTargetType();

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield return new MaterialComponent();
                yield return new FocusComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<SpellComponent> DivineComponents
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

        public override string DisplayName => @"Hold Monster";
    }

    [Serializable]
    public class HoldEffect : PredispositionBase, IActionProvider, IActionSource
    {
        public HoldEffect(MagicPowerEffect magicPowerEffect, SaveMode saveMode)
            : base(magicPowerEffect)
        {
            _Level = new Deltable(1);
            _SaveMode = saveMode;
        }

        #region data
        private IVolatileValue _Level;
        private SaveMode _SaveMode;
        #endregion

        public SaveMode SaveMode => _SaveMode;

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter?.Actions.Providers.Add(this, this);
            _critter?.Conditions.Add(new Condition(Condition.Paralyzed, this));
            _critter?.Abilities.Strength.SetZeroHold(this, true);
            _critter?.AddAdjunct(new Immobilized(this, false));
            base.OnActivate(source);
        }
        #endregion

        #region OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter?.Conditions.Remove(_critter.Conditions[Condition.Paralyzed, this]);
            _critter?.Abilities.Strength.SetZeroHold(this, false);
            _critter?.Adjuncts.OfType<Immobilized>().FirstOrDefault(_i => _i.Source == this)?.Eject();
            _critter?.Actions.Providers.Remove(this);
            base.OnDeactivate(source);
        }
        #endregion

        public override string Description
            => @"Held";

        public IVolatileValue ActionClassLevel => _Level;
        public MagicPowerEffect MagicPowerEffect => Source as MagicPowerEffect;

        public override object Clone()
            => new HoldEffect(MagicPowerEffect, SaveMode);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget is LocalActionBudget _budget)
                && _budget.CanPerformTotal)
            {
                yield return new HoldEffectBreakFree(this, @"202");
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => new AdjunctInfo(@"Hold Effect", ID);
    }

    [Serializable]
    public class HoldEffectBreakFree : ActionBase
    {
        public HoldEffectBreakFree(HoldEffect holdEffect, string orderKey)
            : base(holdEffect, new ActionTime(Contracts.TimeType.Total), false, false, orderKey)
        {
        }

        public override string Key => @"HoldEffect.BreakFree";
        public override string DisplayName(CoreActor actor)
            => @"As a total action, attempt to re-make save versus being magically held.";
        public override bool IsMental => true;

        public HoldEffect HoldEffect => ActionSource as HoldEffect;

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new RollAim(@"Save.Will", @"Remake Will Save Roll", new DieRoller(20));
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _roll = activity.GetFirstTarget<ValueTarget<int>>(@"Save.Will");
            if ((_roll != null)
                && (HoldEffect.Anchor is Creature _critter))
            {
                var _save = new SavingThrowData(_critter, HoldEffect.SaveMode, new Deltable(_roll.Value));

                // let target's handlers alter the roll if possible
                _critter.HandleInteraction(new Interaction(_critter, HoldEffect, _critter, _save));

                if (_save.Success(new Interaction(_critter, HoldEffect, _critter, _save)))
                {
                    // remove effect sourcing the HoldEffect
                    HoldEffect.MagicPowerEffect?.Eject();
                }
            }
            return new RegisterActivityStep(activity, Budget);
        }
    }
}
