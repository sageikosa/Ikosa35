using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class CompositeTarget : AimTarget
    {
        private readonly List<AimTarget> _Components;

        public CompositeTarget(CoreActor actor, CoreAction action,
            string key, List<AimingMode> aimModes, List<AimTargetInfo> targets, IInteractProvider provider)
            : base(key, null)
        {
            _Components = (from _mode in aimModes
                           from _t in _mode.GetTargets(actor, action, targets.ToArray(), provider)
                           select _t).ToList();
        }

        public List<AimTarget> Components => _Components;

        public override AimTargetInfo GetTargetInfo()
            => new CompositeTargetInfo
            {
                Key = Key,
                TargetID = Target?.ID,
                Components = Components.Select(_c => _c.GetTargetInfo()).ToList()
            };
    }
}
