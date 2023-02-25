using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Proxy;
using Uzi.Ikosa.Proxy.ViewModel;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for ReactivePrerequisiteItem.xaml
    /// </summary>
    public partial class ReactivePrerequisiteItem : UserControl
    {
        public ReactivePrerequisiteItem()
        {
            try { InitializeComponent(); } catch { }
            DataContextChanged += ReactivePrerequisiteItem_DataContextChanged;
            _DoAction = new RelayCommand<object>(DoAction_Executed, DoAction_CanExecute);
        }

        #region state
        private readonly RelayCommand<object> _DoAction;
        #endregion

        private void ReactivePrerequisiteItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _react = ReactivePrerequisiteInfo;
            if (_react != null)
            {
                // list attacks (and no action)
                var _actions = _react.ReactiveActions.ToList();
                var _noAct = new ActionInfo { ID = Guid.Empty, Key = string.Empty, DisplayName = @"No Action", TimeCost = @"-None-" };
                _actions.Insert(0, _noAct);
                mnuResponse.ItemsSource = GetResponses(_actions).ToList();

                // start ready
                _react.IsReady = true;
            }
        }

        public PrerequisiteModel PrerequisiteModel
            => DataContext as PrerequisiteModel;

        public ReactivePrerequisiteInfo ReactivePrerequisiteInfo
            => PrerequisiteModel?.Prerequisite as ReactivePrerequisiteInfo;

        #region public void PerformAction(ActivityInfo activity)
        /// <summary>Perform action via proxy, then update available action list</summary>
        /// <param name="activity"></param>
        public void PerformAction(ActivityInfo activity)
        {
            try
            {
                var _react = ReactivePrerequisiteInfo;
                if (_react != null)
                {
                    _react.ResponseActivity = activity;
                    PrerequisiteModel.PrerequisiteProxy.DoSendPrerequisites.Execute(null);
                    PrerequisiteModel.IsSent = true;
                }
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Ikosa Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region private void DoAction_Executed(object parameter)
        private void DoAction_Executed(object parameter)
        {
            var _observableActor = (PrerequisiteModel.PrerequisiteProxy as ActorModel)?.ObservableActor;
            if (parameter is Tuple<ActionInfo, AimTargetInfo> _tuple)
            {
                #region One Aim Target that is Ready to Go
                // One Aim Target Ready to Go
                PerformAction(new ActivityInfo
                {
                    ActionID = _tuple.Item1.ID,
                    ActionKey = _tuple.Item1.Key,
                    ActorID = ReactivePrerequisiteInfo.FulfillerID,
                    Targets = new AimTargetInfo[] { _tuple.Item2 }
                });
                #endregion
            }
            else
            {
                if (parameter is Tuple<ActionInfo, List<AimTargetInfo>> _list)
                {
                    #region multiple aim targets that are ready to go
                    // do action with supplied targets
                    PerformAction(new ActivityInfo
                    {
                        ActionID = _list.Item1.ID,
                        ActionKey = _list.Item1.Key,
                        ActorID = ReactivePrerequisiteInfo.FulfillerID,
                        Targets = _list.Item2.ToArray()
                    });
                    #endregion
                }
                else if (parameter is ActionInfo _action)
                {
                    if (_action.AimingModes.Any() && !_action.AimingModes.All(_aim => _aim is FixedAimInfo))
                    {
                        _observableActor?.SetActivityBuilder(_action, (activity) => PerformAction(activity));
                    }
                    else
                    {
                        #region no aiming needed, or all are fixed aims
                        // No Aiming needed, or all are Fixed Aims
                        PerformAction(new ActivityInfo
                        {
                            ActionID = _action.ID,
                            ActionKey = _action.Key,
                            ActorID = ReactivePrerequisiteInfo.FulfillerID
                        });
                        #endregion
                    }
                }
                else
                {
                    // no action
                    var _react = ReactivePrerequisiteInfo;
                    if (_react != null)
                    {
                        _react.ResponseActivity = null;
                        PrerequisiteModel.PrerequisiteProxy.DoSendPrerequisites.Execute(null);
                        PrerequisiteModel.IsSent = true;
                    }
                }
            }
            _observableActor?.DoFocus();
        }
        #endregion

        private bool DoAction_CanExecute(object parameter)
            => parameter != null;

        #region private IEnumerable<MenuBaseViewModel> GetResponses(List<ActionInfo> actions)
        private IEnumerable<MenuBaseViewModel> GetResponses(List<ActionInfo> actions)
        {
            var _react = ReactivePrerequisiteInfo;
            foreach (var _action in actions)
            {
                // build activity from action
                if ((_action.ID == Guid.Empty) && (_action.Key == string.Empty))
                {
                    yield return new MenuViewModel
                    {
                        IconSource = null,  // TODO: NO ACTION icon...
                        Key = string.Empty,
                        IsAction = true,
                        Header = @"No reaction",
                        Order = @"000",
                        Command = _DoAction,
                        Parameter = null,
                        ToolTip = @"Make no response to triggering condition"
                    };
                }
                else
                {
                    var _header = true //includeProvider
                        ? (object)new ContentStack(Orientation.Vertical, _action.Provider.ProviderInfo, _action)
                        : _action;
                    string _key(AimingModeInfo aimMode, string extra)
                        => $@"{_action.Provider.ID}\{_action.ID}\{_action.Key}\{aimMode.Key}{extra}";

                    if (_action.AimingModes.Any()
                        && (_action.AimingModes.Count() == 1))
                    {
                        var _first = _action.AimingModes.First();

                        // is the aiming mode required (generally yes)
                        if (_first.MinimumAimingModes == 1)
                        {
                            switch (_first)
                            {
                                case CoreListAimInfo _coreListAim:
                                    if (_coreListAim.ObjectInfos.Any())
                                    {
                                        var _coreCount = _coreListAim.ObjectInfos.Count();
                                        if (_coreCount > 1)
                                        {
                                            #region multiple objects available
                                            yield return new MenuViewModel
                                            {
                                                Key = _key(_coreListAim, @"\[Multi]"),
                                                Header = _header,
                                                IsAction = true,
                                                Order = _action.OrderKey,
                                                SubItems = new ObservableCollection<MenuBaseViewModel>(
                                                    ActionMenuBuilder.GetMinOneCoreInfos(_action, _coreListAim, null /*command*/))
                                            };
                                            #endregion
                                        }
                                        else if (_coreCount == 1)
                                        {
                                            #region only one object available
                                            var _coreInfo = _coreListAim.ObjectInfos.FirstOrDefault();
                                            _header = new ContentStack(Orientation.Horizontal, _header, " \u2794 ", _coreInfo);
                                            yield return new MenuViewModel
                                            {
                                                IconSource = _coreInfo,
                                                Key = _key(_coreListAim, $@"\{_coreInfo.ID}"),
                                                IsAction = true,
                                                Header = _header,
                                                Order = _action.OrderKey,
                                                Command = _DoAction,
                                                Parameter =
                                                new Tuple<ActionInfo, AimTargetInfo>(_action, new CoreInfoTargetInfo
                                                {
                                                    Key = _coreListAim.Key,
                                                    TargetID = _coreInfo.ID,
                                                    CoreInfo = _coreInfo
                                                }),
                                                ToolTip = _coreInfo
                                            };
                                            #endregion
                                        }
                                    }
                                    break;

                                case ObjectListAimInfo _objListAim:
                                    if (_objListAim.ObjectInfos.Any())
                                    {
                                        var _objCount = _objListAim.ObjectInfos.Count();
                                        if (_objCount > 1)
                                        {
                                            #region multiple objects available
                                            yield return new MenuViewModel
                                            {
                                                Key = _key(_objListAim, @"\[Multi]"),
                                                IsAction = true,
                                                Header = _header,
                                                Order = _action.OrderKey,
                                                SubItems = new ObservableCollection<MenuBaseViewModel>(
                                                    ActionMenuBuilder.GetMinOneObjects(_action, _objListAim, _DoAction))
                                            };
                                            #endregion
                                        }
                                        else if (_objCount == 1)
                                        {
                                            #region only one object available
                                            var _obj = _objListAim.ObjectInfos.FirstOrDefault();
                                            _header = new ContentStack(Orientation.Horizontal, _header, " \u2794 ", _obj);
                                            yield return new MenuViewModel
                                            {
                                                IconSource = _obj,
                                                IsAction = true,
                                                Key = _key(_objListAim, $@"\{_obj.ID}"),
                                                Header = _header,
                                                Order = _action.OrderKey,
                                                Command = _DoAction,
                                                Parameter =
                                                new Tuple<ActionInfo, AimTargetInfo>(_action, new AimTargetInfo
                                                {
                                                    Key = _objListAim.Key,
                                                    TargetID = _obj.ID
                                                }),
                                                ToolTip = _obj
                                            };
                                            #endregion
                                        }
                                    }
                                    break;

                                case OptionAimInfo _optionAim:
                                    #region option aim
                                    if (_optionAim.Options.Any())
                                    {
                                        var _allOptions = true; // TODO: determine boolean
                                        var _optCount = _optionAim.Options.Count(_o => !_o.IsCurrent || _allOptions);
                                        if (_optCount == 1)
                                        {
                                            #region only one option available
                                            var _option = _optionAim.Options.FirstOrDefault(_o => !_o.IsCurrent || _allOptions);
                                            _header = new ContentStack(Orientation.Horizontal, _header, " \u2794 ", _option);
                                            yield return new MenuViewModel
                                            {
                                                Key = _key(_optionAim, $@"\{_option.Key}"),
                                                IsAction = true,
                                                Header = _header,
                                                Command = _DoAction,
                                                Order = _action.OrderKey,
                                                Parameter =
                                                   new Tuple<ActionInfo, AimTargetInfo>(_action, new OptionTargetInfo
                                                   {
                                                       Key = _optionAim.Key,
                                                       OptionKey = _option.Key
                                                   }),
                                                ToolTip = _option
                                            };
                                            #endregion
                                        }
                                        else
                                        if (_optCount > 1)
                                        {
                                            #region multiple options available
                                            yield return new MenuViewModel
                                            {
                                                Key = _key(_optionAim, @"\[Multi]"),
                                                IsAction = true,
                                                Header = _header,
                                                Order = _action.OrderKey,
                                                SubItems = new ObservableCollection<MenuBaseViewModel>(
                                                    ActionMenuBuilder.GetMinOneOptions(_action, _optionAim, _DoAction))
                                            };
                                            #endregion
                                        }
                                    }
                                    #endregion
                                    break;

                                case PersonalAimInfo _personalAim:
                                    #region personal aim
                                    var _persCount = _personalAim.ValidTargets.Count();
                                    if (_persCount > 1)
                                    {
                                        yield return new MenuViewModel
                                        {
                                            Key = _key(_personalAim, @"\[Multi]"),
                                            IsAction = true,
                                            Header = _header,
                                            Order = _action.OrderKey,
                                            SubItems = new ObservableCollection<MenuBaseViewModel>(
                                                ActionMenuBuilder.GetMinOnePersonal(_action, _personalAim, _DoAction))
                                        };
                                    }
                                    else if (_persCount == 1)
                                    {
                                        var _targetID = _personalAim.ValidTargets.Select(_vt => _vt.ID).FirstOrDefault();
                                        yield return new MenuViewModel
                                        {
                                            Key = _key(_personalAim, $@"\{_targetID}"),
                                            IsAction = true,
                                            Header = _header,
                                            Order = _action.OrderKey,
                                            Command = _DoAction,
                                            Parameter =
                                              new Tuple<ActionInfo, AimTargetInfo>(_action, new AimTargetInfo
                                              {
                                                  Key = _personalAim.Key,
                                                  TargetID = _targetID
                                              })
                                        };
                                    }
                                    #endregion
                                    break;

                                default:
                                    // too complex for menu
                                    yield return new MenuViewModel
                                    {
                                        IconSource = _action.Provider.ProviderInfo,
                                        Key = $@"{_action.Provider.ID}\{_action.ID}\{_action.Key}\",
                                        IsAction = true,
                                        Header = _header,
                                        Command = _DoAction,
                                        Order = _action.OrderKey,
                                        Parameter = _action
                                    };
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // no aiming modes, so simply do!
                        yield return new MenuViewModel
                        {
                            IconSource = _action.Provider.ProviderInfo,
                            Key = $@"{_action.Provider.ID}\{_action.ID}\{_action.Key}\",
                            IsAction = true,
                            Header = _header,
                            Command = _DoAction,
                            Order = _action.OrderKey,
                            Parameter = _action
                        };
                    }
                }
            }
        }
        #endregion
    }
}
