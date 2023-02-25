using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Dice;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SuccessCheckAim : AimingMode
    {
        public SuccessCheckAim(string key, string displayName, SuccessCheck check)
            : base(key, displayName, FixedRange.One, FixedRange.One)
        {
            _Check = check;
        }

        private SuccessCheck _Check;
        public SuccessCheck Check { get { return _Check; } }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action,
            AimTargetInfo[] infos, IInteractProvider provider)
        {
            // auto-roll if no CheckRoll provided
            return SelectedTargets<SuccessCheckTargetInfo>(actor, action, infos)
                .Select(_i => new SuccessCheckTarget(Key, Check)
                {
                    IsUsingPenalty = _i.IsUsingPenalty,
                    CheckRoll = new Deltable(Math.Min(_i.CheckRoll ?? DieRoller.RollDie(actor.ID, 20, Key, DisplayName, actor.ID), 20))
                });
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<SuccessCheckAimInfo>(action, actor);
            _info.VoluntaryPenalty = Check.VoluntaryPenalty;
            return _info;
        }
    }
}
