using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class RoomTracker
    {
        public RoomTracker(LocalMap map)
        {
            _Map = map;
            Rebuild();
        }

        #region data
        private LocalMap _Map;
        private Dictionary<RoomTrackerKey, Dictionary<Guid, Room>> _Tracker;
        #endregion

        public void Rebuild()
        {
            _Tracker = [];
            foreach (var _room in _Map.Rooms)
            {
                Add(_room);
            }
        }

        public void ReIndex(Room room)
        {
            Remove(room);
            Add(room);
        }

        public void Add(Room room)
        {
            foreach (var _key in RoomTrackerKey.GetKeys(room))
            {
                if (_Tracker.TryGetValue(_key, out var _rooms))
                {
                    _rooms[room.ID] = room;
                }
                else
                {
                    _Tracker[_key] = new Dictionary<Guid, Room>
                    {
                        { room.ID, room }
                    };
                }
            }
        }

        public void Remove(Room room)
        {
            foreach (var _key in RoomTrackerKey.GetKeys(room))
            {
                if (_Tracker.TryGetValue(_key, out var _rooms))
                {
                    _rooms.Remove(room.ID);
                    if (!_rooms.Any())
                    {
                        _Tracker.Remove(_key);
                    }
                }
            }
        }

        public Room GetRoom(ICellLocation location)
        {
            var _key = new RoomTrackerKey(location.Z, location.Y, location.X);
            if (_Tracker.TryGetValue(_key, out var _rooms))
            {
                foreach (var _kvp in _rooms)
                {
                    if (_kvp.Value.ContainsCell(location))
                    {
                        return _kvp.Value;
                    }
                }
            }
            return null;
        }

        public IEnumerable<Room> GetRooms(IGeometricRegion region)
        {
            IEnumerable<Room> _roomStream()
            {
                // get all roomKeys for Cubic
                foreach (var _key in RoomTrackerKey.GetKeys(region))
                {
                    // see if tracking something for the roomkey
                    if (_Tracker.TryGetValue(_key, out var _rooms))
                    {
                        // yield all rooms overlapping the cubic
                        foreach (var _kvp in _rooms)
                        {
                            if (_kvp.Value.IsOverlapping(region))
                            {
                                yield return _kvp.Value;
                            }
                        }
                    }
                }
                yield break;
            }

            // distinct rooms
            return _roomStream().Distinct();
        }
    }
}
