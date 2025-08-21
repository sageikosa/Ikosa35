using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase]</summary>
    [Serializable]
    public class CastSpell : PowerUse<SpellSource>, IDistractable
    {
        #region Constructor
        /// <summary>[ActionBase]</summary>
        public CastSpell(SpellSource source, ISpellMode mode, ActionTime actTime, SpellSlot slot, string orderKey)
            : base(source, actTime, actTime.ActionTimeType != TimeType.Twitch, false, orderKey)
        {
            _SpellMode = mode;
            _Slot = slot;
            _ConBase = new Deltable(PowerActionSource.PowerLevel);
            _Spellcrafters = [];
        }

        /// <summary>[ActionBase]</summary>
        protected CastSpell(SpellSource source, ISpellMode mode, ActionTime actTime, SpellSlot slot, bool provokesMelee, string orderKey)
            : base(source, actTime, provokesMelee, false, orderKey)
        {
            _SpellMode = mode;
            _Slot = slot;
            _ConBase = new Deltable(PowerActionSource.PowerLevel);
            _Spellcrafters = [];
        }
        #endregion

        #region state
        private ISpellMode _SpellMode;
        private SpellSlot _Slot;
        private Deltable _ConBase;
        private PowerAffectTracker _Tracker;
        private Dictionary<Guid, bool> _Spellcrafters;
        #endregion

        public SpellSource SpellSource => Source as SpellSource;
        public SpellSlot Slot => _Slot;
        public ISpellMode SpellMode => _SpellMode;
        public override ICapabilityRoot CapabilityRoot => SpellMode;

        /// <summary>casters that made a spell-craft check and whether is succeeded</summary>
        public Dictionary<Guid, bool> SpellCrafters => _Spellcrafters;

        public override bool IsMental
            => (PowerActionSource.CasterClass.MagicType == MagicType.Arcane)
            ? PowerActionSource.SpellDef.ArcaneComponents.Any()
            : PowerActionSource.SpellDef.DivineComponents.Any();

        public override string Key
            => ((Slot is PreparedSpellSlot _prepSlot)
            && (_prepSlot.PreparedSpell != null)
            && (_prepSlot.PreparedSpell.SpellDef != PowerActionSource.SpellDef))
            ? $@"Magic.CastSpell.{PowerActionSource.MagicPowerActionDef.Key}[{_prepSlot.PreparedSpell.SpellDef.Key},{Slot.SlotLevel}]"
            : $@"Magic.CastSpell.{PowerActionSource.MagicPowerActionDef.Key}[{Slot.SlotLevel}]";

        public override string DisplayName(CoreActor actor)
        {
            if (SpellSource.CasterClass.OwnerID == actor.ID)
            {
                // caster knows everything about the spell
                if ((Slot is PreparedSpellSlot _prepSlot)
                    && (_prepSlot.PreparedSpell != null)
                    && (_prepSlot.PreparedSpell.SpellDef != PowerActionSource.SpellDef))
                {
                    return $@"Cast: {SpellMode.DisplayName}; lose: {_prepSlot.PreparedSpell.SpellDef.DisplayName} (level: {Slot.SlotLevel})";
                }
                return $@"Cast: {SpellMode.DisplayName}";
            }
            return @"Spell-casting";
        }

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            // TODO: actor == caster?  spellcraft?
            var _info = ObservedActivityInfoFactory.CreateInfo(DisplayName(observer), activity.Actor, observer);
            if ((observer == activity?.Actor) && (activity?.Actor != null))
            {
                _info.Details = new Info { Message = SpellMode.Description };
            }
            return _info;
        }

        /// <summary>Overrides default IsHarmless false setting, and sets it to true for attack actions</summary>
        public override bool IsHarmless
            => SpellMode.IsHarmless;

        /// <summary>Make sure all spell components can be used, before effort can even be expended</summary>
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            var _components = (PowerActionSource.CasterClass.MagicType == Contracts.MagicType.Arcane)
               ? PowerActionSource.SpellDef.ArcaneComponents.ToList()
               : PowerActionSource.SpellDef.DivineComponents.ToList();
            if (_components.All(_c => _c.CanStartActivity(budget.Actor as Creature)) || !_components.Any())
            {
                return BaseCanPerformNow(budget);
            }

            return new ActivityResponse(false);
        }

        /// <summary>Used by derived classes that want to avoid the CastSpell spell component checks</summary>
        protected ActivityResponse BaseCanPerformNow(CoreActionBudget budget)
            => base.CanPerformNow(budget);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // use budget: doesn't matter if the spell can even start, effort was made
            activity.EnqueueRegisterPreEmptively(Budget);

            // meet component requirements...
            var _components = ((PowerActionSource.CasterClass.MagicType == Contracts.MagicType.Arcane)
                ? PowerActionSource.SpellDef.ArcaneComponents
                : PowerActionSource.SpellDef.DivineComponents).ToList();

            var _start = _components.All(_c => _c.WillUseSucceed(activity)) || !_components.Any();

            // use components: doesn't matter if spell start is successful, we tried to use them
            foreach (var _c in _components)
            {
                activity.Targets.Add(new SpellComponentFinalizeTarget(_c));
                _c.StartUse(activity);
            }

            // if start was successful
            if (_start)
            {
                // use slot (pre-emptively, will be added before hold-process)
                activity.AppendPreEmption(new SpellSlotUse(activity, Slot, _Budget.TurnTick.TurnTracker.Map.CurrentTime));

                // make sure all components complete successfully
                // NOTE: this step is the "normal" next step, so pre-emptions happen before it
                return new SpellComponentCheck(activity,
                    new PowerActivationStep<SpellSource>(activity, this, activity.Actor));
            }

            // unable to start using the spell (but made the effort and used the components)
            return activity.GetActivityResultNotifyStep(@"Spell start failure");
        }

        public void Interrupted()
        {
            Slot.UseSlot(_Budget.TurnTick.TurnTracker.Map.CurrentTime);
        }

        /// <summary>Passes through to the underlying spell mode for this action</summary>
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            foreach (var _aimMode in SpellMode.AimingMode(activity.Actor, SpellMode))
            {
                yield return _aimMode;
            }

            yield break;
        }

        public override void ActivatePower(PowerActivationStep<SpellSource> step)
        {
            SpellMode.ActivateSpell(step);
        }

        public override void ApplyPower(PowerApplyStep<SpellSource> step)
        {
            SpellMode.ApplySpell(step);
        }

        public override PowerAffectTracker PowerTracker
            => _Tracker ??= new PowerAffectTracker();

        // ISpanConcentration Members
        public Deltable ConcentrationBase => _ConBase;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
