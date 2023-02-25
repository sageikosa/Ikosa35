using System;
using System.Collections.Generic;
using System.Linq;


namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class ObjectStateModelKey
    {
        public ObjectStateModelKey(string stateKey)
        {
            _Key = stateKey;
        }

        #region state
        private string _Key;
        private string _ModelKey;
        #endregion

        public string StateKey => _Key;
        public string ModelKey { get => _ModelKey; set { _ModelKey = value; } }
    }
}
