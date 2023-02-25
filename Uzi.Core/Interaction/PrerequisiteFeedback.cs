using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>
    /// Prerequisites that must be fulfilled
    /// </summary>
    [Serializable]
    public abstract class PrerequisiteFeedback : InteractionFeedback
    {
        public PrerequisiteFeedback(object source)
            :base(source)
        {
            _Yielded = false;
        }

        public abstract IEnumerable<StepPrerequisite> Prerequisites { get; }

        private bool _Yielded;

        /// <summary>
        /// True if the feedback has already yielded its prerequisites (defaults to false)
        /// </summary>
        public bool Yielded
        {
            get { return _Yielded; }
            set { _Yielded = value; }
        }
    }
}
