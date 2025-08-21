using System;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class DismissSpell : ActionBase
    {
        // NOTE: if spell has verbal components, verbal dismissal; make sure can use
        // NOTE: if spell has no verbal components, gesture dismissal; make sure can use

        #region ctor()
        public DismissSpell(DurableMagicEffect effect, CoreActor actor, string orderKey)
            : this(effect, actor, new ActionTime(TimeType.Regular), orderKey)
        {
        }

        public DismissSpell(DurableMagicEffect effect, CoreActor actor, ActionTime timeCost, string orderKey)
            : base(effect.MagicPowerActionSource, timeCost, false, false, orderKey)
        {
            _Effect = effect;
            _Actor = actor;
        }
        #endregion

        #region state
        private DurableMagicEffect _Effect;
        private CoreActor _Actor;
        #endregion

        /// <summary>Make sure all spell components can be used, before effort can even be expended</summary>
        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            if (_Effect.MagicPowerActionSource is SpellSource _spellSource)
            {
                // only if it is a spell
                var _components = (_spellSource.CasterClass.MagicType == Contracts.MagicType.Arcane)
                   ? _spellSource.SpellDef.ArcaneComponents.ToList()
                   : _spellSource.SpellDef.DivineComponents.ToList();
                if (_components.OfType<VerbalComponent>().Any())
                {
                    _components = _components.OfType<VerbalComponent>().Cast<SpellComponent>().ToList();
                }
                else
                {
                    _components = (new[] { new SomaticComponent() }).Cast<SpellComponent>().ToList();
                }
                if (_components.All(_c => _c.CanStartActivity(budget.Actor as Creature)))
                {
                    return base.CanPerformNow(budget);
                }
            }

            return new ActivityResponse(false);
        }

        private Info GetAnchorInfo()
            => GetInfoData.GetInfoFeedback(_Effect.Anchor as ICoreObject, _Actor);

        public override string DisplayName(CoreActor actor)
            => $@"Dismiss {_Effect.MagicPowerActionSource.DisplayName} on {GetAnchorInfo()?.Message}";

        public override string Key
            => $@"Spell.Dismiss";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => Enumerable.Empty<AimingMode>();

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _info = ObservedActivityInfoFactory.CreateInfo(DisplayName(observer), activity.Actor, observer);
            if ((observer == activity?.Actor) && (activity?.Actor != null))
            {
                //_info.Details = new Info { Message = SpellMode.Description };
            }
            return _info;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // use budget
            activity.EnqueueRegisterPreEmptively(Budget);

            var _dismissStep = new DismissDurableMagicEffectStep(activity, _Effect);

            if (_Effect.MagicPowerActionSource is SpellSource _spellSource)
            {
                // only if it is a spell
                var _components = (_spellSource.CasterClass.MagicType == Contracts.MagicType.Arcane)
                   ? _spellSource.SpellDef.ArcaneComponents.ToList()
                   : _spellSource.SpellDef.DivineComponents.ToList();
                if (_components.OfType<VerbalComponent>().Any())
                {
                    _components = _components.OfType<VerbalComponent>().Cast<SpellComponent>().ToList();
                }
                else
                {
                    _components = (new[] { new SomaticComponent() }).Cast<SpellComponent>().ToList();
                }
                var _start = _components.All(_c => _c.WillUseSucceed(activity)) || !_components.Any();

                // use components: doesn't matter if dismiss start is successful, we tried to use them
                foreach (var _c in _components)
                {
                    activity.Targets.Add(new SpellComponentFinalizeTarget(_c));
                    _c.StartUse(activity);
                }

                if (_start)
                {
                    // make sure all components complete successfully
                    // NOTE: this step is the "normal" next step, so pre-emptions happen before it
                    return new SpellComponentCheck(activity, _dismissStep);
                }

                // unable to start using the spell (but made the effort and used the components)
                return activity.GetActivityResultNotifyStep(@"Couldn't use dismiss spell components");
            }

            // not a spell, simple dismiss
            return _dismissStep;
        }
    }
}
