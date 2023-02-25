using System;
using Uzi.Core;

namespace Uzi.Ikosa.Creatures
{
    [Serializable]
    public class BodyFeature : ISourcedObject
    {
        public BodyFeature(object source, string key, bool isMajor, string description)
        {
            _Src = source;
            _Key = key;
            _Major = isMajor;
            _Descr = description;
        }

        #region private data
        private object _Src;
        private string _Key;
        private bool _Major;
        private string _Descr;
        #endregion

        public object Source { get { return _Src; } }
        public string Key { get { return _Key; } }
        public bool IsMajor { get { return _Major; } }
        public string Description { get { return _Descr; } }
    }
}