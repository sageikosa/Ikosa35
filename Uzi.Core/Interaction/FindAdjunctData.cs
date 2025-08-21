using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    public class FindAdjunctData : InteractData
    {
        public FindAdjunctData(CoreActor actor, params Type[] adjunctTypes)
            : base(actor)
        {
            _AdjunctTypes = adjunctTypes.ToList();
        }

        public FindAdjunctData(CoreActor actor, List<Type> adjunctTypes)
            : base(actor)
        {
            _AdjunctTypes = adjunctTypes;
        }

        private readonly List<Type> _AdjunctTypes;

        public List<Type> AdjunctTypes => _AdjunctTypes;

        public static IEnumerable<Adjunct> FindAdjuncts(ICoreObject coreObject, List<Type> adjunctTypes)
        {
            var _findInteract = new Interaction(null, typeof(FindAdjunctData), coreObject, new FindAdjunctData(null, adjunctTypes));
            coreObject.HandleInteraction(_findInteract);
            foreach (var _found in (from _back in _findInteract.Feedback.OfType<FindAdjunctFeedback>()
                                    from _adj in _back.Adjuncts
                                    select _adj).Distinct())
            {
                yield return _found;
            }

            yield break;
        }

        public static IEnumerable<Adjunct> FindAdjuncts(ICoreObject coreObject, params Type[] adjunctTypes)
        {
            var _findInteract = new Interaction(null, typeof(FindAdjunctData), coreObject, new FindAdjunctData(null, adjunctTypes));
            coreObject.HandleInteraction(_findInteract);
            foreach (var _found in (from _back in _findInteract.Feedback.OfType<FindAdjunctFeedback>()
                                    from _adj in _back.Adjuncts
                                    select _adj).Distinct())
            {
                yield return _found;
            }

            yield break;
        }

        private static readonly FindAdjunctDataHandler _Handler = new FindAdjunctDataHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Handler;
            yield break;
        }
    }
}
