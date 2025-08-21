using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    [Obsolete(@"Superceded by RoomTracker")]
    public class IndexStrip
    {
        public IndexStrip()
        {
            _Nuggets = [];
        }

        private const int THICK = 20;
        private Dictionary<int, Dictionary<Guid, Room>> _Nuggets;

        private int GetIndex(int val)
            => val < 0
            ? ((val + 1) / THICK) - 1
            : (val / THICK);

        private IEnumerable<Dictionary<Guid, Room>> GetRoomNuggets(Room room, bool addIfMissing = true)
        {
            var _lo = GetIndex(room.LowerX);
            var _hi = GetIndex(room.UpperX);
            for (var _idx = _lo; _idx <= _hi; _idx++)
            {
                if (!_Nuggets.ContainsKey(_idx))
                {
                    if (addIfMissing)
                    {
                        var _new = new Dictionary<Guid, Room>();
                        _Nuggets.Add(_idx, _new);
                        yield return _new;
                    }
                }
                else
                {
                    yield return _Nuggets[_idx];
                }
            }
            yield break;
        }

        public void Add(Room room)
        {
            foreach (var _nugget in GetRoomNuggets(room))
            {
                if (!_nugget.ContainsKey(room.ID))
                {
                    _nugget.Add(room.ID, room);
                }
            }
        }


        public void Remove(Room room)
        {
            foreach (var _nugget in GetRoomNuggets(room, false))
            {
                if (_nugget.ContainsKey(room.ID))
                {
                    _nugget.Remove(room.ID);
                }
            }
            foreach (var _n in _Nuggets.Where(_kvp => _kvp.Value.Count == 0).Select(_kvp => _kvp.Key).ToList())
            {
                _Nuggets.Remove(_n);
            }
        }

        public Room GetRoom(ICellLocation location)
        {
            var _idx = GetIndex(location.X);
            if (_Nuggets.TryGetValue(_idx, out var _nugget))
            {
                return _nugget
                    .Where(_kvp => _kvp.Value.ContainsCell(location))
                    .Select(_kvp => _kvp.Value)
                    .FirstOrDefault();
            }
            return null;
        }

        public bool IsEmpty => _Nuggets.Count == 0;
    }
}
