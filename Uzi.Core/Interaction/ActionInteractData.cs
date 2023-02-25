using System;

namespace Uzi.Core
{
    [Serializable]
    public class ActionInteractData : InteractData
    {
        public ActionInteractData(CoreAction action)
            : base(null)
        {
            _Action = action;
        }

        private CoreAction _Action;
        public CoreAction Action { get { return _Action; } }
    }
}
