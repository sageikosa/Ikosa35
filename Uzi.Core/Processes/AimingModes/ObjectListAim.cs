using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Actor Integration layer must create instances of Uzi.Core.AimTarget
    /// </summary>
    [Serializable]
    public class ObjectListAim : AimingMode
    {
        /// <summary>
        /// Actor Integration layer must create instances of Uzi.Core.AimTarget
        /// </summary>
        public ObjectListAim(string key, string displayName, Range minModes, Range maxModes, IEnumerable<ICoreObject> objects)
            : base(key, displayName, minModes, maxModes)
        {
            _List = objects;
        }
        /// <summary>
        /// Actor Integration layer must create instances of Uzi.Core.AimTarget
        /// </summary>
        public ObjectListAim(string key, string displayName, Range minModes, Range maxModes, IEnumerable<ICoreObject> objects, string preposition)
            : base(key, displayName, minModes, maxModes, preposition)
        {
            _List = objects;
        }

        private readonly IEnumerable<ICoreObject> _List;
        public IEnumerable<ICoreObject> ListObjects => _List; 

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, 
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<AimTargetInfo>(actor, action, infos)
                .Select(_i => new AimTarget(_i.Key, provider.GetIInteract(_i.TargetID ?? Guid.Empty)));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<ObjectListAimInfo>(action, actor);
            _info.ObjectInfos = (from _listed in ListObjects
                                 let _obj = GetInfoData.GetInfoFeedback(_listed, actor) as CoreInfo
                                 where _obj != null
                                 select _obj).ToArray();
            return _info;
        }
    }
}
