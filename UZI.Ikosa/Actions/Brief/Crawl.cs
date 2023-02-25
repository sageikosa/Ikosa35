using System;
using Uzi.Core;
using Uzi.Ikosa.Movement;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using System.Collections.Generic;

namespace Uzi.Ikosa.Actions
{
    /// <summary>[ActionBase (Brief)]</summary>
    [Serializable]
    public class Crawl : MovementAction
    {
        /// <summary>[ActionBase (Brief)]</summary>
        public Crawl(MovementBase source)
            : base(source, new ActionTime(TimeType.Brief), true)
        {
        }

        public override string Key => @"Movement.Crawl";
        public override string DisplayName(CoreActor actor) => @"Crawl";

        public override bool IsStackBase(CoreActivity activity) 
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Crawl", activity.Actor, observer);

        public override CoreActivity NextMoveInSequence(CoreActivity activity, List<AimTarget> targets) 
            => null;

        // TODO: override OnMoveCostCheck?
    }
}
