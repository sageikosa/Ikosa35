using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    [Obsolete(@"Superceded by RoomTracker")]
    public class RoomIndex
    {
        public RoomIndex(LocalMap map)
        {
            _Map = map;
            Rebuild();
        }

        private const int THICK = 5;

        private LocalMap _Map;
        private Dictionary<int, IndexLayer> _Layers;

        public void Rebuild()
        {
            _Layers = new Dictionary<int, IndexLayer>(10);
            foreach (var _room in _Map.Rooms)
                Add(_room);
        }

        private int GetIndex(int val)
            => val < 0
            ? ((val + 1) / THICK) - 1
            : (val / THICK);

        private IEnumerable<IndexLayer> GetRoomLayers(Room room, bool addIfMissing = true)
        {
            var _lo = GetIndex(room.LowerZ);
            var _hi = GetIndex(room.UpperZ);
            for (var _idx = _lo; _idx <= _hi; _idx++)
            {
                if (!_Layers.ContainsKey(_idx))
                {
                    if (addIfMissing)
                    {
                        var _new = new IndexLayer();
                        _Layers.Add(_idx, _new);
                        yield return _new;
                    }
                }
                else
                {
                    yield return _Layers[_idx];
                }
            }
            yield break;
        }

        public void ReIndex(Room room)
        {
            Remove(room);
            Add(room);
        }

        public void Add(Room room)
        {
            foreach (var _layer in GetRoomLayers(room))
            {
                _layer.Add(room);
            }
        }

        public void Remove(Room room)
        {
            foreach (var _layer in GetRoomLayers(room, false))
            {
                _layer.Remove(room);
            }
            foreach (var _n in _Layers.Where(_kvp => _kvp.Value.IsEmpty).Select(_kvp => _kvp.Key).ToList())
                _Layers.Remove(_n);
        }

        public Room GetRoom(ICellLocation location)
        {
            var _idx = GetIndex(location.Z);
            if (_Layers.TryGetValue(_idx, out IndexLayer _layer))
            {
                return _layer.GetRoom(location);
            }
            return null; 
        }
    }
}
