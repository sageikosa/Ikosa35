using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PivotActionModel : IFurnishingAction
    {
        public PivotActionModel(ActionInfo action)
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

        public Tuple<ActionInfo, AimTargetInfo> Left
            => GenerateTarget(nameof(Left));

        public Tuple<ActionInfo, AimTargetInfo> Right
            => GenerateTarget(nameof(Right));

        public Tuple<ActionInfo, AimTargetInfo> TwistLeft
            => GenerateTarget(nameof(TwistLeft));

        public Tuple<ActionInfo, AimTargetInfo> TwistRight
            => GenerateTarget(nameof(TwistRight));

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
