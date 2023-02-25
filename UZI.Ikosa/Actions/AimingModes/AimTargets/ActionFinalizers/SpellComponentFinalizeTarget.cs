using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SpellComponentFinalizeTarget : FinalizeTarget
    {
        public SpellComponentFinalizeTarget(SpellComponent component)
            : base(null)
        {
            _Component = component;
        }

        #region state
        protected SpellComponent _Component;
        #endregion

        public SpellComponent SpellComponent => _Component;

        public override bool SerialStateDependent => true;

        public override void DoActionFinalized(CoreActivity activity, bool deactivated)
        {
            SpellComponent?.StopUse(activity);
        }
    }
}
