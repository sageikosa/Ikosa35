using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public static class ActionMenuBuilder
    {
        public static void AddContextMenu(MenuViewModel target, IEnumerable<ActionInfo> actions, ICommand command)
        {
            foreach (var _mnu in (from _a in actions
                                  from _ami in GetActionMenuItems(_a, command, false)
                                  select _ami))
            {
                target.SubItems.Add(_mnu);
            }
        }

        #region public static IEnumerable<MenuBaseViewModel> GetMinOneOptions(ActionInfo actionInfo, OptionAimInfo optionAim, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetMinOneOptions(ActionInfo actionInfo, OptionAimInfo optionAim, ICommand command)
        {
            string _key(string extra)
                => $@"{actionInfo.Provider.ID}\{actionInfo.ID}\{actionInfo.Key}\{optionAim.Key}{extra}";

            // individual options
            var _order = 101;
            foreach (var _opt in optionAim.Options)
            {
                yield return new MenuViewModel
                {
                    Key = _key($@"\{_opt.Key}"),
                    Order = $@"{_order:00#}",
                    Header = _opt,
                    Command = command,
                    Parameter =
                    new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new OptionTargetInfo
                    {
                        Key = optionAim.Key,
                        OptionKey = _opt.Key
                    }),
                    IsChecked = _opt.IsCurrent
                };
                _order++;
            }

            // multiple selection
            if (optionAim.MaximumAimingModes > 1)
            {
                yield return new SeparatorViewModel { Order = @"200" }; // separator MenuViewModel
                if (optionAim.Options.Count() == 2)
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Both]"),
                        Header = @"Both",
                        Order = @"201",
                        Command = command,
                        Parameter =
                            new Tuple<ActionInfo, List<AimTargetInfo>>(actionInfo,
                                optionAim.Options.Select(_o => (new OptionTargetInfo
                                {
                                    Key = optionAim.Key,
                                    OptionKey = _o.Key
                                }) as AimTargetInfo).ToList())
                    };
                }
                else
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Choose]"),
                        Order = @"202",
                        Header = @"Choose",
                        Command = command,
                        Parameter = actionInfo
                    };
                }
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetMinOneObjects(ActionInfo actionInfo, ObjectListAimInfo objectListAim, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetMinOneObjects(ActionInfo actionInfo, ObjectListAimInfo objectListAim, ICommand command)
        {
            string _key(string extra)
                => $@"{actionInfo.Provider.ID}\{actionInfo.ID}\{actionInfo.Key}\{objectListAim.Key}{extra}";

            // individual options
            var _order = 101;
            foreach (var _obj in objectListAim.ObjectInfos)
            {
                yield return new MenuViewModel
                {
                    IconSource = _obj,
                    Key = _key($@"\{_obj.ID}"),
                    Order = $@"{_order:00#}",
                    Header = _obj,
                    Command = command,
                    Parameter =
                    new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new AimTargetInfo
                    {
                        Key = objectListAim.Key,
                        TargetID = _obj.ID
                    })
                };
                _order++;
            }

            // multiple selection
            if (objectListAim.MaximumAimingModes > 1)
            {
                yield return new SeparatorViewModel { Order = @"200" };
                if (objectListAim.ObjectInfos.Count() == 2)
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Both]"),
                        Order = @"201",
                        Header = @"Both",
                        Command = command,
                        Parameter =
                            new Tuple<ActionInfo, List<AimTargetInfo>>(actionInfo,
                                objectListAim.ObjectInfos.Select(_o => new AimTargetInfo
                                {
                                    Key = objectListAim.Key,
                                    TargetID = _o.ID
                                }).ToList())
                    };
                }
                else
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Choose]"),
                        Order = @"202",
                        Header = @"Choose",
                        Command = command,
                        Parameter = actionInfo
                    };
                }
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetMinOneCoreInfos(ActionInfo actionInfo, CoreListAimInfo coreListAim, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetMinOneCoreInfos(ActionInfo actionInfo, CoreListAimInfo coreListAim, ICommand command)
        {
            string _key(string extra)
                => $@"{actionInfo.Provider.ID}\{actionInfo.ID}\{actionInfo.Key}\{coreListAim.Key}{extra}";

            // individual options
            var _order = 101;
            foreach (var _coreInfo in coreListAim.ObjectInfos)
            {
                yield return new MenuViewModel
                {
                    IconSource = _coreInfo,
                    Key = _key($@"\{_coreInfo.ID}"),
                    Order = $@"{_order:00#}",
                    Header = _coreInfo,
                    Command = command,
                    Parameter =
                    new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new CoreInfoTargetInfo
                    {
                        Key = coreListAim.Key,
                        TargetID = _coreInfo.ID,
                        CoreInfo = _coreInfo
                    })
                };
                _order++;
            }

            // multiple selection
            if (coreListAim.MaximumAimingModes > 1)
            {
                yield return new SeparatorViewModel { Order = @"200" };
                if (coreListAim.ObjectInfos.Count() == 2)
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Both]"),
                        Order = @"201",
                        Header = @"Both",
                        Command = command,
                        Parameter =
                            new Tuple<ActionInfo, List<AimTargetInfo>>(actionInfo,
                                coreListAim.ObjectInfos.Select(_o => (new CoreInfoTargetInfo
                                {
                                    Key = coreListAim.Key,
                                    TargetID = _o.ID,
                                    CoreInfo = _o
                                }) as AimTargetInfo).ToList())
                    };
                }
                else
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Choose]"),
                        Order = @"202",
                        Header = @"Choose",
                        Command = command,
                        Parameter = actionInfo
                    };
                }
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetMinOnePersonal(ActionInfo actionInfo, PersonalAimInfo personalAim, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetMinOnePersonal(ActionInfo actionInfo, PersonalAimInfo personalAim, ICommand command)
        {
            string _key(string extra)
                => $@"{actionInfo.Provider.ID}\{actionInfo.ID}\{actionInfo.Key}\{personalAim.Key}{extra}";

            // individual options
            var _order = 101;
            foreach (var _aware in personalAim.ValidTargets)
            {
                yield return new MenuViewModel
                {
                    Key = _key($@"\{_aware.ID}"),
                    Order = $@"{_order:00#}",
                    Header = _aware,
                    Command = command,
                    Parameter =
                    new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new AimTargetInfo
                    {
                        Key = personalAim.Key,
                        TargetID = _aware.ID
                    })
                };
                _order++;
            }

            // multiple selection
            if (personalAim.MaximumAimingModes > 1)
            {
                yield return new SeparatorViewModel { Order = @"200" };
                if (personalAim.ValidTargets.Count() == 2)
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Both]"),
                        Order = @"201",
                        Header = @"Both",
                        Command = command,
                        Parameter =
                            new Tuple<ActionInfo, List<AimTargetInfo>>(actionInfo,
                                personalAim.ValidTargets.Select(_aware => new AimTargetInfo
                                {
                                    Key = personalAim.Key,
                                    TargetID = _aware.ID
                                }).ToList())
                    };
                }
                else
                {
                    yield return new MenuViewModel
                    {
                        Key = _key(@"\[Choose]"),
                        Order = @"202",
                        Header = @"Choose",
                        Command = command,
                        Parameter = actionInfo
                    };
                }
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetActionMenuItem(ActionInfo actionInfo, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetActionMenuItems(ActionInfo actionInfo, ICommand command, bool includeProvider, bool allOptions = false)
        {
            var _header = includeProvider
                ? (object)new ContentStack(Orientation.Vertical, actionInfo.Provider.ProviderInfo, actionInfo)
                : actionInfo;
            string _key(AimingModeInfo aimMode, string extra)
                => $@"{actionInfo.Provider.ID}\{actionInfo.ID}\{actionInfo.Key}\{aimMode.Key}{extra}";

            // one aiming mode only?
            if (actionInfo.AimingModes.Any() && (actionInfo.AimingModes.Count() == 1))
            {
                var _first = actionInfo.AimingModes.First();

                // does the aiming mode allow only one target?
                if (_first.MinimumAimingModes == 1)
                {
                    if (_first is CoreListAimInfo _coreListAim)
                    {
                        #region core info list aim
                        if (_coreListAim.ObjectInfos.Any())
                        {
                            var _count = _coreListAim.ObjectInfos.Count();
                            if (_count > 1)
                            {
                                #region multiple objects available
                                yield return new MenuViewModel
                                {
                                    Key = _key(_coreListAim, @"\[Multi]"),
                                    Header = _header,
                                    IsAction = true,
                                    Order = actionInfo.OrderKey,
                                    SubItems = new ObservableCollection<MenuBaseViewModel>(GetMinOneCoreInfos(actionInfo, _coreListAim, command))
                                };
                                #endregion
                            }
                            else if (_count == 1)
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
                                    Order = actionInfo.OrderKey,
                                    Command = command,
                                    Parameter =
                                    new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new CoreInfoTargetInfo
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
                        #endregion

                        yield break;
                    }
                    else if (_first is ObjectListAimInfo _objListAim)
                    {
                        #region object list aim
                        if (_objListAim.ObjectInfos.Any())
                        {
                            var _count = _objListAim.ObjectInfos.Count();
                            if (_count > 1)
                            {
                                #region multiple objects available
                                yield return new MenuViewModel
                                {
                                    Key = _key(_objListAim, @"\[Multi]"),
                                    IsAction = true,
                                    Header = _header,
                                    Order = actionInfo.OrderKey,
                                    SubItems = new ObservableCollection<MenuBaseViewModel>(GetMinOneObjects(actionInfo, _objListAim, command))
                                };
                                #endregion
                            }
                            else if (_count == 1)
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
                                    Order = actionInfo.OrderKey,
                                    Command = command,
                                    Parameter =
                                    new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new AimTargetInfo
                                    {
                                        Key = _objListAim.Key,
                                        TargetID = _obj.ID
                                    }),
                                    ToolTip = _obj
                                };
                                #endregion
                            }
                        }
                        #endregion

                        yield break;
                    }
                    else if (_first is OptionAimInfo _optionAim)
                    {
                        #region option aim
                        if (_optionAim.Options.Any())
                        {
                            var _count = _optionAim.Options.Count(_o => !_o.IsCurrent || allOptions);
                            if (_count == 1)
                            {
                                #region only one option available
                                var _option = _optionAim.Options.FirstOrDefault(_o => !_o.IsCurrent || allOptions);
                                _header = new ContentStack(Orientation.Horizontal, _header, " \u2794 ", _option);
                                yield return new MenuViewModel
                                {
                                    Key = _key(_optionAim, $@"\{_option.Key}"),
                                    IsAction = true,
                                    Header = _header,
                                    Command = command,
                                    Order = actionInfo.OrderKey,
                                    Parameter =
                                       new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new OptionTargetInfo
                                       {
                                           Key = _optionAim.Key,
                                           OptionKey = _option.Key
                                       }),
                                    ToolTip = _option
                                };
                                #endregion
                            }
                            else
                            if (_count > 1)
                            {
                                #region multiple options available
                                yield return new MenuViewModel
                                {
                                    Key = _key(_optionAim, @"\[Multi]"),
                                    IsAction = true,
                                    Header = _header,
                                    Order = actionInfo.OrderKey,
                                    SubItems = new ObservableCollection<MenuBaseViewModel>(GetMinOneOptions(actionInfo, _optionAim, command))
                                };
                                #endregion
                            }
                        }
                        #endregion

                        yield break;
                    }
                    else if (_first is PersonalAimInfo _personalAim)
                    {
                        #region personal aim
                        var _count = _personalAim.ValidTargets.Count();
                        if (_count > 1)
                        {
                            yield return new MenuViewModel
                            {
                                Key = _key(_personalAim, @"\[Multi]"),
                                IsAction = true,
                                Header = _header,
                                Order = actionInfo.OrderKey,
                                SubItems = new ObservableCollection<MenuBaseViewModel>(GetMinOnePersonal(actionInfo, _personalAim, command))
                            };
                        }
                        else if (_count == 1)
                        {
                            var _targetID = _personalAim.ValidTargets.Select(_vt => _vt.ID).FirstOrDefault();
                            yield return new MenuViewModel
                            {
                                Key = _key(_personalAim, $@"\{_targetID}"),
                                IsAction = true,
                                Header = _header,
                                Order = actionInfo.OrderKey,
                                Command = command,
                                Parameter =
                                  new Tuple<ActionInfo, AimTargetInfo>(actionInfo, new AimTargetInfo
                                  {
                                      Key = _personalAim.Key,
                                      TargetID = _targetID
                                  })
                            };
                        }
                        #endregion

                        yield break;
                    }
                }
            }

            yield return new MenuViewModel
            {
                IconSource = actionInfo.Provider.ProviderInfo,
                Key = $@"{actionInfo.Provider.ID}\{actionInfo.ID}\{actionInfo.Key}\",
                IsAction = true,
                Header = _header,
                Command = command,
                Order = actionInfo.OrderKey,
                Parameter = actionInfo
            };
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetPowerClassMenuItems(IEnumerable<ActionInfo> actions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetPowerClassMenuItems(IEnumerable<ActionInfo> actions, ICommand command)
        {
            // get powerClassInfo provided actions, grouped by level and ProviderID
            return (from _act in actions.OfType<PowerActionInfo>()
                    where string.IsNullOrEmpty(_act.HeadsUpMode)
                    group _act by new
                    {
                        _act.Provider.ID,
                        Level = _act.PowerLevel
                    } into _providerLevels
                    group _providerLevels by _providerLevels.Key.ID
                    into _actionGroup
                    let _provider = actions.FirstOrDefault(_a => _a.Provider.ID == _actionGroup.Key).Provider
                    where _provider.ProviderInfo is PowerClassInfo
                    select new MenuViewModel
                    {
                        Order = _provider.OrderKey,
                        IconSource = _provider.ProviderInfo,
                        Key = $@"{_actionGroup.Key}",
                        Header = _provider.ProviderInfo,
                        // sub-items are levels
                        SubItems = new ObservableCollection<MenuBaseViewModel>(
                            _actionGroup.Count() > 1
                            // multiple levels
                            ? (from _lvl in _actionGroup
                               orderby _lvl.Key.Level
                               select (new MenuViewModel
                               {
                                   Order = $@"~{_lvl.Key.Level:000#}",
                                   Key = $@"{_lvl.Key.ID}\({_lvl.Key.Level})",
                                   Header = new Info { Message = $@"Level {_lvl.Key.Level}" },
                                   // sub-sub levels are actions
                                   SubItems = new ObservableCollection<MenuBaseViewModel>(
                                       _lvl
                                       .OrderBy(_act => _act.OrderKey)
                                       .SelectMany(_act => GetActionMenuItems(_act, command, false))
                                       )
                               }) as MenuBaseViewModel)
                            // only 1 level
                            : _actionGroup.First()
                            .OrderBy(_act => _act.OrderKey)
                            .SelectMany(_act => GetActionMenuItems(_act, command, false))
                            )
                    });
        }
        #endregion

        private static MenuViewModel GetProviderContextMenu(ActorModel actor, ActionProviderInfo provider, List<ActionInfo> actions, List<AwarenessInfo> awarenesses)
            => new MenuViewModel
            {
                Order = provider.OrderKey,
                Key = $@"{provider.PresenterID}",
                Header = (object)AwarenessInfo.FindAwareness(awarenesses, provider.ID) ?? provider.ProviderInfo,
                SubItems = new ObservableCollection<MenuBaseViewModel>(
                            (from _act in actions
                             orderby _act.OrderKey
                             from _ami in ActionMenuBuilder.GetActionMenuItems(_act, actor.DoAction, false)
                             select _ami))
            };

        #region public static List<MenuBaseViewModel> GetContextMenu(ActorModel actor, List<AwarenessInfo> awarenesses)
        public static List<MenuBaseViewModel> GetContextMenu(ActorModel actor, List<AwarenessInfo> awarenesses)
        {
            AwarenessInfo _rootFinder(Guid presenterID)
               => awarenesses.FirstOrDefault(_a => _a.IsAnyAware(presenterID));

            // external, non-heads-up actions
            var _rootedActions = (from _act in actor.Actions
                                  where _act.IsExternalProvider && string.IsNullOrEmpty(_act.HeadsUpMode)
                                  // grouped by presenter
                                  group _act by _act.Provider.PresenterID
                                  into _actionGroup
                                  // make sure presenter is in selection
                                  let _provider = _actionGroup.FirstOrDefault(_a => _a.Provider.ID == _actionGroup.Key).Provider
                                  let _presenter = _rootFinder(_provider.PresenterID)
                                  where _presenter != null
                                  // group into presenters
                                  group (Provider: _provider, Actions: _actionGroup.ToList()) by _presenter
                                  into _root
                                  select (Presenter: _root.Key, Providers: _root.ToList()))
                                  .ToList();

            List<MenuBaseViewModel> _items = null;
            if (_rootedActions.Any())
            {
                if (_rootedActions.Count == 1)
                {
                    // single presenter selection
                    _items = (from _prov in _rootedActions.FirstOrDefault().Providers
                              select ActionMenuBuilder.GetProviderContextMenu(actor, _prov.Provider, _prov.Actions, awarenesses) as MenuBaseViewModel)
                              .ToList();
                }
                else
                {
                    // multiple presenter selection
                    _items = (from _ra in _rootedActions
                              orderby _ra.Presenter.Distance
                              select new MenuViewModel
                              {
                                  Order = $@"~~{_ra.Presenter.Distance:0000.00}",
                                  Key = $@"{_ra.Presenter.ID}",
                                  Header = _ra.Presenter,
                                  SubItems = new ObservableCollection<MenuBaseViewModel>(
                                      from _p in _ra.Providers
                                      select ActionMenuBuilder.GetProviderContextMenu(actor, _p.Provider, _p.Actions, awarenesses) as MenuBaseViewModel)
                              } as MenuBaseViewModel)
                              .ToList();
                }
            }
            return _items;
        }
        #endregion

        #region private static IEnumerable<MenuBaseViewModel> GetItemSelectionMenu(ObservableActor actor, Guid id)
        private static IEnumerable<MenuBaseViewModel> GetItemSelectionMenu(ObservableActor actor, Guid id)
        {
            if (id != Guid.Empty)
            {
                yield return new SeparatorViewModel { Order = @"~3100" };
                var _selected = actor.SelectedAwarenesses.Any(_sa => _sa.ID == id);
                yield return new MenuViewModel
                {
                    Order = @"~3110",
                    IsChecked = _selected,
                    Command = actor.DoFlipAwareness,
                    Parameter = id,
                    Header = $@"Target ({(_selected ? "unselect" : "exclusively")})",
                    ToolTip = _selected ? @"Unselect this item" : @"Target this item",
                    Key = $@"{id}.FlipSelection"
                };

                // more than one selected, or something other than this selected
                yield return new MenuViewModel
                {
                    Order = @"~3120",
                    Header = @"Target Include",
                    Command = actor.DoAddSelectAwareness,
                    Parameter = id,
                    ToolTip = @"Include in target selection",
                    Key = $@"{id}.IncludeSelection"
                };
            }
            yield break;
        }
        #endregion

        #region private static object GetPresenterCard(ActionProviderInfo provider)
        private static object GetPresenterCard(ActionProviderInfo provider)
            => (provider?.ProviderInfo is ObjectInfo)
            ? (object)new ObjectInfoVM
            {
                ObjectInfo = provider.ProviderInfo as ObjectInfo,
                IconVisibility = System.Windows.Visibility.Visible,
                TextVisibility = System.Windows.Visibility.Collapsed,
                Size = 48d
            }
            : provider.ProviderInfo;
        #endregion

        #region private static object GetPresenterCard(Info presenter)
        private static object GetPresenterCard(AwarenessInfo awareness)
            => (awareness.Info is ObjectInfo)
            ? (object)new ObjectInfoVM
            {
                ObjectInfo = awareness.Info as ObjectInfo,
                IconVisibility = System.Windows.Visibility.Visible,
                TextVisibility = System.Windows.Visibility.Collapsed,
                Size = 48d
            }
            : awareness;
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetItemsMenuItems(IEnumerable<ActionInfo> actions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetItemsMenuItems(ObservableActor actor,
            IEnumerable<ActionInfo> actions, ICommand command)
        {
            AwarenessInfo _rootFinder(Guid presenterID)
                => actor.Awarenesses.FirstOrDefault(_a => _a.IsAnyAware(presenterID));

            // all object rooted actions valid for top-menu
            var _rootedActions = (from _act in actions
                                  where (_act.Provider.ProviderInfo is ObjectInfo)
                                    && string.IsNullOrEmpty(_act.HeadsUpMode)
                                    && !_act.IsContextMenuOnly
                                  // grouped by presenter
                                  group _act by _act.Provider.PresenterID
                                  into _actionGroup
                                  // get provider and presenter
                                  let _provider = _actionGroup.FirstOrDefault(_a => _a.Provider.ID == _actionGroup.Key).Provider
                                  let _presenter = _rootFinder(_provider.PresenterID)
                                  // group into presenters
                                  group (Provider: _provider, Actions: _actionGroup.ToList()) by _presenter
                                  into _root
                                  select (Presenter: _root.Key, Providers: _root.ToList()))
                                  .ToList();

            MenuViewModel _getProviderMenu(ActionProviderInfo provider, List<ActionInfo> actions, bool isPresenter)
                => new MenuViewModel
                {
                    Order = provider.OrderKey,
                    Key = $@"{provider.PresenterID}",
                    Header = isPresenter
                        ? GetPresenterCard(provider)
                        : ((object)AwarenessInfo.FindAwareness(actor.Awarenesses, provider.ID) ?? provider.ProviderInfo),
                    SubItems = new ObservableCollection<MenuBaseViewModel>(
                                (from _act in actions
                                 orderby _act.OrderKey
                                 from _ami in GetActionMenuItems(_act, command, false)
                                 select _ami)
                                .Union(GetItemSelectionMenu(actor, (provider.ProviderInfo as ObjectInfo)?.ID ?? Guid.Empty)))
                };

            // if rooted to us, or unrooted, menu per provider
            foreach (var (_provider, _actions) in _rootedActions
                .Where(_ra => _ra.Presenter?.ID == actor.ActorID)
                .Concat(_rootedActions.Where(_ra => _ra.Presenter == null))
                .SelectMany(_ra => _ra.Providers))
            {
                yield return _getProviderMenu(_provider, _actions, true);
            }

            // if rooted to something else, menu per root, sub-menu per provider
            foreach (var (_presenter, _providers) in _rootedActions
                .Where(_ra => (_ra.Presenter != null) && (_ra.Presenter.ID != actor.ActorID))
                .OrderBy(_ra => _ra.Presenter.Distance))
            {
                yield return new MenuViewModel
                {
                    Order = $@"~~{_presenter.Distance:0000.00}",
                    Key = $@"{_presenter.ID}",
                    Header = GetPresenterCard(_presenter),
                    SubItems = new ObservableCollection<MenuBaseViewModel>((
                        from _p in _providers
                        select _getProviderMenu(_p.Provider, _p.Actions, false) as MenuBaseViewModel)
                        .Union(GetItemSelectionMenu(actor, _presenter.ID)))
                };
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetAdjunctMenuItems(ActorModel actor, IEnumerable<ActionInfo> actions, IEnumerable<ActionInfo> choiceActions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetAdjunctMenuItems(ActorModel actor, IEnumerable<ActionInfo> actions,
            IEnumerable<ActionInfo> choiceActions, ICommand command)
        {
            // exclude choices, timeline actions, power actions and object sourced actions...
            foreach (var _ami in actions
                .Where(_a => !choiceActions.Contains(_a)
                    && (_a.Provider.ProviderInfo is AdjunctInfo)
                    && !(_a.Provider.ProviderInfo is TimelineActionProviderInfo))
                .OrderBy(_a => _a.OrderKey)
                .SelectMany(_act => GetActionMenuItems(_act, command, true)))
            {
                yield return _ami;
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetCreatureMenuItems(ObservableActor actor, IEnumerable<ActionInfo> actions, IEnumerable<ActionInfo> choiceActions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetCreatureMenuItems(ObservableActor actor,
            IEnumerable<ActionInfo> actions, IEnumerable<ActionInfo> choiceActions, ICommand command)
        {
            yield return new MenuViewModel
            {
                Order = @"+",
                Key = actor.Actor.FulfillerID.ToString(),
                Header = actor.Actor,
                SubItems = new ObservableCollection<MenuBaseViewModel>(
                    GetPowerClassMenuItems(actions, command)
                    .Union(GetChoiceMenuItems(actor.Actor, choiceActions, command))
                    .Union(GetOtherMenuHeader(actor.Actor, actions, choiceActions, command))
                    .Union(GetSensorHostSelections(actor.Actor))
                    .Union(GetItemSelectionMenu(actor, actor.ActorID))
                    )
            };
            foreach (var _item in GetItemsMenuItems(actor, actions, command))
                yield return _item;
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetChoiceMenuItems(ActorModel actor, IEnumerable<ActionInfo> actions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetChoiceMenuItems(ActorModel actor, IEnumerable<ActionInfo> actions, ICommand command)
        {
            var _choices = actions
                .OrderBy(_act => _act.OrderKey)
                .SelectMany(_act => GetActionMenuItems(_act, command, false, true))
                .ToList();
            if (_choices.Any())
                yield return new MenuViewModel
                {
                    Order = @"~1",
                    Key = $@"CHOICES:{actor.FulfillerID}",
                    Header = @"Choices",
                    SubItems = new ObservableCollection<MenuBaseViewModel>(_choices)
                };
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetOtherMenuHeader(ActorModel actor, IEnumerable<ActionInfo> actions, IEnumerable<ActionInfo> choiceActions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetOtherMenuHeader(ActorModel actor, IEnumerable<ActionInfo> actions,
            IEnumerable<ActionInfo> choiceActions,
            ICommand command)
        {
            var _adj = GetAdjunctMenuItems(actor, actions, choiceActions, command).ToList();
            if (_adj.Any())
                yield return new MenuViewModel
                {
                    Order = @"~2",
                    Key = $@"OTHER:{actor.FulfillerID}",
                    Header = @"Other",
                    SubItems = new ObservableCollection<MenuBaseViewModel>(_adj)
                };
            yield break;
        }
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetSensorHostSelections(ActorModel actor)
        public static IEnumerable<MenuBaseViewModel> GetSensorHostSelections(ActorModel actor)
        {
            if (actor.ObservableActor?.SensorHosts?.Count() > 1)
            {
                // add selection if available
                yield return new MenuViewModel
                {
                    Order = @"~3000",
                    Key = $@"SENSORS:{actor.FulfillerID}",
                    Header = @"Sensors",
                    SubItems = new ObservableCollection<MenuBaseViewModel>(
                        actor.ObservableActor.SensorHosts.Select(_sh =>
                        new MenuViewModel
                        {
                            Command = actor.ObservableActor.DoSetSensors,
                            Parameter = _sh,
                            Header = $@"Sensor: {_sh.SensorHostName} ({_sh.ID})",
                            ToolTip = @"Observe from these sensors",
                            Key = $@"{_sh.ID}.SetSensors"
                        }).ToList())
                };
            }
            yield break;
        }
        #endregion

        #region public static IEnumerable<IFurnishingAction> GetFurnishingActions(IEnumerable<ActionInfo> actions)
        public static IEnumerable<IFurnishingAction> GetFurnishingActions(IEnumerable<ActionInfo> actions)
            => from _act in actions
               where !string.IsNullOrEmpty(_act.HeadsUpMode)
               select ((_act.Key == @"Furnishing.Pivot") || (_act.Key == @"Conveyance.Pivot"))
                        ? new PivotActionModel(_act)
                        : (_act.Key == @"Furnishing.Tilt")
                        ? new TiltActionModel(_act)
                        : (IFurnishingAction)new ReleaseActionModel(_act);
        #endregion

        #region public static IEnumerable<MenuBaseViewModel> GetTimelineMenuItems(IEnumerable<ActionInfo> actions, ICommand command)
        public static IEnumerable<MenuBaseViewModel> GetTimelineMenuItems(IEnumerable<ActionInfo> actions, ICommand command)
        {
            return (from _act in actions
                    where (_act.Provider.ProviderInfo is TimelineActionProviderInfo) || (_act.TimeType == TimeType.TimelineScheduling)
                    group _act by _act.Provider.ID
                    into _actionGroup
                    let _provider = actions.FirstOrDefault(_a => _a.Provider.ID == _actionGroup.Key).Provider
                    select new MenuViewModel
                    {
                        Order = _provider.OrderKey,
                        IconSource = _provider.ProviderInfo,
                        Key = $@"{_actionGroup.Key}",
                        Header = _provider.ProviderInfo,
                        // sub-levels are actions
                        SubItems = new ObservableCollection<MenuBaseViewModel>(
                            _actionGroup
                            .OrderBy(_act => _act.OrderKey)
                            .SelectMany(_act => GetActionMenuItems(_act, command, false))
                            )
                    });
        }
        #endregion

        #region public static void SyncMenus(ObservableCollection<MenuBaseViewModel> existing, IEnumerable<MenuBaseViewModel> current)
        public static void SyncMenus(ObservableCollection<MenuBaseViewModel> existing, IEnumerable<MenuBaseViewModel> current)
        {
            if (current == null)
            {
                existing?.Clear();
            }
            else
            {
                // remove interpolated separators
                foreach (var _sep in existing.OfType<SeparatorViewModel>()
                    .Where(_svm => string.IsNullOrEmpty(_svm.Order))
                    .ToList())
                {
                    existing.Remove(_sep);
                }

                // remove existing not in current
                foreach (var _rmv in (from _e in existing
                                      where !current.Any(_c => _c.Key == _e.Key)
                                      select _e).ToList())
                    existing.Remove(_rmv);

                // update existing
                foreach (var _updt in (from _e in existing
                                       join _c in current
                                       on _e.Key equals _c.Key
                                       select new { Exist = _e, Current = _c }).ToList())
                {
                    if ((_updt.Exist is MenuViewModel) && (_updt.Current is MenuViewModel))
                    {
                        var _exist = _updt.Exist as MenuViewModel;
                        var _curr = _updt.Current as MenuViewModel;
                        _exist.Header = _curr.Header;
                        _exist.IsChecked = _curr.IsChecked;
                        _exist.Command = _curr.Command;
                        _exist.Parameter = _curr.Parameter;
                        _exist.ToolTip = _curr.ToolTip;
                        SyncMenus(_exist.SubItems, _curr.SubItems);
                    }
                }

                // add current not in existing
                foreach (var _add in (from _c in current
                                      where !existing.Any(_e => _e.Key == _c.Key)
                                      select _c).ToList())
                {
                    var _next = existing.FirstOrDefault(_m => string.Compare(_m.Order, _add.Order, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (_next == null)
                    {
                        existing.Add(_add);
                    }
                    else
                    {
                        existing.Insert(existing.IndexOf(_next), _add);
                    }
                }

                // add interpolated separators
                if (existing.OfType<MenuViewModel>().Any(_e => _e.IsAction))
                {
                    var _groups = existing.OfType<MenuViewModel>()
                        .Where(_mvm => _mvm.IsAction)
                        .GroupBy(_mvm => _mvm.Order[0])
                        .ToList();
                    foreach (var _g in _groups.Skip(1))
                    {
                        var _first = _g.First();
                        var _idx = existing.IndexOf(_first);
                        existing.Insert(_idx, new SeparatorViewModel());
                    }
                }
            }
        }
        #endregion
    }
}
