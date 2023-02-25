using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class CompositeAim : AimingMode
    {
        #region state
        private readonly List<AimingMode> _Components;
        #endregion

        public CompositeAim(string key, string displayName, Range minModes, Range maxModes,
            params AimingMode[] components)
            : base(key, displayName, minModes, maxModes)
        {
            _Components = components.ToList();
        }

        public List<AimingMode> Components => _Components;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            return infos
                .OfType<CompositeTargetInfo>()
                .Where(_i => _i.Key.Equals(Key, StringComparison.OrdinalIgnoreCase))
                .Select(_i => new CompositeTarget(actor, action, _i.Key, Components, _i.Components, provider));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<CompositeAimInfo>(action, actor);
            _info.Components = Components.Select(_c => _c.ToAimingModeInfo(action, actor)).ToList();
            return _info;
        }
    }
}
