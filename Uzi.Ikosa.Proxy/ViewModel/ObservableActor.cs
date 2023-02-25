using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ObservableActor : ViewModelBase, IActivityBuilderTacticalActor
    {
        #region ctor()
        public ObservableActor(ActorModel actor, Dispatcher dispatcher, ResourceDictionary resources,
            Action shutdown, Action doFocus)
        {
            _Actor = actor;
            _Dispatcher = dispatcher;
            _ActivityBuilder = null;
            _Resources = resources;
            _Shutdown = shutdown;
            _DoFocus = doFocus;
            _Icons = new MapIconResolver(_Actor.Proxies);
            _SensorHosts = new List<SensorHostInfo>();
            _SensorHostID = Actor.FulfillerID.ToString();

            _Awarenesses = new ObservableCollection<AwarenessInfo>();

            // action menus
            _TopActions = new ObservableCollection<MenuBaseViewModel>();
            _CtxMenu = new ObservableCollection<MenuBaseViewModel>();
            _ChoiceActions = new ObservableCollection<ActionInfo>();

            // selection commands
            _PrevAwareness = new RelayCommand(PrevSelection_Executed, PrevSelection_CanExecute);
            _NextAwareness = new RelayCommand(NextSelection_Executed, NextSelection_CanExecute);
            _FlipAwareness = new RelayCommand<Guid?>(FlipAwareness_Executed, FlipAwareness_CanExecute);
            _AddSelectAwareness = new RelayCommand<Guid?>(AddSelectAwareness_Executed, AddSelectAwareness_CanExecute);

            // aim-point commands
            _AimExtent = new RelayCommand(AimExtent_Executed, AimExtent_CanExecute);
            _AdjustAimPoint = new RelayCommand(AdjustAimPoint_Executed, AdjustAimPoint_CanExecute);
            _SetSensors = new RelayCommand<SensorHostInfo>(SetSensors_Executed, SetSensors_CanExecute);

            // target pointer/queue commands
            _AdjustTargetPointer = new RelayCommand(AdjustTargetPointer_Executed, TargetPointer_CanExecute);
            _HideTargetPointer = new RelayCommand(HideTargetPointer_Executed, TargetPointer_CanExecute);
            _ResetTargetPointer = new RelayCommand(ResetTargetPointer_Executed, TargetPointer_CanExecute);
            _QueueTargetPointer = new RelayCommand(QueueTargetPointer_Executed, QueueTargetPointer_CanExecute);
            _ClearTargets = new RelayCommand(ClearTargets_Executed, ClearTargets_CanExecute);

            _IsMoveEnabled = true;
            _AimPointActivation = ViewModel.AimPointActivation.Off;
            _MoveCommands = new MovementCommands(this);

            _SensorCancel = null;
            UpdateSensors(true);
            UpdateActions();
        }
        #endregion

        #region state
        private readonly ActorModel _Actor;
        private readonly Dispatcher _Dispatcher;
        private readonly ResourceDictionary _Resources;
        private readonly Action _Shutdown;
        private readonly Action _DoFocus;
        private readonly MapIconResolver _Icons;
        private List<SensorHostInfo> _SensorHosts;
        private string _SensorHostID;

        private LocalMapInfo _Map;
        private LocaleViewModel _LocaleViewModel;
        private CancellationTokenSource _SensorCancel;
        private readonly List<QueuedTargetItem> _QueuedTargets = new List<QueuedTargetItem>();
        private ActivityInfoBuilder _ActivityBuilder;
        private bool _IsMoveEnabled;
        private AimPointActivation _AimPointActivation;
        private readonly MovementCommands _MoveCommands;

        private readonly ObservableCollection<AwarenessInfo> _Awarenesses;
        private readonly ObservableCollection<ActionInfo> _ChoiceActions;
        private readonly ObservableCollection<SysNotifyVM> _Notifies = new ObservableCollection<SysNotifyVM>();
        private long _NotifyID = 0;

        // action menus
        private readonly ObservableCollection<MenuBaseViewModel> _TopActions;
        private readonly ObservableCollection<MenuBaseViewModel> _CtxMenu;
        private bool _CtxMenuVisible;

        // selection commands
        private readonly RelayCommand _PrevAwareness;
        private readonly RelayCommand _NextAwareness;
        private readonly RelayCommand<Guid?> _FlipAwareness;
        private readonly RelayCommand<Guid?> _AddSelectAwareness;

        // aim-point commands
        private readonly RelayCommand _AimExtent;
        private readonly RelayCommand _AdjustAimPoint;
        private readonly RelayCommand<SensorHostInfo> _SetSensors;

        // target pointer/queue commands
        private readonly RelayCommand _AdjustTargetPointer;
        private readonly RelayCommand _HideTargetPointer;
        private readonly RelayCommand _ResetTargetPointer;
        private readonly RelayCommand _QueueTargetPointer;
        private readonly RelayCommand _ClearTargets;
        #endregion

        public ActorModel Actor => _Actor;
        public IResolveIcon IconResolver => _Icons;
        public Guid ActorID => Actor.FulfillerID;
        public Dispatcher Dispatcher => _Dispatcher;
        public void DoShutdown() => _Dispatcher.BeginInvoke(DispatcherPriority.Normal, _Shutdown);
        public void DoFocus() => _Dispatcher.BeginInvoke(DispatcherPriority.Normal, _DoFocus);
        public ProxyModel Proxies => _Actor.Proxies;
        public LocaleViewModel LocaleViewModel => _LocaleViewModel;
        public ObservableCollection<AwarenessInfo> Awarenesses => _Awarenesses;

        // WPF resources! used to improve SubSelect menu
        public ResourceDictionary Resources => _Resources;

        // bindable commands, all movement commands are grouped into a class
        public MovementCommands MovementCommands => _MoveCommands;

        // action menus
        public ObservableCollection<MenuBaseViewModel> TopActions => _TopActions;
        public ObservableCollection<MenuBaseViewModel> ContextMenu => _CtxMenu;
        public ObservableCollection<ActionInfo> ChoiceActions => _ChoiceActions;
        public ObservableCollection<SysNotifyVM> Notifies => _Notifies;

        public long GetNextNotifyID()
        {
            _NotifyID++;
            return _NotifyID;
        }

        #region public bool IsContextMenuVisible { get; set; }
        public bool IsContextMenuVisible
        {
            get { return _CtxMenuVisible; }
            set
            {
                _CtxMenuVisible = value;
                DoPropertyChanged(nameof(IsContextMenuVisible));
            }
        }
        #endregion

        /// <summary>True if there is a non-null reference for ActivityInfoBuilder</summary>
        public bool IsActivityBuilding => _ActivityBuilder != null;

        public ActivityInfoBuilder ActivityInfoBuilder => _ActivityBuilder;

        public Visibility PerformVisibility => Visibility.Visible;

        // target awareness
        public RelayCommand DoPrevAwareness => _PrevAwareness;
        public RelayCommand DoNextAwareness => _NextAwareness;
        public RelayCommand<Guid?> DoFlipAwareness => _FlipAwareness;
        public RelayCommand<Guid?> DoAddSelectAwareness => _AddSelectAwareness;

        // Aim-Point aommands
        public RelayCommand DoAimExtent => _AimExtent;
        public RelayCommand DoAdjustAimPoint => _AdjustAimPoint;
        public RelayCommand<SensorHostInfo> DoSetSensors => _SetSensors;

        // target pointer/queue commands
        public RelayCommand DoAdjustTargetPointer => _AdjustTargetPointer;
        public RelayCommand DoHideTargetPointer => _HideTargetPointer;
        public RelayCommand DoQueueTargetPointer => _QueueTargetPointer;
        public RelayCommand DoResetTargetPointer => _ResetTargetPointer;
        public RelayCommand DoClearTargets => _ClearTargets;

        // sensor host list and selection
        public IEnumerable<SensorHostInfo> SensorHosts
            => _SensorHosts.Select(_sh => _sh);

        public SensorHostInfo SelectedSensorHost
        {
            get => _SensorHosts.FirstOrDefault(_s => _s.ID.Equals(_SensorHostID)) ?? _SensorHosts.FirstOrDefault();
            set
            {
                // only if sensors actually change
                if (!string.Equals(value?.ID, _SensorHostID, StringComparison.OrdinalIgnoreCase))
                {
                    LocaleViewModel.StoreViewPointState();
                    _SensorHostID = value?.ID ?? string.Empty;
                    UpdateSensors(false);
                }
            }
        }

        public Visibility SensorHostListVisibility
            => _SensorHosts.Any() ? Visibility.Visible : Visibility.Collapsed;

        // ----- Target Queuing -----

        public Visibility QueuedTargetVisibility
            => _QueuedTargets.Any() ? Visibility.Visible : Visibility.Collapsed;

        public IEnumerable<QueuedTargetItem> QueuedTargets
            => _QueuedTargets.Select(_qt => _qt);

        public IEnumerable<QueuedAwareness> QueuedAwarenesses
            => _QueuedTargets.OfType<QueuedAwareness>();

        public IEnumerable<QueuedCell> QueuedCells
            => _QueuedTargets.OfType<QueuedCell>().Where(_qc => !(_qc is QueuedIntersection));

        public IEnumerable<ICellLocation> QueuedLocations
            => QueuedCells.Select(_qc => _qc.Location);

        public IEnumerable<Point3D> QueuedPoints
            => QueuedIntersections.Select(_qi => _qi.Location.Point3D());

        public IEnumerable<QueuedIntersection> QueuedIntersections
            => _QueuedTargets.OfType<QueuedIntersection>();

        #region private void ConformulateAwareness(List<AwarenessInfo> sourceAwarenesses)
        private void ConformulateAwareness(List<AwarenessInfo> sourceAwarenesses)
        {
            // remove items not in source
            foreach (var _rmv in (from _ai in _Awarenesses
                                  where !sourceAwarenesses.Any(_s => _s.ID == _ai.ID)
                                  select _ai).ToList())
            {
                _Awarenesses.Remove(_rmv);
            }

            // update items
            var _resolver = _Icons;
            foreach (var _updt in (from _ai in _Awarenesses
                                   join _s in sourceAwarenesses
                                   on _ai.ID equals _s.ID
                                   select new { Awareness = _ai, Source = _s }).ToList())
            {
                _updt.Awareness.Conformulate(_updt.Source);
                _updt.Awareness.SetIconResolver(_resolver);
            }

            // add source not in items
            foreach (var _add in (from _s in sourceAwarenesses
                                  where !_Awarenesses.Any(_ai => _ai.ID == _s.ID)
                                  select _s).ToList())
            {
                _Awarenesses.Add(_add);
                _add.SetIconResolver(_resolver);
            }

            // order by distance
            var _sorted = _Awarenesses.OrderBy(x => x.Distance).ToList();
            for (var i = 0; i < _sorted.Count(); i++)
                _Awarenesses.Move(_Awarenesses.IndexOf(_sorted[i]), i);

            UpdateActions();
        }
        #endregion

        // ----- Awareness -----

        /// <summary>Gets AwarenessInfos that are selected by traversing the AwarenessInfo tree.</summary>
        public List<AwarenessInfo> SelectedAwarenesses
            => (from _a in Awarenesses
                from _sa in _a.SelectedItems
                select _sa).ToList();

        #region private int CountValidAwarenesses(TargetTypeInfo[] validTypes)
        private int countValidAwarenesses(TargetTypeInfo[] validTypes)
        {
            bool _isValid(Info info)
            {
                if (info is CreatureObjectInfo)
                    return validTypes.Any(_vt => _vt is CreatureTargetTypeInfo || _vt is LivingCreatureTargetTypeInfo);
                if (info is ObjectInfo)
                    return validTypes.Any(_vt => _vt is ObjectTargetTypeInfo);
                return false;
            }
            return SelectedAwarenesses.Where(_a => _isValid(_a.Info)).Count();
        }
        #endregion

        #region public void ResyncAwarenessQueue()
        /// <summary>
        /// Updates Awareness Queue to match IsInSelection of Awareness tree.  
        /// Refreshes Presentations.
        /// </summary>
        public void ResyncAwarenessQueue()
        {
            var _current = Awarenesses.SelectMany(_a => _a.SelectedIDs).ToList();
            var _exclude = QueuedAwarenesses.Select(_qa => _qa.ID).Except(_current).ToList();
            var _include = _current.Except(_QueuedTargets.OfType<QueuedAwareness>().Select(_qa => _qa.ID)).ToList();

            // remove
            _QueuedTargets.RemoveAll(_qt => (_qt is QueuedAwareness) && _exclude.Contains((_qt as QueuedAwareness).ID));

            // add
            foreach (var _add in _include)
                _QueuedTargets.Add(new QueuedAwareness(this, _add));

            // refresh LocaleViewmodel
            DoPropertyChanged(nameof(QueuedAwarenesses));
            DoPropertyChanged(nameof(QueuedTargets));
            DoPropertyChanged(nameof(QueuedTargetVisibility));
            LocaleViewModel?.RefreshPresentations();
        }
        #endregion

        #region private void ApplyAwarenessSelection()
        /// <summary>
        /// Use all Guids in _QueuedAwarenessGuids to select/de-select Awarenesses in tree
        /// </summary>
        private void ApplyAwarenessQueue()
        {
            var _redraw = false;

            // grab list reference, in case it is replaced
            foreach (var _a in _Awarenesses)
                _redraw |= _a.ApplySelection(QueuedAwarenesses.Select(_qa => _qa.ID));
            DoPropertyChanged(nameof(Awarenesses));

            if (_redraw)
            {
                // only if awareness selection actually changed should these change
                DoPropertyChanged(nameof(SelectedAwarenesses));
                LocaleViewModel.RefreshPresentations();
            }

            // always refresh these
            DoPropertyChanged(nameof(QueuedAwarenesses));
            DoPropertyChanged(nameof(QueuedTargets));
            DoPropertyChanged(nameof(QueuedTargetVisibility));
            SyncItemMenus();
        }
        #endregion

        #region public void SelectAwareness(Guid awarenessID)
        /// <summary>
        /// Selects a single awareness. If passed Guid.Empty, clears selection.
        /// </summary>
        public void SelectAwareness(Guid awarenessID)
        {
            // set none or one
            _QueuedTargets.Clear();
            DoPropertyChanged(nameof(QueuedIntersections));
            DoPropertyChanged(nameof(QueuedCells));
            DoPropertyChanged(nameof(QueuedLocations));
            if ((awarenessID != Guid.Empty) && _Awarenesses.Any(_a => _a.IsAnyAware(awarenessID)))
            {
                _QueuedTargets.Add(new QueuedAwareness(this, awarenessID));
            }
            ApplyAwarenessQueue();
        }
        #endregion

        #region public void AddAwarenesses(IEnumerable<Guid> adders)
        /// <summary>Add all IDs into the Awareness queue and indicate they are selected</summary>
        /// <param name="adders"></param>
        public void AddAwarenesses(IEnumerable<Guid> adders)
        {
            var _include = adders.Except(QueuedAwarenesses.Select(_qa => _qa.ID))
                .Where(_aid => _Awarenesses.Any(_a => _a.IsAnyAware(_aid)))
                .Select(_a => new QueuedAwareness(this, _a))
                .ToList();
            _QueuedTargets.AddRange(_include);
            ApplyAwarenessQueue();
        }
        #endregion

        #region public void RemoveAwarenesses(IEnumerable<Guid> removers)
        /// <summary>Remove all IDs from the Awareness queue and clear their selection state</summary>
        /// <param name="removers"></param>
        public void RemoveAwarenesses(IEnumerable<Guid> removers)
        {
            _QueuedTargets.RemoveAll((_qt) => (_qt is QueuedAwareness) && removers.Contains((_qt as QueuedAwareness).ID));
            ApplyAwarenessQueue();
        }
        #endregion

        #region Next Selection
        private bool NextSelection_CanExecute()
            => Awarenesses.Any();

        private void NextSelection_Executed()
        {
            if (QueuedAwarenesses.Any())
            {
                var _select = SelectedAwarenesses.Last();
                if (_select != Awarenesses.Last())
                {
                    SelectAwareness(Awarenesses.SkipWhile(_a => _a != _select).Skip(1).Select(_a => _a.ID).First());
                    return;
                }
            }

            // first item
            SelectAwareness(Awarenesses.Select(_a => _a.ID).First());
        }
        #endregion

        #region Prev Selection
        private bool PrevSelection_CanExecute()
            => Awarenesses.Any();

        private void PrevSelection_Executed()
        {
            if (QueuedAwarenesses.Any())
            {
                var _select = SelectedAwarenesses.First();
                if (_select != Awarenesses.First())
                {
                    SelectAwareness(Awarenesses.TakeWhile(_a => _a != _select).Last().ID);
                    return;
                }
            }

            // last item
            SelectAwareness(Awarenesses.Last().ID);
        }
        #endregion

        #region Clear Targets
        private bool ClearTargets_CanExecute()
            => _QueuedTargets.Any();

        private void ClearTargets_Executed()
        {
            SelectAwareness(Guid.Empty);
            SetContextMenu();
            DoFocus();
        }
        #endregion

        #region DoFlipAwareness
        private bool FlipAwareness_CanExecute(Guid? id)
            => (Awarenesses?.Any() ?? false);

        private void FlipAwareness_Executed(Guid? id)
        {
            if (SelectedAwarenesses?.Any(_a => _a.ID == id) ?? true)
            {
                RemoveAwarenesses(new Guid[] { id ?? Guid.Empty });
            }
            else
            {
                SelectAwareness(id ?? Guid.Empty);
            }
            DoFocus();
        }
        #endregion

        #region DoAddSelectAwareness
        private bool AddSelectAwareness_CanExecute(Guid? id)
            => (Awarenesses?.Any() ?? false) && !(SelectedAwarenesses?.Any(_a => _a.ID == id) ?? true);

        private void AddSelectAwareness_Executed(Guid? id)
        {
            AddAwarenesses(new Guid[] { id ?? Guid.Empty });
            DoFocus();
        }
        #endregion

        private void SyncItemMenus()
        {
            var _choiceActions = Actor.Actions
                .Where(_a => _a.IsChoice && (_a.FirstOptionAimInfo != null))
                .ToList();
            var _topActions = ActionMenuBuilder.GetCreatureMenuItems(this, Actor.Actions, _choiceActions, Actor.DoAction).ToList();
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    ActionMenuBuilder.SyncMenus(_TopActions, _topActions);
                }));
        }

        #region public void UpdateSensors(bool newMap)
        public void UpdateSensors(bool newMap)
        {
            var _running = _SensorCancel;
            try
            {
                using (_running)
                {
                    _running?.Cancel();
                }
            }
            catch { }

            var _cancel = new CancellationTokenSource();
            _SensorCancel = _cancel;

            var _token = _cancel.Token;

            // ID
            var _creatureID = Actor.CreatureLoginInfo.ID.ToString();
            try
            {
                // get offset of current targetCell
                ICellLocation _offset = null;
                if ((LocaleViewModel != null) && (LocaleViewModel.Sensors != null))
                    _offset = LocaleViewModel.Sensors.AimCell.Subtract(LocaleViewModel.TargetCell);

                var _viewProxy = Proxies.ViewProxy;
                var _ikosaProxy = Proxies.IkosaProxy;

                _SensorHosts = _viewProxy.Service.GetSensorHosts(_creatureID).ToList();
                DoPropertyChanged(nameof(SensorHosts));
                DoPropertyChanged(nameof(SensorHostListVisibility));

                var _sensors = SelectedSensorHost;
                if (_sensors == null)
                {
                    // clear sensor host
                    _SensorHostID = string.Empty;
                    DoPropertyChanged(nameof(SelectedSensorHost));
                }
                else
                {
                    // set sensor host ID (just in case the one we had before got lost)
                    _SensorHostID = _sensors.ID;
                    DoPropertyChanged(nameof(SelectedSensorHost));
                    if (!_token.IsCancellationRequested)
                    {
                        _Map = newMap
                            ? new LocalMapInfo(Proxies, Actor.CreatureLoginInfo.ID, _sensors.ID)
                            : _Map?.RefreshTerrain();

                        if (!_token.IsCancellationRequested)
                        {
                            // TODO: mechanism to not update shadings, presentations, transients or sounds if only appropriate changes occur...
                            // TODO: service mechanism to update room awareness and inform client when links change...
                            // terrain
                            var _shadings = _viewProxy.Service.GetShadingInfos(_creatureID, _sensors.ID).ToList();
                            if (!_token.IsCancellationRequested)
                            {
                                // objects
                                var _presentations = _viewProxy.Service.GetObjectPresentations(_creatureID, _sensors.ID).ToList();
                                if (!_token.IsCancellationRequested)
                                {
                                    // transient visualizers
                                    var _transients = _viewProxy.Service.GetTransientVisualizers(_creatureID, _sensors.ID).ToList();
                                    if (!_token.IsCancellationRequested)
                                    {
                                        // sounds
                                        var _sounds = _viewProxy.Service.GetSoundAwarenesses(_creatureID, _sensors.ID).ToList();
                                        if (!_token.IsCancellationRequested)
                                        {
                                            // extra info...
                                            var _extraMarkers = _ikosaProxy.Service.GetExtraInfos(_creatureID, _sensors.ID).ToList();
                                            if (!_token.IsCancellationRequested)
                                            {
                                                // get new target cell (by adding any offset)
                                                var _target = _sensors.AimCell;
                                                if (_offset != null)
                                                    _target = _target.Add(_offset);

                                                // capture any previous client-side only settings
                                                var _overlay = _LocaleViewModel?.ShowOverlay ?? false;
                                                var _sound = _LocaleViewModel?.ShowSounds ?? false;
                                                var _extra = _LocaleViewModel?.ShowExtraMarkers ?? false;
                                                var _zoomedIcon = _LocaleViewModel?.ZoomedIcon ?? Guid.Empty;
                                                var _heading = _LocaleViewModel?.Sensors.Heading ?? _sensors.Heading;
                                                var _incline = _LocaleViewModel?.Sensors.Incline ?? _sensors.Incline;
                                                var _resync = (_LocaleViewModel?.Sensors.ID?.Equals(_sensors.ID) ?? false)
                                                    && ((_heading != _sensors.Heading) || (_incline != _sensors.Incline));

                                                // regenerate Local View Model
                                                // TODO: supply old presentations
                                                // TODO: extra info markers
                                                _LocaleViewModel = new LocaleViewModel(this, _Map, _sensors,
                                                    _shadings, _presentations, _transients, _sounds, _extraMarkers, _target, _zoomedIcon)
                                                {
                                                    ShowOverlay = _overlay,
                                                    ShowSounds = _sound,
                                                    ShowExtraMarkers = _extra
                                                };

                                                // notifications
                                                // TODO: switch localViewModel update to allow passage of cancellation token source
                                                DoPropertyChanged(nameof(LocaleViewModel));

                                                // awarnesses conformulation
                                                var _sourceAware = Proxies.IkosaProxy.Service.GetAwarenessInfo(_creatureID, _sensors.ID);
                                                Dispatcher?.BeginInvoke(DispatcherPriority.Normal,
                                                    new Action(() =>
                                                    {
                                                        // TODO: switch localViewModel update to allow passage of cancellation token source
                                                        if (_resync)
                                                            _LocaleViewModel.SetSensorHostHeading(_heading, _incline);
                                                        ConformulateAwareness(_sourceAware);
                                                        ResyncAwarenessQueue();
                                                    }));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        #endregion

        #region public void SetContextMenu()
        public void SetContextMenu(bool clear = false)
        {
            var _selected = SelectedAwarenesses;
            if (!clear && _selected.Any())
            {
                List<MenuBaseViewModel> _items = ActionMenuBuilder.GetContextMenu(Actor, _selected);
                if (_items?.Any() ?? false)
                {
                    ActionMenuBuilder.SyncMenus(_CtxMenu, _items);
                }
                else
                {
                    _CtxMenu.Clear();
                    IsContextMenuVisible = false;
                }
            }
            else
            {
                _CtxMenu.Clear();
                IsContextMenuVisible = false;
            }
        }
        #endregion

        #region public void UpdateActions()
        public void UpdateActions()
        {
            // sync PowerClassActions and ItemActions
            var _choiceActions = Actor.Actions
                .Where(_a => _a.IsChoice && (_a.FirstOptionAimInfo != null))
                .ToList();
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    var _topActions = ActionMenuBuilder.GetCreatureMenuItems(this, Actor.Actions, _choiceActions, Actor.DoAction).ToList();
                    ActionMenuBuilder.SyncMenus(_TopActions, _topActions);
                    SyncChoices(_choiceActions);
                    SetContextMenu();

                    // ¿ and activity building... ?
                    var _buildAction = ActivityInfoBuilder?.Action;
                    if (_buildAction != null)
                    {
                        // find the new action that matches the building one
                        var _same = Actor.Actions.FirstOrDefault(_a => _a.IsSameAction(_buildAction));
                        if (_same != null)
                        {
                            // conformulate it
                            ActivityInfoBuilder.Action = _same;
                        }
                        else
                        {
                            // didn't find one, so clear
                            ClearActivityBuilding();
                        }
                    }
                }));
        }
        #endregion

        #region public void ClearActions()
        /// <summary>Clear Actions on Logout</summary>
        public void ClearActions()
        {
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    _TopActions.Clear();

                    // and activity building
                    ClearActivityBuilding();
                }));
        }
        #endregion

        #region public void SetActivityBuilder(ActionInfo action)
        public void SetActivityBuilder(ActionInfo action, Action<ActivityInfo> perform)
        {
            Dispatcher?.BeginInvoke(DispatcherPriority.Normal,
                new Action(() =>
                {
                    _ActivityBuilder = new ActivityInfoBuilder(action, this, perform);
                    DoPropertyChanged(nameof(ActivityInfoBuilder));
                    DoPropertyChanged(nameof(IsActivityBuilding));
                    DoPropertyChanged(nameof(BuilderVisible));
                }));
        }
        #endregion

        #region private void SyncChoices(List<ActionInfo> choiceActions)
        private void SyncChoices(List<ActionInfo> choiceActions)
        {
            // remove existing not in current
            foreach (var _rmv in (from _e in _ChoiceActions
                                  where !choiceActions.Any(_c => _c.IsSameAction(_e))
                                  select _e).ToList())
                _ChoiceActions.Remove(_rmv);

            // update existing
            foreach (var _updt in (from _e in _ChoiceActions
                                   from _c in choiceActions
                                   where _e.IsSameAction(_c)
                                   select new { Exist = _e, Current = _c }).ToList())
            {
                if ((_updt.Exist.FirstOptionAimInfo?.Key != _updt.Current.FirstOptionAimInfo?.Key)
                    || (_updt.Exist.FirstOptionAimInfo?.FirstOption?.Key != _updt.Current.FirstOptionAimInfo?.FirstOption?.Key))
                {
                    _updt.Exist.AimingModes = _updt.Current.AimingModes;
                    _updt.Exist.DoNotify(nameof(ActionInfo.FirstOptionAimInfo));
                }
                else
                {
                    _updt.Exist.AimingModes = _updt.Current.AimingModes;
                }
            }

            // add current not in existing
            foreach (var _add in (from _c in choiceActions
                                  where !_ChoiceActions.Any(_e => _e.IsSameAction(_c))
                                  select _c).ToList())
                _ChoiceActions.Add(_add);
        }
        #endregion

        public void ClearActivityBuilding()
        {
            _ActivityBuilder = null;
            DoPropertyChanged(nameof(ActivityInfoBuilder));
            DoPropertyChanged(nameof(IsActivityBuilding));
            DoPropertyChanged(nameof(BuilderVisible));
        }

        public Visibility BuilderVisible
            => _ActivityBuilder != null ? Visibility.Visible : Visibility.Collapsed;


        // ----- Aim-Point -----

        #region public bool IsMoveEnabled { get; set; }
        /// <summary>Whether regular single-step movement commands should be enabled</summary>
        public bool IsMoveEnabled
        {
            get => _IsMoveEnabled && (SelectedSensorHost?.ID?.Equals(ActorID.ToString()) ?? false);
            set
            {
                _IsMoveEnabled = value;
                DoPropertyChanged(nameof(IsMoveEnabled));
            }
        }
        #endregion

        #region public AimPointActivation AimPointActivation { get; set; }
        /// <summary>AimPoint manipulation mode</summary>
        public AimPointActivation AimPointActivation
        {
            get { return _AimPointActivation; }
            set
            {
                _AimPointActivation = value;
                DoPropertyChanged(nameof(AimPointActivation));
            }
        }
        #endregion

        #region public void EnableMove()
        private void EnableMove()
        {
            IsMoveEnabled = true;
            AimPointActivation = AimPointActivation.Off;
        }
        #endregion

        #region AimExtent
        private bool AimExtent_CanExecute()
            => true;

        private void AimExtent_Executed()
        {
            if (AimPointActivation != AimPointActivation.SetExtent)
            {
                IsMoveEnabled = false;
                AimPointActivation = AimPointActivation.SetExtent;
            }
            else
            {
                EnableMove();
            }
        }
        #endregion

        #region AdjustAimPoint
        private bool AdjustAimPoint_CanExecute()
            => true;

        private void AdjustAimPoint_Executed()
        {
            if (AimPointActivation != AimPointActivation.AdjustPoint)
            {
                IsMoveEnabled = false;
                AimPointActivation = AimPointActivation.AdjustPoint;
                LocaleViewModel.ToggleAimPoint();
            }
            else
            {
                LocaleViewModel.ToggleAimPoint();
                EnableMove();
            }
        }
        #endregion

        #region SetSensors
        private bool SetSensors_CanExecute(SensorHostInfo sensors)
            => SensorHosts.Any(_sh => _sh.ID.Equals(sensors?.ID));

        private void SetSensors_Executed(SensorHostInfo sensors)
            => SelectedSensorHost = sensors;
        #endregion

        // ----- Target Pointer "Mode" and Reset -----

        #region Target Cell
        private void AdjustTargetPointer_Executed()
        {
            switch (AimPointActivation)
            {
                case AimPointActivation.Off:
                case AimPointActivation.AdjustPoint:
                case AimPointActivation.SetExtent:
                    // go to target cell mode
                    IsMoveEnabled = false;
                    AimPointActivation = AimPointActivation.TargetCell;
                    break;

                case AimPointActivation.TargetCell:
                    // go to target intersection mode
                    IsMoveEnabled = false;
                    AimPointActivation = AimPointActivation.TargetIntersection;
                    LocaleViewModel.CellToIntersect();
                    break;

                case AimPointActivation.TargetIntersection:
                default:
                    // turn target mode off
                    EnableMove();
                    LocaleViewModel.IntersectToCell();
                    break;
            }
        }

        private void HideTargetPointer_Executed()
        {
            switch (AimPointActivation)
            {
                case AimPointActivation.TargetCell:
                    // go to target intersection mode
                    EnableMove();
                    break;

                case AimPointActivation.TargetIntersection:
                    // turn target mode off
                    EnableMove();
                    LocaleViewModel.IntersectToCell();
                    break;
            }
        }

        private bool TargetPointer_CanExecute()
            => true;

        private void ResetTargetPointer_Executed()
        {
            LocaleViewModel.ResetTargetCell();
        }
        #endregion

        // ----- target queue mode -----

        #region Queue Target Pointer
        private bool QueueTargetPointer_CanExecute()
            => AimPointActivation == AimPointActivation.TargetCell
            || AimPointActivation == AimPointActivation.TargetIntersection;

        private void QueueTargetPointer_Executed()
        {
            if (AimPointActivation == AimPointActivation.TargetCell)
            {
                var _cell = new CellPosition(LocaleViewModel.TargetCell);
                _QueuedTargets.Add(new QueuedCell(this, _cell));
                DoPropertyChanged(nameof(QueuedTargets));
                DoPropertyChanged(nameof(QueuedCells));
                DoPropertyChanged(nameof(QueuedLocations));
                DoPropertyChanged(nameof(QueuedTargetVisibility));
            }
            else if (AimPointActivation == AimPointActivation.TargetIntersection)
            {
                var _cell = new CellPosition(LocaleViewModel.TargetCell);
                _QueuedTargets.Add(new QueuedIntersection(this, _cell));
                DoPropertyChanged(nameof(QueuedTargets));
                DoPropertyChanged(nameof(QueuedIntersections));
                DoPropertyChanged(nameof(QueuedTargetVisibility));
            }
        }
        #endregion

        #region Clear Target Queue
        public void ClearQueuedLocations()
        {
            _QueuedTargets.RemoveAll(_qt => (_qt is QueuedCell));
            DoPropertyChanged(nameof(QueuedTargets));
            DoPropertyChanged(nameof(QueuedCells));
            DoPropertyChanged(nameof(QueuedLocations));
            DoPropertyChanged(nameof(QueuedIntersections));
            DoPropertyChanged(nameof(QueuedTargetVisibility));
        }
        #endregion

        // ----- Local State
        public ActorLocalState GetLocalState()
            => Proxies.GetLocalState(Guid.Parse(_SensorHostID),
                _SensorHostID.Equals(ActorID.ToString()) ? ViewPointType.ThirdPerson : ViewPointType.FirstPerson);

        public void UpdateLocalState(Func<ActorLocalState, ActorLocalState> update)
            => Proxies.UpdateLocalState(Guid.Parse(_SensorHostID),
                _SensorHostID.Equals(ActorID.ToString()) ? ViewPointType.ThirdPerson : ViewPointType.FirstPerson,
                update);
    }
}
