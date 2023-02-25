using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Contracts.Faults;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Contracts.Host;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Services
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext = false
        )]
    public class MasterServices : IMasterControl
    {
        #region static data
        private static ConcurrentDictionary<string, IMasterControlCallback> _Callbacks =
            new ConcurrentDictionary<string, IMasterControlCallback>();
        #endregion

        // hooks
        public static Action OnShutdown { get; set; }
        public static Action<FlowState> OnFlowStateChanged { get; set; }
        public static Action<bool> OnPauseChanged { get; set; }
        public static Action<ConsoleMessage> Console { get; set; }
        public static MapContext MapContext { get; set; }

        public static ReaderWriterLockSlim Synchronizer => MapContext?.Map.Synchronizer;
        public static ICreatureProvider CreatureProvider => MapContext?.Map;
        public static IkosaProcessManager ProcessManager => MapContext?.ContextSet.ProcessManager as IkosaProcessManager;

        #region private static void ConsoleLog(string title, string userName, string details)
        private static void ConsoleLog(string title, string userName, string details)
            => Console?.Invoke(new ConsoleMessage
            {
                Title = title,
                Message = $@"User: '{userName}' errored out.",
                Details = details,
                Source = typeof(LoginService)
            });
        #endregion

        #region public void RegisterCallback()
        public void RegisterCallback()
        {
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                if (_principal.IsInRole(@"Master"))
                {
                    // gather parameters
                    var _tracker = OperationContext.Current.GetCallbackChannel<IMasterControlCallback>();

                    // add or update
                    _Callbacks.AddOrUpdate(_principal.Identity.Name, _tracker,
                        (name, cBack) =>
                        {
                            return _tracker;
                        });
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
            }
        }
        #endregion

        #region public void DeRegisterCallback()
        public void DeRegisterCallback()
        {
            var _principal = Thread.CurrentPrincipal;
            _Callbacks.TryRemove(_principal.Identity.Name, out _);
        }
        #endregion

        public static void DeRegisterCallback(string userName)
        {
            _Callbacks.TryRemove(userName, out _);
        }

        #region public void KickUser(string user)
        public void KickUser(string user)
        {
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                if (_principal.IsInRole(@"Master"))
                {
                    LoginService.Logout(user);
                }
                else
                {
                    ConsoleLog(@"Authorization Failure", _principal.Identity.Name, @"User not authorized");
                    throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
                }
            }
            else
            {
                ConsoleLog(@"Login Failure", _principal?.Identity.Name, @"User not authenticated");
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authenticated");
            }
        }
        #endregion

        #region public void SetUserDisabled(string user, bool isDisabled)
        public void SetUserDisabled(string user, bool isDisabled)
        {
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                if (_principal.IsInRole(@"Master"))
                {
                    // get and edit user
                    var _user = UserValidator.UserDefinitions.GetUser(user);
                    if (_user != null)
                        _user.IsDisabled = isDisabled;

                    // kick if disabled now
                    if (isDisabled)
                        LoginService.Logout(user);
                }
                else
                {
                    ConsoleLog(@"Authorization Failure", _principal.Identity.Name, @"User not authorized");
                    throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
                }
            }
            else
            {
                ConsoleLog(@"Login Failure", _principal?.Identity.Name, @"User not authenticated");
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authenticated");
            }
        }
        #endregion

        // read lock section

        #region public List<CreatureLoginInfo> GetAdvancementCreatures(bool isAdvancing)
        public List<CreatureLoginInfo> GetAdvancementCreatures(bool isAdvancing)
        {
            var _critterList = MapContext.CreatureLoginsInfos
                .OrderBy(_ci => _ci.Name)
                .ToList();
            using var _lock = new SecureReadLockHolder(Synchronizer, @"Master");
            return _critterList
                .Where(_cli => (!isAdvancing) ^ (MapContext?.Map.GetCreature(_cli.ID)?.HasAdjunct<AdvancementCapacity>() ?? false))
                .ToList();
        }
        #endregion

        #region public List<TeamGroupInfo> GetTeams()
        public List<TeamGroupInfo> GetTeams()
        {
            var _userList = LoginService.GetLoggedInUserList();

            // CreatureTrackerInfo[] for a team-group (either primary of associate)
            CreatureTrackerInfo[] _getCreatureTeams(TeamGroup teamGroup, bool isPrimary)
                => (from _member in teamGroup.TeamMembers
                    where _member.IsPrimary == isPrimary
                    let _critter = _member.Creature
                    let _critterLogin = _critter.ToCreatureLoginInfo()
                    orderby _critter.Name
                    select new CreatureTrackerInfo
                    {
                        CreatureLoginInfo = _critterLogin,
                        UserInfos = (from _user in _userList
                                     where _user.CreatureInfos.Any(_c => _c.ID == _critter.ID)
                                     orderby _user.UserName
                                     select new UserInfo
                                     {
                                         UserName = _user.UserName,
                                         CreatureInfos = new[] { _critterLogin },
                                         IsDisabled = _user.IsDisabled,
                                         IsMaster = _user.IsMaster
                                     }).ToArray(),
                        LocalActionBudgetInfo = _critter.GetLocalActionBudget()?.ToLocalActionBudgetInfo(null)
                    }).ToArray();

            using var _lock = new SecureReadLockHolder(Synchronizer, @"Master");
            return (from _group in MapContext.ContextSet.AdjunctGroups.All().OfType<TeamGroup>()
                    orderby _group.Name
                    select new TeamGroupInfo
                    {
                        ID = _group.ID,
                        Name = _group.Name,
                        PrimaryCreatures = _getCreatureTeams(_group, true),
                        AssociateCreatures = _getCreatureTeams(_group, false)
                    }).ToList();
        }
        #endregion

        #region public List<StandDownGroupInfo> GetStandDownGroups()
        public List<StandDownGroupInfo> GetStandDownGroups()
        {
            using var _lock = new SecureReadLockHolder(Synchronizer, @"Master");
            var _critters = MapContext.CreatureLoginsInfos.ToDictionary(_cli => _cli.ID);
            return MapContext?.ContextSet.AdjunctGroups.All().OfType<StandDownGroup>()
                .Select(_sdg => new StandDownGroupInfo
                {
                    Guid = _sdg.ID,
                    GroupName = _sdg.Name,
                    Creatures = (from _p in _sdg.StandDownGroupParticipants
                                 where _critters.ContainsKey(_p.Anchor.ID)
                                 select _critters[_p.Anchor.ID]).ToList()
                })
                .ToList() ?? new List<StandDownGroupInfo>();
        }
        #endregion

        // write lock section

        #region public void AllowCreatureAdvancement(Guid id)
        public void AllowCreatureAdvancement(Guid id)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = GetUpdateSerialState();
            var _critter = CreatureProvider.GetCreature(id);
            if ((_critter != null) && !_critter.HasAdjunct<AdvancementCapacity>())
            {
                var _ctxSet = MapContext?.ContextSet;
                if (_ctxSet != null)
                {
                    // add to advancement capacity
                    _critter.AddAdjunct(
                            new AdvancementCapacity(
                                // find or make capacity
                                _ctxSet?.AdjunctGroups.All().OfType<AdvancementCapacityGroup>().FirstOrDefault()
                                ?? new AdvancementCapacityGroup()));
                }
            }
            IkosaServices.DoNotifySerialState(_serial);
        }
        #endregion

        #region public void RevokeCreatureAdvancement(Guid id)
        public void RevokeCreatureAdvancement(Guid id)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = GetUpdateSerialState();
            CreatureProvider.GetCreature(id)?.Adjuncts.EjectAll<AdvancementCapacity>();
            IkosaServices.DoNotifySerialState(_serial);
        }
        #endregion

        private static bool IsTopStep<StepType>()
            where StepType : CoreStep
            => ProcessManager?.CurrentStep is StepType;

        private static bool HasCoreStep<StepType>()
            where StepType : CoreStep
            => ProcessManager?.AllProcesses.Any(_proc => _proc.GetCurrentStep() is StepType) ?? false;

        /// <summary>Pause if not already paused</summary>
        private void DoPause()
        {
            if (!IsTopStep<PauseStep>())
            {
                // pause process
                ProcessManager?.StartProcess(new CoreProcess(new PauseStep(), @"Pause"));
            }
            OnPauseChanged?.Invoke(true);
        }

        public static FlowState GetFlowState()
        {
            if (HasCoreStep<AdvancementStep>())
                return FlowState.Advancement;
            return FlowState.Normal;
        }

        public static bool GetPauseState()
            => (IsTopStep<PauseStep>());

        #region private void RemoveProcess<StepType>()
        /// <summary>If there's a process with this step, remove any outer pause, then the process</summary>
        private void RemoveProcess<StepType>()
            where StepType : CoreStep, IStoppableStep
        {
            if (HasCoreStep<StepType>())
            {
                // if StepType is paused, remove the pause
                if (ProcessManager?.CurrentStep is PauseStep _pStep)
                {
                    // release the step
                    _pStep.StopStep();
                    _pStep.Process.ProcessManager.DoProcessAll();
                }

                // unpause if topmost step
                if (ProcessManager?.CurrentStep is StepType _step)
                {
                    // release the step
                    _step.StopStep();
                    _step.Process.ProcessManager.DoProcessAll();
                }
            }
        }
        #endregion

        #region public void SetFlowState(FlowState flowState)
        public void SetFlowState(FlowState flowState)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = MapContext?.SerialState ?? 0;
            switch (flowState)
            {
                case FlowState.Normal:
                    RemoveProcess<AdvancementStep>();
                    OnFlowStateChanged?.Invoke(flowState);
                    OnPauseChanged?.Invoke(GetPauseState());
                    break;

                case FlowState.Advancement:
                    // must have advancement capacity
                    if (MapContext?.ContextSet.GetCoreIndex()
                        .Select(_idx => _idx.Value).OfType<Creature>()
                        .Any(_c => _c.HasAdjunct<AdvancementCapacity>()) ?? false)
                    {
                        if (!HasCoreStep<AdvancementStep>())
                        {
                            // pause
                            DoPause();

                            // add advancement
                            ProcessManager?.StartProcess(
                                new CoreProcess(new AdvancementStep(MapContext?.ContextSet), @"Advancement"));
                        }

                        OnFlowStateChanged?.Invoke(flowState);
                        OnPauseChanged?.Invoke(GetPauseState());
                    }
                    break;

                case FlowState.Shutdown:
                default:
                    OnFlowStateChanged?.Invoke(flowState);
                    OnPauseChanged?.Invoke(GetPauseState());
                    OnShutdown?.Invoke();
                    break;
            }

            // pump some processing
            ProcessManager.DoProcessAll();
            IkosaServices.DoNotifySerialState(_serial);
        }
        #endregion

        #region public void SetPause(bool isPaused)
        public void SetPause(bool isPaused)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = MapContext?.SerialState ?? 0;
            if (isPaused)
            {
                // pause process
                DoPause();
            }
            else
            {
                // unpause if topmost step
                if (ProcessManager?.CurrentStep is PauseStep _pStep)
                {
                    // release the pause
                    _pStep.StopStep();
                    OnPauseChanged?.Invoke(false);
                }
            }

            // pump some processing
            ProcessManager.DoProcessAll();
            IkosaServices.DoNotifySerialState(_serial);
        }
        #endregion

        #region public void SetIsTimeTickAuto(bool isAuto)
        public void SetIsTimeTickAuto(bool isAuto)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = GetUpdateSerialState();
            if (ProcessManager?.LocalTurnTracker is LocalTurnTracker _tracker)
            {
                _tracker.IsAutoTimeTick = isAuto;
            }
            IkosaServices.DoNotifySerialState(_serial);
        }
        #endregion

        #region public bool PushTimeTick()
        public bool PushTimeTick()
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = MapContext?.SerialState ?? 0;
            if (ProcessManager?.CurrentStep is LocalTimeStep _timeStep)
            {
                _timeStep.TimeTickablePrerequisite?.PushForward();
                ProcessManager.DoProcessAll();
                IkosaServices.DoNotifySerialState(_serial);
                return true;
            }
            else if (ProcessManager?.CurrentStep is RoundMarkerTickStep _roundMarker)
            {
                _roundMarker.RoundMarkerCompletePrerequisite?.PushForward();
                ProcessManager.DoProcessAll();
                IkosaServices.DoNotifySerialState(_serial);
                return true;
            }
            return false;
        }
        #endregion

        #region public StandDownGroupInfo AddToGroup(Guid standDowngroupID, Guid[] creatures)
        public StandDownGroupInfo AddToStandDownGroup(Guid standDowngroupID, string groupName, Guid[] creatures)
        {
            using (var _lock = new SecureWriteLockHolder(Synchronizer, @"Master"))
            {
                var _group = MapContext?.ContextSet.AdjunctGroups.All().OfType<StandDownGroup>()
                    .FirstOrDefault(_g => _g.ID == standDowngroupID);
                if ((_group != null)
                    || ((standDowngroupID == Guid.Empty) && !string.IsNullOrWhiteSpace(groupName)))
                {
                    var _serial = GetUpdateSerialState();
                    if (_group == null)
                    {
                        // new group
                        _group = new StandDownGroup(groupName);
                    }

                    foreach (var _c in creatures)
                    {
                        // make sure not already in group
                        if (!_group.StandDownGroupParticipants.Any(_p => _p.Anchor.ID == _c))
                        {
                            // make sure creature exists
                            var _critter = CreatureProvider?.GetCreature(_c);
                            if (_critter != null)
                            {
                                // add
                                _critter.AddAdjunct(new StandDownGroupParticipant(_group));
                            }
                        }
                    }

                    // return
                    IkosaServices.DoNotifySerialState(_serial);
                    var _critters = MapContext.CreatureLoginsInfos.ToDictionary(_cli => _cli.ID);
                    return new StandDownGroupInfo
                    {
                        Guid = _group.ID,
                        GroupName = _group.Name,
                        Creatures = (from _p in _group.StandDownGroupParticipants
                                     where _critters.ContainsKey(_p.Anchor.ID)
                                     select _critters[_p.Anchor.ID]).ToList()
                    };
                }
                return null;
            };
        }
        #endregion

        #region public StandDownGroupInfo RemoveFromGroup(Guid standDowngroupID, Guid[] creatures)
        public StandDownGroupInfo RemoveFromStandDownGroup(Guid standDowngroupID, Guid[] creatures)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _group = MapContext?.ContextSet.AdjunctGroups.All().OfType<StandDownGroup>()
.FirstOrDefault(_g => _g.ID == standDowngroupID);
            if (_group != null)
            {
                var _serial = GetUpdateSerialState();
                if (creatures.Any())
                {
                    // eject all participants contained in creatures
                    (from _p in _group.StandDownGroupParticipants
                     join _c in creatures
                     on _p.Anchor.ID equals _c
                     select _p)
                     .Distinct()
                     .ToList()
                     .ForEach(_rmv => _rmv.Eject());
                }
                else
                {
                    // no creatures list, so eject all
                    _group.StandDownGroupParticipants.ToList().ForEach(_rmv => _rmv.Eject());
                }

                IkosaServices.DoNotifySerialState(_serial);
                if (_group.Members.Any())
                {
                    // return
                    var _critters = MapContext.CreatureLoginsInfos.ToDictionary(_cli => _cli.ID);
                    return new StandDownGroupInfo
                    {
                        Guid = _group.ID,
                        GroupName = _group.Name,
                        Creatures = (from _p in _group.StandDownGroupParticipants
                                     where _critters.ContainsKey(_p.Anchor.ID)
                                     select _critters[_p.Anchor.ID]).ToList()
                    };
                }
            }
            return null;
        }
        #endregion

        // ----- turn-tracker controls -----

        #region public void TurnTrackerStart(Guid[] creatures)
        public void TurnTrackerStart(Guid[] creatures)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            if (!(ProcessManager?.LocalTurnTracker?.IsInitiative ?? true)
                && (ProcessManager.LocalTurnTracker.GetTrackerStep<PromptTurnTrackerStep>() is PromptTurnTrackerStep _prompt))
            {
                // prompt complete...
                var _serial = GetUpdateSerialState();
                _prompt.PromptTurnTrackerPrerequisite.Done = true;

                // get creatures
                var _critters = (from _c in creatures
                                 let _crit = CreatureProvider?.GetCreature(_c)
                                 where _crit != null
                                 select _crit).ToList();

                // start turn tracker
                new LocalTurnTracker(_critters, MapContext?.ContextSet, true);

                ProcessManager?.DoProcessAll();
                IkosaServices.DoNotifySerialState(_serial);
            }
        }
        #endregion

        #region public void TurnTrackerAdd(Guid[] creatures)
        public void TurnTrackerAdd(Guid[] creatures)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            if ((ProcessManager?.LocalTurnTracker is LocalTurnTracker _tracker)
                && _tracker.IsInitiative
                && (_tracker.GetTrackerStep<NeedsTurnTickStep>() is NeedsTurnTickStep))
            {
                var _roundTick = _tracker.RoundMarker;
                if (_roundTick == _tracker.LeadingTick)
                {
                    // build candidate budgets list
                    var _serial = GetUpdateSerialState();
                    var _candidates = (from _id in creatures
                                       let _budget = _tracker.GetBudget(_id)
                                       let _critter = CreatureProvider?.GetCreature(_id)
                                       where _critter != null
                                       select (Budget: _budget, Transfer: _budget?.TurnTick == _roundTick, Creature: _critter)).ToList();

                    var _current = _tracker.TickOrderedBudgets
                        .Where(_b => _b.IsInitiative)
                        .Select(_b => _b.Creature.ID)
                        .ToList();
                    var _testOrder = _candidates
                        .Where(_test => _current.Any(_c => _c == _test.Creature.ID))
                        .Select(_test => _test.Creature.ID)
                        .ToList();
                    if (_current.Count != _testOrder.Count)
                    {
                        // didn't find in remainder of list, won't work...
                        throw new FaultException<InvalidArgumentFault>(
                            new InvalidArgumentFault(nameof(creatures)),
                            @"Invalid sequence of creatures (count mismatch)");
                    }
                    for (var _tx = 0; _tx < _current.Count; _tx++)
                    {
                        if (_current[_tx] != _testOrder[_tx])
                            throw new FaultException<InvalidArgumentFault>(
                                new InvalidArgumentFault(nameof(creatures)),
                                $@"Invalid sequence of creatures (@[{_tx}]: {_current[_tx]} != {_testOrder[_tx]})");
                    }

                    // group candidates into add groups...
                    var _addGroups = new List<(double startTime, LinkedListNode<CoreTurnTick> tailTick, List<(LocalActionBudget _budget, Creature _critter)> critters)>();
                    var _gather = new List<(LocalActionBudget _budget, Creature _critter)>();
                    foreach (var (_budget, _transfer, _critter) in _candidates)
                    {
                        if ((_budget == null) || (_transfer))
                        {
                            // new or transfer budget
                            _gather.Add((_budget, _critter));
                        }
                        else
                        {
                            // reached an existing initiative budget...
                            if (_gather.Count != 0)
                            {
                                var _tail = _tracker.Ticks.Find(_budget.TurnTick);
                                var _head = _tail.Previous.Value as LocalTurnTick;
                                var _save = new List<(LocalActionBudget _budget, Creature _critter)>(_gather);
                                _addGroups.Add((_head.Time, _tail, _save));
                                _gather.Clear();
                            }
                        }
                    }
                    if (_gather.Count != 0)
                    {
                        // last gathering group
                        var _save = new List<(LocalActionBudget _budget, Creature _critter)>(_gather);
                        _addGroups.Add(((_tracker.Ticks.Last.Value as LocalTurnTick).Time, _tracker.Ticks.Find(_tracker.RoundMarker), _save));
                    }

                    // add the adders
                    foreach (var (_startTime, _tailTick, _critters) in _addGroups)
                    {
                        var _atEnd = _tailTick.Value == _tracker.RoundMarker;
                        var _endTime = _atEnd
                            ? _tracker.RoundMarker.Time + Round.UnitFactor
                            : ((_tailTick.Value as LocalTurnTick)?.Time ?? (_tracker.RoundMarker.Time + Round.UnitFactor));
                        var _duration = _startTime - _endTime;
                        var _timeStep = _duration / _critters.Count;
                        var _nextTime = _startTime;
                        foreach (var (_budget, _critter) in _critters)
                        {
                            // add to "head" end of List keep popping them 
                            var _newTick = _atEnd
                                ? new LocalTurnTick(_nextTime, _tracker, null, null)
                                : new LocalTurnTick(_nextTime, _tracker, null, _tailTick);
                            if (_budget != null)
                            {
                                _budget.TurnTick = _newTick;
                                _budget.BudgetItems.Remove(typeof(NeedsTurnTick));
                            }
                            else
                            {
                                _critter.CreateActionBudget(_newTick);
                            }

                            // step time forward
                            _nextTime += _timeStep;
                        }
                    }
                    ProcessManager?.DoProcessAll();
                    IkosaServices.DoNotifySerialState(_serial);
                }
            }
        }
        #endregion

        #region public void TurnTrackerDrop(Guid[] creatures)
        public void TurnTrackerDrop(Guid[] creatures)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            // get creatures
            var _serial = GetUpdateSerialState();
            var _critters = (from _c in creatures
                             let _crit = CreatureProvider?.GetCreature(_c)
                             where _crit != null && !LoginService.HasUser(_c)
                             select _crit).ToList();

            // any dead creatures can obviously be removed (if desired)
            foreach (var _critter in _critters.Where(_c => _c.HasAdjunct<DeadEffect>()).ToList())
            {
                if (_critter.GetLocalActionBudget() is LocalActionBudget _budget)
                {
                    ProcessManager.LocalTurnTracker.RemoveBudget(_budget);
                }
                _critters.Remove(_critter);
            }

            // no remaining creatures in the turn tracker are aware of the creature
            var _remaining = ProcessManager.LocalTurnTracker
                .TimeOrderedBudgets.Select(_b => _b.Creature)
                .Except(_critters).ToDictionary(_c => _c.ID);
            foreach (var _critter in _critters)
            {
                // creature is not aware of anything unfriendly
                if (!_critter.Awarenesses.UnFriendlyAwarenesses.Any()
                    // and nothing left in tracker is aware in anyway and unfriendly towards the creature
                    && !_remaining.Any(_other => (_other.Value.Awarenesses[_critter.ID] >= Senses.AwarenessLevel.Presence) && _other.Value.IsUnfriendly(_critter.ID)))
                {
                    if (_critter.GetLocalActionBudget() is LocalActionBudget _budget)
                    {
                        ProcessManager.LocalTurnTracker.RemoveBudget(_budget);
                    }
                }
            }
            ProcessManager?.DoProcessAll();
            IkosaServices.DoNotifySerialState(_serial);
        }
        #endregion

        #region public void TurnTrackerStop(bool standDownGroupName)
        public void TurnTrackerStop(string standDownGroupName)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            if (ProcessManager?.LocalTurnTracker?.IsInitiative ?? false)
            {
                var _serial = GetUpdateSerialState();
                if (!string.IsNullOrWhiteSpace(standDownGroupName))
                {
                    // create stand-down group so prompting doesn't kick in immediately if unfriendlies still in play
                    var _standDown = new StandDownGroup(standDownGroupName);
                    foreach (var _budget in ProcessManager.LocalTurnTracker.TimeOrderedBudgets)
                    {
                        _budget.Creature.AddAdjunct(new StandDownGroupParticipant(_standDown));
                    }
                }

                // then shutdown tracker
                ProcessManager?.PopTracker();
                ProcessManager?.DoProcessAll();
                IkosaServices.DoNotifySerialState(_serial);
            }
        }
        #endregion

        // ----- needs-turn-tick control -----
        public void AddNeedsTurnTick(Guid creatureID)
        {
            using var _lock = new SecureWriteLockHolder(Synchronizer, @"Master");
            var _serial = GetUpdateSerialState();
            var _critter = CreatureProvider?.GetCreature(creatureID);
            if (_critter != null)
            {
                NeedsTurnTick.TryBindToCreature(_critter, true);
                IkosaServices.DoNotifySerialState(_serial);
            }
        }

        public ulong GetUpdateSerialState()
        {
            if (MapContext != null)
            {
                var _serial = MapContext.SerialState;
                MapContext.SerialState++;
                return _serial;
            }
            return 0;
        }
    }
}
