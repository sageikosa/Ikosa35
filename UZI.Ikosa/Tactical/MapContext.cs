using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Uzi.Core;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;
using Uzi.Packaging;
using System.Runtime.Serialization;
using Uzi.Ikosa.Services;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Senses;
using System.Diagnostics;
using System.Collections.Concurrent;
using Uzi.Ikosa.Objects;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Wraps map context together
    /// </summary>
    [Serializable]
    public class MapContext : CoreSettingContext,
        IControlChange<Locator>, ICorePart, INotifyCollectionChanged, IEnumerable<Locator>,
        IDeserializationCallback
    {
        #region construction
        public MapContext(string name, LocalMap map)
            : base(name, map.ContextSet)
        {
            _InteractTransitZones = new WatchableSet<InteractCapture>();
            _LCtrl = new ChangeController<Locator>(this, null);
            _LocatorZones = new LocatorCaptureSet(this);
            _StrikeZones = new StrikeCaptureSet();
            _TransientVisualizers = new List<TransientVisualizer>();
        }
        #endregion

        #region data
        private WatchableSet<InteractCapture> _InteractTransitZones;
        private Collection<MovementZone> _MoveZones = new Collection<MovementZone>();
        private LocatorCaptureSet _LocatorZones;
        private StrikeCaptureSet _StrikeZones;
        private ChangeController<Locator> _LCtrl;
        private LocatorIndex _Index;

        [NonSerialized, JsonIgnore]
        private CreatureLoginInfoCollection _CreatureLogins;

        [NonSerialized, JsonIgnore]
        private List<TransientVisualizer> _TransientVisualizers = new List<TransientVisualizer>();

        [NonSerialized, JsonIgnore]
        private ulong _SerialState = 0;
        #endregion

        public ulong SerialState { get => _SerialState; set => _SerialState = value; }

        public LocalMap Map
            => ContextSet.Setting as LocalMap;

        public WatchableSet<InteractCapture> InteractTransitZones
            => _InteractTransitZones;

        public Collection<MovementZone> MovementZones
            => _MoveZones;

        public LocatorCaptureSet LocatorZones
            => _LocatorZones;

        public StrikeCaptureSet StrikeZones
            => _StrikeZones;

        public List<CreatureLoginInfo> CreatureLoginsInfos
            => _CreatureLogins?.GetList() ?? new List<CreatureLoginInfo>();

        public CreatureLoginInfo GetCreatureLoginInfo(Guid id)
            => _CreatureLogins.GetCreatureLoginInfo(id);

        #region public IEnumerable<Illumination> AllIlluminations { get; }
        public IEnumerable<Illumination> AllIlluminations
        {
            get
            {
                return Map
                    // room lights
                    .Rooms.SelectMany(_r => ListEffectsInRoom<Illumination>(_r).Where(_i => _i.IsActive))
                    // background lights
                    .Union(ListEffectsInBackground<Illumination>().Where(_i => _i.IsActive));
            }
        }
        #endregion

        // TODO: IStep manager for the entire campaign?  Action manager...?

        #region internal void CacheLocatorGroups()
        /// <summary>Called when map is reloaded, or when rooms or background change</summary>
        internal void CacheLocatorGroups()
        {
            // clear all groups (big change)
            Parallel.ForEach(Map.AllLocalCellGroups, (_group) =>
            {
                _group.ClearLocators();
            });

            // get all groups for all locators
            Parallel.ForEach(AllTokensOf<Locator>(), (_loc) =>
            {
                foreach (var _group in _loc.CalculateLocalCellGroups())
                {
                    _group.AddLocator(_loc);
                }
            });
        }
        #endregion

        #region internal void SetLocatorRegion(Locator locator, IGeometricRegion oldRegion)
        /// <summary>Intended to be called from Locator only</summary>
        internal void SetLocatorRegion(Locator locator, IGeometricRegion oldRegion, PlanarPresence prevPresence)
        {
            // TODO: planar presence changes and considerations

            #region LCG cache maintenance
            // if just added, this will be same as current
            var _lastGroups = locator.GetLocalCellGroups().ToList();
            var _currGroups = locator.CalculateLocalCellGroups().ToList();
            foreach (var _new in _currGroups.Except(_lastGroups))
            {
                _new.AddLocator(locator);
            }
            foreach (var _old in _lastGroups.Except(_currGroups))
            {
                _old.RemoveLocator(locator);
            }
            #endregion

            LocatorIndex.ReIndex(locator);

            #region capture zone updates
            // TODO: monitor change instead?
            var _iCore = locator.ICore as ICoreObject;
            var _planar = locator.PlanarPresence;
            var _geom = locator.GeometricRegion;
            foreach (var _capture in LocatorZones.AllCaptures())
            {
                // does the capture already contain the locator?
                if (_capture.Contents.Contains(locator))
                {
                    // does it still contain the locator?
                    if (_capture.ContainsGeometricRegion(_geom, _iCore, _planar))
                    {
                        // moving within a locatorCapture
                        _capture.MoveInArea(locator, false);
                    }
                    else
                    {
                        // exited a locatorCapture
                        _capture.Exit(locator);
                    }
                }
                else
                {
                    if (_capture.ContainsGeometricRegion(_geom, _iCore, _planar))
                    {
                        // entered a capture zone
                        _capture.Enter(locator);
                    }
                }
            }
            #endregion

            // light changes?
            var _lightNotify = new Lazy<Dictionary<Guid, LocalCellGroup>>();
            void _addLightNotify(LocalCellGroup group)
            {
                if (!_lightNotify.Value.ContainsKey(group.ID))
                    _lightNotify.Value.Add(group.ID, group);
            }

            // TODO: light/sound notify (ethereal/ethereal change)
            #region light/sound connected to locator
            // light or sound connected to object(s) in locator
            foreach (var _obj in locator.ICoreAs<ICoreObject>())
            {
                // see if any light/dark was connected
                if (FindAdjunctData.FindAdjuncts(_obj, typeof(Illumination), typeof(MagicDark)).Any())
                {
                    // relight all cell groups for both old and new locations
                    if (oldRegion != null)
                    {
                        foreach (var _grp in _currGroups
                            .Union(_lastGroups)
                            .Distinct())
                        {
                            _addLightNotify(_grp);
                        }
                    }
                    else
                    {
                        foreach (var _grp in _currGroups)
                        {
                            _addLightNotify(_grp);
                        }
                    }
                    break;
                }

                // update connected sounds
                var _sounds = FindAdjunctData.FindAdjuncts(_obj, typeof(SoundParticipant)).ToList();
                if (_sounds.Any())
                {
                    var _serial = _obj.IncreaseSerialState();
                    foreach (var _sg in _sounds.OfType<SoundParticipant>().Select(_sp => _sp.SoundGroup).Distinct())
                    {
                        _sg.SetSoundRef(_sg.SoundPresence.GetRefresh(_serial));
                    }
                }
            }
            #endregion

            // TODO: IAlterLink (ethereal/ethereal change)
            #region object can significantly alter a local link light/sound properties
            // see if IAlterLink object in locator is moving through any links (consider indexing links)
            var _alters = locator.ICoreAs<IAlterLocalLink>().ToList();
            if (_alters.Any())
            {
                // all links
                var _alteredLinks = (from _cg in _currGroups
                                     from _cl in _cg.Links.All
                                     where _cl.GeometricRegion.ContainsGeometricRegion(locator.GeometricRegion)
                                     select _cl)
                                     .Union((oldRegion != null)
                                     ? (from _og in _lastGroups
                                        from _ol in _og.Links.All
                                        where _ol.GeometricRegion.ContainsGeometricRegion(oldRegion)
                                        select _ol)
                                     : new List<LocalLink>()).Distinct().ToList();
                foreach (var _lightGroup in _alteredLinks.SelectMany(_l => _l.Groups).Distinct())
                {
                    _addLightNotify(_lightGroup);
                }

                // recalculate sound difficulty at each link
                var _serial = SerialState;
                var _updateGroups = new ConcurrentDictionary<Guid, LocalCellGroup>();
                Parallel.ForEach(_alteredLinks, _l => _l.RecalculateSoundDifficulty());

                // only update distinct sounds moving through portal's affected channels
                var _soundGroups = _alteredLinks.SelectMany(_l => _l.SoundChannelsToA.Union(_l.SoundChannelsToB))
                    .Select(_sc => _sc.Value.SoundGroup)
                    .Distinct()
                    .ToDictionary(_sg => _sg.SoundPresence.Audible.SoundGroupID);

                // refresh the sounds (portal changes will affect accordingly)
                Parallel.ForEach(_soundGroups.Values,
                    _sg => _sg.SetSoundRef(_sg.SoundPresence.GetRefresh(_serial), _updateGroups));

                // allow locators in groups to handle apparent sound changes
                if (_updateGroups.Any())
                {
                    // all potential listeners
                    _updateGroups.UpdateListenersInGroups((id) => _soundGroups.ContainsKey(id));
                }
            }
            #endregion

            // locator sensors get to refresh room awarenesses
            var _roomRefresh = locator.ICoreAs<ITacticalInquiry>().Any();

            if (!_roomRefresh)
            {
                // since room awarenesses won't be updated, the sensors on the locator should update directly
                foreach (var _sensor in locator.ICoreAs<ISensorHost>())
                {
                    _sensor.RoomAwarenesses?.RecalculateAwareness(_sensor);
                }
            }

            if (_lightNotify.IsValueCreated)
            {
                // light was calculated to affect multiple rooms...
                var _notifiers = _lightNotify.Value
                    .SelectMany(_g => _g.Value.NotifyLighting())
                    .Distinct()
                    .ToList();
                AwarenessSet.RecalculateAllSensors(Map, _notifiers, _roomRefresh);
            }
            else
            {
                // no notify light, but still want to recalculate all sensors in main groups...
                var _notifiers =
                    ((oldRegion != null)
                    ? _currGroups.Union(_lastGroups).SelectMany(_g => _g.GetSensorKeys())
                    : _currGroups.SelectMany(_g => _g.GetSensorKeys()))
                    .Distinct().ToList();
                AwarenessSet.RecalculateAllSensors(Map, _notifiers, _roomRefresh);
            }
        }
        #endregion

        public IEnumerable<InteractCapture> GetInteractionTransitZones(Interaction workSet)
            => _InteractTransitZones.Where(_z => _z.CanAlterInteraction(workSet));

        /// <summary>Enumerator over all zones in effect at this cell location</summary>
        public IEnumerable<InteractCapture> GetInteractionTransitZones(Interaction workSet, ICellLocation location)
            => _InteractTransitZones
            .Where(_z => _z.CanAlterInteraction(workSet) && _z.Geometry.Region.ContainsCell(location));

        /// <summary>Yields all locators that contain the given cell</summary>
        public IEnumerable<Locator> LocatorsInCell(ICellLocation location, PlanarPresence locPlanes)
            => LocatorIndex.GetLocators(location, locPlanes);

        /// <summary>Yields all distinct locators that have at least one cell in the geometric extent</summary>
        public IEnumerable<Locator> LocatorsInRegion(IGeometricRegion geometry, PlanarPresence locPlanes)
            => LocatorIndex.GetLocatorsInRegion(geometry, locPlanes)
            .Where(_l => _l.GeometricRegion.ContainsGeometricRegion(geometry));

        /// <summary>Both background groups and rooms that are not enclosed</summary>
        public IEnumerable<Locator> LocatorsInBackground()
            => AllTokensOf<Locator>().Where(_l => _l.HasBackgroundPresence() && _l.PlanarPresence.HasMaterialPresence());

        /// <summary>Only gets those directly on locators in the cell</summary>
        public IEnumerable<IType> AllInCell<IType>(ICellLocation location, PlanarPresence locPlanes)
            where IType : ICore
            => from _loc in LocatorsInCell(location, locPlanes)
               from _base in _loc.ICoreAs<IType>()
               select _base;

        /// <summary>Enumerate all effects on all locatable objects that are assignable to a specific type.  Material presence ONLY</summary>
        public IEnumerable<EffectType> ListEffectsInRoom<EffectType>(Room room) where EffectType : Adjunct
            => from _loc in LocatorsInRegion(room, PlanarPresence.Material)
               from _obj in _loc.ICoreAs<ICoreObject>()
               from _adj in FindAdjunctData.FindAdjuncts(_obj, typeof(EffectType))
               select _adj as EffectType;

        public IEnumerable<EffectType> ListEffectsInBackground<EffectType>() where EffectType : Adjunct
            => from _loc in LocatorsInBackground()
               from _obj in _loc.ICoreAs<ICoreObject>()
               from _adj in FindAdjunctData.FindAdjuncts(_obj, typeof(EffectType))
               select _adj as EffectType;

        #region IControlChange<Locator> Members
        public void AddChangeMonitor(IMonitorChange<Locator> monitor)
        {
            _LCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Locator> monitor)
        {
            _LCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region protected override void OnAdd(CoreToken newItem)
        protected override void OnAdd(CoreToken newItem)
        {
            if (newItem is Locator)
            {
                var _loc = newItem as Locator;
                _loc.Delocate();
                _LCtrl.DoValueChanged(_loc, @"Add");
                _loc.Locate();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                // creature?
                if (_loc?.Chief is Creature)
                {
                    _CreatureLogins.Add((_loc?.Chief as Creature)?.ToCreatureLoginInfo());
                }
            }
            base.OnAdd(newItem);
        }
        #endregion

        #region protected override void OnRemove(CoreToken oldItem)
        protected override void OnRemove(CoreToken oldItem)
        {
            base.OnRemove(oldItem);
            if (oldItem is Locator)
            {
                _LCtrl.DoValueChanged(oldItem as Locator, @"Remove");
                var _loc = (oldItem as Locator);
                _loc.Delocate();
                LocatorIndex.Remove(oldItem as Locator);

                // remove from last rooms
                var _lastRooms = Map.AllLocalCellGroups.Where(_r => _r.Contains(_loc)).ToList();
                foreach (var _old in _lastRooms)
                {
                    _old.RemoveLocator(_loc);
                }

                // creature?
                if (_loc?.Chief is Creature _critter)
                {
                    _CreatureLogins.Remove(_critter.ID);

                    // remove from budgets...
                    Map.IkosaProcessManager.RemoveFromTrackers(_critter);
                }

                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        #endregion

        public IEnumerable<ICorePart> Relationships
            => AllTokens.OfType<Locator>();

        public string TypeName
            => typeof(MapContext).FullName;

        #region INotifyCollectionChanged Members
        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        #endregion

        #region IEnumerable<Locator> Members

        public IEnumerator<Locator> GetEnumerator()
        {
            foreach (var _loc in AllTokensOf<Locator>().OrderBy(_l => _l.Name))
                yield return _loc;
            yield break;
        }

        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var _loc in AllTokensOf<Locator>().OrderBy(_l => _l.Name))
                yield return _loc;
            yield break;
        }
        #endregion

        /// <summary>Transient Visualizers as specified by actions</summary>
        public List<TransientVisualizer> TransientVisualizers
            => _TransientVisualizers ??= new List<TransientVisualizer>();

        public void RefreshAwarenesses()
        {
            // clear sensor notifiers
            Parallel.ForEach(Map.AllLocalCellGroups, _lcg => _lcg.ClearSensorNotifiers());

            // connect creatures with local cell groups
            Parallel.ForEach(Map.GetAllICore<ISensorHost>()
                .Where(_sh => _sh.IsSensorHostActive),
                _sensors =>
                {
                    _sensors.RoomAwarenesses?.RecalculateAwareness(_sensors);
                    _sensors.Awarenesses?.RecalculateAwareness(_sensors);
                });
        }

        public void OnDeserialization(object sender)
        {
            // NOTE: these may be retired at some point...

            if (_Index == null)
            {
                RebuildIndex();
            }
            else
            {
                // load creatures
                var _logins = new CreatureLoginInfoCollection();
                foreach (var _critter in Map.GetAllICore<Creature>())
                {
                    _logins.Add(_critter.ToCreatureLoginInfo());
                }
                _CreatureLogins = _logins;
            }
            EnsureGrips();
        }

        private void EnsureGrips()
        {
            // ensure that cellspaces that can have grips have grips
            foreach (var _space in Map.CellSpaces
                .Where(_cs => _cs.GripRules.Rules.Count == 0)
                .Select(_cs => _cs))
            {
                var _materials = _space.AllMaterials.ToList();
                if (_materials.Count > 1)
                {
                    _space.GripRules.InitializeMaterials(_materials[0], _materials[1]);
                }
                else
                {
                    _space.GripRules.InitializeUniform(_space.CellMaterial);
                }
            }
            // ¿¿¿ panel space ???
        }

        public void RebuildIndex()
        {
            var _index = new LocatorIndex(Map);
            foreach (var _loc in AllTokensOf<Locator>())
                _index.Add(_loc);
            _Index = _index;

            CacheLocatorGroups();

            // load creatures
            var _logins = new CreatureLoginInfoCollection();
            foreach (var _critter in Map.GetAllICore<Creature>())
            {
                _logins.Add(_critter.ToCreatureLoginInfo());
            }
            _CreatureLogins = _logins;
        }

        private LocatorIndex LocatorIndex
            => _Index;
    }
}
