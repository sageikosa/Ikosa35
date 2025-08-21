using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ForceOpen : ActionBase
    {
        public ForceOpen(IActionSource source, IOpenable openable, OptionAim option, string orderKey)
            : base(source, new ActionTime(TimeType.Total), true, false, orderKey)
        {
            _Openable = openable;
            _Option = option;
        }

        #region data
        private IOpenable _Openable;
        private OptionAim _Option;
        #endregion

        public IOpenable Openable => _Openable;
        public OptionAim OptionAim => _Option;

        public override string Key => @"Openable.Force";
        public override string DisplayName(CoreActor actor) => @"Force Open";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Force Open", activity.Actor, observer, Openable as ICoreObject);

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            if (OptionAim != null)
            {
                yield return _Option;
            }

            yield return new RollAim(@"Strength", @"Strength Check Roll", new DieRoller(20));
            yield break;
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // TODO: force open should have some extra sound beyond the IOpenable sound generation
            // consume budget
            activity.EnqueueRegisterPreEmptively(Budget);

            if (Openable is IInteract _iAct)
            {
                // work through force open (might handle stuck/blocked/arcane locked)
                var _interact = new Interaction(activity.Actor, ActionSource, _iAct, new ForceOpenData(activity));
                _iAct.HandleInteraction(_interact);
                var _source = _interact.Feedback.OfType<ValueFeedback<object>>().FirstOrDefault()?.Value;
                if (_source != null)
                {
                    // whatever feedback source is used, sources the opening
                    var _step = new StartOpenCloseStep(activity, _Openable, activity.Actor, _source, 1);
                    _step.AppendFollowing(activity.GetActivityResultNotifyStep(@"Force Open"));
                    _step.AppendFollowing(activity.GetNotifyStep(
                        new RefreshNotify(false, true, true, false, false)));
                    return _step;
                }
            }
            return activity.GetActivityResultNotifyStep(@"Nothing happens");
        }
    }
}
