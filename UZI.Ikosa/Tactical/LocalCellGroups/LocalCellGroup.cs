using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Uzi.Ikosa.Senses;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;
using System.Threading.Tasks;
using Uzi.Core.Contracts;
using Uzi.Visualize.Contracts.Tactical;
using System.Diagnostics;
using System.Text;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    /// <summary>Base class for Room and BackgroundCellGroup</summary>
    public abstract class LocalCellGroup : Cubic, INotifyPropertyChanged, ISerializable
    {
        #region construction
        protected LocalCellGroup(ICellLocation start, IGeometricSize size, LocalMap map, string name, bool deepShadows)
            : base(start, size)
        {
            _Map = map;
            _Name = name;
            _DeepShadows = deepShadows;
            _Links = new LocalLinkSet(this);
            _Fresh = new FreshnessTime();
            _Locators = [];
            _ID = Guid.NewGuid();
        }

        protected LocalCellGroup(SerializationInfo info, StreamingContext context)
            : base(info, context, true)
        {
            string _fullName(string field)
                => $@"{nameof(LocalCellGroup)}+{field}";

            _Map = (LocalMap)info.GetValue(_fullName(nameof(_Map)), typeof(LocalMap));
            _Name = info.GetString(_fullName(nameof(_Name)));
            _DeepShadows = info.GetBoolean(_fullName(nameof(_DeepShadows)));
            _Links = (LocalLinkSet)info.GetValue(_fullName(nameof(_Links)), typeof(LocalLinkSet));
            _Light = (AmbientLight)info.GetValue(_fullName(nameof(_Light)), typeof(AmbientLight));
            _Fresh = (FreshnessTime)info.GetValue(_fullName(nameof(_Fresh)), typeof(FreshnessTime));
            _Locators = (List<Locator>)info.GetValue(_fullName(nameof(_Locators)), typeof(List<Locator>));
            _ID = (Guid)info.GetValue(_fullName(nameof(_ID)), typeof(Guid));

            foreach (var _p in info)
            {
                if (_p.Name.Equals(@"AwareSensors"))
                {
                    var _awareness = (Dictionary<Guid, Guid>)info.GetValue(_fullName(@"AwareSensors"), typeof(Dictionary<Guid, Guid>));
                    _AwareSensors = new ConcurrentDictionary<Guid, Guid>(_awareness);
                    break;
                }
            }
            _AwareSensors ??= new();
        }
        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            string _fullName(string field)
                => $@"{nameof(LocalCellGroup)}+{field}";

            base.GetObjectData(info, context);
            info.AddValue(_fullName(nameof(_Map)), _Map);
            info.AddValue(_fullName(nameof(_Name)), _Name);
            info.AddValue(_fullName(nameof(_DeepShadows)), _DeepShadows);
            info.AddValue(_fullName(nameof(_Links)), _Links);
            info.AddValue(_fullName(nameof(_Light)), _Light);
            info.AddValue(_fullName(nameof(_Fresh)), _Fresh);
            info.AddValue(_fullName(nameof(_Locators)), _Locators);
            info.AddValue(_fullName(nameof(_ID)), _ID);
            info.AddValue(_fullName(@"AwareSensors"), _AwareSensors.ToDictionary(_x => _x.Key, _x => _x.Value));
        }

        #region state
        protected LocalMap _Map;
        private string _Name;
        private bool _DeepShadows;
        private LocalLinkSet _Links;
        private AmbientLight _Light = null;
        private FreshnessTime _Fresh;
        private List<Locator> _Locators; // TODO: internally maintain
        private Guid _ID = Guid.Empty;
        private ConcurrentDictionary<Guid, Guid> _AwareSensors;
        #endregion

        public LocalMap Map => _Map;
        public IEnumerable<Locator> Locators => _Locators;
        public FreshnessTime Freshness => _Fresh;
        public LocalLinkSet Links => _Links;

        #region public Guid ID { get; }
        public Guid ID
        {
            get
            {
                if (_ID == Guid.Empty)
                {
                    _ID = Guid.NewGuid();
                }

                return _ID;
            }
        }
        #endregion

        /// <summary>true if the locator is currently cached</summary>
        public bool Contains(Locator locator)
            => _Locators.Contains(locator);

        internal void ClearLocators()
            => _Locators.Clear();

        #region internal void AddLocator(Locator locator)
        internal void AddLocator(Locator locator)
        {
            if (!_Locators.Contains(locator))
            {
                _Locators.Add(locator);
            }
        }
        #endregion

        #region internal void RemoveLocator(Locator locator)
        internal void RemoveLocator(Locator locator)
        {
            if (_Locators.Contains(locator))
            {
                _Locators.Remove(locator);
            }
        }
        #endregion

        #region public AmbientLight Light { get; set; }
        // NOTE: currently only allowing one ambient light per local cell group max
        public AmbientLight Light
        {
            get => _Light;
            set
            {
                _Light = value;
                DoPropertyChanged(@"Light");

                var _notifiers = NotifyLighting();
                AwarenessSet.RecalculateAllSensors(Map, _notifiers, false);
            }
        }
        #endregion

        #region public string Name { get; set; }
        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                DoPropertyChanged(nameof(Name));
            }
        }
        #endregion

        /// <summary>Gets a CellSpace using LocalMap coordinates</summary>
        public abstract ref readonly CellStructure GetCellSpace(ICellLocation location);

        public abstract ref readonly CellStructure GetCellSpace(int z, int y, int x);

        //public abstract CellStructure? GetContainedCellSpace(int z, int y, int x);

        #region protected IEnumerable<LocalCellGroup> OnNotifyLinkLighting()
        protected IEnumerable<LocalCellGroup> OnNotifyLinkLighting()
        {
            // lights for the local cell group (or background set)
            var _lights = GetIlluminators(true).Distinct().ToList();
            if (IsPartOfBackground)
            {
                // background links
                foreach (var _lcgLink in from _lcg in Map.GetSharedGroups(this)
                                         from _lnk in _lcg.Links.All
                                         select new { Group = _lcg, Link = _lnk })
                {
                    // light source must not be a link, or the link can only share 1 group with the target
                    var _notify = _lcgLink.Link.NotifyLight(_lcgLink.Group, _lights.Union(_lcgLink.Group.GetAmbientIlluminators()));
                    if (_notify != null)
                    {
                        yield return _notify;
                    }
                }
            }
            else
            {
                // direct links only
                foreach (var _lnk in Links.All)
                {
                    // light source must not be a link, or the link can only share 1 group with the target
                    var _notify = _lnk.NotifyLight(this, _lights.Union(GetAmbientIlluminators()));
                    if (_notify != null)
                    {
                        yield return _notify;
                    }
                }
            }
            yield break;
        }
        #endregion

        #region public List<Guid> NotifyLighting()
        /// <summary>
        /// Called whenever a light is added, removed, or altered. 
        /// Returns keys of sensors needing to be refreshed.
        /// </summary>
        public List<Guid> NotifyLighting(bool fullSweep = true)
        {
            Debug.WriteLine($@"Relight from {Name}");
            var _updaters = new List<LocalCellGroup>
            {
                this
            };

            #region semi-parallel propagation
            // propagate (ensure each room is hit once per sweep)
            var _nextGroups = OnNotifyLinkLighting().Distinct().ToList();
            while (_nextGroups.Count > 0)
            {
                // merge updaters
                _updaters = _updaters.Union(_nextGroups).ToList();

                // multi-next = all non-background groups, plus background (if exists)
                //              setup as a snapshot and as parallel query
                var _multiNext = _nextGroups.Where(_g => !_g.IsPartOfBackground)
                    .Union(_nextGroups.FirstOrDefault(_g => _g.IsPartOfBackground).ToEnumerable())
                    .Where(_g => _g != null).ToList().AsParallel();

                // get next sweep in parallel
                _nextGroups = (from _group in _multiNext
                               from _next in _group.OnNotifyLinkLighting()
                               select _next).Distinct().ToList();
            }
            #endregion

            if (!fullSweep)
            {
                return _updaters.SelectMany(_u => _u.GetSensorKeys()).Distinct().ToList();
            }

            // gather locators in all notified regions
            var _locList = _updaters.SelectMany(_g => _g.Locators).Distinct().ToList();
            Parallel.ForEach(_locList, (_loc) =>
            {
                _loc.DetermineIllumination();
            });

            Parallel.ForEach(_updaters.OfType<Room>(), (_room) =>
            {
                // update each room shading
                _room.RefreshTerrainShading();
            });

            // update background shading
            var _bgGroup = _updaters.OfType<BackgroundCellGroup>().FirstOrDefault();
            if (_bgGroup != null)
            {
                _bgGroup.RefreshTerrainShading();
            }

            // report sensors that need to be notified
            return _updaters.SelectMany(_u => _u.GetSensorKeys()).Distinct().ToList();
        }
        #endregion

        #region public bool DeepShadows { get; set; }
        public bool DeepShadows
        {
            get => _DeepShadows;
            set
            {
                _DeepShadows = value;
                DoPropertyChanged(nameof(DeepShadows));
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public virtual IEnumerable<(SoundRef sound, IGeometricRegion region, double magnitude)> GetSoundRefs(IGeometricContext target)
        public virtual IEnumerable<(SoundRef sound, IGeometricRegion region, double magnitude)> GetSoundRefs(IGeometricContext target,
            Func<Guid, bool> soundFilter)
        {
            // background sounds (no links between background, so get them all)
            foreach (var _sound in Map.MapContext.ListEffectsInBackground<SoundParticipant>()
                .Where(_s => soundFilter(_s.SoundGroup.SoundPresence.Audible.SoundGroupID)))
            {
                var _rgn = _sound.GetGeometricRegion();
                yield return (_sound.SoundGroup.SoundPresence, _rgn, _sound.SoundGroup.SoundPresence.GetRelativeMagnitude(_rgn, target));
            }

            // link sounds from links to every background...
            if (IsPartOfBackground)
            {
                foreach (var _channel in from _lcg in Map.GetSharedGroups(this)
                                         from _l in _lcg.Links.All
                                         from _sckvp in _l.ChannelsTo(_lcg)
                                         let _sc = _sckvp.Value
                                         where _sc.ChannelFlow.project.IsPartOfBackground
                                         && _sc.SoundPresence != null
                                         && soundFilter(_sc.SoundPresence.Audible.SoundGroupID)
                                         select _sc)
                {
                    var _cell = _channel.ChannelFlow.link.InteractionCell(_channel.ChannelFlow.project);
                    yield return (_channel.SoundPresence, _channel.SoundPresence.Cell ?? _cell, _channel.SoundPresence.GetRelativeMagnitude(_cell, target));
                }
            }
            yield break;
        }
        #endregion

        #region public virtual IEnumerable<SoundGroup> GetSoundGroups(Func<Guid, bool> soundFilter)
        public virtual IEnumerable<SoundGroup> GetSoundGroups(Func<Guid, bool> soundFilter)
        {
            // background sounds (no links between background, so get them all)
            foreach (var _sound in Map.MapContext.ListEffectsInBackground<SoundParticipant>()
                .Where(_s => soundFilter(_s.SoundGroup.SoundPresence.Audible.SoundGroupID)))
            {
                yield return _sound.SoundGroup;
            }

            // link sounds from links to every background...
            if (IsPartOfBackground)
            {
                foreach (var _channel in from _lcg in Map.GetSharedGroups(this)
                                         from _l in _lcg.Links.All
                                         from _sckvp in _l.ChannelsTo(_lcg)
                                         let _sc = _sckvp.Value
                                         where _sc.ChannelFlow.project.IsPartOfBackground
                                         && _sc.SoundPresence != null
                                         && soundFilter(_sc.SoundPresence.Audible.SoundGroupID)
                                         select _sc)
                {
                    yield return _channel.SoundGroup;
                }
            }
            yield break;
        }
        #endregion

        #region public virtual IEnumerable<IIllumination> GetIlluminators(bool linking)
        /// <summary>All point source illuminators in the local cell group</summary>
        public virtual IEnumerable<IIllumination> GetIlluminators(bool linking)
        {
            // background illuminations (no links between background, so get them all)
            foreach (var _illum in Map.MapContext.ListEffectsInBackground<Illumination>()
                .OfType<IIllumination>()
                .Where(_i => _i.IsActive && _i.PlanarPresence.HasMaterialPresence()))
            {
                yield return _illum;
            }

            // link illuminations from links to every background...
            if (IsPartOfBackground)
            {
                foreach (var _light in from _lcg in Map.GetSharedGroups(this)
                                       from _lnk in _lcg.Links.All
                                       from _l in _lnk.GetLights(_lcg, linking)
                                       select _l)
                {
                    yield return _light;
                }
            }
            yield break;
        }
        #endregion

        #region public IEnumerable<IIllumination> GetAmbientIlluminators()
        /// <summary>Provides ambient illuminator(s)</summary>
        public IEnumerable<IIllumination> GetAmbientIlluminators()
        {
            if (Light != null)
            {
                // note: Currently only a single ambient illuminator
                yield return new AmbientIllumination(Light);
            }
            yield break;
        }
        #endregion

        private ConcurrentDictionary<Guid, Guid> AwareSensors
        {
            get
            {
                _AwareSensors ??= new ConcurrentDictionary<Guid, Guid>();
                return _AwareSensors;
            }
        }

        /// <summary>Called in MapContext.RefreshAwareness to wipe everything clean before refilling.</summary>
        internal void ClearSensorNotifiers()
        {
            _AwareSensors?.Clear();
        }

        public void AddSensorsNotifier(ISensorHost sensors)
        {
            AwareSensors.TryAdd(sensors.ID, sensors.ID);
        }

        public void RemoveSensorsNotifier(ISensorHost sensors)
        {
            AwareSensors.TryRemove(sensors.ID, out var _);
        }

        /// <summary>Sensors IDs that need to be notified on a change to the local cell group</summary>
        public IEnumerable<Guid> GetSensorKeys()
            => AwareSensors.Keys.Select(_k => _k);

        public virtual void RefreshTerrainShading()
        {
        }

        /// <summary>Reports the light level in the specified cell</summary>
        public abstract LightRange GetLightLevel(ICellLocation location);

        /// <summary>Reports whether the specified cell is in magical darkness</summary>
        public abstract bool IsInMagicDarkness(ICellLocation location);

        /// <summary>Indicates this cell group shares in background cell group lighting</summary>
        public abstract bool IsPartOfBackground { get; }

        #region protected LCGInfo ToInfo<LCGInfo>()
        protected LCGInfo ToInfo<LCGInfo>()
            where LCGInfo : LocalCellGroupInfo, new()
        {
            var _info = new LCGInfo();
            _info.SetCubicInfo(this);
            _info.CellGroupName = Name;
            _info.IsDeepShadows = DeepShadows;
            _info.Freshness = Freshness;
            _info.ID = ID;
            _info.TouchingRooms = Links.TouchingRooms.Select(_r => _r.ID).ToList();
            return _info;
        }
        #endregion

        public LocalCellGroupInfo ToLocalCellGroup()
            => ToInfo<LocalCellGroupInfo>();
    }
}
