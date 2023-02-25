using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Infos;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CreatureTimelinePendingModel : CreatureTrackerModel, IActivityBuilderActor
    {
        public CreatureTimelinePendingModel()
        {
            _ActionMenu = new ObservableCollection<MenuBaseViewModel>()
            {
                new MenuViewModel
                {
                     Header = ActivityInfo
                }
            };
            _DoAction = new RelayCommand<object>((action) => SetAction(action));
            _Awarenesses = null;
        }

        #region data
        private readonly ObservableCollection<MenuBaseViewModel> _ActionMenu;
        private readonly ICommand _DoAction;
        private ActivityInfoBuilder _ActivityBuilder;
        private ObservableCollection<AwarenessInfo> _Awarenesses;
        #endregion

        #region private void SetAction(object actionSelection)
        private void SetAction(object actionSelection)
        {
            if (actionSelection is Tuple<ActionInfo, AimTargetInfo> _tuple)
            {
                _ActivityBuilder = new ActivityInfoBuilder(_tuple.Item1, this, (activity) => { });
                var _builder = _ActivityBuilder.TargetBuilders.FirstOrDefault();
                if (_builder != null)
                {
                    _builder.SetTargets(_tuple.Item2.ToEnumerable().ToList());
                }
            }
            else
            {
                if (actionSelection is Tuple<ActionInfo, List<AimTargetInfo>> _list)
                {
                    _ActivityBuilder = new ActivityInfoBuilder(_list.Item1, this, (activity) => { });
                    var _builder = _ActivityBuilder.TargetBuilders.FirstOrDefault();
                    if (_builder != null)
                    {
                        _builder.SetTargets(_list.Item2);
                    }
                }
                else if (actionSelection is ActionInfo _action)
                {
                    _ActivityBuilder = new ActivityInfoBuilder(_action, this, (activity) => { });
                }
            }
            DoPropertyChanged(nameof(ActivityInfoBuilder));
            DoPropertyChanged(nameof(ActivityInfo));
            (_ActionMenu[0] as MenuViewModel).Header = ActivityInfo;
        }
        #endregion

        #region private List<ActionInfo> FixupActions(List<ActionInfo> actions)
        private List<ActionInfo> FixupActions(List<ActionInfo> actions)
        {
            // filter actions
            actions = actions.Where(_a => (_a.Provider?.ProviderInfo is TimelineActionProviderInfo) || (_a.TimeType == TimeType.TimelineScheduling)).ToList();
            var _resolver = TickTrackerMode?.MasterModel.Proxies.IconResolver;
            if (_resolver != null)
            {
                // object aim options
                foreach (var _obj in (from _act in actions
                                      from _oList in _act.AimingModes.OfType<ObjectListAimInfo>()
                                      from _oi in _oList.ObjectInfos.OfType<IIconInfo>()
                                      select _oi))
                {
                    _obj.IconResolver = _resolver;
                }
            }
            return actions;
        }
        #endregion

        public TimelineGroupModel TimelineGroupModel { get => Group as TimelineGroupModel; }

        public ObservableCollection<MenuBaseViewModel> ActionMenu
            => _ActionMenu;

        private void SyncMenuItems()
        {
            if (CreatureTrackerInfo?.LocalActionBudgetInfo?.HeldActivity != null)
            {
            }
            else if (CreatureTrackerInfo?.LocalActionBudgetInfo?.NextActivity != null)
            {
            }
            else
            {
                var _actions = FixupActions(TickTrackerMode?.MasterModel.Proxies.IkosaProxy.Service.GetActions(CreatureTrackerInfo.ID.ToString()));
                var _actionMenu = ActionMenuBuilder.GetTimelineMenuItems(_actions, _DoAction);
                TickTrackerMode.MasterModel.Proxies?.DoMasterDispatch(() => ActionMenuBuilder.SyncMenus((_ActionMenu[0] as MenuViewModel).SubItems, _actionMenu));
            }
        }

        #region public override void Conformulate(CreatureTrackerModel conform)
        public override void Conformulate(CreatureTrackerModel conform)
        {
            base.Conformulate(conform);
            SyncMenuItems();
            if (_Awarenesses != null)
            {
                // conformulate awareness
                var _sourceAware = Proxies.IkosaProxy.Service.GetAwarenessInfo(ID.ToString(), ID.ToString());
                TickTrackerMode.MasterModel.Proxies.DoMasterDispatch(() => ConformulateAwareness(_sourceAware));
            }
        }
        #endregion

        public ActivityInfoBuilder ActivityInfoBuilder => _ActivityBuilder;

        public ActivityInfo ActivityInfo
            => CreatureTrackerInfo?.LocalActionBudgetInfo?.HeldActivity
            ?? CreatureTrackerInfo?.LocalActionBudgetInfo?.NextActivity
            ?? ActivityInfoBuilder?.GetPendingActivity()
            ?? new ActivityInfo
            {
                ActorID = Guid.Empty,
                ActionID = Guid.Empty,
                ActionKey = string.Empty,
                DisplayName = @"-- Select --",
                Description = string.Empty
            };

        public Guid ActorID => ID;
        public ProxyModel Proxies => TickTrackerMode.MasterModel.Proxies;
        public Visibility PerformVisibility => Visibility.Collapsed;
        public IEnumerable<QueuedTargetItem> QueuedTargets { get { yield break; } }
        public IEnumerable<QueuedAwareness> QueuedAwarenesses { get { yield break; } }

        #region public ObservableCollection<AwarenessInfo> Awarenesses { get; }
        public ObservableCollection<AwarenessInfo> Awarenesses
        {
            get
            {
                if (_Awarenesses == null)
                {
                    _Awarenesses = new ObservableCollection<AwarenessInfo>();

                    // conformulate awareness
                    var _sourceAware = Proxies.IkosaProxy.Service.GetAwarenessInfo(ID.ToString(), ID.ToString());
                    TickTrackerMode.MasterModel.Proxies.DoMasterDispatch(() => ConformulateAwareness(_sourceAware));
                }
                return _Awarenesses;
            }
        }
        #endregion

        #region public void ClearActivityBuilding()
        public void ClearActivityBuilding()
        {
            _ActivityBuilder = null;
            DoPropertyChanged(nameof(ActivityInfoBuilder));
            DoPropertyChanged(nameof(ActivityInfo));
            (_ActionMenu[0] as MenuViewModel).Header = ActivityInfo;
            SyncMenuItems();
        }
        #endregion

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
            var _resolver = Proxies.IconResolver;
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
        }
        #endregion
    }
}
