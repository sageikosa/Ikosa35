using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ReleaseActionModel : IFurnishingAction
    {
        public ReleaseActionModel(ActionInfo actionInfo)
        {
            _Action = actionInfo;
        }

        #region data
        private ActionInfo _Action;
        #endregion

        public ActionInfo Action
            => _Action;

        public object Parameter
            => null;
    }
}
