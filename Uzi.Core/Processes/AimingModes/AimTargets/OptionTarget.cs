using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class OptionTarget : AimTarget
    {
        #region Construction
        public OptionTarget(string key, OptionAimOption option)
            : base(key, null)
        {
            _Opt = option;
        }
        #endregion

        private OptionAimOption _Opt;
        public OptionAimOption Option => _Opt;

        public override AimTargetInfo GetTargetInfo()
            => new OptionTargetInfo
            {
                Key = Key,
                TargetID = Target != null ? Target.ID : (Guid?)null,
                OptionKey = Option.Key,
                OptionName = Option.Name,
                OptionDescription = Option.Description
            };
    }
}
