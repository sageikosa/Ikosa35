using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using System.Collections.ObjectModel;
using System.Threading;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Uzi.Visualize.Packaging;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;
using Uzi.Core.Contracts.Faults;
using Uzi.Visualize.Contracts;
using System.Diagnostics;

namespace Uzi.Ikosa.Services
{
    // NOTE: If you change the class name "VisualizationService" here, you must also update the reference to "VisualizationService" in App.config.
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext = false
        )]
    public class VisualizationService : IVisualizationService
    {
        // TODO: fully destatic?

        #region private data
        private static LocalMap _Map;
        private static ConcurrentDictionary<string, VisualizationCallbackTracker> _Callbacks =
            new ConcurrentDictionary<string, VisualizationCallbackTracker>();
        #endregion

        public static Action<ConsoleMessage> Console { get; set; }
        public static IVisualizationCallback HostCallback { get; set; }

        #region private static void ConsoleLog(string title, string id, string details)
        private static void ConsoleLog(string title, string id, string details)
        {
            Console?.Invoke(new ConsoleMessage
            {
                Title = title,
                Message = string.Format(@"ID: '{0}' errored out.", id),
                Details = details,
                Source = typeof(VisualizationService)
            });
        }
        #endregion

        #region public static LocalMap Map { get; set; }
        public static LocalMap Map
        {
            get { return _Map; }
            set
            {
                if (_Map != null)
                {
                    // unhook old map
                    _Map.MapChanged -= new EventHandler(_Map_MapChanged);
                }

                // set map
                _Map = value;

                if (_Map != null)
                {
                    // hook new map
                    _Map.MapChanged += new EventHandler(_Map_MapChanged);
                }
            }
        }
        #endregion

        #region public static void DoNotifyMapChanged()
        static void _Map_MapChanged(object sender, EventArgs e)
        {
            // event handler
            DoNotifyMapChanged();
        }

        public static void DoNotifyMapChanged()
        {
            if (HostCallback != null)
            {
                HostCallback.MapChanged();
            }
            foreach (var _cback in _Callbacks)
            {
                var _target = _cback;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        _target.Value.Callback.MapChanged();
                    }
                    catch (Exception _ex)
                    {
                        ConsoleLog(@"MapChanged Notify Failure", _target.Key, _ex.Message);

                        // evict
                        _Callbacks.TryRemove(_target.Key, out VisualizationCallbackTracker _out);
                    }
                });
            }
        }
        #endregion

        #region private Guid GetGuid(string id)
        private Guid GetGuid(string id)
        {
            // parse guid
            Guid _id;
            try
            {
                _id = new Guid(id);
                return _id;
            }
            catch
            {
                throw new FaultException<InvalidArgumentFault>(new InvalidArgumentFault(@"id"), @"Unable to coerce value to a creature Id");
            }
        }
        #endregion

        #region public void RegisterCallback(string[] creatureIDs)
        public void RegisterCallback(string[] creatureIDs)
        {
            // parse guid
            var _principal = Thread.CurrentPrincipal;
            if (creatureIDs.All(_c => _principal.IsInRole(_c)))
            {
                // gather parameters
                var _tracker = new VisualizationCallbackTracker
                {
                    UserName = _principal.Identity.Name,
                    IDs = creatureIDs.Select(_i => GetGuid(_i)).ToList(),
                    Callback = OperationContext.Current.GetCallbackChannel<IVisualizationCallback>()
                };

                // add or update
                _Callbacks.AddOrUpdate(_tracker.UserName, _tracker,
                    (name, cBack) =>
                    {
                        return _tracker;
                    });
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access these creatures");
            }
        }
        #endregion

        #region public void DeRegisterCallback()
        public void DeRegisterCallback()
        {
            // parse guid
            var _principal = Thread.CurrentPrincipal;
            _Callbacks.TryRemove(_principal.Identity.Name, out _);
        }
        #endregion

        public static void DeRegisterCallback(string userName)
        {
            _Callbacks.TryRemove(userName, out _);
        }

        #region public BrushCollectionInfo GetRootBrushCollection()
        public BrushCollectionInfo GetRootBrushCollection()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                return new BrushCollectionInfo(Map.Resources.Brushes);
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public BrushCollectionInfo GetBrushCollection(string name)
        public BrushCollectionInfo GetBrushCollection(string name)
        {
            // TODO: any authorization issues with brushes?
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _brushCollection = Map.Resources.ResolvableBrushCollections
                    .FirstOrDefault(_bci => _bci.BrushCollectionPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (_brushCollection != null)
                {
                    return new BrushCollectionInfo(_brushCollection.BrushCollectionPart);
                }
                return null;

            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public Model3DInfo GetModel3D(string name)
        public Model3DInfo GetModel3D(string name)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _model3D = Map.Resources.ResolvableModels
                    .FirstOrDefault(_mdl => _mdl.Model3DPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .Model3DPart;
                if (_model3D != null)
                {
                    if (_model3D is MetaModel)
                    {
                        return new MetaModel3DInfo(_model3D as MetaModel);
                    }
                    else
                    {
                        return new Model3DInfo(_model3D);
                    }
                }
                return null;

            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public MetaModelFragmentInfo GetModel3DFragmentForModel(string metaModelName, string fragmentName)
        public MetaModelFragmentInfo GetModel3DFragmentForModel(string metaModelName, string fragmentName)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _item = Map.Resources.ResolvableModels
                    .FirstOrDefault(_mli => _mli.Model3DPart.Name.Equals(metaModelName, StringComparison.OrdinalIgnoreCase));
                if (_item != null)
                {
                    if (_item.Model3DPart is MetaModel _metaModel)
                    {
                        if (_metaModel.Fragments.Any(_f => _f.Key.Equals(fragmentName, StringComparison.OrdinalIgnoreCase)))
                        {
                            return new MetaModelFragmentInfo(_metaModel.Fragments.FirstOrDefault(_f => _f.Key.Equals(fragmentName, StringComparison.OrdinalIgnoreCase)).Value);
                        }
                    }
                }
                return null;

            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public string[] MetaModelFragmentNames()
        public string[] MetaModelFragmentNames()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                return Map.Resources.ResolvableFragments.Select(_f => _f.MetaModelFragment.Name).ToArray();
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public MetaModelFragmentInfo GetModel3DFragment(string fragmentName)
        public MetaModelFragmentInfo GetModel3DFragment(string fragmentName)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _fragment = Map.Resources.ResolvableFragments
                    .FirstOrDefault(_mdl => _mdl.MetaModelFragment.Name.Equals(fragmentName, StringComparison.OrdinalIgnoreCase))
                    .MetaModelFragment;
                if (_fragment != null)
                {
                    return new MetaModelFragmentInfo(_fragment);
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public string[] BitmapImageNames()
        public string[] BitmapImageNames()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                return Map.Resources.ResolvableImages.Select(_i => _i.BitmapImagePart.Name).ToArray();
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public BitmapImageInfo GetBitmapImage(string name)
        public BitmapImageInfo GetBitmapImage(string name)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _image = Map.Resources.ResolvableImages
                    .FirstOrDefault(_img => _img.BitmapImagePart.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .BitmapImagePart;
                if (_image != null)
                {
                    return new BitmapImageInfo(_image);
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public BitmapImageInfo GetBitmapImageForModel(string modelName, string name)
        public BitmapImageInfo GetBitmapImageForModel(string modelName, string name)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _item = Map.Resources.ResolvableModels
                   .FirstOrDefault(_mli => _mli.Model3DPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (_item != null)
                {
                    var _model3D = _item.Model3DPart;
                    if (_model3D != null)
                    {
                        var _image = _model3D.Images.FirstOrDefault(_kvp => _kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
                        if (_image != null)
                        {
                            return new BitmapImageInfo(_image);
                        }
                    }
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public BitmapImageInfo GetBitmapImageForBrushCollection(string brushCollectionName, string name)
        public BitmapImageInfo GetBitmapImageForBrushCollection(string brushCollectionName, string name)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _brushCollection = Map.Resources.ResolvableBrushCollections
                    .FirstOrDefault(_bci => _bci.BrushCollectionPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (_brushCollection != null)
                {
                    var _image = _brushCollection.BrushCollectionPart.Images.FirstOrDefault(_kvp => _kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
                    if (_image != null)
                    {
                        return new BitmapImageInfo(_image);
                    }
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public BitmapImageInfo GetBitmapImageForMetaModelBrushesCollection(string metaModelName, string name)
        public BitmapImageInfo GetBitmapImageForMetaModelBrushesCollection(string metaModelName, string name)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _item = Map.Resources.ResolvableModels
                   .FirstOrDefault(_mli => _mli.Model3DPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (_item != null)
                {
                    if (_item.Model3DPart is MetaModel _metaModel)
                    {
                        var _image = _metaModel.Brushes.Images.FirstOrDefault(_kvp => _kvp.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;
                        if (_image != null)
                        {
                            return new BitmapImageInfo(_image);
                        }
                    }
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public TileSetInfo GetTileSet(string cellMaterial, string tiling)
        public TileSetInfo GetTileSet(string cellMaterial, string tiling)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _material = _Map.CellMaterials[cellMaterial];
                if (_material != null)
                {
                    var _tiling = _material.AvailableTilings.FirstOrDefault(_t => _t?.Name.Equals(tiling, StringComparison.OrdinalIgnoreCase) ?? false);
                    return _tiling?.ToTileSetInfo(cellMaterial);
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public IEnumerable<BackgroundCellGroupInfo> GetBackgrounds()
        public IEnumerable<BackgroundCellGroupInfo> GetBackgrounds()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                return (from _bg in Map.Backgrounds.All()
                        where Map.BackgroundViewport.IsOverlapping(_bg)
                        select _bg.ToBackgroundCellGroupInfo())
                       .ToList();
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public IEnumerable<CellSpaceInfo> GetCellSpaces()
        public IEnumerable<CellSpaceInfo> GetCellSpaces()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                return Map.CellSpaces.Select(_cs => _cs.ToCellSpaceInfo());
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public string[] IconNames()
        public string[] IconNames()
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                return Map.Resources.ResolvableIcons.Select(_i => _i.IconPart.Name).ToArray();
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public IconInfo GetIcon(string name)
        public IconInfo GetIcon(string name)
        {
            try
            {
                Map.Synchronizer.EnterReadLock();
                var _icon = Map.Resources.ResolvableIcons
                    .FirstOrDefault(_img => _img.IconPart.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    .IconPart;
                if (_icon != null)
                {
                    return new IconInfo(_icon);
                }
                return null;
            }
            finally
            {
                if (Map.Synchronizer.IsReadLockHeld)
                {
                    Map.Synchronizer.ExitReadLock();
                }
            }
        }
        #endregion

        #region public IEnumerable<FreshnessTag> GetRooms(string creatureID)
        public IEnumerable<FreshnessTag> GetRooms(string creatureID, string sensorHostID)
        {
            Guid _critterID = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_critterID.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();

                    // find sensors
                    var _sensors = GetSensorHost(sensorHostID, _critterID);
                    var _sweep = _sensors?.RoomAwarenesses?.GetSweptRooms();
                    return Map.Rooms
                        .Where(_r => _sweep?.ContainsKey(_r.ID) ?? false)
                        .Select(_r => new FreshnessTag
                        {
                            ID = _r.ID,
                            FreshTime = _r.Freshness
                        }).ToList();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public RoomInfo GetRoom(string creatureID, string id)
        public RoomInfo GetRoom(string creatureID, string sensorHostID, string roomID)
        {
            Guid _critterID = GetGuid(creatureID);
            var _roomID = GetGuid(roomID);
            if (Thread.CurrentPrincipal.IsInRole(_roomID.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();

                    // find sensors
                    var _sensors = GetSensorHost(sensorHostID, _critterID);
                    if (_sensors?.RoomAwarenesses?.IsSweptRoom(_roomID) ?? false)
                    {
                        return Map.Rooms
                            .Where(_r => _r.ID == _roomID)
                            .Select(_r => _r.ToRoomInfo())
                            .FirstOrDefault();
                    }

                    return null;
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public CubicInfo GetMapViewport(string creatureID, string sensorHostID)
        public CubicInfo GetMapViewport(string creatureID, string sensorHostID)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();
                    // TODO: sensor host relative map viewport
                    var _cubic = new CubicInfo();
                    _cubic.SetCubicInfo(Map.BackgroundViewport);
                    return _cubic;
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public IEnumerable<SensorHostInfo> GetSensorHosts(string creatureID)
        public IEnumerable<SensorHostInfo> GetSensorHosts(string creatureID)
        {
            // parse guid
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();
                    // find creature
                    var _critter = Map.GetCreature(_id);
                    return (_critter.ToEnumerable()
                        .Union(RemoteSenseMaster.GetSensorHosts(_critter))
                        .Select(_sh => _sh.ToSensorHostInfo()))
                        .ToList();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public IEnumerable<ShadingInfo> GetShadingInfos(string creatureID, string sensorHostID)
        public IEnumerable<ShadingInfo> GetShadingInfos(string creatureID, string sensorHostID)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();
                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    var _loc = _sensors.GetLocated();
                    var _senses = _sensors.GetTerrainVisualizer();

                    // get effect information
                    var _render = _sensors?.RoomAwarenesses?.GetRenderedRooms();
                    var _roomShade = from _room in Map.Rooms
                                     where _render?.ContainsKey(_room.ID) ?? false
                                     select new ShadeZoneEffects(_room.ID, _room, OptionalAnchorFace.None,
                                     _room.YieldEffects(_loc.Locator.GeometricRegion, false, _senses),
                                     _room.YieldPanels(_sensors));
                    var _mapShade = _Map.ShadingZones.YieldShadeZoneEffects(_loc.Locator.GeometricRegion, false, _senses);
                    // TODO: alpha channel-stuff...

                    // project into service format
                    return _roomShade
                        .Union(_mapShade)
                        .Select(_sze => _sze.ToShadingInfo())
                        .ToList();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region private ISensorHost GetSensorHost(string sensorHostID, Guid critterID)
        private ISensorHost GetSensorHost(string sensorHostID, Guid critterID)
        {
            // find creature
            var _critter = Map.GetCreature(critterID);
            ISensorHost _sensors = _critter;

            Guid _sensorID = GetGuid(sensorHostID);
            if (_sensorID != critterID)
            {
                _sensors = (from _sh in _critter.Adjuncts.OfType<RemoteSenseMaster>()
                            where _sh.SensorHost.ID == _sensorID
                            select _sh.SensorHost).FirstOrDefault();
            }
            if (_sensors == null)
            {
                throw new FaultException<InvalidArgumentFault>(new InvalidArgumentFault(@"sensorHostID"), @"Unable to find matching sensor host");
            }

            return _sensors;
        }
        #endregion

        #region public SensorHostInfo SetSensorHostHeading(string creatureID, string sensorHostID, int heading, int incline)
        public SensorHostInfo SetSensorHostHeading(string creatureID, string sensorHostID, int heading, int incline)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                var _serial = Map?.MapContext?.SerialState ?? 0;
                if (Map != null)
                {
                    Map.MapContext.SerialState++;
                }
                try
                {
                    Map.Synchronizer.EnterWriteLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    _sensors.Incline = incline;
                    _sensors.Heading = heading;
                    return _sensors.ToSensorHostInfo();
                }
                finally
                {
                    if (Map.Synchronizer.IsWriteLockHeld)
                    {
                        IkosaServices.DoNotifySerialState(_serial);
                        Map.Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public IEnumerable<PresentationInfo> GetObjectPresentations(string creatureID, string sensorHostID)
        public IEnumerable<PresentationInfo> GetObjectPresentations(string creatureID, string sensorHostID)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    if (_sensors?.IsSensorHostActive ?? false)
                    {
                        //_sensors.Awarenesses.RecalculateAwareness(_sensors);
                        var _senseLoc = _sensors.GetLocated().Locator;
                        var _planar = _senseLoc.PlanarPresence;
                        var _senses = _sensors.Senses.BestTerrainSenses
                            .Where(_s => (_s.PlanarPresence & _planar) != PlanarPresence.None)
                            .ToList();
                        var _critter = IkosaServices.CreatureProvider.GetCreature(_id);

                        #region converter: _convert
                        PresentationInfo _convert(Presentation presentation, IEnumerable<Guid> guids)
                        {
                            if (presentation is IconPresentation)
                            {
                                return new IconPresentationInfo(presentation as IconPresentation, guids);
                            }
                            else if (presentation is ModelPresentation)
                            {
                                return new ModelPresentationInfo(presentation as ModelPresentation, guids);
                            }
                            else
                            {
                                return new PresentationInfo(presentation, guids);
                            }
                        }
                        ;
                        #endregion

                        // TODO: review use of _critter/actor in these calls
                        return (from _loc in Map.MapContext.AllTokensOf<Locator>()
                                from _pr in _loc.GetPresentations(_senseLoc.GeometricRegion, _critter, _sensors, _senses)
                                let _guids = _loc.AllAccessible(_critter).Select(_ic => _ic.ID)
                                select _convert(_pr, _guids))
                               .ToList();
                    }
                    return new List<PresentationInfo>();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public SensorHostInfo AdjustSensorHostAiming(string creatureID, string sensorHostID, short zOff, short yOff, short xOff)
        public SensorHostInfo AdjustSensorHostAiming(string creatureID, string sensorHostID, short zOff, short yOff, short xOff)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    // NOTE: taking a read lock, as the updated data is non-critical, and conflicting updates unlikely
                    Map.Synchronizer.EnterReadLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);

                    // get locator for sensor host
                    var _loc = _sensors.GetLocated();
                    if (_loc != null)
                    {
                        _loc.Locator.AdjustAimPoint(_sensors, zOff, yOff, xOff);
                    }

                    return _sensors.ToSensorHostInfo();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public SensorHostInfo SetSensorHostAimExtent(string creatureID, string sensorHostID, double relativeLongitude, double latitude)
        public SensorHostInfo SetSensorHostAimExtent(string creatureID, string sensorHostID, double relativeLongitude, double latitude)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    // NOTE: taking a read lock, as the updated data is non-critical, and conflicting updates unlikely
                    Map.Synchronizer.EnterReadLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    _sensors.AimPointLatitude = latitude;
                    _sensors.AimPointRelativeLongitude = relativeLongitude;

                    // get locator for sensor host
                    var _loc = _sensors.GetLocated();
                    if (_loc != null)
                    {
                        var _locator = _loc.Locator;
                        var _rgn = _locator.GeometricRegion;
                        var _zExt = (double)(_rgn.UpperZ - _rgn.LowerZ + 1) * 5d;
                        var _yExt = (double)(_rgn.UpperY - _rgn.LowerY + 1) * 5d;
                        var _xExt = (double)(_rgn.UpperX - _rgn.LowerX + 1) * 5d;
                        _sensors.AimPointDistance = 0.5d * Math.Sqrt((_zExt * _zExt) + (_yExt * _yExt) + (_xExt * _xExt));
                        _sensors.AimPoint = _locator.ResyncTacticalPoint(_sensors, _sensors.AimPointRelativeLongitude, _sensors.AimPointLatitude,
                            _sensors.AimPointDistance);
                    }

                    return _sensors.ToSensorHostInfo();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public SensorHostInfo SetSensorHostThirdCamera(string creatureID, string sensorHostID, int relativeHeading, int incline)
        public SensorHostInfo SetSensorHostThirdCamera(string creatureID, string sensorHostID, int relativeHeading, int incline)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    // NOTE: taking a read lock, as the updated data is non-critical, and conflicting updates unlikely
                    Map.Synchronizer.EnterReadLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    _sensors.ThirdCameraRelativeHeading = relativeHeading;
                    _sensors.ThirdCameraIncline = incline;

                    // get locator for sensor host
                    var _loc = _sensors.GetLocated();
                    if (_loc != null)
                    {
                        var _locator = _loc.Locator;
                        _sensors.ThirdCameraPoint = _locator.ResyncTacticalPoint(_sensors,
                            _sensors.ThirdCameraRelativeHeading * 45d, _sensors.ThirdCameraIncline * 45d, _sensors.ThirdCameraDistance);
                    }

                    return _sensors.ToSensorHostInfo();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region public IEnumerable<TransientVisualizerInfo> GetTransientVisualizers(string creatureID, string sensorHostID)

        public IEnumerable<TransientVisualizerInfo> GetTransientVisualizers(string creatureID, string sensorHostID)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();

                    // get sense information
                    // TODO: make visualizers dependent on observation
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    if (_sensors?.IsSensorHostActive ?? false)
                    {
                        var _view = _sensors.Senses.CanViewTransientVisualizations;

                        // return converted transient visualizers
                        return Map.MapContext
                            .TransientVisualizers
                            .Where(_tv => _view) // but only if the visualization can be viewed
                            .Select(_tv => _tv.ToInfo());
                    }
                    return new List<TransientVisualizerInfo>();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }

        #endregion

        #region public IEnumerable<SoundAwarenessInfo> GetSoundAwarenesses(string creatureID, string sensorHostID)
        public IEnumerable<SoundAwarenessInfo> GetSoundAwarenesses(string creatureID, string sensorHostID)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Map.Synchronizer.EnterReadLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    if (_sensors?.IsSensorHostActive ?? false)
                    {
                        var _critter = _sensors.Senses.Creature;

                        // return converted sound awarenesses
                        return (from _s in _sensors.SoundAwarenesses.GetAll()
                                where _s.HasNoticed
                                select _s.ToSoundAwarenessInfo(Map, _critter))
                                .ToList();
                    }
                    return new List<SoundAwarenessInfo>();
                }
                finally
                {
                    if (Map.Synchronizer.IsReadLockHeld)
                    {
                        Map.Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion
    }
}