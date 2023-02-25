using System;
using Uzi.Core;

namespace Uzi.Ikosa.Movement
{
    /// <summary>Was free-falling, but then entered liquid</summary>
    [Serializable]
    public class LiquidFalling : Adjunct
    {
        /// <summary>Was free-falling, but then entered liquid</summary>
        public LiquidFalling(LiquidFallMovement liquidFallMove)
            : base(liquidFallMove)
        {
        }

        public LiquidFallMovement LiquidFallMovement { get { return Source as LiquidFallMovement; } }
        public override bool IsProtected { get { return true; } }

        protected override void OnActivate(object source)
        {
            var _coreObj = LiquidFallMovement.CoreObject;
            _coreObj.Adjuncts.EjectAll<Falling>();
            _coreObj.Adjuncts.EjectAll<SlowFalling>();
            _coreObj.Adjuncts.EjectAll<Swimming>();
            _coreObj.Adjuncts.EjectAll<Sinking>();
            _coreObj.Adjuncts.EjectAll<LiquidFalling>(this);
            base.OnActivate(source);
        }

        public override object Clone()
        {
            return new LiquidFalling(LiquidFallMovement);
        }
    }
}
