using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Items requiring search before awareness
    /// </summary>
    [Serializable]
    public class FoundSet: IEnumerable<Guid>
    {
        public FoundSet()
        {
            _Guids = new Dictionary<Guid, Guid>();
        }

        /// <summary>Reset FoundSet</summary>
        public void Clear()
        {
            _Guids.Clear();
        }

        private Dictionary<Guid, Guid> _Guids;

        #region public bool this[Guid guid] { get; set; }
        public bool this[Guid guid]
        {
            get
            {
                return _Guids.ContainsKey(guid);
            }
            set
            {
                if (_Guids.ContainsKey(guid) && !value)
                {
                    _Guids.Remove(guid);
                }
                else if (!_Guids.ContainsKey(guid) && value)
                {
                    _Guids.Add(guid, guid);
                }
            }
        }
        #endregion

        #region IEnumerable<Guid> Members
        public IEnumerator<Guid> GetEnumerator()
        {
            foreach (Guid _g in _Guids.Select(s => s.Key))
                yield return _g;
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (Guid _g in _Guids.Select(s => s.Key))
                yield return _g;
            yield break;
        }
        #endregion
    }
}
