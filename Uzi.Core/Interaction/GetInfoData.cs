using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>Used to probe objects for InfoFeedback (and thus IInfo)</summary>
    public class GetInfoData : InteractData
    {
        /// <summary>Used to probe objects for InfoFeedback (and thus IInfo)</summary>
        public GetInfoData(CoreActor actor)
            : base(actor)
        {
        }

        /// <summary>Provides info about an object as known by the actor</summary>
        public static Info GetInfoFeedback(ICoreObject obj, CoreActor actor)
        {
            var _infoInteract = new Interaction(actor, typeof(GetInfoData), obj, new GetInfoData(actor));
            obj?.HandleInteraction(_infoInteract);
            return obj?.MergeConnectedInfos(_infoInteract.Feedback.OfType<InfoFeedback>().FirstOrDefault()?.Information, actor);
        }

        private static readonly GetInfoDataHandler _Static = new GetInfoDataHandler();
        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }
    }
}
