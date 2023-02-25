using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using System.Collections.Concurrent;
using System.ComponentModel;
using Uzi.Ikosa.Senses;
using System.Threading.Tasks;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items;
using Uzi.Core.Dice;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts.Faults;
using System.Diagnostics;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Skills;
using Uzi.Visualize;
using System.Threading.Tasks.Dataflow;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Feats;

namespace Uzi.Ikosa.Services
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single,
        UseSynchronizationContext = false
        )]
    public class IkosaServices : IIkosaCombinedServices
    {
        // TODO: fully destatic?

        #region repeater
        static ITargetBlock<DateTimeOffset> RepeatTask(
            Action action, CancellationToken cancellationToken)
        {
            // Validate
            if (action == null) throw new ArgumentNullException(nameof(action));

            // Declare the block variable, it needs to be captured.
            ActionBlock<DateTimeOffset> _block = null;

            // Create block, it will call itself, so separate declaration and assignment.
            // Async so you can wait easily when the delay comes.
            _block = new ActionBlock<DateTimeOffset>(async now =>
            {
                // TODO: disable repeat task while saving/re-loading
                if (!cancellationToken.IsCancellationRequested)
                {
                    // Perform the action.
                    action();

                    // Wait.
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1)).
                                // Doing this here because synchronization context more than
                                // likely *doesn't* need to be captured for the continuation
                                // here.  As a matter of fact, that would be downright dangerous.
                                ConfigureAwait(false);

                            if (!cancellationToken.IsCancellationRequested)
                            {
                                // Post the action back to the block.
                                _block.Post(DateTimeOffset.Now);
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // OK, just cancel
                    }
                }
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            // Return the block.
            return _block;
        }
        #endregion

        #region static data
        private static ConcurrentDictionary<string, IkosaCallbackTracker> _Callbacks =
            new ConcurrentDictionary<string, IkosaCallbackTracker>();
        private static ConcurrentQueue<Tuple<Guid[], SysNotify>> _StatusNotify =
            new ConcurrentQueue<Tuple<Guid[], SysNotify>>();
        private static ConcurrentQueue<Guid> _PreReqNotify = new ConcurrentQueue<Guid>();
        private static CancellationTokenSource _TokenSource;
        private static ITargetBlock<DateTimeOffset> _Task;
        private static MapContext _Context;
        #endregion

        #region public static MapContext MapContext { get; set; }
        public static MapContext MapContext
        {
            get => _Context;
            set
            {
                _Context = value;
                IkosaStatics.CreatureProvider = _Context?.Map;
            }
        }
        #endregion

        public static ReaderWriterLockSlim Synchronizer => MapContext?.Map.Synchronizer;
        public static ICreatureProvider CreatureProvider => MapContext?.Map;

        public static Action<ConsoleMessage> Console { get; set; }

        #region public static IInteractProvider InteractProvider { get; set; }
        public static IInteractProvider InteractProvider
        {
            get => IkosaStatics.InteractProvider;
            set => IkosaStatics.InteractProvider = value;
        }
        #endregion

        #region public static IkosaProcessManager ProcessManager { get; set; }
        public static IkosaProcessManager ProcessManager
        {
            get => IkosaStatics.ProcessManager;
            set
            {
                // remove old event handling
                if (IkosaStatics.ProcessManager != null)
                {
                    IkosaStatics.ProcessManager.NewPrerequisiteActors -= _Manager_NewPrerequisiteActors;
                    IkosaStatics.ProcessManager.SysStatusAvailable -= _Manager_SysStatusComplete;
                    //IkosaStatics.ProcessManager.CurrentCoreProcess -= ProcessManager_CurrentCoreProcess;

                    // cleanup DoProcess pulser
                    using (_TokenSource)
                    {
                        _TokenSource?.Cancel();
                    }
                    _TokenSource = null;
                    _Task = null;
                }

                IkosaStatics.ProcessManager = value;

                // add new event handling
                if (IkosaStatics.ProcessManager != null)
                {
                    IkosaStatics.ProcessManager.NewPrerequisiteActors += _Manager_NewPrerequisiteActors;
                    IkosaStatics.ProcessManager.SysStatusAvailable += _Manager_SysStatusComplete;
                    //IkosaStatics.ProcessManager.CurrentCoreProcess += ProcessManager_CurrentCoreProcess;

                    // DoProcess pulser
                    _TokenSource = new CancellationTokenSource();
                    _Task = RepeatTask(() =>
                    {
                        // assume won't process
                        var _processed = false;
                        var _serial = GetUpdateSerialState();
                        try
                        {
                            if (Synchronizer?.TryEnterWriteLock(0) ?? false)
                            {
                                // can process happen?
                                if ((ProcessManager?.CurrentStep as LocalTimeStep)?.TimeTickablePrerequisite?.IsReady ?? false)
                                {
                                    // yes!
                                    while (ProcessManager?.DoProcessAll() ?? false) ;
                                    _processed = true;
                                }
                            }
                        }
                        finally
                        {
                            // false if write-lock failed
                            if (Synchronizer?.IsWriteLockHeld ?? false)
                            {
                                // did process?
                                if (_processed)
                                {
                                    DoNotifySerialState(_serial);
                                }
                                Synchronizer?.ExitWriteLock();
                            }
                        }
                    }, _TokenSource.Token);
                    _Task.Post(DateTimeOffset.Now);
                }
            }
        }
        #endregion

        #region private static void ProcessManager_CurrentCoreProcess(object sender, ProcessEventArgs e)
        private static void ProcessManager_CurrentCoreProcess(object sender, ProcessEventArgs e)
        {
            // TODO: evaluate whether unlocking here is "safe"?
            var _locked = Synchronizer.IsWriteLockHeld;
            try
            {
                // if locked when flushing, allow readers to sweep through
                if (_locked)
                    Synchronizer.ExitWriteLock();

                FlushNotifySysStatus();
            }
            finally
            {
                // re-aquire lock if previously held
                if (_locked)
                    Synchronizer.EnterWriteLock();
            }
        }
        #endregion

        #region static void _Manager_SysStatusComplete(object sender, SysStatusEventArgs e)
        static void _Manager_SysStatusComplete(object sender, SysStatusEventArgs e)
        {
            QueueNotifySysStatus(e.SysStatus, e.Targets);
        }
        #endregion

        #region static void _Manager_NewPrerequisiteActors(object sender, PrerequisiteActorsEventArgs e)
        static void _Manager_NewPrerequisiteActors(object sender, PrerequisiteActorsEventArgs e)
        {
            QueueNotifyPrerequisite(e.Actors);
        }
        #endregion

        #region private static void WriteConsole(string title, string message = null, string details = null)
        private static void WriteConsole(string title, string message = null, string details = null)
        {
            Console?.Invoke(new ConsoleMessage
            {
                Title = $@"[{Thread.CurrentPrincipal.Identity.Name}] {title}",
                Message = $@"[{DateTime.Now}] {message}",
                Details = details,
                Source = typeof(IkosaServices)
            });
        }
        #endregion

        #region private static void ConsoleLog(string title, string id, string details)
        private static void ConsoleLog(string title, string id, string details)
        {
            Console?.Invoke(new ConsoleMessage
            {
                Title = title,
                Message = string.Format(@"ID: '{0}' errored out.", id),
                Details = details,
                Source = typeof(IkosaServices)
            });
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
                throw new FaultException<InvalidArgumentFault>(
                    new InvalidArgumentFault(nameof(id)),
                    $@"Unable to coerce value [{id}] to a Guid");
            }
        }
        #endregion

        // lock-free

        #region public static void FlushNotifications()
        public static void FlushNotifications()
        {
            FlushNotifySysStatus();
            SignalWaitingOn();
        }
        #endregion

        #region public static void QueueNotifyPrerequisite(IEnumerable<Guid> targets)
        public static void QueueNotifyPrerequisite(IEnumerable<Guid> targets)
        {
            if ((targets != null) && targets.Any())
            {
                Parallel.ForEach(targets, _g => _PreReqNotify.Enqueue(_g));
            }
        }
        #endregion

        #region private static void SignalWaitingOn()
        private static void SignalWaitingOn()
        {
            // notify everyone who we are waiting on
            if (ProcessManager.CurrentStep != null)
            {
                var _waitOn = (from _sp in ProcessManager.CurrentStep.AllPrerequisites<StepPrerequisite>()
                               where !_sp.IsReady && _sp.Fulfiller != null
                               from _c in _Callbacks
                               where _c.Value.IDs.Contains(_sp.Fulfiller.ID)
                               orderby _c.Key
                               select _c.Key).Distinct().ToList();
                DoNotifyWaiting(_waitOn);
            }
        }
        #endregion

        #region public static void QueueNotifySysStatus(SysStatus status, IEnumerable<Guid> targets)
        public static void QueueNotifySysStatus(SysNotify status, IEnumerable<Guid> targets)
        {
            if ((targets != null) && targets.Any())
            {
                _StatusNotify.Enqueue(new Tuple<Guid[], SysNotify>(targets.ToArray(), status));
            }
        }
        #endregion

        #region private static void FlushNotifySysStatus()
        private static void FlushNotifySysStatus()
        {
            // build lists of statuses for each notified
            var _gather = new Dictionary<Guid, List<SysNotify>>();
            while (_StatusNotify.TryDequeue(out Tuple<Guid[], SysNotify> _notify))
            {
                // for each notifier in this particular status
                foreach (var _id in _notify.Item1.Union(Guid.Empty.ToEnumerable())) // always send to master
                {
                    // create a list if none already gathering
                    if (!_gather.ContainsKey(_id))
                        _gather.Add(_id, new List<SysNotify>());

                    // add to list
                    var _list = _gather[_id];
                    _list.Add(_notify.Item2);
                }
            }

            // get callbacks
            var _signals = new Dictionary<string, List<Notification>>();
            foreach (var _g in _gather.Distinct())
            {
                // NOTE: KVP might have been removed between calls!  therefore FirstOrDEFAULT
                var _target = _Callbacks.FirstOrDefault(_kvp => _kvp.Value.IDs.Contains(_g.Key));
                if (_target.Value != null) // this helps make sure KVP was found in dictionary
                {
                    // list yet for the callback?
                    List<Notification> _list = null;
                    if (_signals.ContainsKey(_target.Key))
                    {
                        _list = _signals[_target.Key];
                    }
                    else
                    {
                        // no, build one
                        _list = new List<Notification>();
                        _signals.Add(_target.Key, _list);
                    }

                    // add to list
                    _list.Add(new Notification
                    {
                        NotifyID = _g.Key,
                        Notifications = _g.Value.ToList()
                    });
                }
            }

            // callback
            foreach (var _s in _signals)
            {
                // close over callback and id list
                var _back = _Callbacks[_s.Key];
                var _infos = _s.Value.ToArray();
                Task.Factory.StartNew((Action)(() =>
                {
                    try
                    {
                        _back.Callback.SystemNotifications(_infos);
                    }
                    catch (Exception _ex)
                    {
                        ConsoleLog(@"Prerequisite Notify Failure", _back.UserName, _ex.Message);

                        // evict
                        _Callbacks.TryRemove(_back.UserName, out IkosaCallbackTracker _out);
                    }
                }));
            }
        }
        #endregion

        #region public static void DoNotifyWaiting(List<string> waitList)
        public static void DoNotifyWaiting(List<string> waitList)
        {
            foreach (var _cBack in _Callbacks)
            {
                var _target = _cBack;
                Task.Factory.StartNew((Action)(() =>
                {
                    try
                    {
                        _target.Value.Callback.WaitingOnUsers(waitList);
                    }
                    catch (Exception _ex)
                    {
                        ConsoleLog(@"WaitOnUsers Notify Failure", _target.Key, _ex.Message);

                        // evict
                        _Callbacks.TryRemove(_target.Key, out IkosaCallbackTracker _out);
                    }
                }));
            }
        }
        #endregion

        #region public static void DoNotifySerialState()
        public static void DoNotifySerialState(ulong original)
        {
            if (original < MapContext.SerialState)
                foreach (var _cBack in _Callbacks)
                {
                    var _target = _cBack;
                    Task.Factory.StartNew((Action)(() =>
                    {
                        try
                        {
                            _target.Value.Callback.SerialStateChanged();
                        }
                        catch (Exception _ex)
                        {
                            ConsoleLog(@"DoUpdateSerialState Notify Failure", _target.Key, _ex.Message);

                            // evict
                            _Callbacks.TryRemove(_target.Key, out IkosaCallbackTracker _out);
                        }
                    }));
                }
        }
        #endregion

        #region public void RegisterCallback(string[] ids)
        public void RegisterCallback(string[] ids)
        {
            var _principal = Thread.CurrentPrincipal;
            if (ids.All(_i => _principal.IsInRole(_i)))
            {
                // gather parameters
                var _tracker = new IkosaCallbackTracker
                {
                    UserName = _principal.Identity.Name,
                    IDs = ids.Select(_i => GetGuid(_i)).ToList(),
                    Callback = OperationContext.Current.GetCallbackChannel<IIkosaCallback>()
                };

                // master registers for master-level pre-requisites
                if (_principal.IsInRole(@"Master"))
                    _tracker.IDs.Add(Guid.Empty);

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
            var _principal = Thread.CurrentPrincipal;
            _Callbacks.TryRemove(_principal.Identity.Name, out _);
        }
        #endregion

        public static void DeRegisterCallback(string userName)
        {
            _Callbacks.TryRemove(userName, out _);
        }

        // read lock section

        #region public List<string> GetWaitingOnUsers()
        public List<string> GetWaitingOnUsers()
        {
            if (ProcessManager != null)
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    try
                    {
                        Synchronizer.EnterReadLock();

                        // notify everyone who we are waiting on
                        if (ProcessManager.CurrentStep != null)
                        {
                            return (from _sp in ProcessManager.CurrentStep.AllPrerequisites<StepPrerequisite>()
                                    where !_sp.IsReady && _sp.Fulfiller != null
                                    from _c in _Callbacks
                                    where _c.Value.IDs.Contains(_sp.Fulfiller.ID)
                                    orderby _c.Key
                                    select _c.Key).Distinct().ToList();
                        }
                    }
                    finally
                    {
                        if (Synchronizer.IsReadLockHeld)
                            Synchronizer.ExitReadLock();
                    }
                }
                else
                    throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
            }
            return new List<string>();
        }
        #endregion

        #region public List<PrerequisiteInfo> GetPreRequisites(string id)
        public List<PrerequisiteInfo> GetPreRequisites(string id)
        {
            if (ProcessManager != null)
            {
                #region PrerequisiteInfo _preReqConverter(CoreStep _step, StepPrerequisite _pre)
                PrerequisiteInfo _preReqConverter(CoreStep _step, StepPrerequisite _pre)
                {
                    if (_pre is RollPrerequisite)
                    {
                        var _roll = _pre as RollPrerequisite;
                        if (!(_roll.Roller is ConstantRoller))
                            return _roll.ToPrerequisiteInfo(_step);
                    }
                    else
                    {
                        return _pre.ToPrerequisiteInfo(_step);
                    }
                    return null;
                };
                #endregion

                if (string.IsNullOrEmpty(id))
                {
                    // master can see all (fulfiller and system)
                    if (Thread.CurrentPrincipal.IsInRole(@"Master"))
                    {
                        try
                        {
                            Synchronizer.EnterReadLock();
                            if (ProcessManager.CurrentStep != null)
                            {
                                return (from _dp in ProcessManager.CurrentStep.DispensedPrerequisites
                                        let _pre = _preReqConverter(ProcessManager.CurrentStep, _dp)
                                        where (_pre != null)
                                        select _pre).ToList();
                            }
                        }
                        finally
                        {
                            if (Synchronizer.IsReadLockHeld)
                                Synchronizer.ExitReadLock();
                        }
                    }
                    else
                        throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access generic prerequisites");
                }
                else
                {
                    // parse guid
                    Guid _id = GetGuid(id);
                    if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
                    {
                        try
                        {
                            Synchronizer.EnterReadLock();
                            if (ProcessManager.CurrentStep != null)
                                return (from _dp in ProcessManager.CurrentStep.DispensedPrerequisites
                                        where (_dp.Fulfiller?.ID.Equals(_id) ?? false)
                                        let _pre = _preReqConverter(ProcessManager.CurrentStep, _dp)
                                        where (_pre != null)
                                        select _pre).ToList();
                        }
                        finally
                        {
                            if (Synchronizer.IsReadLockHeld)
                                Synchronizer.ExitReadLock();
                        }
                    }
                    else
                        throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
                }
            }
            return new List<PrerequisiteInfo>();
        }
        #endregion

        #region public LocalActionBudgetInfo GetActionBudget(string id)
        public LocalActionBudgetInfo GetActionBudget(string id)
        {
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();
                    if (ProcessManager.LocalTurnTracker.GetBudget(_id) is LocalActionBudget _budget)
                        return _budget.ToLocalActionBudgetInfo();
                    return new LocalActionBudgetInfo();

                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public LocalTurnTrackerInfo GetTurnTracker()
        public LocalTurnTrackerInfo GetTurnTracker(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (Thread.CurrentPrincipal.IsInRole(@"Master"))
                {
                    try
                    {
                        Synchronizer.EnterReadLock();
                        return ProcessManager?.LocalTurnTracker.ToLocalTurnTrackerInfo(null);

                    }
                    finally
                    {
                        if (Synchronizer.IsReadLockHeld)
                            Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                // parse guid
                Guid _id = GetGuid(id);
                if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
                {
                    try
                    {
                        Synchronizer.EnterReadLock();
                        var _critter = CreatureProvider.GetCreature(_id);
                        if (_critter != null)
                            return ProcessManager?.LocalTurnTracker.ToLocalTurnTrackerInfo(_critter);
                    }
                    finally
                    {
                        if (Synchronizer.IsReadLockHeld)
                            Synchronizer.ExitReadLock();
                    }
                }
            }
            return null;
        }
        #endregion

        #region public Guid? CurrentStepID()
        public Guid? CurrentStepID()
        {
            if (ProcessManager != null)
            {
                try
                {
                    Synchronizer.EnterReadLock();
                    if (ProcessManager.CurrentStep != null)
                        return ProcessManager.CurrentStep.ID;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            return null;
        }
        #endregion

        #region public ulong GetSerialState()
        public ulong GetSerialState()
        {
            if (ProcessManager != null)
            {
                try
                {
                    Synchronizer.EnterReadLock();
                    if (ProcessManager.CurrentStep != null)
                        return MapContext.SerialState;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            return 0;
        }
        #endregion

        #region public List<ItemSlotInfo> GetSlottedItems(string id)
        public List<ItemSlotInfo> GetSlottedItems(string id)
        {
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);

                    // build collection of object infos to return
                    var _slots = new List<ItemSlotInfo>();
                    foreach (var _slot in _critter.Body.ItemSlots.AllSlots)
                    {
                        if (_slot is MountSlot)
                            _slots.Add((_slot as MountSlot).ToMountSlotInfo());
                        else
                            _slots.Add(_slot.ToItemSlotInfo());
                    }

                    // return
                    return _slots;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region private static AwarenessInfo UnspinAwareness(ICoreObject coreObj, ISensorHost critter)
        private static AwarenessInfo UnspinAwareness(ICoreObject coreObj, Creature critter, ISensorHost sensors,
            bool autoMelee, double distance)
        {
            if (coreObj == critter)
                autoMelee = true;
            else
                autoMelee = (coreObj as Creature)?.IsFriendly(critter.ID) ?? autoMelee;

            var _info = GetInfoData.GetInfoFeedback(coreObj, critter);
            return new AwarenessInfo
            {
                ID = coreObj.ID,
                Info = _info,
                Distance = distance,
                AutoMeleeHit = autoMelee,
                IsTargetable = coreObj.IsTargetable,
                Items = new ObservableCollection<AwarenessInfo>(from _c in coreObj.Accessible(sensors)
                                                                select UnspinAwareness(_c, critter, sensors, autoMelee, distance))
            };
        }
        #endregion

        #region public List<AwarenessInfo> GetAwarenessInfo(string critterID, string sensorHostID)
        public List<AwarenessInfo> GetAwarenessInfo(string critterID, string sensorHostID)
        {
            Guid _critterID = GetGuid(critterID);
            if (Thread.CurrentPrincipal.IsInRole(_critterID.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // build list of all accessible objects of which the creature is aware
                    var _list = new List<AwarenessInfo>();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_critterID);
                    var _sensors = GetSensorHost(sensorHostID, _critterID);
                    if (_sensors?.IsSensorHostActive ?? false)
                    {
                        var _rgn = _sensors.GetLocated()?.Locator.GeometricRegion;
                        if (_rgn != null)
                        {
                            foreach (var _alc in _sensors.Awarenesses.GetAwareLocatorCores())
                            {
                                var _distance = _alc.Locator?.GeometricRegion.NearDistance(_rgn) ?? 0d;
                                _list.Add(UnspinAwareness(_alc.ICoreObject, _critter, _sensors, false, _distance));
                            }
                        }

                        // and grasp awarenesses
                        if (_sensors == _critter)
                        {
                            foreach (var _gg in GraspAwareness.GetGraspAwarenesses(_critter))
                            {
                                if (InteractProvider.GetIInteract(_gg) is ICoreObject _coreObj)
                                {
                                    var _distance = _coreObj.GetLocated()?.Locator.GeometricRegion.NearDistance(_rgn) ?? 0d;
                                    _list.Add(UnspinAwareness(_coreObj, _critter, _sensors, false, _distance));
                                }
                            }
                        }
                    }

                    // return list
                    return _list;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region private ISensorHost GetSensorHost(string sensorHostID, Guid critterID)
        private ISensorHost GetSensorHost(string sensorHostID, Guid critterID)
        {
            // find creature
            var _critter = CreatureProvider.GetCreature(critterID);
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

        #region public IEnumerable<ExtraInfoInfo> GetExtraInfos(string creatureID, string sensorHostID)
        public IEnumerable<ExtraInfoInfo> GetExtraInfos(string creatureID, string sensorHostID)
        {
            Guid _id = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // get sense information
                    var _sensors = GetSensorHost(sensorHostID, _id);
                    if (_sensors?.IsSensorHostActive ?? false)
                    {
                        var _critter = _sensors.Senses.Creature;
                        if (_critter != null)
                        {

                            // converter
                            ExtraInfoInfo _convert(ExtraInfo extra)
                            {
                                if (extra is ExtraInfoMarker _marker)
                                    return _marker.ToExtraInfoMarkerInfo(_critter);
                                return extra.ToExtraInfoInfo(_critter);
                            }

                            var _markers = _sensors.ExtraInfoMarkers.All.Select(_ex => _convert(_ex)).ToList();
                            return _markers;
                        }
                    }
                    return new List<ExtraInfoInfo>();
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
            }
        }
        #endregion

        #region private static AwarenessInfo UnspinMasterAwareness(ICoreObject coreObj)
        private static AwarenessInfo UnspinMasterAwareness(ICoreObject coreObj, double distance)
        {
            var _info = coreObj.GetInfo(null, false);
            return new AwarenessInfo
            {
                ID = coreObj.ID,
                Info = _info,
                Distance = distance,
                AutoMeleeHit = false,
                IsTargetable = coreObj.IsTargetable,
                Items = new ObservableCollection<AwarenessInfo>(from _c in coreObj.Connected
                                                                select UnspinMasterAwareness(_c, distance))
            };
        }
        #endregion

        #region public List<AwarenessInfo> GetMasterAwarenesses(string[] ids)
        public List<AwarenessInfo> GetMasterAwarenesses(string[] ids)
        {
            if (Thread.CurrentPrincipal.IsInRole(@"Master"))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // build list of all objects requested
                    var _list = new List<AwarenessInfo>();
                    foreach (var _data in from _i in ids
                                          let _id = GetGuid(_i)
                                          let _ip = InteractProvider.GetIInteract(_id)
                                          let _obj = _ip as ICoreObject
                                          where _obj != null
                                          select UnspinMasterAwareness(_obj, 0d))
                    {
                        _list.Add(_data);
                    }
                    return _list;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            return null;
        }
        #endregion

        #region public CreatureInfo GetCreature(string id)
        public CreatureInfo GetCreature(string id)
        {
            // parse guid
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);
                    return _critter.ToCreatureInfo();
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public List<Info> GetObjectLoadInfo(string id)
        public List<Info> GetObjectLoadInfo(string id)
        {
            // parse guid
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);

                    // build collection of object infos to return
                    var _load = new List<Info>();
                    foreach (var _obj in _critter.ObjectLoad.OfType<CoreObject>())
                    {
                        _load.Add(GetInfoData.GetInfoFeedback(_obj, _critter));
                    }

                    // return
                    return _load;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        // possession review

        #region public List<PossessionInfo> GetPossessionInfo(string id)
        public List<PossessionInfo> GetPossessionInfo(string id)
        {
            // parse guid
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);
                    if (_critter != null)
                    {
                        var _loc = _critter.GetLocated()?.Locator;
                        return _critter.Possessions
                            .OfType<IItemBase>()
                            .Select(_obj => new PossessionInfo
                            {
                                ObjectInfo = GetInfoData.GetInfoFeedback(_obj, _critter) as ObjectInfo,
                                Location = _obj.GetPath(),
                                HasIdentities = GetIdentityData.GetIdentities(_obj, _critter).Count > 0,
                                IsLocal = _obj.IsLocal(_loc)
                            })
                            .ToList();
                    }

                    // return
                    return new List<PossessionInfo>();
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public List<IdentityInfo> GetIdentityInfos(string critterID, string objectID)
        public List<IdentityInfo> GetIdentityInfos(string critterID, string objectID)
        {
            if (Thread.CurrentPrincipal.IsInRole(critterID))
            {
                var _critterID = GetGuid(critterID);
                var _objID = GetGuid(objectID);
                try
                {
                    Synchronizer.EnterReadLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_critterID);
                    if (_critter != null)
                    {
                        if (_critter.Possessions.Contains(_objID))
                        {
                            return GetIdentityData.GetIdentityInfos(_critter.Possessions[_objID], _critter);
                        }
                        else if (_critter.ObjectLoad.Contains(_objID)
                            || (_critter.Awarenesses.GetAwarenessLevel(_objID) > AwarenessLevel.UnAware))
                        {
                            return GetIdentityData.GetIdentityInfos(InteractProvider.GetIInteract(_objID) as ICoreObject, _critter);
                        }
                        else
                        {
                            return new List<IdentityInfo>();
                        }
                    }
                    return null;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        // budget lock section

        #region public bool CanStartAction(ActivityInfo activity)
        public bool CanStartAction(ActivityInfo activity)
        {
            Guid _id = activity.ActorID;
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();
                    if (ProcessManager.LocalTurnTracker.GetBudget(_id) is LocalActionBudget _budget)
                    {
                        lock (_budget)
                        {
                            var _activity = activity.CreateActivity();
                            return (_activity?.Action.CanPerformNow(_budget).Success ?? false)
                                && (_activity?.CanPerform().Success ?? false);
                        }
                    }
                    return false;
                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public IEnumerable<ActionInfo> GetActions(string id)
        public List<ActionInfo> GetActions(string id)
        {
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                try
                {
                    Synchronizer.EnterReadLock();
                    if (ProcessManager.LocalTurnTracker.GetBudget(_id) is LocalActionBudget _budget)
                    {
                        // NOTE: GetActions might lazy-load the budget sub-items, so best to lock each creature's budget
                        lock (_budget)
                        {
                            var _actions =
                                (from _provided in _budget.GetActions()
                                 where _provided.Action is ActionBase
                                 select _provided.ToActionInfo(_budget.Actor))
                                .ToList();

                            // merge in choices whose action is not equivalent to an available action
                            // NOTE: choices allow unavailable actions to still show they were available
                            _actions.AddRange(_budget.Choices
                                .Where(_c => !_actions.Any(_a => (_a.ID == _c.Value.Action.ID) && (_a.Key == _c.Value.Action.Key)))
                                .ToList()
                                .Select(_c => _c.Value.ToActionInfo()));
                            //foreach (var _a in _actions.OrderBy(_a => _a.Key))
                            //{
                            //    Debug.WriteLine($@"{_budget.Actor.Name}: {_a.Key}, {_a.DisplayName}");
                            //}
                            return _actions;
                        }
                    }
                    return null;

                }
                finally
                {
                    if (Synchronizer.IsReadLockHeld)
                        Synchronizer.ExitReadLock();
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        // write lock section

        #region public void SetPreRequisites(PrerequisiteInfo[] prereqInfos)
        public void SetPreRequisites(PrerequisiteInfo[] prereqInfos)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                WriteConsole(@"SetPrerequesite", prereqInfos?[0]?.Name);
                Synchronizer.EnterWriteLock();

                if ((ProcessManager != null)
                    && (ProcessManager.CurrentStep != null))
                {
                    // process each supplied info
                    foreach (var _info in prereqInfos
                        .Where(_i => _i.StepID.Equals(ProcessManager.CurrentStep.ID)))
                    {
                        // match the info to a dispensed prerequisite on bind-key
                        var _pre = ProcessManager.CurrentStep.DispensedPrerequisites
                            .FirstOrDefault(_dp => _dp.BindKey.Equals(_info.BindKey, StringComparison.OrdinalIgnoreCase));
                        if (_pre != null)
                        {
                            if (_pre.Fulfiller == null)
                            {
                                if (Thread.CurrentPrincipal.IsInRole(@"Master"))
                                {
                                    _pre.MergeFrom(_info);
                                }
                                else
                                    throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access generic prerequisites");
                            }
                            else
                            {
                                // parse guid
                                Guid _id = _pre.Fulfiller.ID;
                                if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
                                {
                                    _pre.MergeFrom(_info);
                                }
                                else
                                    throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
                            }

                            // Do step's activity, or get more prerequisites (or possible start a new step)
                            while (ProcessManager.DoProcess()) ;
                        }
                    }

                    // flush notify
                    FlushNotifications();
                }
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public void DoAction(ActivityInfo activity)
        public void DoAction(ActivityInfo activity)
        {
            var _id = activity.ActorID;
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                var _serial = MapContext?.SerialState ?? 0;
                try
                {
                    Synchronizer.EnterWriteLock();

                    var _action = activity.GetAction();
                    if (_action != null)
                    {
                        WriteConsole(@"DoAction", activity?.ActionID.ToString(), activity.ActionKey);
                        var _tracker = ProcessManager.TurnTrackingStep.Tracker;
                        if (_tracker?.GetBudget(_id) is LocalActionBudget _budget)
                        {
                            if (_tracker.CanTrackerStepDoAction(_budget, _action))
                            {
                                var _activity = activity.CreateActivity();
                                if (_activity?.Action != null)
                                {
                                    _budget.DoAction(ProcessManager, _activity);
                                }
                            }
                        }
                        ProcessManager.DoProcessAll();
                    }
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public void EndTurn(string id)
        public void EndTurn(string id)
        {
            Guid _id = GetGuid(id);
            if (Thread.CurrentPrincipal.IsInRole(_id.ToString()))
            {
                var _serial = GetUpdateSerialState();
                try
                {
                    WriteConsole(@"EndTurn", id.ToString());
                    Synchronizer.EnterWriteLock();

                    // get budget
                    var _budget = ProcessManager.LocalTurnTracker.GetBudget(_id);

                    // answer the action inquiry (if defined)
                    var _pre = ProcessManager.CurrentStep.DispensedPrerequisites
                        .OfType<ActionInquiryPrerequisite>().FirstOrDefault();
                    if (_pre != null)
                    {
                        // can only end the focused budget
                        if (_pre.Budget == _budget)
                        {
                            _pre.Acted = _budget.IsOneTurnAction;
                            _budget.EndTurn();
                        }
                        else
                        {
                            // somebody's out of sync?
                            IkosaServices.QueueNotifySysStatus(
                                new RefreshNotify(true, false, false, false, false),
                                new Guid[] { _id });

                            FlushNotifications();
                            return;
                        }
                    }
                    else if (!ProcessManager.LocalTurnTracker.IsInitiative)
                    {
                        // end the budget's turn
                        _budget.EndTurn();
                    }

                    // NOTE: this will cause any current LocalTickStep or DelayedTickStep to DoStep()
                    // NOTE: ITickEndBudgetItems will EndTick(), ITurnEndBudgetItems will EndTurn() if IsUsingTurn
                    ProcessManager.DoProcessAll();
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public void CancelAction(string creatureID)
        public void CancelAction(string creatureID)
        {
            // NOTE: held activities are cancelled by an AbortSpan action provided by the LocalActionBudget
            Guid _critterID = GetGuid(creatureID);
            if (Thread.CurrentPrincipal.IsInRole(_critterID.ToString()))
            {
                var _serial = GetUpdateSerialState();
                try
                {
                    Synchronizer.EnterWriteLock();

                    // cancel activities in flight
                    var _acts = ProcessManager.AllActivities.Where(_a => _a.Actor.ID == _critterID).ToList();
                    foreach (var _activity in _acts)
                    {
                        _activity.IsActive = false;
                    }
                    ProcessManager.DoProcessAll();
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public void DoProcess()
        public void DoProcess()
        {
            if (Thread.CurrentPrincipal.IsInRole(@"Master"))
            {
                var _serial = MapContext?.SerialState ?? 0;
                try
                {
                    Synchronizer.EnterWriteLock();

                    // process
                    ProcessManager.DoProcessAll();
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to force processing");
        }
        #endregion

        #region public Take10Info SetTake10(string id, DeltableInfo target, double duration)
        public Take10Info SetTake10(string id, DeltableInfo target, double duration)
        {
            if (Thread.CurrentPrincipal.IsInRole(id))
            {
                Guid _id = GetGuid(id);
                var _serial = GetUpdateSerialState();
                try
                {
                    Synchronizer.EnterWriteLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);
                    if (_critter != null)
                    {
                        if (target is SkillInfo)
                        {
                            // get critter's matching Skill
                            var _skill = _critter.Skills.FirstOrDefault(_s => _s.SkillName == target.Message);
                            if (_skill != null)
                            {
                                _critter.SetTake10Duration(_skill.GetType(), duration);
                                if (duration > 0)
                                    return new Take10Info { RemainingRounds = duration };
                            }
                        }
                        else if (target is AbilityInfo)
                        {
                            var _ability = _critter.Abilities[(target as AbilityInfo).Mnemonic];
                            if (_ability != null)
                            {
                                _critter.SetTake10Duration(_ability.GetType(), duration);
                                if (duration > 0)
                                    return new Take10Info { RemainingRounds = duration };
                            }
                        }
                    }
                    return null;
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public Take10Info SetAbilitiesTake10(string id, double duration)
        public Take10Info SetAbilitiesTake10(string id, double duration)
        {
            if (Thread.CurrentPrincipal.IsInRole(id))
            {
                Guid _id = GetGuid(id);
                var _serial = GetUpdateSerialState();
                try
                {
                    Synchronizer.EnterWriteLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);
                    if (_critter != null)
                    {
                        _critter.SetTake10Duration(typeof(AbilityBase), duration);
                        if (duration > 0)
                            return new Take10Info { RemainingRounds = duration };
                    }
                    return null;
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public Take10Info SetSkillsTake10(string id, double duration)
        public Take10Info SetSkillsTake10(string id, double duration)
        {
            if (Thread.CurrentPrincipal.IsInRole(id))
            {
                Guid _id = GetGuid(id);
                var _serial = GetUpdateSerialState();
                try
                {
                    Synchronizer.EnterWriteLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_id);
                    if (_critter != null)
                    {
                        _critter.SetTake10Duration(typeof(SkillBase), duration);
                        if (duration > 0)
                            return new Take10Info { RemainingRounds = duration };
                    }
                    return null;
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        #region public void SetActiveInfo(string critterID, string objectID, string infoID)
        public void SetActiveInfo(string critterID, string objectID, string infoID)
        {
            if (Thread.CurrentPrincipal.IsInRole(critterID))
            {
                var _critterID = GetGuid(critterID);
                var _objID = GetGuid(objectID);
                var _infoID = GetGuid(infoID);
                var _serial = GetUpdateSerialState();
                try
                {
                    Synchronizer.EnterWriteLock();

                    // find creature
                    var _critter = CreatureProvider.GetCreature(_critterID);
                    if (_critter != null)
                    {
                        var _idents = _critter.Possessions.Contains(_objID)
                            ? GetIdentityData.GetIdentities(_critter.Possessions[_objID], _critter)
                            : (_critter.ObjectLoad.Contains(_objID) || (_critter.Awarenesses.GetAwarenessLevel(_objID) > AwarenessLevel.UnAware))
                            ? GetIdentityData.GetIdentities(InteractProvider.GetIInteract(_objID) as ICoreObject, _critter)
                            : new List<Identity>();
                        foreach (var _identity in _idents)
                        {
                            // is this the one being set?
                            if (_identity.MergeID == _infoID)
                            {
                                // is creature not using it already and can creature use it?
                                if (!_identity.Users.Contains(_critterID)
                                    && _identity.CreatureIDs.ContainsKey(_critterID))
                                {
                                    _identity.Users.Add(_critterID);
                                }
                            }
                            else
                            {
                                // being cleared otherwise
                                if (_identity.Users.Contains(_critterID))
                                {
                                    _identity.Users.Remove(_critterID);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (Synchronizer.IsWriteLockHeld)
                    {
                        DoNotifySerialState(_serial);
                        Synchronizer.ExitWriteLock();
                    }
                }
            }
            else
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized to access this creature");
        }
        #endregion

        // IIkosaAdvancement

        #region private Creature GetCreatureForAdvancement(Guid id)
        /// <summary>provides a creature if it has advancement capacity</summary>
        private Creature GetCreatureForAdvancement(Guid id)
        {
            if ((Thread.CurrentPrincipal is IkosaPrincipal _principal)
               && _principal.IsInRole(id.ToString()))
            {
                // get creature and confirm advancementCapacity
                var _critter = CreatureProvider.GetCreature(id);
                return (_critter?.HasAdjunct<AdvancementCapacity>() ?? false)
                    ? _critter
                    : throw new FaultException<InvalidArgumentFault>(
                        new InvalidArgumentFault(nameof(id)),
                        @"Creature not ready to advance");
            }
            else
            {
                ConsoleLog(@"Authorization Failure", Thread.CurrentPrincipal?.Identity.Name ?? @"¿Unknown?", @"User not authorized");
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
            }
        }
        #endregion

        #region private List<ClassInfo> ListAvailableClasses(Guid id)
        private List<ClassInfo> ListAvailableClasses(Guid id)
        {
            var _critter = GetCreatureForAdvancement(id);
            var _classes = new List<ClassInfo>();
            // has monster classes already?
            _classes.AddRange(_critter.Classes.OfType<BaseMonsterClass>()
                .Select(_bmc => _bmc.ToClassInfo()));

            // can creature advance by class?
            if (_critter.Species.IsCharacterCapable)
            {
                _classes.AddRange(Campaign.SystemCampaign.ClassLists[@"Character"]
                    .Select(_ccKVP => _ccKVP.Value)
                    .Select(_v => ClassInfoAttribute.GetClassInfo(_v.ListedType, _critter)));
            }
            return _classes;
        }
        #endregion

        #region public List<AdvanceableCreature> GetAdvanceableCreatures()
        public List<AdvanceableCreature> GetAdvanceableCreatures()
        {
            if (Thread.CurrentPrincipal is IkosaPrincipal _principal)
            {
                var _provider = CreatureProvider;
                if (_principal.IsInRole(@"Master"))
                {
                    try
                    {
                        Synchronizer.EnterReadLock();
                        return (from _login in MapContext.CreatureLoginsInfos
                                let _critter = _provider.GetCreature(_login.ID)
                                where (_critter?.HasAdjunct<AdvancementCapacity>() ?? false)
                                select _critter.ToAdvanceableCreature())
                                .ToList();
                    }
                    finally
                    {
                        if (Synchronizer.IsReadLockHeld)
                            Synchronizer.ExitReadLock();
                    }
                }
                else
                {
                    try
                    {
                        Synchronizer.EnterReadLock();
                        return (from _login in _principal.GetCreatureLogins()
                                let _critter = _provider.GetCreature(_login.ID)
                                where (_critter?.HasAdjunct<AdvancementCapacity>() ?? false)
                                select _critter.ToAdvanceableCreature())
                                .ToList();
                    }
                    finally
                    {
                        if (Synchronizer.IsReadLockHeld)
                            Synchronizer.ExitReadLock();
                    }
                }
            }
            else
            {
                ConsoleLog(@"Authorization Failure", Thread.CurrentPrincipal?.Identity.Name ?? @"¿Unknown?", @"User not authorized");
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
            }
        }
        #endregion

        #region public List<ClassInfo> GetAvailableClasses(Guid id)
        public List<ClassInfo> GetAvailableClasses(Guid id)
        {
            try
            {
                Synchronizer.EnterReadLock();
                return ListAvailableClasses(id);
            }
            finally
            {
                if (Synchronizer.IsReadLockHeld)
                    Synchronizer.ExitReadLock();
            }
        }
        #endregion

        #region public List<AdvancementOptionInfo> GetAvailableFeats(Guid id, int powerLevel)
        public List<AdvancementOptionInfo> GetAvailableFeats(Guid id, int powerLevel)
        {
            try
            {
                Synchronizer.EnterReadLock();
                var _critter = GetCreatureForAdvancement(id);
                var _powerDie = _critter?.AdvancementLog[powerLevel];
                if (_powerDie?.IsFeatPowerDie ?? false)
                {
                    return (from _feat in _powerDie.AvailableFeats
                            select _feat.ToAdvancementOptionInfo())
                            .ToList();
                }
                return new List<AdvancementOptionInfo>();
            }
            finally
            {
                if (Synchronizer.IsReadLockHeld)
                    Synchronizer.ExitReadLock();
            }
        }
        #endregion

        #region public List<SkillInfo> GetAvailableSkills(Guid id, int powerLevel)
        public List<SkillInfo> GetAvailableSkills(Guid id, int powerLevel)
        {
            try
            {
                Synchronizer.EnterReadLock();
                var _critter = GetCreatureForAdvancement(id);
                var _powerDie = _critter.AdvancementLog[powerLevel];
                if (_powerDie != null)
                {
                    return (from _skill in _critter.Skills
                            let _pdSkill = _skill.ToSkillInfo(_critter)
                            select new SkillInfo
                            {
                                // is class skill depends on which power-die is buying the skill
                                IsClassSkill = _powerDie.AdvancementClass.IsClassSkill(_skill),
                                Message = _pdSkill.Message,
                                KeyAbilityMnemonic = _pdSkill.KeyAbilityMnemonic,
                                UseUntrained = _pdSkill.UseUntrained,
                                IsTrained = _pdSkill.IsTrained,
                                CheckFactor = _pdSkill.CheckFactor,
                                BaseDoubleValue = _pdSkill.BaseDoubleValue,
                                BaseValue = _pdSkill.BaseValue,
                                DeltaDescriptions = _pdSkill.DeltaDescriptions,
                                EffectiveValue = _pdSkill.EffectiveValue
                            }).ToList();
                }
                return new List<SkillInfo>();
            }
            finally
            {
                if (Synchronizer.IsReadLockHeld)
                    Synchronizer.ExitReadLock();
            }
        }
        #endregion

        #region public AbilitySetInfo GetBoostableAbilities(Guid id, int powerLevel)
        public AbilitySetInfo GetBoostableAbilities(Guid id, int powerLevel)
        {
            try
            {
                Synchronizer.EnterReadLock();
                var _critter = GetCreatureForAdvancement(id);
                if (_critter?.AdvancementLog[powerLevel]?.IsAbilityBoostPowerDie ?? false)
                {
                    return _critter.Abilities.ToAbilitySetInfo(_critter);
                }
                return null;
            }
            finally
            {
                if (Synchronizer.IsReadLockHeld)
                    Synchronizer.ExitReadLock();
            }
        }
        #endregion

        #region public AdvanceableCreature PushClassLevel(Guid id, ClassInfo classInfo)
        public AdvanceableCreature PushClassLevel(Guid id, ClassInfo classInfo)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);

                // only push if everything is locked
                if (_critter.AdvancementLog.FirstUnlockedAdvancementLogItem() == null)
                {
                    var _mustIncrease = true;

                    // try to get an unlisted (intrinsic) monster class
                    var _advClass = _critter.Classes.OfType<BaseMonsterClass>()
                        .Where(_bmc => _bmc.GetType().FullName.Equals(classInfo.FullName, StringComparison.OrdinalIgnoreCase))
                        .OfType<AdvancementClass>()
                        .FirstOrDefault();

                    // not an intrinsic monster class, look for other class
                    if (_advClass == null)
                    {
                        // lookup class from campaign
                        var _tli = Campaign.SystemCampaign.GetClassTypeListItem(@"Character", classInfo.FullName);

                        // get see if creature has it already
                        _advClass = _critter.Classes.Get<AdvancementClass>(_tli.ListedType);
                        if (_advClass == null)
                        {
                            // if not, create it and bind to creature
                            _advClass = Activator.CreateInstance(_tli.ListedType) as AdvancementClass;
                            _advClass.BindTo(_critter);
                            _mustIncrease = _advClass.CurrentLevel == 0;
                        }
                    }

                    // if class found, or created and init-bound with 0 levels ...
                    // ... then must increase
                    if (_mustIncrease)
                    {
                        // if bound and can increase, then do so
                        if ((_advClass?.Creature == _critter) && _advClass.CanIncreaseLevel())
                        {
                            _advClass.IncreaseLevel(PowerDieCalcMethod.Roll);
                        }
                        else
                        {
                            throw new FaultException<InvalidArgumentFault>(
                                new InvalidArgumentFault(nameof(classInfo)),
                                @"Class not found, cannot bind or cannot increase");
                        }
                    }
                }
                return _critter.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature PopClassLevel(Guid id)
        public AdvanceableCreature PopClassLevel(Guid id)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);

                // only allow popping if there are unlocked levels
                if (_critter.AdvancementLog.FirstUnlockedAdvancementLogItem() != null)
                {
                    _critter.AdvancementLog.RemoveLast();
                }
                return _critter?.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature LockClassLevel(Guid id)
        public AdvanceableCreature LockClassLevel(Guid id)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);

                // if lock succeeds, no more advancement capacity
                if (_critter.AdvancementLog.LockNext())
                    _critter.Adjuncts.EjectAll<AdvancementCapacity>();

                return _critter?.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature UnlockClassLevel(Guid id)
        public AdvanceableCreature UnlockClassLevel(Guid id)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);
                // TODO: master function only?
                //_critter?.AdvancementLog.LastOrDefault()?.Unlock();
                return _critter?.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature SetAdvancementOption(Guid id, AdvancementRequirementInfo requirement, AdvancementOptionInfo option)
        public AdvanceableCreature SetAdvancementOption(Guid id, AdvancementRequirementInfo requirement, AdvancementOptionInfo option)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);
                var _level = _critter.AdvancementLog.FirstUnlockedAdvancementLogItem();
                if (_level != null)
                {
                    // get requirement for the argument
                    var _advItem = _level.Requirements.FirstOrDefault(_r => _r.Key.IsKey(requirement.Key));
                    if (_advItem != null)
                    {
                        // get option matching
                        if (!_advItem.SetAdvancementOption(option))
                        {
                            throw new FaultException<InvalidArgumentFault>(
                                new InvalidArgumentFault(nameof(option)), @"AdvancementOption not found");
                        }
                    }
                    else
                    {
                        throw new FaultException<InvalidArgumentFault>(
                            new InvalidArgumentFault(nameof(requirement)), @"RequirementKey not found");
                    }
                }
                return _critter?.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature SetHealthPoints(Guid id, int powerLevel, int hitPoints)
        public AdvanceableCreature SetHealthPoints(Guid id, int powerLevel, int hitPoints)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);
                var _powerDie = _critter?.AdvancementLog[powerLevel];
                if (!(_powerDie?.IsLocked ?? true))
                {
                    _powerDie.SetHealthPoints(hitPoints);
                }
                return _critter?.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature SetAbilityBoost(Guid id, int powerLevel, string mnemonic)
        public AdvanceableCreature SetAbilityBoost(Guid id, int powerLevel, string mnemonic)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);
                var _powerDie = _critter?.AdvancementLog[powerLevel];
                if (!(_powerDie?.IsLocked ?? true)
                    && (_powerDie?.IsAbilityBoostPowerDie ?? false))
                {
                    _powerDie.AbilityBoostMnemonic = mnemonic;
                }
                return _critter?.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature SetFeat(Guid id, int powerLevel, AdvancementOptionInfo feat)
        public AdvanceableCreature SetFeat(Guid id, int powerLevel, AdvancementOptionInfo feat)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);

                // confirm feat can be set
                var _powerDie = _critter?.AdvancementLog[powerLevel];
                if (!(_powerDie?.IsLocked ?? true)
                    && (_powerDie?.IsFeatPowerDie ?? false)
                    && (feat != null))
                {
                    // find an available feat that resolves
                    var _feat = _powerDie.AvailableFeats
                        .Select(_f => _f.ResolveFeat(feat, _powerDie))
                        .Where(_f => _f != null)
                        .FirstOrDefault();
                    if (_feat != null)
                    {
                        // found, so set...
                        _powerDie.Feat = _feat;
                    }
                }
                return _critter.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public AdvanceableCreature SetSkillPoints(Guid id, int powerLevel, SkillInfo skill, int points)
        public AdvanceableCreature SetSkillPoints(Guid id, int powerLevel, SkillInfo skill, int points)
        {
            var _serial = GetUpdateSerialState();
            try
            {
                Synchronizer.EnterWriteLock();
                var _critter = GetCreatureForAdvancement(id);
                var _powerDie = _critter?.AdvancementLog[powerLevel];
                if (!(_powerDie?.IsLocked ?? true))
                {
                    var _skill = _critter.Skills
                        .FirstOrDefault(_s => _s.SkillName.Equals(skill?.SkillName, StringComparison.OrdinalIgnoreCase));
                    if (_skill != null)
                    {
                        _powerDie.AssignSkillPoints(_skill, Math.Max(0, points));
                    }
                }
                return _critter.ToAdvanceableCreature();
            }
            finally
            {
                if (Synchronizer.IsWriteLockHeld)
                {
                    DoNotifySerialState(_serial);
                    Synchronizer.ExitWriteLock();
                }
            }
        }
        #endregion

        public static ulong GetUpdateSerialState()
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
