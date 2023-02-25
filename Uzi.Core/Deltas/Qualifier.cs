using System;

namespace Uzi.Core
{
    /// <summary>
    /// Used for qualifying values.
    /// Used for prerequisites to indicate who must meet the prerequisite.
    /// </summary>
    [Serializable]
    public class Qualifier
    {
        /// <summary>
        /// Used for qualifying values.
        /// Used for prerequisites to indicate who must meet the prerequisite.
        /// </summary>
        public Qualifier(CoreActor actor, object source, IInteract target)
        {
            _Actor = actor;
            _Source = source;
            _Target = target;
        }

        #region Private Data
        private CoreActor _Actor;
        private object _Source;
        private IInteract _Target;
        #endregion

        public CoreActor Actor => _Actor;
        public object Source => _Source;

        /// <summary>Target of the qualification, or prerequisite</summary>
        public IInteract Target => _Target;
    }
}
