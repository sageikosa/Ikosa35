using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>Dictionary of reactors for a setting context</summary>
    [Serializable]
    public class ReactorSet : IEnumerable<ICanReact>
    {
        #region Construction
        public ReactorSet()
        {
            _Reactors = [];
        }
        #endregion

        private Dictionary<object, ICanReact> _Reactors;

        public Dictionary<object, ICanReact> Reactors { get { return _Reactors; } }

        #region IEnumerable<ICanReact> Members

        public IEnumerator<ICanReact> GetEnumerator()
        {
            foreach (var _r in (from _react in _Reactors
                                select _react.Value).Distinct())
            {
                yield return _r;
            }

            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _r in (from _react in _Reactors
                                select _react.Value).Distinct())
            {
                yield return _r;
            }

            yield break;
        }

        #endregion
    }
}
