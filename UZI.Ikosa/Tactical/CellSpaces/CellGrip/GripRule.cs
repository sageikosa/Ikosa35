using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public abstract class GripRule
    {
        #region private data
        private int? _Ledge;
        private int? _Base;
        private int? _Dangling; // face or internal is opposed to gravity
        private int? _Balance;
        #endregion

        public int? Ledge { get { return _Ledge; } set { _Ledge = value; } }
        public int? Base { get { return _Base; } set { _Base = value; } }
        public int? Dangling { get { return _Dangling; } set { _Dangling = value; } }

        /// <summary>Used for balance checks</summary>
        public int? Balance { get { return _Balance; } set { _Balance = value; } }
    }
}
