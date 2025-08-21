using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// ObjectLoad is only intended for weight tracking.
    /// </summary>
    [Serializable]
    public abstract class ObjectLoad : IEnumerable<ICoreObject>, IMonitorChange<Physical>,
        IControlChange<Physical>, INotifyCollectionChanged, IControlChange<ICoreObject>, IMonitorChange<ICoreObject>
    {
        #region Construction
        protected ObjectLoad(CoreActor owner)
        {
            _Owner = owner;
            _Objects = [];
            _WCtrl = new ChangeController<Physical>(this, new Physical(Physical.PhysicalType.Weight, 0d));
            _ICOCtrl = new ChangeController<ICoreObject>(this, null);
        }
        #endregion

        #region Data
        private Dictionary<Guid, ICoreObject> _Objects;
        private readonly CoreActor _Owner;
        private double _Weight = 0d;
        #endregion

        public CoreActor Owner => _Owner;

        /// <summary>Total weight of the load</summary>
        public double Weight => _Weight;

        /// <summary>Numer of items directly in the object load</summary>
        public int Count => _Objects.Count;

        public bool Contains(Guid id) { return _Objects.ContainsKey(id); }
        public bool Contains(ICoreObject obj) { return _Objects.ContainsValue(obj); }

        /// <summary>Indicates whether the object load contains this in itself or any nested container</summary>
        public bool NestedContains(Guid id) { return (AllLoadedObjects().Count(_o => _o.ID.Equals(id)) != 0); }

        protected abstract double MaxLoad { get; }

        /// <summary>TRUE if object not in the direct load and can handle the weight, or somewhere in nested load already</summary>
        public bool CanAdd(ICoreObject obj)
        {
            if ((obj != null) && !_Objects.ContainsKey(obj.ID))
            {
                if (!NestedContains(obj.ID))
                {
                    if ((_Weight + obj.Weight) <= MaxLoad)
                    {
                        var _val = new Physical(Physical.PhysicalType.Weight, _Weight + obj.Weight);
                        return !_WCtrl.WillAbortChange(_val, @"Add");
                    }
                    else
                    {
                        // too heavy!
                        return false;
                    }
                }
                else
                {
                    // object is already in load somewhere, so we can handle it...
                    return true;
                }
            }
            return false;
        }

        #region public void Add(ICoreObject obj)
        /// <summary>
        /// Adds the weight, but only if the object is not null and does not exist in the load.  
        /// If the object was not in the nested load, a ValueChanged("Add") is signalled.
        /// </summary>
        public void Add(ICoreObject obj)
        {
            // true if the object was already in the load
            var _nested = NestedContains(obj.ID);
            if (CanAdd(obj))
            {
                var _val = new Physical(Physical.PhysicalType.Weight, _Weight + obj.Weight);
                _Weight += obj.Weight;

                // weight will be changing, since we weren't already accounting for it
                if (!_nested)
                {
                    _WCtrl.DoPreValueChanged(_val);
                }

                _Objects.Add(obj.ID, obj);

                // monitor object's weight
                obj.AddChangeMonitor(this);

                var _loaded = obj as ILoadedObjects;
                if ((_loaded != null) && _loaded.ContentsAddToLoad)
                {
                    // monitoring objects inventory (if they add to load)
                    _loaded.AddChangeMonitor((IMonitorChange<Physical>)this);
                }

                if (!_nested)
                {
                    // since we weren't accounting for it, indicate the change in load (by weight and object)
                    _WCtrl.DoValueChanged(_val, @"Add");
                    _ICOCtrl.DoValueChanged(obj, @"Add");
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Weight)));
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, obj));

                    // and anything in the object that adds to load
                    if (_loaded != null)
                    {
                        // signal add of all loaded objects in container
                        foreach (var _iCore in _loaded.AllLoadedObjects())
                        {
                            _ICOCtrl.DoValueChanged(_iCore, @"Add");
                        }
                    }
                }
            }
        }
        #endregion

        #region public void Remove(ICoreObject obj, ILoadedObjects targetContainer)
        /// <summary>
        /// Subtracts the weight, but only if the object is not null and exists in the load.  
        /// If the repository is not also in the object load, a ValueChanged("Remove") will be signalled.
        /// </summary>
        public void Remove(ICoreObject obj, ILoadedObjects targetLoad)
        {
            // nested if the load being extracted from is in our object load
            var _nested = (targetLoad != null) && NestedContains(targetLoad.ID) && targetLoad.ContentsAddToLoad;
            if ((obj != null) && _Objects.ContainsKey(obj.ID))
            {
                // the weight will decrease
                _Weight -= obj.Weight;

                // signal the decrease in weight
                var _val = new Physical(Physical.PhysicalType.Weight, _Weight);
                if (!_nested)
                {
                    _WCtrl.DoPreValueChanged(_val, @"Remove");
                }

                _Objects.Remove(obj.ID);

                // stop tracking it's weight (directly)
                obj.RemoveChangeMonitor(this);
                var _loaded = obj as ILoadedObjects;
                if (_loaded != null)
                {
                    // and stop tracking its contents (directly)
                    _loaded.RemoveChangeMonitor((IMonitorChange<Physical>)this);
                }

                // NOTE: an in-load container shift will not signal add-remove, so a remove where it is still nested shouldn't indicate the remove
                if (!_nested)
                {
                    // wasn't still nested, so we can really remove it
                    _WCtrl.DoValueChanged(_val, @"Remove");
                    _ICOCtrl.DoValueChanged(obj, @"Remove");
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Weight)));
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj));

                    // removing a container, so signale a remove of any objects that are in load
                    if (_loaded != null)
                    {
                        // remove all loaded objects in container
                        foreach (var _iCore in _loaded.AllLoadedObjects())
                        {
                            _ICOCtrl.DoValueChanged(_iCore, @"Remove");
                        }
                    }
                }
            }
        }
        #endregion

        #region IEnumerable<ICoreObject> Members
        public IEnumerator<ICoreObject> GetEnumerator()
        {
            foreach (KeyValuePair<Guid, ICoreObject> _kvp in _Objects)
            {
                yield return _kvp.Value;
            }
            yield break;
        }
        #endregion

        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (KeyValuePair<Guid, ICoreObject> _kvp in _Objects)
            {
                yield return _kvp.Value;
            }
            yield break;
        }
        #endregion

        #region IMonitorChange<Weight> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            if (args.NewValue.PropertyType == Physical.PhysicalType.Weight)
            {
                // get the weight of all direct objects
                var _tally = 0d;
                foreach (KeyValuePair<Guid, ICoreObject> _kvp in _Objects)
                {
                    _tally += _kvp.Value.Weight;
                }

                // set the weight, and signal the change
                _Weight = _tally;
                var _val = new Physical(Physical.PhysicalType.Weight, _Weight);
                _WCtrl.DoValueChanged(_val, @"WeightChange");
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"Weight"));
            }
        }
        #endregion

        #region IControlChange<Physical> Members
        private ChangeController<Physical> _WCtrl;
        public void AddChangeMonitor(IMonitorChange<Physical> monitor)
        {
            _WCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Physical> monitor)
        {
            _WCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        [field:NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region IControlChange<ICoreObject> Members
        private ChangeController<ICoreObject> _ICOCtrl;
        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            _ICOCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            _ICOCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        /// <summary>Returns all loaded objects and all objects in containers that contribute contents to object load</summary>
        public IEnumerable<ICoreObject> AllLoadedObjects()
        {
            foreach (KeyValuePair<Guid, ICoreObject> _kvp in _Objects)
            {
                yield return _kvp.Value;
            }

            foreach (ICoreObject _iCore in from _kvp in _Objects
                                           let _loader = _kvp.Value as ILoadedObjects
                                           where _loader != null
                                           from _iC in _loader.AllLoadedObjects()
                                           select _iC)
            {
                yield return _iCore;
            }
            yield break;
        }

        #region IMonitorChange<ICoreObject> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<ICoreObject> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ICoreObject> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<ICoreObject> args)
        {
            if (args.Action.Equals(@"Remove"))
            {
                // see if the show up somewhere else in the object load
                if (!NestedContains(args.NewValue.ID))
                {
                    // removed and we don't hold it now, continue to propagate
                    _ICOCtrl.DoValueChanged(args.NewValue, @"Remove");
                    if (args.NewValue is ILoadedObjects _loaded)
                    {
                        // signal remove of all loaded objects in container
                        foreach (var _iCore in _loaded.AllLoadedObjects())
                        {
                            _ICOCtrl.DoValueChanged(_iCore, @"Remove");
                        }
                    }
                }
            }
            else if (args.Action.Equals(@"Add"))
            {
                // expand all loaded objects if it is a container that handles load...
                _ICOCtrl.DoValueChanged(args.NewValue, @"Add");
                if (args.NewValue is ILoadedObjects _loaded)
                {
                    // signal add of all loaded objects in container
                    foreach (var _iCore in _loaded.AllLoadedObjects())
                    {
                        _ICOCtrl.DoValueChanged(_iCore, @"Add");
                    }
                }
            }
        }
        #endregion
    }
}
