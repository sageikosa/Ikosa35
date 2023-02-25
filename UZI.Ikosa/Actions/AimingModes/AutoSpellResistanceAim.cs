using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used to automatically roll a spell resistance check as a target.  Yields a ValueTarget&lt;int&gt;</summary>
    [Serializable]
    public class AutoSpellResistanceAim : AimingMode
    {
        /// <summary>Used to automatically roll a spell resistance check as a target.  Yields a ValueTarget&lt;int&gt;</summary>
        public AutoSpellResistanceAim(string key)
            : base(key, string.Empty, FixedRange.One, FixedRange.One)
        {
        }

        public override IEnumerable<AimTarget> GetTargets(CoreActor actor, CoreAction action, AimTargetInfo[] infos, IInteractProvider provider)
        {
            // auto-roll if value is null
            return SelectedTargets<ValueIntTargetInfo>(actor, action, infos)
                .Select(_i => new ValueTarget<int>(Key, Math.Min(_i.Value ?? DieRoller.RollDie(actor.ID, 20, Key, @"Spell Resistance", actor.ID), 20)));
        }

        public override AimingModeInfo ToAimingModeInfo(CoreAction action, CoreActor actor)
        {
            return ToInfo<AutoSpellResistanceAimInfo>(action, actor);
        }
    }
}
