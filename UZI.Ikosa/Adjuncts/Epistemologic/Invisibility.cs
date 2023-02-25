using System;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Base class for several invisibility effects</summary>
    [Serializable]
    public class Invisibility : Adjunct
    {
        public Invisibility(object source)
            : base(source)
        {
            _InvisHandler = new InvisibilityHandler();
            // TODO: ¿add +2 ATK versus sighted non-invisible negating opponents?
        }

        private InvisibilityHandler _InvisHandler;

        protected override void OnActivate(object source)
        {
            var _obj = Anchor as CoreObject;
            _obj?.AddIInteractHandler(_InvisHandler);
            // TODO: ¿add +2 ATK versus sighted non-invisible negating opponents?

            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _obj = Anchor as CoreObject;
            _obj?.RemoveIInteractHandler(_InvisHandler);
            // TODO: ¿remove +2 ATK versus sighted non-invisible negating opponents?

            base.OnDeactivate(source);
        }

        protected override bool OnPreActivate(object source)
        {
            return base.OnPreActivate(source);
        }

        /// <summary>False if the holder has the Invisibility aspect; true otherwise.</summary>
        public static bool IsVisible(CoreObject holder)
            => !holder.HasActiveAdjunct<Invisibility>();

        public override object Clone()
            => new Invisibility(Source);
    }
}
