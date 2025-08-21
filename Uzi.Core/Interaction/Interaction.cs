using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>Used to transit interactions.</summary>
    [Serializable]
    public class Interaction : Qualifier
    {
        /// <summary>Used to transit interactions.</summary>
        public Interaction(CoreActor actor, object source, IInteract target, InteractData interactData,
            bool useDefault = true)
            : base(actor, source, target)
        {
            _InteractData = interactData;
            _Feedback = [];
            _UseDefault = useDefault;
        }

        #region data
        private InteractData _InteractData;
        private List<InteractionFeedback> _Feedback;
        private bool _UseDefault;
        #endregion

        public InteractData InteractData => _InteractData;

        /// <summary>Indicates interaction will use default handler if no feedback found through routed handling</summary>
        public bool UseDefault => _UseDefault;

        /// <summary>Default handler (or null) to be used if routed handling provides no feedback</summary>
        public IEnumerable<IInteractHandler> DefaultHandlers
        {
            get
            {
                if (UseDefault && (InteractData != null))
                {
                    foreach (var _h in InteractData.GetDefaultHandlers(Target))
                    {
                        yield return _h;
                    }
                }
                yield break;
            }
        }

        /// <summary>
        /// List of feedbacks provided by handling the interaction.  
        /// <para>Providing any feedback will prevent further HandleInteraction calls.</para>
        /// <para>To provide context without blocking further processing, use Alterations on the InteractData.</para>
        /// <para>To process feedback, use IProcessFeedback.</para>
        /// </summary>
        public List<InteractionFeedback> Feedback => _Feedback;
    }
}
