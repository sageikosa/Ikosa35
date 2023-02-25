using System;
using Uzi.Core;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ApplySplatterStep : PreReqListStepBase
    {
        #region Construction
        public ApplySplatterStep(CoreActivity activity, ISplatterWeapon splat, bool direct, IInteract target)
            : base(activity)
        {
            _Splat = splat;
            _Direct = direct;
            if (_Direct)
            {
                foreach (var _pre in _Splat.GetDirectPrerequisites(activity))
                    _PendingPreRequisites.Enqueue(_pre);
            }
            else
            {
                foreach (var _pre in _Splat.GetSplatterPrerequisites(activity))
                    _PendingPreRequisites.Enqueue(_pre);
            }
            _Target = target;
        }
        #endregion

        #region Private Data
        private ISplatterWeapon _Splat;
        private bool _Direct;
        private IInteract _Target;
        #endregion

        public CoreActivity Activity { get { return Process as CoreActivity; } }
        public bool Direct { get { return _Direct; } }
        public IInteract Target { get { return _Target; } }
        public ISplatterWeapon Splat { get { return _Splat; } }

        protected override bool OnDoStep()
        {
            // TODO: fail process?
            if (_Direct)
            {
                _Splat.ApplyDirect(this);
            }
            else
            {
                _Splat.ApplySplatter(this);
            }
            // TODO: inform
            return true;
        }
    }
}
