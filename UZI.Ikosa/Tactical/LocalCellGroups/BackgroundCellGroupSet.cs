using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Uzi.Visualize;
using System.Collections.Specialized;
using Uzi.Packaging;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class BackgroundCellGroupSet: IEnumerable<BackgroundCellGroup>, INotifyCollectionChanged, ICorePart
    {
        public BackgroundCellGroupSet(LocalMap map)
        {
            _Groups = [];
            _Map = map;
        }

        #region private data
        private Collection<BackgroundCellGroup> _Groups;
        private LocalMap _Map;
        #endregion

        public LocalMap Map { get { return _Map; } }

        public void AddBase(BackgroundCellGroup item)
        {
            _Groups.Add(item);
            if (_Map.ContextSet.Count > 0)
            {
                _Map.MapContext.CacheLocatorGroups();
            }

            _Map.RelightMap();
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _Groups.IndexOf(item)));
            }
        }

        public void AddOverlay(BackgroundCellGroup overlay, int targetIndex)
        {
            _Groups.Insert(targetIndex, overlay);
            if (_Map.ContextSet.Count > 0)
            {
                _Map.MapContext.CacheLocatorGroups();
            }

            _Map.RelightMap();
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, overlay, targetIndex));
            }
        }

        public void Remove(BackgroundCellGroup item)
        {
            var _index = _Groups.IndexOf(item);
            if (_index >= 0)
            {
                _Groups.Remove(item);
                if (_Map.ContextSet.Count > 0)
                {
                    _Map.MapContext.CacheLocatorGroups();
                }

                _Map.RelightMap();
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, _index));
                }
            }
        }

        public IEnumerable<BackgroundCellGroup> All()
        {
            return _Groups.AsEnumerable();
        }

        public BackgroundCellGroup GetBackgroundCellGroup(ICellLocation location)
        {
            foreach (var _set in _Groups)
            {
                if (_set.ContainsCell(location))
                {
                    return _set;
                }
            }
            return null;
        }

        #region INotifyCollectionChanged Members
        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region ICorePart Members

        public string Name { get { return @"Backgrounds"; } }

        public IEnumerable<ICorePart> Relationships { get { return All(); } }

        public string TypeName
        {
            get { return typeof(BackgroundCellGroupSet).FullName; }
        }

        #endregion

        #region IEnumerable<BackgroundCellGroup> Members

        public IEnumerator<BackgroundCellGroup> GetEnumerator()
        {
            foreach (var _bcg in All())
            {
                yield return _bcg;
            }

            yield break;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var _bcg in All())
            {
                yield return _bcg;
            }

            yield break;
        }

        #endregion
    }
}
