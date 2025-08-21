using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Packaging;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LocalLinkSet : ICorePart
    {
        public LocalLinkSet(LocalCellGroup owner)
        {
            _Owner = owner;
            _Links = [];
        }

        #region private data
        private Collection<LocalLink> _Links;
        private LocalCellGroup _Owner;
        private List<Room> _Touching = [];
        #endregion

        public LocalCellGroup Owner => _Owner;

        public IEnumerable<LocalCellGroup> LinkedGroups
            => _Links.Select(_lnk => _lnk.OutsideGroup(_Owner)).Distinct();

        private List<Room> SafeTouch
        {
            get
            {
                _Touching ??= [];
                return _Touching;
            }
        }

        public IEnumerable<Room> TouchingRooms
            => SafeTouch.Select(_r => _r);

        public void AddRoom(LocalCellGroup group)
        {
            if ((group is Room) && !SafeTouch.Contains(group))
            {
                SafeTouch.Add(group as Room);
            }
        }

        public void Add(LocalLink link)
        {
            // find all our links to the other room already being tracked
            var _other = link.Groups.FirstOrDefault(_r => _r != _Owner);
            if (!_Links.Any(_l => _l.Groups.Contains(_other) && _l.LinkCube.Equals(link.LinkCube)))
            {
                // this set is not tracking any links that contain the other room and that are congruent to this one
                _Links.Add(link);
                _other.Links.Add(link);
            }
            if ((_other is Room) && !SafeTouch.Contains(_other))
            {
                SafeTouch.Add(_other as Room);
            }
        }

        public void Remove(LocalLink link)
        {
            _Links.Remove(link);
        }

        public void Clear()
        {
            _Links.Clear();
            SafeTouch.Clear();
        }

        public IEnumerable<LocalLink> All
            => _Links.Select(_l => _l);

        public int Count
            => _Links.Count;

        #region ICorePart Members

        public string Name => Owner.Name;
        public IEnumerable<ICorePart> Relationships { get { yield break; } }
        public string TypeName => GetType().FullName;

        #endregion
    }
}