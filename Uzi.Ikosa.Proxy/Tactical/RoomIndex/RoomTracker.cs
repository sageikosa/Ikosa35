using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy
{
    public class RoomTracker
    {
        public RoomTracker(RoomTracker copySource)
        {
            _Tracker = copySource?._Tracker.ToDictionary(_kvp => _kvp.Key, _kvp => new Dictionary<Guid, RoomViewModel>(_kvp.Value))
                ?? new Dictionary<RoomTrackerKey, Dictionary<Guid, RoomViewModel>>(10);
        }

        #region data
        private Dictionary<RoomTrackerKey, Dictionary<Guid, RoomViewModel>> _Tracker;
        #endregion

        public void ReIndex(RoomViewModel room)
        {
            Remove(room);
            Add(room);
        }

        public void Add(RoomViewModel room)
        {
            foreach (var _key in RoomTrackerKey.GetKeys(room))
            {
                if (_Tracker.TryGetValue(_key, out var _rooms))
                {
                    _rooms[room.RoomInfo.ID] = room;
                }
                else
                {
                    _Tracker[_key] = new Dictionary<Guid, RoomViewModel>
                    {
                        { room.RoomInfo.ID, room }
                    };
                }
            }
        }

        public void Remove(RoomViewModel room)
        {
            foreach (var _key in RoomTrackerKey.GetKeys(room))
            {
                if (_Tracker.TryGetValue(_key, out var _rooms))
                {
                    _rooms.Remove(room.RoomInfo.ID);
                    if (!_rooms.Any())
                        _Tracker.Remove(_key);
                }
            }
        }

        public RoomViewModel GetRoom(int z, int y, int x)
        {
            var _key = new RoomTrackerKey(z, y, x);
            if (_Tracker.TryGetValue(_key, out var _rooms))
            {
                return _rooms
                     .Where(_kvp => _kvp.Value.ContainsCell(z, y, x))
                    .Select(_kvp => _kvp.Value)
                    .FirstOrDefault();
            }
            return null;
        }
    }
}
