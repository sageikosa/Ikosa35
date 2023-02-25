using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class ActivityInfoBuilder : DependencyObject, INotifyPropertyChanged
    {
        #region ctor(...)
        public ActivityInfoBuilder(
            ActionInfo action, 
            IActivityBuilderActor actor,
            Action<ActivityInfo> perform)
        {
            ActivityBuilderActor = actor;
            _Perform = perform;
            _AimTargetings =
                new ObservableCollection<AimTargetingBase>(from _mode in action.AimingModes
                                                           let _at = AimTargetingFactory.GetAimTargeting(_mode, this)
                                                           where _at != null
                                                           select _at);
            Action = action;
            SetPresenter();

            _DoAction = new RelayCommand(DoAction_Executed, DoAction_CanExecute);
            _CancelBuild = new RelayCommand(CancelBuild_Executed);
        }
        #endregion

        #region state
        private readonly ObservableCollection<AimTargetingBase> _AimTargetings;
        private readonly RelayCommand _DoAction;
        private readonly RelayCommand _CancelBuild;
        private readonly Action<ActivityInfo> _Perform;
        #endregion

        #region public IActivityBuilderActor ActivityBuilderActor { get; set; } (DEPENDENCY)
        public IActivityBuilderActor ActivityBuilderActor
        {
            get { return (IActivityBuilderActor)GetValue(ActivityBuilderActorProperty); }
            set { SetValue(ActivityBuilderActorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActorModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActivityBuilderActorProperty =
            DependencyProperty.Register(nameof(ActivityBuilderActor), typeof(IActivityBuilderActor), typeof(ActivityInfoBuilder),
                new PropertyMetadata(null));
        #endregion

        public IActivityBuilderTacticalActor ActivityBuilderTacticalActor
            => ActivityBuilderActor as IActivityBuilderTacticalActor;

        #region public ActionInfo Action { get; set; } (DEPENDENCY)
        public ActionInfo Action
        {
            get { return (ActionInfo)GetValue(ActionInfoProperty); }
            set { SetValue(ActionInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActionInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActionInfoProperty =
            DependencyProperty.Register(nameof(Action), typeof(ActionInfo), typeof(ActivityInfoBuilder),
                new PropertyMetadata(null, new PropertyChangedCallback(OnActionInfoChanged)));
        #endregion

        #region public object Presenter { get; set; } (DEPENDENCY)
        public object Presenter
        {
            get { return GetValue(PresenterProperty); }
            set { SetValue(PresenterProperty, value); }
        }

        public static readonly DependencyProperty PresenterProperty =
            DependencyProperty.Register(nameof(Presenter), typeof(object), typeof(ActivityInfoBuilder),
                new PropertyMetadata(null));
        #endregion

        public Visibility PresenterVisibility
        {
            get => Presenter != null ? Visibility.Visible : Visibility.Hidden;
            set { }
        }

        #region public object Provider { get; set; } (DEPENDENCY)
        public object Provider
        {
            get { return GetValue(ProviderProperty); }
            set { SetValue(ProviderProperty, value); }
        }

        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(nameof(Provider), typeof(object), typeof(ActivityInfoBuilder),
                new PropertyMetadata(null));
        #endregion

        private void SetPresenter()
        {
            AwarenessInfo _rootFinder(Guid presenterID)
                => ActivityBuilderActor.Awarenesses.FirstOrDefault(_a => _a.IsAnyAware(presenterID));

            object _getPresenterCard(Info presenter)
                => (presenter is ObjectInfo)
                ? (object)new ObjectInfoVM
                {
                    ObjectInfo = presenter as ObjectInfo,
                    IconVisibility = Visibility.Visible,
                    TextVisibility = Visibility.Collapsed,
                    Size = 48d
                }
            : presenter;

            if (Action != null)
            {
                var _provider = Action.Provider;
                if (ActivityBuilderActor != null)
                {
                    Provider = _getPresenterCard(AwarenessInfo.FindAwareness(ActivityBuilderActor.Awarenesses, _provider.ID)?.Info)
                        ?? _getPresenterCard(_provider.ProviderInfo);
                }

                var _presenter = _rootFinder(_provider.PresenterID);
                if ((_presenter != null) && (_presenter.ID != ActivityBuilderActor.ActorID))
                {
                    Presenter = _getPresenterCard(_presenter.Info);
                    DoPropertyChanged(nameof(PresenterVisibility));
                    return;
                }
            }
            else
            {
                // clear provider
                Provider = null;
            }

            // default nothing
            Presenter = null;
            DoPropertyChanged(nameof(PresenterVisibility));
        }

        #region private static void OnActionInfoChanged(DependencyObject builder, DependencyPropertyChangedEventArgs args)
        private static void OnActionInfoChanged(DependencyObject builder, DependencyPropertyChangedEventArgs args)
        {
            if (builder is ActivityInfoBuilder _builder)
            {
                var _action = args.NewValue as ActionInfo;
                void _newTargets()
                {
                    // get all new targets
                    foreach (var _target in (from _mode in _action.AimingModes
                                             let _at = AimTargetingFactory.GetAimTargeting(_mode, _builder)
                                             where _at != null
                                             select _at))
                        _builder.TargetBuilders.Add(_target);
                }

                // sync aim targettings if needed
                if (_builder.Action == null)
                {
                    if (_action != null)
                    {
                        // new action
                        _builder.TargetBuilders.Clear();
                        _builder.SetPresenter();
                        _newTargets();
                    }
                }
                else if (_action == null)
                {
                    // no action
                    _builder.TargetBuilders.Clear();
                    _builder.SetPresenter();
                }
                else if (_builder.Action.IsSameAction(_action))
                {
                    // same action, synchronize targets...
                    foreach (var _rmv in (from _tb in _builder.TargetBuilders
                                          where !_action.AimingModes.Any(_am => _tb.IsSameMode(_am))
                                          select _tb).ToList())
                    {
                        // remove target builders that are no longer available
                        _builder.TargetBuilders.Remove(_rmv);
                    }

                    foreach (var _exist in (from _tb in _builder.TargetBuilders
                                            from _am in _action.AimingModes
                                            where _tb.IsSameMode(_am)
                                            select new { TargetBuilder = _tb, AimMode = _am }).ToList())
                    {
                        // sync continuing aim modes...
                        _exist.TargetBuilder.SyncMode(_exist.AimMode);
                    }

                    foreach (var _add in (from _mode in _action.AimingModes
                                          where !_builder.TargetBuilders.Any(_tb => _tb.IsSameMode(_mode))
                                          let _at = AimTargetingFactory.GetAimTargeting(_mode, _builder)
                                          where _at != null
                                          select _at).ToList())
                    {
                        // add target builders that are now available
                        _builder.TargetBuilders.Add(_add);
                    }
                }
                else
                {
                    // new action
                    _builder.TargetBuilders.Clear();
                    _builder.SetPresenter();
                    _newTargets();
                }
            }
        }
        #endregion

        public RelayCommand DoAction => _DoAction;
        public RelayCommand CancelBuild => _CancelBuild;

        /// <summary>Returns true if all builders are ready</summary>
        public bool IsReady
            => TargetBuilders.All(_tb => _tb.IsReady);

        /// <summary>Signals to ViewModel listeners that IsReady may have changed</summary>
        public void SetIsReady() { DoPropertyChanged(nameof(IsReady)); }

        /// <summary>List of all AimTargetingBase defined for the action</summary>
        public ObservableCollection<AimTargetingBase> TargetBuilders
            => _AimTargetings;

        #region public ActivityInfo BuildActivity()
        /// <summary>Returns performable ActivityInfo if all TargetBuilders are ready</summary>
        public ActivityInfo BuildActivity()
        {
            if (IsReady)
            {
                var _activity = new ActivityInfo
                {
                    ActorID = ActivityBuilderActor.ActorID,
                    ActionID = Action.ID,
                    ActionKey = Action.Key,
                    DisplayName = Action.DisplayName,
                    Description = Action.Description,
                    Targets = TargetBuilders.SelectMany(_tb => _tb.FinishedTargets).ToArray()
                };
                return _activity;
            }
            return null;
        }
        #endregion

        public ActivityInfo GetPendingActivity()
            => new ActivityInfo
            {
                ActorID = ActivityBuilderActor?.ActorID ?? Guid.Empty,
                ActionID = Action?.ID ?? Guid.Empty,
                ActionKey = Action?.Key ?? string.Empty,
                DisplayName = Action.DisplayName ?? @"-- Select --",
                Description = Action.Description ?? string.Empty
            };

        // Execute
        private bool DoAction_CanExecute()
            => IsReady;

        private void DoAction_Executed()
            => _Perform?.Invoke(BuildActivity());

        private void CancelBuild_Executed()
            => ActivityBuilderActor?.ClearActivityBuilding();

        #region INotifyPropertyChanged
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
