using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Dice;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>
    /// Fixed dice roll.  Will get expressed as Value&lt;int&gt; targets
    /// </summary>
    [Serializable]
    public class RollAim : AimingMode
    {
        /// <summary>Fixed dice roll.  Will get expressed as Value&lt;int&gt; targets</summary>
        public RollAim(string key, string displayName, Roller roller)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
            _Roller = roller;
        }

        private readonly Roller _Roller;
        public Roller Roller => _Roller;

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            Guid[] _actor = actor != null ? new[] { actor.ID } : null;

            // auto-roll if value is null
            return SelectedTargets<ValueIntTargetInfo>(actor, action, infos)
                .Select(_i => new ValueTarget<int>(_i.Key, Math.Min(_i.Value ?? Roller.RollValue(actor?.ID ?? Guid.Empty, Key, DisplayName, _actor), Roller.MaxRoll)));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<RollAimInfo>(action, actor);
            _info.RollerString = Roller.ToString();
            return _info;
        }
    }
}
