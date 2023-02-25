using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>Aiming mode for selecting one or more options from a list of options [AimingMode]</summary>
    [Serializable]
    public class OptionAim : AimingMode
    {
        #region ctor()
        /// <summary>Aiming mode for selecting one or more options from a list of options [AimingMode]</summary>
        public OptionAim(string key, string displayName, bool noDuplicates, Range minModes, Range maxModes, IEnumerable<OptionAimOption> options)
            : base(key, displayName, minModes, maxModes)
        {
            _Options = options;
            _NoDuplicates = noDuplicates;
        }

        /// <summary>Aiming mode for selecting one or more options from a list of options [AimingMode]</summary>
        public OptionAim(string key, string displayName, bool noDuplicates, Range minModes, Range maxModes, IEnumerable<OptionAimOption> options, string preposition)
            : base(key, displayName, minModes, maxModes, preposition)
        {
            _Options = options;
            _NoDuplicates = noDuplicates;
        }
        #endregion

        #region data
        private readonly bool _NoDuplicates;
        private readonly IEnumerable<OptionAimOption> _Options;
        #endregion

        public IEnumerable<OptionAimOption> ListOptions => _Options;
        public bool NoDuplicates => _NoDuplicates;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<OptionTargetInfo>(actor, action, infos)
                .Select(_i => new OptionTarget(_i.Key,
                    ListOptions.FirstOrDefault(_oa => _oa.Key.Equals(_i.OptionKey, StringComparison.OrdinalIgnoreCase))));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<OptionAimInfo>(action, actor);
            _info.Options = ListOptions.Select(_o => new OptionAimOption
            {
                Key = _o.Key,
                Description = _o.Description,
                Name = _o.Name,
                IsCurrent = _o.IsCurrent
            }).ToArray();
            _info.NoDuplicates = NoDuplicates;
            return _info;
        }
    }

    [Serializable]
    public class OptionAimValue<Val> : OptionAimOption
    {
        public Val Value { get; set; }
    }
}
