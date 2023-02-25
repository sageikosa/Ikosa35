using System;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Swimming : Adjunct
    {
        #region construction
        public Swimming(SuccessCheckTarget target)
            : base(typeof(Swimming))
        {
            _Target = target;
            _Expiry = false;
        }
        #endregion

        #region private data
        private SuccessCheckTarget _Target;
        private bool _Expiry;
        #endregion

        public enum SwimOutcome : byte { Successful, Immobilized, Uncontrolled }

        public Creature Creature => Anchor as Creature;
        public SuccessCheckTarget SuccessCheckTarget => _Target;
        public override bool IsProtected => true;

        /// <summary>An expired swimming check requires a new check to be made.</summary>
        public bool IsCheckExpired { get => _Expiry; set => _Expiry = value; }

        #region public SwimOutcome Outcome { get; }
        public SwimOutcome Outcome
        {
            get
            {
                if (SuccessCheckTarget.Success)
                    return SwimOutcome.Successful;
                else if (SuccessCheckTarget.SoftFail(4))
                    return SwimOutcome.Immobilized;
                else
                    return SwimOutcome.Uncontrolled;
            }
        }
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Creature != null)
            {
                Creature.Adjuncts.EjectAll<Swimming>(this);
                Creature.Adjuncts.EjectAll<Falling>();
                Creature.Adjuncts.EjectAll<SlowFalling>();
                Creature.Adjuncts.EjectAll<LiquidFalling>();
                Creature.Adjuncts.EjectAll<Sinking>();
            }
            base.OnActivate(source);
        }
        #endregion

        public override object Clone()
            => new Swimming(SuccessCheckTarget);
    }
}
