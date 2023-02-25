using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class DefaultModelKey : Adjunct
    {
        public DefaultModelKey(string modelKey)
            : base(typeof(DefaultModelKey))
        {
            _Key = modelKey;
        }

        private string _Key;

        public string ModelKey { get { return _Key; } set { _Key = value; } }
        public override bool IsProtected { get { return true; } }

        public override object Clone()
        {
            return new DefaultModelKey(ModelKey);
        }
    }
}
