using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// [ActionBase (Brief)]
    /// </summary>
    [Serializable]
    public class UseKey : ActionBase
    {
        /// <summary>
        /// [ActionBase (Brief)]
        /// </summary>
        public UseKey(KeyItem keyItem, string orderKey)
            : base(keyItem, new ActionTime(TimeType.Brief), true, false, orderKey)
        {
        }

        public KeyItem KeyItem => Source as KeyItem;

        public override string Key => @"Move.UseKey";
        public override string DisplayName(CoreActor actor) => $@"Use Key: {KeyItem.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Use Key", activity.Actor, observer, KeyItem);

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _target = activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Object"));
            if ((_target != null)
                && (activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Secure")) is OptionTarget _option))
            {
                if (_target.Target is ISecureLock _lock)
                {
                    // if the target isn't securable, there is no feedback to the character
                    var _success = KeyItem.IsCompatible(_lock.Keys);
                    if (_option.Option.Key.Equals(@"Lock"))
                    {
                        _lock.SecureLock(activity.Actor, KeyItem, _success);
                    }
                    else
                    {
                        _lock.UnsecureLock(activity.Actor, KeyItem, _success);
                    }
                }

                // if successfully targeted, register activity
                activity.EnqueueRegisterPreEmptively(Budget);
                return activity.GetActivityResultNotifyStep(@"Key has been used");
            }
            else
            {
                return activity.GetActivityResultNotifyStep(@"Lockable target and/or lock mode not provided.");
            }
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AwarenessAim(@"Object", @"Securable Object", FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield return new OptionAim(@"Secure", @"Lock/Unlock", true, FixedRange.One, FixedRange.One, UseKeyOptions());
            yield break;
        }
        #endregion

        #region private static IEnumerable<OptionAimOption> UseKeyOptions()
        private static IEnumerable<OptionAimOption> UseKeyOptions()
        {
            yield return new OptionAimOption() { Key = @"Lock", Name = @"Lock", Description = @"Attempt to lock with key" };
            yield return new OptionAimOption() { Key = @"Unlock", Name = @"UnLock", Description = @"Attempt to unlock with key" };
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
