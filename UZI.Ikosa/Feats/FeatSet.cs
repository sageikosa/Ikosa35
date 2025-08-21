using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Uzi.Core;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Feats
{
    [Serializable]
    public class FeatSet: IEnumerable<FeatBase>, ICreatureBound, IControlChange<FeatBase>, INotifyCollectionChanged
    {
        protected Collection<FeatBase> _Feats;
        protected Creature _Creature;

        public FeatSet(Creature creature)
        {
            _Creature = creature;
            _Feats = [];
            _FeatCtrl = new ChangeController<FeatBase>(this, null);
        }

        internal void Add(FeatBase item)
        {
            if (item.MeetsPreRequisite(_Creature))
            {
                _Feats.Add(item);
                _FeatCtrl.DoValueChanged(item, "Added");
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
            }
        }

        public bool Contains(Type featType, int powerLevel)
        {
            foreach (FeatBase _feat in this._Feats)
            {
                if (_feat.GetType() == featType)
                {
                    if (_feat.PowerLevel <= powerLevel)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Contains(Type featType)
        {
            foreach (FeatBase _feat in this._Feats)
            {
                if (_feat.GetType() == featType)
                {
                    return _feat.IsActive;
                }
            }
            return false;
        }

        // TODO: feat sub-types...

        public int Count
        {
            get { return this._Feats.Count; }
        }

        internal void Remove(FeatBase feat)
        {
            int _index = _Feats.IndexOf(feat);
            this._Feats.Remove(feat);
            _FeatCtrl.DoValueChanged(feat, "Removed");
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, feat, _index));
            }
        }

        public FeatBase this[int index]
        {
            get
            {
                return this._Feats[index];
            }
        }

        #region IEnumerable<FeatBase> Members

        public IEnumerator<FeatBase> GetEnumerator()
        {
            return _Feats.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Feats.GetEnumerator();
        }
        #endregion

        #region ICreatureBound Members
        public Creature Creature
        {
            get { return _Creature; }
        }
        #endregion

        #region IControlChange<FeatBase> Members
        private ChangeController<FeatBase> _FeatCtrl;
        public void AddChangeMonitor(IMonitorChange<FeatBase> monitor)
        {
            _FeatCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<FeatBase> monitor)
        {
            _FeatCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
