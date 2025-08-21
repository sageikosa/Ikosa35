using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public abstract class AimingMode
    {
        #region ctor()
        protected AimingMode(string key, string displayName, Range minModes, Range maxModes)
        {
            _Key = key;
            _Disp = displayName;
            _MinModes = minModes;
            _MaxModes = maxModes;
            _PrepOpt = string.Empty;
        }
        protected AimingMode(string key, string displayName, Range minModes, Range maxModes, string preposition)
        {
            _Key = key;
            _Disp = displayName;
            _MinModes = minModes;
            _MaxModes = maxModes;
            _PrepOpt = preposition;
        }
        #endregion

        #region data
        private readonly string _Key;
        private readonly string _Disp;
        private readonly string _PrepOpt;
        private readonly Range _MinModes;
        private readonly Range _MaxModes;
        #endregion

        /// <summary>Key used to hierarchically place in a UI</summary>
        public string Key => _Key;

        /// <summary>Friendly name to display in a UI</summary>
        public string DisplayName => _Disp;

        /// <summary>Nearby word in a "full" description to define the target's relation to the overall activity</summary>
        public string PrepositionOption => _PrepOpt;

        /// <summary>Minimum number of aiming mode targets (or volume units, or points)</summary>
        public Range MinimumAimingModes => _MinModes;

        /// <summary>Maximum number of targets (or volumes units, or points)</summary>
        public Range MaximumAimingModes => _MaxModes;

        public abstract IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider);

        /// <summary>Selected targets by key and makes sure the numbers are within the specified ranges</summary>
        protected IEnumerable<AMode> SelectedTargets<AMode>(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, bool rangeCheck = true) where AMode : AimTargetInfo
        {
            // calculate class level
            var _classLevel = action.CoreActionClassLevel(actor, this);

            // check target counts
            var _targets = from _a in infos.OfType<AMode>()
                           where _a.Key.Equals(Key, StringComparison.OrdinalIgnoreCase)
                           select _a;
            if (rangeCheck)
            {
                if (_targets.Count() < MinimumAimingModes.EffectiveRange(actor, _classLevel))
                {
                    throw new ArgumentOutOfRangeException(@"Too few targets of correct type with correct key", Key);
                }

                if (_targets.Count() > MaximumAimingModes.EffectiveRange(actor, _classLevel))
                {
                    throw new ArgumentOutOfRangeException(@"Too many targets of correct type with correct key", Key);
                }
            }
            return _targets;
        }

        protected AMInfo ToInfo<AMInfo>(CoreAction action, CoreActor actor)
            where AMInfo : AimingModeInfo, new()
        {
            var _level = action.CoreActionClassLevel(actor, this);
            return new AMInfo
            {
                Key = Key,
                DisplayName = DisplayName,
                Preposition = PrepositionOption,
                ClassLevel = _level,
                MinimumAimingModes = MinimumAimingModes.EffectiveRange(actor, _level),
                MaximumAimingModes = MaximumAimingModes.EffectiveRange(actor, _level)
            };
        }

        public abstract AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor);
    }
}
