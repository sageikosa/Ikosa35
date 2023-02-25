using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Interations that handle this should provide <see cref="OpportunisticProvokeFeedback"/> 
    /// which includes a list of <see cref="INegateOpportunity" /> step prerequsities
    /// </summary>
    [Serializable]
    public class OpportunisticProvokeData : InteractData
    {
        /// <summary>
        /// Interations that handle this should provide <see cref="OpportunisticProvokeFeedback"/> 
        /// which includes a list of <see cref="INegateOpportunity" /> step prerequsities
        /// </summary>
        public OpportunisticProvokeData(CoreActivity activity, CoreActor attacker)
            : base(activity.Actor)
        {
            _Activity = activity;
            _Attacker = attacker;
        }

        #region private data
        private CoreActivity _Activity;
        private CoreActor _Attacker;
        #endregion

        public CoreActivity Activity { get { return _Activity; } }
        public CoreActor Attacker { get { return _Attacker; } }

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            // TODO: no default...?
            return base.GetDefaultHandlers(target);
        }
    }

    [Serializable]
    public class OpportunisticProvokeFeedback : InteractionFeedback
    {
        public OpportunisticProvokeFeedback(object source, ISuccessCheckPrerequisite successCheck = null)
            : base(source)
        {
            _Success = successCheck;
        }

        private ISuccessCheckPrerequisite _Success;
        public ISuccessCheckPrerequisite SuccessCheck
        {
            get { return _Success; }
            set { _Success = value; }
        }
    }
}
