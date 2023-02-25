using System;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    [Serializable]
    public class AimTarget
    {
        #region Construction
        public AimTarget(string key, IInteract target)
        {
            _Key = key;
            _Target = target;
        }
        #endregion

        #region data
        private readonly string _Key;
        private readonly IInteract _Target;
        #endregion

        public string Key => _Key;
        public IInteract Target => _Target;

        protected virtual ATInfo ToInfo<ATInfo>()
            where ATInfo : AimTargetInfo, new()
            => new ATInfo
            {
                Key = Key,
                TargetID = Target?.ID
            };

        public virtual AimTargetInfo GetTargetInfo()
            => ToInfo<AimTargetInfo>();

        public static AimTarget GetTarget(AimTargetInfo info, IInteractProvider provider)
            => new AimTarget(info.Key, provider.GetIInteract(info.TargetID ?? Guid.Empty));
    }
}
