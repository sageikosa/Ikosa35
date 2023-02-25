using System;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Sinking : Adjunct
    {
        public Sinking(SinkingMovement sinkMove)
            : base(sinkMove)
        {
        }

        public SinkingMovement SinkingMovement => Source as SinkingMovement;
        public override bool IsProtected => true;

        protected override void OnActivate(object source)
        {
            var _coreObj = SinkingMovement.CoreObject;
            _coreObj.Adjuncts.EjectAll<Sinking>(this);
            _coreObj.Adjuncts.EjectAll<Swimming>();
            _coreObj.Adjuncts.EjectAll<Falling>();
            _coreObj.Adjuncts.EjectAll<SlowFalling>();
            _coreObj.Adjuncts.EjectAll<LiquidFalling>();
            base.OnActivate(source);
        }

        // TODO: conditions...?

        public override object Clone()
        {
            return new Sinking(SinkingMovement);
        }
    }
}
