using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public static class ObservedActivityInfoFactory
    {
        public static ObservedActivityInfo CreateInfo(string title, CoreActor actor, CoreActor observer, params ICoreObject[] targets)
        {
            var _obsAct = new ObservedActivityInfo
            {
                Message = title,
                Actor = GetInfoData.GetInfoFeedback(actor, observer),
                ActorID = actor.ID
            };
            if (targets.Length > 0)
            {
                _obsAct.Targets = (from _t in targets
                                   where _t != null
                                   let _info = GetInfoData.GetInfoFeedback(_t, observer)
                                   where _info != null
                                   select _info).ToArray();
            }
            return _obsAct;
        }
    }
}