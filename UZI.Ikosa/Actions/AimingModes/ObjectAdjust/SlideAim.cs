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
    public class SlideAim : AimingMode
    {
        /// <summary>
        /// <para>Returns CharacterStringTarget</para>
        /// <para>Left | Right</para>
        /// </summary>
        public SlideAim(string key, string displayName, bool canLeft, bool canRight)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
            _Left = canLeft;
            _Right = canRight;
        }

        #region data
        private bool _Left;
        private bool _Right;
        #endregion

        public bool CanSlideLeft => _Left;
        public bool CanSlideRight => _Right;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            return SelectedTargets<CharacterStringTargetInfo>(actor, action, infos)
                .Select(_i => new CharacterStringTarget(Key, _i.CharacterString));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<SlideAimInfo>(action, actor);
            _info.CanSlideLeft = CanSlideLeft;
            _info.CanSlideRight = CanSlideRight;
            return _info;
        }
    }
}
