using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class CellSpaceSet : IEnumerable<CellSpace>
    {
        public CellSpaceSet()
        {
            _Spaces = [];
        }

        private List<CellSpace> _Spaces;

        public void Add(CellSpace space)
        {
            if (!_Spaces.Any(_c => _c.Name.Equals(space.Name, StringComparison.OrdinalIgnoreCase)))
            {
                if (_Spaces.Any())
                {
                    space.Index = (ushort)(_Spaces.Max(_c => _c.Index) + 1);
                }
                else
                {
                    space.Index = 1;
                }

                _Spaces.Add(space);
            }
        }

        public void Remove(CellSpace space)
        {
            if (_Spaces.Contains(space))
            {
                _Spaces.Remove(space);
            }
        }

        public void Renumber()
        {
            ushort _renum = 1;
            foreach (var _space in _Spaces)
            {
                _space.Index = _renum;
                _renum++;
            }
        }

        #region IEnumerable<CellSpace> Members
        public IEnumerator<CellSpace> GetEnumerator()
        {
            foreach (var _c in _Spaces.OrderBy(_s => _s.Name))
            {
                yield return _c;
            }

            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _c in _Spaces.OrderBy(_s => _s.Name))
            {
                yield return _c;
            }

            yield break;
        }
        #endregion
    }
}
