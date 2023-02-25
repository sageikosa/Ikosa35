using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Visualize;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Proxy.ViewModel;
using System.Diagnostics;
using Uzi.Visualize.Contracts;
using Uzi.Visualize.Packaging;
using Uzi.Visualize.Contracts.Tactical;
using System.Collections.Concurrent;
using System.Windows.Media;

namespace Uzi.Ikosa.Proxy
{
    // TODO: handle proxy crash
    public class LocalMapInfo : IResolveBitmapImage, IResolveModel3D, IResolveFragment,
        IResolveMaterial, IResolveBrushCollection, IResolveIcon
    {
        #region construction
        public LocalMapInfo(ProxyModel proxies, Guid critterID, string sensorHostID)
        {
            _Proxies = proxies;
            _FragmentNames = MyProxy.Service.MetaModelFragmentNames();
            _BitmapImages = MyProxy.Service.BitmapImageNames();
            _CritterID = critterID;
            _SensorHostID = sensorHostID;
            GetRootBrushCollectionViewModel();
            RefreshCellSpaceTypes();
            RefreshTerrain();
        }
        #endregion

        public VisualizationServiceClient MyProxy
            => _Proxies.ViewProxy;

        public CellStructureInfo GetCellSpaceInfo(ulong id)
        {
            var _id = new IndexStruct { ID = id };
            _Spaces.TryGetValue(_id.Index, out CellSpaceViewModel _cSpace);
            return new CellStructureInfo
            {
                CellSpace = _cSpace,
                ParamData = _id.StateInfo
            };
        }

        #region state
        private Guid _CritterID;
        private string _SensorHostID;
        private string[] _FragmentNames { get; set; }
        private string[] _BitmapImages { get; set; }
        private Dictionary<uint, CellSpaceViewModel> _Spaces = new Dictionary<uint, CellSpaceViewModel>();
        private Dictionary<Guid, RoomViewModel> _Rooms = new Dictionary<Guid, RoomViewModel>();
        private RoomTracker _RoomIndex;
        private Dictionary<Guid, BackgroundCellGroupViewModel> _Backgrounds = new Dictionary<Guid, BackgroundCellGroupViewModel>();
        private readonly ProxyModel _Proxies;
        private BrushCollectionViewModel _RootBrushes;
        private readonly ConcurrentDictionary<string, MetaModelFragmentInfo> _Fragments
            = new ConcurrentDictionary<string, MetaModelFragmentInfo>();
        private readonly ConcurrentDictionary<string, BitmapImageViewModel> _Images
            = new ConcurrentDictionary<string, BitmapImageViewModel>();
        private readonly ConcurrentDictionary<string, Model3DViewModel> _Models
            = new ConcurrentDictionary<string, Model3DViewModel>();
        private readonly ConcurrentDictionary<string, BrushCollectionViewModel> _Brushes
            = new ConcurrentDictionary<string, BrushCollectionViewModel>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TileSetViewModel>> _TileSets
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, TileSetViewModel>>();
        #endregion

        #region IResolveBitmapImage Members

        public BitmapSource GetImage(object key, VisualEffect effect)
        {
            var _key = key.ToString();
            if (_BitmapImages.Contains(_key))
            {
                var _img = _Images.GetOrAdd(_key,
                    (iKey) => new BitmapImageViewModel(MyProxy.Service.GetBitmapImage(iKey)));
                return _img?.GetImage(effect);
            }
            return null;
        }

        public IGetImageByEffect GetIGetImageByEffect(object key)
        {
            var _key = key.ToString();
            if (_BitmapImages.Contains(_key))
            {
                return _Images.GetOrAdd(_key,
                    (iKey) => new BitmapImageViewModel(MyProxy.Service.GetBitmapImage(iKey)));
            }
            return null;
        }

        public IResolveBitmapImage IResolveBitmapImageParent 
            => null;

        /// <summary>Only used by package editors to inspect</summary>
        public IEnumerable<BitmapImagePartListItem> ResolvableImages
            => Enumerable.Empty<BitmapImagePartListItem>();

        #endregion

        public Model3DViewModel GetModel3DViewModel(Model3DInfo info)
        {
            if (info is MetaModel3DInfo)
                return new MetaModel3DViewModel(info as MetaModel3DInfo, this);
            return new Model3DViewModel(info, this);
        }

        public RoomViewModel GetRoom(Guid id)
        {
            var _rooms = _Rooms;
            return _rooms.ContainsKey(id) ? _rooms[id] : null;
        }

        public IList<RoomViewModel> AllRooms()
        {
            var _rooms = _Rooms;
            return _rooms.Select(_r => _r.Value).ToList();
        }

        public Model3DViewModel GetModel(string name)
            => _Models.GetOrAdd(name,
                (key) =>
                {
                    var _model = MyProxy.Service.GetModel3D(name);
                    return _model != null
                        ? GetModel3DViewModel(_model)
                        : null;
                });

        #region public BrushCollectionViewModel GetRootBrushCollectionViewModel()
        public BrushCollectionViewModel GetRootBrushCollectionViewModel()
        {
            var _brushSet = MyProxy.Service.GetRootBrushCollection();
            if (_brushSet != null)
            {
                _RootBrushes = new BrushCollectionViewModel(_brushSet, this);
            }
            return _RootBrushes;
        }
        #endregion

        #region public BrushCollectionInfo GetBrushCollectionViewModel(string name)
        public BrushCollectionViewModel GetBrushCollectionViewModel(string name)
            => _Brushes.GetOrAdd(name,
                (key) =>
                {
                    var _brushSet = MyProxy.Service.GetBrushCollection(name);
                    return _brushSet != null
                        ? new BrushCollectionViewModel(_brushSet, this)
                        : null;
                });
        #endregion

        public void RefreshCellSpaceTypes()
        {
            var _spaces = MyProxy.GetCellSpaceViewModels(this).ToDictionary(_csvm => _csvm.Index);
            _Spaces = _spaces;
        }

        #region public void RefreshTerrain()
        public LocalMapInfo RefreshTerrain()
        {
            // capture stable references
            var _stableRooms = _Rooms;
            var _stableIndex = _RoomIndex;

            // copy rooms dictionary for update
            var _rooms = new Dictionary<Guid, RoomViewModel>(_stableRooms);
            var _roomIndex = new Lazy<RoomTracker>(() => new RoomTracker(_stableIndex));

            var _roomCheck = MyProxy.Service.GetRooms(_CritterID.ToString(), _SensorHostID).ToList();

            // rooms to be removed
            foreach (var _rmv in _rooms.Where(_r => !_roomCheck.Any(_rc => _rc.ID == _r.Key)).ToList())
            {
                _rooms.Remove(_rmv.Key);
                _roomIndex.Value.Remove(_rmv.Value);
            }

            // rooms to be updated...
            foreach (var _rmv in (from _fresh in _roomCheck
                                  let _room = _rooms.ContainsKey(_fresh.ID) ? _rooms[_fresh.ID] : null
                                  where (_room != null) && (_fresh.FreshTime > _room.RoomInfo.Freshness)
                                  select _room).ToList())
            {
                _rooms.Remove(_rmv.RoomInfo.ID);
                _roomIndex.Value.Remove(_rmv);
            }

            // rooms to be (re-)added
            foreach (var _fresh in _roomCheck.Where(_rc => !_rooms.ContainsKey(_rc.ID)))
            {
                var _newRoom = new RoomViewModel(MyProxy.Service.GetRoom(_CritterID.ToString(), _SensorHostID, _fresh.ID.ToString()));
                if (_newRoom.RoomInfo != null)
                {
                    _rooms.Add(_newRoom.RoomInfo.ID, _newRoom);
                    _roomIndex.Value.Add(_newRoom);
                    _newRoom.RedrawMatrix(this);
                }
            }

            // update all backgrounds...
            // TODO: consider freshness?
            _Backgrounds = MyProxy.Service.GetBackgrounds().ToDictionary(_b => _b.ID, _b => new BackgroundCellGroupViewModel(_b, this));

            if (_roomIndex.IsValueCreated)
            {
                _Rooms = _rooms;
                _RoomIndex = _roomIndex.Value;
            }
            return this;
        }
        #endregion

        public CellStructureInfo this[int z, int y, int x, AnchorFace adjacentTo, IGeometricRegion currentRoom]
            => adjacentTo switch
            {
                AnchorFace.XHigh => this[z, y, x + 1],
                AnchorFace.XLow => this[z, y, x - 1],
                AnchorFace.YHigh => this[z, y + 1, x],
                AnchorFace.YLow => this[z, y - 1, x],
                AnchorFace.ZHigh => this[z + 1, y, x],
                _ => this[z - 1, y, x],
            };

        #region public CellStructureInfo this[int z, int y, int x] { get; }
        public CellStructureInfo this[int z, int y, int x]
        {
            get
            {
                var _rooms = _RoomIndex;
                var _bgCells = _Backgrounds;

                var _room = _rooms.GetRoom(z, y, x);
                if (_room != null)
                {
                    var _cell = _room.GetContainedCellSpace(z, y, x);
                    if (_cell?.CellSpace != null)
                    {
                        return _cell.Value;
                    }
                }

                // background cell-spaces
                foreach (var _bgSet in _bgCells.Select(_b => _b.Value))
                {
                    if (_bgSet.BackgroundCellGroupInfo.ContainsCell(z, y, x))
                        return new CellStructureInfo
                        {
                            CellSpace = _bgSet.TemplateSpace,
                            ParamData = _bgSet.BackgroundCellGroupInfo.ParamData
                        };
                }

                // no base cell-space anymore (it is in the backgrounds)
                return new CellStructureInfo { CellSpace = null };
            }
        }
        #endregion

        #region public TileSetInfo GetTileSet(string cellMaterial, string tileSet)
        public TileSetViewModel GetTileSet(string cellMaterial, string tileSet)
        {
            // collection for material
            var _setList = _TileSets.GetOrAdd(cellMaterial,
                (key) => new ConcurrentDictionary<string, TileSetViewModel>());

            // tiling in collection
            if (!string.IsNullOrWhiteSpace(tileSet))
                return _setList.GetOrAdd(tileSet,
                    (key) =>
                    {
                        var _tilings = new TileSetViewModel(MyProxy.Service.GetTileSet(cellMaterial, key), this)
                        {
                            LocalMap = this
                        };
                        if (_tilings.TileSetInfo != null)
                            GetBrushCollectionViewModel(_tilings.TileSetInfo.BrushCollectionKey);
                        return _tilings;
                    });
            return null;
        }
        #endregion

        // TODO: room info?
        // TODO: background info?

        // IResolveModel3D
        public Model3D GetPrivateModel3D(object key)
            => GetModel(key.ToString())?.Model;

        public IResolveModel3D IResolveModel3DParent
            => RootModel3DResolver.Root;

        public bool CanResolveModel3D(object key)
            => _Models.ContainsKey(key.ToString());

        /// <summary>Only used by package editors to inspect</summary>
        public IEnumerable<Model3DPartListItem> ResolvableModels
            => Enumerable.Empty<Model3DPartListItem>();

        // IResolveFragment
        private MetaModelFragmentInfo GetFragInfo(string name)
            => _Fragments.GetOrAdd(name,
                (key) => MyProxy.Service.GetModel3DFragment(key));

        public Model3D GetFragment(FragmentReference fragRef, MetaModelFragmentNode node)
        {
            var _group = new Model3DGroup();

            if (_FragmentNames.Any(_f => _f.Equals(node.FragmentKey)))
            {
                var _fragPart = GetFragInfo(node.FragmentKey);
                var _mdl = _fragPart.ResolveModel(node);

                // collect and end resolve of fragment
                if (_mdl != null)
                    _group.Children.Add(fragRef.ApplyTransforms(node, _mdl));
            }

            // return smallest possible grouping
            if (_group.Children.Count == 1)
                return _group.Children.First();
            else if (_group.Children.Any())
                return _group;
            return null;
        }

        public IResolveFragment IResolveFragmentParent
            => null;

        public IEnumerable<MetaModelFragmentListItem> ResolvableFragments
            => Enumerable.Empty<MetaModelFragmentListItem>();

        // IResolveMaterial
        public Material GetMaterial(object key, VisualEffect effect)
            => _RootBrushes.GetMaterial(key, effect);

        public IResolveMaterial IResolveMaterialParent
            => null;

        public IEnumerable<BrushDefinitionListItem> ResolvableBrushes
            => Enumerable.Empty<BrushDefinitionListItem>();

        // IResolveBrushCollection
        public BrushCollection GetBrushCollection(object key)
            => GetBrushCollectionViewModel(key.ToString())?.BrushDefinitions;

        public IResolveBrushCollection IResolveBrushCollectionParent
            => null;

        public IEnumerable<BrushCollectionListItem> ResolvableBrushCollections
            => Enumerable.Empty<BrushCollectionListItem>();

        // IResolveIcon
        public Visual GetIconVisual(string key, IIconReference iconRef)
            => null;

        public Material GetIconMaterial(string key, IIconReference iconRef, IconDetailLevel detailLevel)
            => null;

        public IResolveIcon IResolveIconParent
            => _Proxies?.IconResolver;

        public IEnumerable<IconPartListItem> ResolvableIcons
            => Enumerable.Empty<IconPartListItem>();
    }
}