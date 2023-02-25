using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Objects
{
    public interface ISecureLock : ICoreObject, IControlChange<SecureState>, IActionSource
    {
        void AddKey(Guid key);
        void RemoveKey(Guid key);
        IEnumerable<Guid> Keys { get; }
        Deltable PickDifficulty { get; }
        /// <summary>changes or flags an attempt to change the security state to secured</summary>
        void SecureLock(CoreActor actor, object source, bool success);
        /// <summary>changes or flags an attempt to change the security state to unsecured</summary>
        void UnsecureLock(CoreActor actor, object source, bool success);
    }

    [Serializable]
    /// <summary>Not used for abortable actions</summary>
    public struct SecureState
    {
        private bool _Securing;
        private bool _Success;
        private object _Source;

        public bool Securing { get { return _Securing; } set { _Securing = value; } }
        public bool Success { get { return _Success; } set { _Success = value; } }
        public object Source { get { return _Source; } set { _Source= value; } }
    }
}
