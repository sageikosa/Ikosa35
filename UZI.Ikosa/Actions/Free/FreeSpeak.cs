using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class FreeSpeak : ActionBase
    {
        public FreeSpeak(IActionSource source, string orderKey)
            : base(source, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }

        public override string Key => @"Speak";
        public override string DisplayName(CoreActor actor) => @"Speak Word(s) in a Known Language";

        public override bool IsMental => false;

        public override bool IsStackBase(CoreActivity activity)
        {
            return false;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            return ObservedActivityInfoFactory.CreateInfo(@"Speak", activity.Actor, observer);
        }

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // TODO: may be suppressed if locator is in zone of silence
            // TODO: sound transit step (range?)
            throw new NotImplementedException();
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            Creature _critter = activity.Actor as Creature;
            // TODO: character string aim (limit character length for free speak)
            yield return new OptionAim(@"Language", @"Language", true, FixedRange.One, FixedRange.One, _critter.Languages.ProjectableLanguageOptions);
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
