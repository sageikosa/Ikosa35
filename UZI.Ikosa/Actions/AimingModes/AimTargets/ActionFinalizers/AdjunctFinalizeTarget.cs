using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class AdjunctFinalizeTarget : FinalizeTarget
    {
        public AdjunctFinalizeTarget(Adjunct adjunct, bool serialStateDependent)
            : base(null)
        {
            _Adjunct = adjunct;
            _Serial = serialStateDependent;
        }

        #region state
        protected Adjunct _Adjunct;
        private bool _Serial;
        #endregion

        public Adjunct Adjunct => _Adjunct;

        /// <summary>Don't report serial dependency if there's defnitely nothing to do</summary>
        public override bool SerialStateDependent => _Adjunct.Anchor != null ? _Serial : false;

        public override void DoActionFinalized(CoreActivity activity, bool deactivated)
        {
            Adjunct?.Eject();
        }
    }
}
