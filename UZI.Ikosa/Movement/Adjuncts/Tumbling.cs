using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class Tumbling : Adjunct
    {
        #region construction
        public Tumbling(SuccessCheckTarget target)
            : base(typeof(Tumbling))
        {
            _Target = target;
            _Expiry = false;
        }
        #endregion

        #region private data
        private SuccessCheckTarget _Target;
        private bool _Expiry;
        #endregion

        public Creature Creature
            => Anchor as Creature;

        public SuccessCheckTarget SuccessCheckTarget => _Target;
        public override bool IsProtected => true;

        /// <summary>
        /// True if the tumble check has expired (IInteractHandler and Condition may still be in effect, though).
        /// An expired Tumbling check requires a new check to be made, but this adjunct will continue to provide Tumbling
        /// mechanics until replaced.
        /// </summary>
        public bool IsCheckExpired { get { return _Expiry; } set { _Expiry = value; } }

        public bool IsAccelerated
            => SuccessCheckTarget.IsUsingPenalty;

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            if (Creature != null)
            {
                // remove all tumbles that are not this
                foreach (var _tumble in Creature.Adjuncts.OfType<Tumbling>().Where(_c => this != _c).ToList())
                    _tumble.Eject();
            }
            base.OnActivate(source);
        }
        #endregion

        public override object Clone()
        {
            return new Tumbling(SuccessCheckTarget);
        }
    }
}
