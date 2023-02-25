using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class SlowFalling : Adjunct
    {
        public SlowFalling(SlowFallMovement slowFallMove)
            : base(slowFallMove)
        {
        }

        public SlowFallMovement SlowFallMovement => Source as SlowFallMovement;
        public override bool IsProtected => true;

        protected override void OnActivate(object source)
        {
            var _coreObj = SlowFallMovement.CoreObject;
            _coreObj.Adjuncts.EjectAll(this);
            _coreObj.Adjuncts.EjectAll<Swimming>();
            _coreObj.Adjuncts.EjectAll<Sinking>();
            _coreObj.Adjuncts.EjectAll<LiquidFalling>();
            _coreObj.Adjuncts.EjectAll<Falling>();
            base.OnActivate(source);
        }
        // TODO: conditions...?

        public override object Clone()
            => new SlowFalling(SlowFallMovement);
    }
}
