using Uzi.Core.Contracts;
using System;
using Uzi.Core;

namespace Uzi.Ikosa.Actions.Steps
{
    /// <summary>
    /// Used to indicate that the step has added divination information for the creature
    /// </summary>
    [Serializable]
    public class ExtraInfoStep : CoreStep, ISourcedObject
    {
        public ExtraInfoStep(CoreActivity activity, object source) :
            base(activity)
        {
            _Source = source;
        }

        private object _Source;

        /// <summary>Indicates the divination source to be used for reference in the creature's ExtraInfo</summary>
        public object Source => _Source;
        public CoreActivity Activity => Process as CoreActivity;

        protected override bool OnDoStep()
        {
            AppendFollowing(Activity.GetActivityResultNotifyStep(@"See Divination Info for details"));
            return true;
        }

        protected override StepPrerequisite OnNextPrerequisite() => null;
        public override bool IsDispensingPrerequisites => false;
    }
}
