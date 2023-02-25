using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    public class NonLethalOverload : ActorStateBase, IHealthActivityLimiter
    {
        public NonLethalOverload(object source) 
            :            base(source)
        {
            _Unconscious = new UnconsciousEffect(this, double.MaxValue, Round.UnitFactor);
        }

        private UnconsciousEffect _Unconscious;

        #region Activate
        protected override void OnActivate(object source)
        {
            // add unconsciousness
            Anchor.AddAdjunct(_Unconscious);

            // notify
            NotifyStateChange();
            base.OnActivate(source);
        }
        #endregion

        #region DeActivate
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);

            // remove unconsciousness
            _Unconscious.Eject();
        }
        #endregion

        public override object Clone()
        {
            return new NonLethalOverload(Source);
        }
    }
}
