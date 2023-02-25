using System;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Falling : Adjunct
    {
        public Falling(FallMovement fallMove)
            : base(fallMove)
        {
        }

        public FallMovement FallMovement => Source as FallMovement;
        public override bool IsProtected => true;

        protected override void OnActivate(object source)
        {
            var _coreObj = FallMovement.CoreObject;
            _coreObj.Adjuncts.EjectAll(this);
            _coreObj.Adjuncts.EjectAll<Swimming>();
            _coreObj.Adjuncts.EjectAll<Sinking>();
            _coreObj.Adjuncts.EjectAll<LiquidFalling>();
            _coreObj.Adjuncts.EjectAll<SlowFalling>();
            base.OnActivate(source);
        }
        // TODO: conditions...?

        public override object Clone()
            => new Falling(FallMovement);
    }
}
