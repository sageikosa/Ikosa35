using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase (Total)]</summary>
    [Serializable]
    public class PickLockAction : ActionBase
    {
        /// <summary>[ActionBase (Total)]</summary>
        public PickLockAction(ISecureLock keySlot, string orderKey)
            : base(keySlot, new ActionTime(TimeType.Total), true, false, orderKey)
        {
        }

        public ISecureLock SecureLock => Source as ISecureLock;

        public override string Key => @"FullRound.PickLock";
        public override string DisplayName(CoreActor actor) => $@"Pick Lock {SecureLock.GetKnownName(actor)}";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
            => new ActivityResponse((activity.Actor as Creature)?.Skills.Skill<PickLockSkill>()?.IsTrained ?? false);

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Reload", activity.Actor, observer, SecureLock as CoreObject);

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _critter = activity.Actor as Creature;
            var _check = activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Check")) as SuccessCheckTarget;
            if (_check != null)
            {
                if (_critter == null)
                {
                    var _tools = _critter.Body.ItemSlots.HeldObjects.OfType<ThievesTools>().FirstOrDefault();
                    Delta _delta = null;
                    if (_tools == null)
                    {
                        // -2 circumstance penalty if no tools...
                        _delta = new Delta(-2, typeof(Deltas.Circumstance<ThievesTools>), @"No Tools in Hand");
                    }
                    else if (_tools.IsMasterwork())
                    {
                        // +2 circumstance boost if masterwork
                        _delta = new Delta(2, typeof(Deltas.Circumstance<ThievesTools>), @"Masterwork Tools in Hand");
                    }

                    // add any circumstance...
                    if (_delta != null)
                    {
                        _check.CheckRoll.Deltas.Add(_delta);
                    }
                }
                SecureLock.UnsecureLock(activity.Actor, activity, _check.Success);
            }
            return null;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            var _pick = (activity.Actor as Creature)?.Skills.Skill<PickLockSkill>();
            if (_pick != null)
            {
                yield return new SuccessCheckAim(@"Check", @"Pick Lock Check", new SuccessCheck(_pick, SecureLock.PickDifficulty.EffectiveValue, SecureLock));
            }
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
