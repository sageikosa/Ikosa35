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
    /// <para>Push | Pull | Clock | CounterClock</para>
    /// </summary>
    [Serializable]
    public class TiltAim : AimingMode
    {
        /// <summary>
        /// <para>Returns CharacterStringTarget</para>
        /// <para>Push | Pull | Clock | CounterClock</para>
        /// </summary>
        public TiltAim(string key, string displayName)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, 
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<CharacterStringTargetInfo>(actor, action, infos)
                .Select (_i => new CharacterStringTarget(Key, _i.CharacterString));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<TiltAimInfo>(action, actor);
            return _info;
        }
    }
}
