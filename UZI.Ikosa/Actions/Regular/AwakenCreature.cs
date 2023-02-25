using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class AwakenCreature : ActionBase
    {
        public AwakenCreature(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
        }

        public override string Key => @"AwakenCreature";
        public override string DisplayName(CoreActor actor) => @"Awaken Creature from Sleep or Fascination";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Awaken", activity.Actor, observer, activity.Targets[0].Target as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            AimTarget _target = activity.Targets.FirstOrDefault(_t => _t.Key.Equals(@"Creature"));
            if (_target != null)
            {
                Creature _critter = _target.Target as Creature;
                if (_critter != null)
                {
                    SleepEffect _sleep = _critter.Adjuncts.FirstOrDefault(_adj => typeof(SleepEffect).IsAssignableFrom(_adj.GetType())) as SleepEffect;
                    if (_sleep != null)
                    {
                        _sleep.Awaken();
                    }
                    else
                    {
                        FascinatedEffect _fascinate = _critter.Adjuncts.FirstOrDefault(_adj => typeof(SleepEffect).IsAssignableFrom(_adj.GetType())) as FascinatedEffect;
                        if (_fascinate != null)
                        {
                            _fascinate.SnapOutOfIt();
                        }
                    }

                }
            }
            return null;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AwarenessAim(@"Creature", @"Creature", FixedRange.One, FixedRange.One, new MeleeRange(), new CreatureTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
