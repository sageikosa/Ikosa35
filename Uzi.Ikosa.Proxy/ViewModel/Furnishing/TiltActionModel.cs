using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class TiltActionModel : IFurnishingAction
    {
        public TiltActionModel(ActionInfo action)
        {
            _Action = action;
        }

        #region data
        private ActionInfo _Action;
        #endregion

        public ActionInfo Action
            => _Action;

        public object Parameter
            => null;

        public Tuple<ActionInfo, AimTargetInfo> Push
            => GenerateTarget(nameof(Push));

        public Tuple<ActionInfo, AimTargetInfo> Pull
            => GenerateTarget(nameof(Pull));

        public Tuple<ActionInfo, AimTargetInfo> Clock
            => GenerateTarget(nameof(Clock));

        public Tuple<ActionInfo, AimTargetInfo> CounterClock
            => GenerateTarget(nameof(CounterClock));

        private Tuple<ActionInfo, AimTargetInfo> GenerateTarget(string result)
        {
            return new Tuple<ActionInfo, AimTargetInfo>(_Action, new CharacterStringTargetInfo
            {
                Key = _Action.AimingModes[0].Key,
                CharacterString = result
            });
        }
    }
}
