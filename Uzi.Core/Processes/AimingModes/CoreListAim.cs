using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class CoreListAim : AimingMode
    {
        public CoreListAim(string key, string displayName, Range minModes, Range maxModes, IEnumerable<CoreInfo> infos)
            : base(key, displayName, minModes, maxModes)
        {
            _Infos = infos.ToList();
        }

        public CoreListAim(string key, string displayName, Range minModes, Range maxModes, IEnumerable<CoreInfo> infos, string preposition)
            : base(key, displayName, minModes, maxModes, preposition)
        {
            _Infos = infos.ToList();
        }

        #region data
        private List<CoreInfo> _Infos;
        #endregion

        public List<CoreInfo> Infos => _Infos;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<CoreInfoTargetInfo>(actor, action, infos)
                .Select(_i => new ValueTarget<CoreInfo>(_i.Key, _i.CoreInfo));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<CoreListAimInfo>(action, actor);
            _info.ObjectInfos = Infos.ToArray();
            return _info;
        }
    }
}
