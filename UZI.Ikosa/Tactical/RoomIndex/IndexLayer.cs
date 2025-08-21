using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    [Obsolete(@"Superceded by RoomTracker")]
    public class IndexLayer
    {
        public IndexLayer()
        {
            _Strips = [];
        }

        private const int THICK = 20;
        private Dictionary<int, IndexStrip> _Strips;

        private int GetIndex(int val)
            => val < 0
            ? ((val + 1) / THICK) - 1
            : (val / THICK);

        private IEnumerable<IndexStrip> GetRoomStrips(Room room, bool addIfMissing = true)
        {
            var _lo = GetIndex(room.LowerY);
            var _hi = GetIndex(room.UpperY);
            for (var _idx = _lo; _idx <= _hi; _idx++)
            {
                if (!_Strips.ContainsKey(_idx))
                {
                    if (addIfMissing)
                    {
                        var _new = new IndexStrip();
                        _Strips.Add(_idx, _new);
                        yield return _new;
                    }
                }
                else
                {
                    yield return _Strips[_idx];
                }
            }
            yield break;
        }

        public void Add(Room room)
        {
            foreach (var _strip in GetRoomStrips(room))
            {
                _strip.Add(room);
            }
        }

        public void Remove(Room room)
        {
            foreach (var _strip in GetRoomStrips(room, false))
            {
                _strip.Remove(room);
            }
            foreach (var _n in _Strips.Where(_kvp => _kvp.Value.IsEmpty).Select(_kvp => _kvp.Key).ToList())
            {
                _Strips.Remove(_n);
            }
        }

        public Room GetRoom(ICellLocation location)
        {
            var _idx = GetIndex(location.Y);
            if (_Strips.TryGetValue(_idx, out IndexStrip _strip))
            {
                return _strip.GetRoom(location);
            }
            return null;
        }

        public bool IsEmpty => _Strips.Count == 0;
    }
}
