using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class QuantitySelectAim : AimingMode
    {
        public QuantitySelectAim(string key, string displayName, Range minModes, Range maxModes, 
            IEnumerable<QuantitySelectorInfo> selectors)
            : base(key, displayName, minModes, maxModes)
        {
            _Selectors = selectors.ToList();
        }

        public QuantitySelectAim(string key, string displayName, Range minModes, Range maxModes, 
            IEnumerable<QuantitySelectorInfo> selectors, string preposition)
            : base(key, displayName, minModes, maxModes, preposition)
        {
            _Selectors = selectors.ToList();
        }

        #region data
        private List<QuantitySelectorInfo> _Selectors;
        #endregion

        public List<QuantitySelectorInfo> Selectors => _Selectors;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<QuantitySelectTargetInfo>(actor, action, infos)
                .Select(_i => new ValueTarget<QuantitySelectTargetInfo>(_i.Key, _i));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<QuantitySelectAimInfo>(action, actor);
            _info.Selectors = Selectors.ToArray();
            return _info;
        }
    }
}
