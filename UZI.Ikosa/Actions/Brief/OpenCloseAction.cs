using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Opens or closes an openable (if possible) [ActionBase (Brief)]</summary>
    [Serializable]
    public class OpenCloseAction : ActionBase
    {
        /// <summary>Opens or closes an openable (if possible) [ActionBase (Brief)]</summary>
        public OpenCloseAction(IActionSource source, IOpenable openable, string orderKey)
            : base(source, new ActionTime(TimeType.Brief), false, false, orderKey)
        {
            _Openable = openable;
        }

        private IOpenable _Openable;
        public IOpenable Openable => _Openable;

        public override string Key => @"Move.OpenClose";
        public override string DisplayName(CoreActor actor) => (Openable?.OpenState.IsClosed ?? false) ? @"Open" : @"Close";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Open/Close", activity.Actor, observer, Openable as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _opt = (activity.Targets[0] as OptionTarget).Option;
            var _newVal = (_opt as OptionAimValue<double>).Value;

            // register (might hold process)
            activity.EnqueueRegisterPreEmptively(Budget);

            // after registration, open/close
            var _step = new StartOpenCloseStep(activity, _Openable, activity.Actor, this, _newVal);
            _step.AppendFollowing(activity.GetActivityResultNotifyStep(_opt.Name));
            _step.AppendFollowing(activity.GetNotifyStep(
                new RefreshNotify(false, true, true, false, false)));
            return _step;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"OpenState", @"Open Percent", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        #region private IEnumerable<OptionAimOption> Options()
        private IEnumerable<OptionAimOption> Options()
        {
            yield return new OptionAimValue<double>()
            {
                Key = @"0",
                Name = @"Close",
                Value = 0d,
                IsCurrent = Openable?.OpenState.IsClosed ?? false
            };
            yield return new OptionAimValue<double>()
            {
                Key = @"1",
                Name = @"Open",
                Value = 1d,
                IsCurrent = !(Openable?.OpenState.IsClosed ?? false)
            };
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    /// <summary>Opens or closes an openable, and relocks a specific previously locked lock when done. [ActionBase (Brief)]</summary>
    [Serializable]
    public class OpenCloseLockedAction : OpenCloseAction
    {
        /// <summary>Opens or closes an openable, and relocks a specific previously locked lock when done. [ActionBase (Brief)]</summary>
        public OpenCloseLockedAction(IActionSource source, IOpenable openable, LockGroup lockTarget, string orderKey)
            : base(source, openable, orderKey)
        {
            _Lock = lockTarget;
        }

        private LockGroup _Lock;
        public LockGroup Lock => _Lock;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // TODO: consider stepifying this more...
            var _relock = _Lock.IsLocked;
            _Lock.IsLocked = false;

            CoreStep _step = base.OnPerformActivity(activity);

            if (_relock)
            {
                _Lock.IsLocked = true;
            }

            return _step;
        }
    }
}
