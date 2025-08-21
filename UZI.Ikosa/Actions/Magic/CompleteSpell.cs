using System.Linq;
using System.Collections.Generic;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Magic;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Complete a spell completion spell
    /// </summary>
    public class CompleteSpell : CastSpell
    {
        #region Construction
        public CompleteSpell(SpellCompletion spellComplete, ISpellMode mode, ActionTime actTime, string orderKey)
            : base(spellComplete.SpellSource, mode, actTime, null, orderKey)
        {
            _Completion = spellComplete;
        }
        #endregion

        #region state
        private SpellCompletion _Completion;
        #endregion

        public SpellCompletion SpellCompletion => _Completion;
        public override string Key => $@"Item.CompleteSpell.{SpellCompletion.ID}";
        public override string DisplayName(CoreActor actor)
            => $@"Complete Spell: {SpellMode.DisplayName}; {SpellMode.Description}";

        public override ActivityResponse CanPerformNow(CoreActionBudget budget)
        {
            var _components = ((PowerActionSource.CasterClass.MagicType == Contracts.MagicType.Arcane)
                ? PowerActionSource.SpellDef.ArcaneComponents
                : PowerActionSource.SpellDef.DivineComponents)
                .Where(_c => _c is VerbalComponent || _c is SomaticComponent)
                .ToList();
            if (_components.All(_c => _c.CanStartActivity(budget.Actor as Creature)) || !_components.Any())
            {
                return BaseCanPerformNow(budget);
            }

            return new ActivityResponse(false);
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _info = base.GetActivityInfo(activity, observer);
            _info.Implement = GetInfoData.GetInfoFeedback(SpellCompletion.Anchor as ICoreObject, observer);
            return _info;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // NOTE: after spell completion, the adjunct is defunct
            activity.EnqueueRegisterPreEmptively(Budget);

            // meet component requirements...(verbal and gesture only)
            var _components = ((PowerActionSource.CasterClass.MagicType == Contracts.MagicType.Arcane)
                ? PowerActionSource.SpellDef.ArcaneComponents
                : PowerActionSource.SpellDef.DivineComponents)
                .Where(_c => _c is VerbalComponent || _c is SomaticComponent)
                .ToList();
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
                // make sure all components complete successfully
                return new SpellComponentCheck(activity, new SpellCompletionStep(activity));
            }

            // unable to start using the spell (but made the effort and used the components)
            return activity.GetActivityResultNotifyStep(@"Spell start failure");
        }
    }
}
