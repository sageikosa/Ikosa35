using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Time
{
    /// <summary>
    /// When action time is completed, FreshMind is reapplied
    /// </summary>
    [Serializable]
    public class RegainFreshMind : SimpleActionBase
    {
        /// <summary>
        /// When action time is completed, FreshMind is reapplied
        /// </summary>
        public RegainFreshMind(ActionTime actionTime)
            : base(null, actionTime, true, false, @"101")
        {
        }

        public override string Key => @"Timeline.RegainFreshMind";
        public override string DisplayName(CoreActor actor) => @"Regain Fresh Mind after Interruption";
        public override bool IsMental => true;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Meditation", activity.Actor, observer);

        protected override NotifyStep OnSuccessNotify(CoreActivity activity)
            => activity.GetActivityResultNotifyStep(@"Fresh mind regained");

        public override bool DoStep(CoreStep actualStep)
        {
            (actualStep.Process as CoreActivity)?.Actor?.Adjuncts.OfType<FreshMind>().FirstOrDefault()?.Eject() ;
            return true;
        }
    }
}
