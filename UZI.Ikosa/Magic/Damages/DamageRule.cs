using System;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    /// <summary>
    /// Allows definition of complex multi-part damage rules
    /// </summary>
    [Serializable]
    public class DamageRule
    {
        public DamageRule(string key, Range range, bool nonLethal, string name)
        {
            _Key = key;
            _Range = range;
            _NonLethal = nonLethal;
            _Name = name;
        }

        #region Private Data
        private string _Key;
        private Range _Range;
        private bool _NonLethal;
        private string _Name;
        #endregion

        public string Key { get { return _Key; } }
        public Range Range { get { return _Range; } }
        public bool NonLethal { get { return _NonLethal; } }
        public string Name { get { return _Name; } }
    }
}
