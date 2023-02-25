using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// <para>Returns CharacterStringTarget</para>
    /// <para>Left | Right</para>
    /// </summary>
    [Serializable]
    public class RotateAim : AimingMode
    {
        /// <summary>
        /// <para>Returns CharacterStringTarget</para>
        /// <para>Left | Right</para>
        /// </summary>
        public RotateAim(string key, string displayName)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<CharacterStringTargetInfo>(actor, action, infos)
                .Select(_i => new CharacterStringTarget(Key, _i.CharacterString));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<RotateAimInfo>(action, actor);
            return _info;
        }
    }
}
