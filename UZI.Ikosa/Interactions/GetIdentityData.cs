using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Get all identities of which an actor is aware
    /// </summary>
    public class GetIdentityData : InteractData
    {
        /// <summary>
        /// Get all identities of which an actor is aware
        /// </summary>
        public GetIdentityData(CoreActor actor)
            : base(actor)
        {
            _Actor = actor;
        }

        private CoreActor _Actor;
        public CoreActor Actor => _Actor;

        /// <summary>Provides info about an object as known by the actor</summary>
        public static List<Identity> GetIdentities(ICoreObject obj, CoreActor actor)
        {
            if (!(obj?.HasAdjunct<Identity>() ?? false))
            {
                return [];
            }

            var _idInteract = new Interaction(actor, typeof(GetIdentityData), obj, new GetIdentityData(actor));
            if (obj != null)
            {
                obj.HandleInteraction(_idInteract);
            }
            return _idInteract.Feedback.OfType<GetIdentityDataFeedback>().FirstOrDefault()?.Identities
                ?? [];
        }

        /// <summary>Provides info about an object as known by the actor</summary>
        public static List<IdentityInfo> GetIdentityInfos(ICoreObject obj, CoreActor actor)
        {
            var _idInteract = new Interaction(actor, typeof(GetIdentityData), obj, new GetIdentityData(actor));
            if (obj != null)
            {
                obj.HandleInteraction(_idInteract);
            }
            return _idInteract.Feedback.OfType<GetIdentityDataFeedback>().FirstOrDefault()?.Identities
                .Select(_i => new IdentityInfo
                {
                    InfoID = _i.MergeID ?? Guid.Empty,
                    IsActive = _i.Users.Contains(actor.ID),
                    ObjectInfo = _i.Infos.FirstOrDefault() as ObjectInfo
                })
                .ToList()
                ?? [];
        }

        private static GetIdentityDataHandler _Static = new GetIdentityDataHandler();

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            yield return _Static;
            yield break;
        }
    }

    /// <summary>
    /// Build feedback bucket
    /// </summary>
    public class GetIdentityDataFeedback : InteractionFeedback
    {
        /// <summary>
        /// Build feedback bucket
        /// </summary>
        public GetIdentityDataFeedback(object source)
            : base(source)
        {
            _Identities = [];
        }

        private List<Identity> _Identities;

        public List<Identity> Identities => _Identities;
    }

    /// <summary>
    /// Build feedback bucket
    /// </summary>
    [Serializable]
    public class GetIdentityDataHandler : IInteractHandler
    {
        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetIdentityData);
            yield break;
        }

        public void HandleInteraction(Interaction workSet)
        {
            if (workSet?.InteractData is GetIdentityData)
            {
                workSet.Feedback.Add(new GetIdentityDataFeedback(this));
            }
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // always last processor, post-processors collect identities
            return false;
        }
    }
}
