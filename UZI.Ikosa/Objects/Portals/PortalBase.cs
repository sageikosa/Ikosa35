using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public abstract class PortalBase : LocatableObject, ITacticalActionProvider, IAudibleOpenable,
        IMonitorChange<Activation>, IAnchorage, IActionSource, IArmorRating, IStructureDamage, IProvideSaves
    {
        // TODO: local link model for activation/de-activation of "room" link
        // TODO: attenutation of light

        #region Construction
        protected PortalBase(string name, IFlatObjectSide portalObjA, IFlatObjectSide portalObjB)
            : base(name, true)
        {
            // default is not stuck, nor locked
            // NOTE: these have the same source, so will not stack
            if ((portalObjA.ObjectMaterial is MetalMaterial)
                || (portalObjA.ObjectMaterial is StoneMaterial))
            {
                _BlockedForce = new Delta(28, this);
                _StuckForce = new Delta(28, this);
            }
            else
            {
                _BlockedForce = new Delta(15, this);
                _StuckForce = new Delta(13, this);
            }
            _BlockedForce.Enabled = false;
            _StuckForce.Enabled = false;

            // determine the difficulty to force open the portal (if needed)
            _ForceOpen = new Deltable(0);
            _ForceOpen.Deltas.Add(_StuckForce);
            _ForceOpen.Deltas.Add(_BlockedForce);

            _OpenState = this.GetOpenStatus(null, this, 0);
            _OCtrl = new ChangeController<OpenStatus>(this, OpenState);
            _Flip = false;

            // portalled objects
            _PObjA = portalObjA;
            _PObjB = portalObjB;
            _PObjA.BindToObject(this);
            _PObjB.BindToObject(this);
            _PObjA.AddChangeMonitor(this);
            _PObjB.AddChangeMonitor(this);

            _Area = 20;
        }

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new GraspActionAwarenessProviderHandler());
            AddIInteractHandler(new ItemAttackHandler());
            AddIInteractHandler(new TransitAttackHandler());
            AddIInteractHandler(new SpellTransitHandler());
            base.InitInteractionHandlers();
        }
        #endregion

        #region Data
        private Deltable _ForceOpen;
        private Delta _BlockedForce;
        private Delta _StuckForce;
        private IFlatObjectSide _PObjA;
        private IFlatObjectSide _PObjB;
        private bool _Flip;
        private OpenStatus _OpenState;
        private double _Area;
        private ChangeController<OpenStatus> _OCtrl;
        private double _TareWeight;
        #endregion

        #region Peek at Adjuncts to see if they block or stick the door
        public override bool AddAdjunct(Adjunct adjunct)
        {
            var _return = base.AddAdjunct(adjunct);
            _BlockedForce.Enabled = this.HasActiveAdjunct<OpenBlocked>();
            _StuckForce.Enabled = this.HasActiveAdjunct<StuckAdjunct>();
            return _return;
        }

        public override bool RemoveAdjunct(Adjunct adjunct)
        {
            var _return = base.RemoveAdjunct(adjunct);
            _BlockedForce.Enabled = this.HasActiveAdjunct<OpenBlocked>();
            _StuckForce.Enabled = this.HasActiveAdjunct<StuckAdjunct>();
            return _return;
        }
        #endregion

        #region protected void Represent()
        protected void Represent()
        {
            var _loc = this.GetLocated();
            if (_loc != null)
            {
                if (_loc.Locator is ObjectPresenter _objPres)
                {
                    // TODO: signal a need to redraw to observers
                    //_objPres.TransformModel();
                }
            }
        }
        #endregion

        #region public override string Name { get; set; }
        public override string Name
        {
            get { return base.Name; }
            set
            {
                base.Name = value;
                if (PortalledObjectA != null)
                {
                    PortalledObjectA.SetName(value);
                }

                if (PortalledObjectB != null)
                {
                    PortalledObjectB.SetName(value);
                }

                DoPropertyChanged(nameof(Name));
            }
        }
        #endregion

        /// <summary>Difficulty to force open when blocked</summary>
        public Delta BlockedForce => _BlockedForce;

        /// <summary>Difficulty to force open when stuck</summary>
        public Delta StuckForce => _StuckForce;

        /// <summary>Difficulty to force open the portal</summary>
        public Deltable ForceOpenDifficulty => _ForceOpen;

        public IFlatObjectSide PortalledObjectA => _PObjA;
        public IFlatObjectSide PortalledObjectB => _PObjB;

        public double Area { get { return _Area; } set { _Area = value; } }

        #region public IEnumerable<IFlatObjectSide> PortalledObjects { get; }
        public IEnumerable<IFlatObjectSide> PortalledObjects
        {
            get
            {
                yield return _PObjA;
                yield return _PObjB;
                yield break;
            }
        }
        #endregion

        /// <summary>Used to fix-up model rendering and sense effect mapping</summary>
        public bool Flip { get { return _Flip; } set { _Flip = value; } }

        #region protected bool NotifyLighting(OpenStatus oldValue)
        // NOTE: probably want this more decoupled...
        /// <summary>True if the map was signalled to redraw (because light changed)</summary>
        protected bool NotifyLighting(OpenStatus oldValue)
        {
            // notify lighting...
            var _oldStep = Math.Round(oldValue.Value, 1);
            var _newStep = Math.Round(OpenState.Value, 1);
            if ((_newStep != _oldStep)
                || ((oldValue.Value != OpenState.Value)
                    && ((oldValue.Value == 0) || (OpenState.Value == 0) || (oldValue.Value == 1) || (OpenState.Value == 1))))
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    // get all groups in links that this locator is part of
                    var _groups = (from _locGroup in _loc.Locator.GetLocalCellGroups()
                                   from _lnk in _locGroup.Links.All
                                   where _lnk.GeometricRegion.ContainsGeometricRegion(_loc.Locator.GeometricRegion)
                                   from _lnkGroup in _lnk.Groups
                                   select _lnkGroup)
                                   .Distinct()
                                   .ToList();

                    // notify lighting
                    var _notifiers = _groups
                        .SelectMany(_g => _g.NotifyLighting())
                        .Distinct()
                        .ToList();

                    AwarenessSet.RecalculateAllSensors(_loc.Locator.Map, _notifiers, true);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region protected bool NotifySoundChannels(OpenStatus oldValue)
        protected bool NotifySoundChannels(OpenStatus oldValue)
        {
            // notify lighting...
            var _oldStep = Math.Round(oldValue.Value, 1);
            var _newStep = Math.Round(OpenState.Value, 1);
            if ((_newStep != _oldStep)
                || ((oldValue.Value != OpenState.Value)
                    && ((oldValue.Value == 0) || (OpenState.Value == 0) || (oldValue.Value == 1) || (OpenState.Value == 1))))
            {
                var _loc = this.GetLocated();
                if (_loc != null)
                {
                    var _serial = _loc.Locator.MapContext.SerialState;
                    var _updateGroups = new ConcurrentDictionary<Guid, LocalCellGroup>();

                    // get all links affected by this locator
                    var _links = (from _locGroup in _loc.Locator.GetLocalCellGroups()
                                  from _lnk in _locGroup.Links.All
                                  where _lnk.GeometricRegion.ContainsGeometricRegion(_loc.Locator.GeometricRegion)
                                  select _lnk).ToList();
                    Parallel.ForEach(_links, _l => _l.RecalculateSoundDifficulty());

                    // only update distinct sounds moving through portal's affected channels
                    var _soundGroups = _links.SelectMany(_l => _l.SoundChannelsToA.Union(_l.SoundChannelsToB))
                        .Select(_sc => _sc.Value.SoundGroup)
                        .Distinct()
                        .ToDictionary(_sg => _sg.SoundPresence.Audible.SoundGroupID);

                    // refresh the sounds (portal changes will affect accordingly)
                    Parallel.ForEach(_soundGroups.Values,
                        _sg => _sg.SetSoundRef(_sg.SoundPresence.GetRefresh(_serial), _updateGroups));

                    // allow locators in groups to handle apparent sound changes
                    if (_updateGroups.Any())
                    {
                        // all potential listeners
                        _updateGroups.UpdateListenersInGroups((id) => _soundGroups.ContainsKey(id));
                    }
                    return true;
                }
            }
            return false;
        }
        #endregion

        protected bool NotifyLocatorZones()
        {
            var _loc = this.GetLocated();
            if (_loc != null)
            {
                // get all groups in links that this locator is part of
                var _keys = new HashSet<RoomTrackerKey>((from _locGroup in _loc.Locator.GetLocalCellGroups()
                                                         from _lnk in _locGroup.Links.All
                                                         where _lnk.GeometricRegion.ContainsGeometricRegion(_loc.Locator.GeometricRegion)
                                                         from _lnkGroup in _lnk.Groups
                                                         select _lnkGroup)
                                                         .Distinct()
                                                         .SelectMany(_g => RoomTrackerKey.GetKeys(_g))
                                                         .Distinct());

                // notify locator zones
                var _zones = (from _z in _loc.Locator.MapContext.LocatorZones.AllCaptures()
                              where RoomTrackerKey.GetKeys(_z.Geometry.Region).Any(_k => _keys.Contains(_k))
                              select _z).ToList();
                foreach (var _z in _zones)
                {
                    _z.Geometry.RecalcGeometry();
                }
                return true;
            }
            return false;
        }

        protected ObjectPresenter MyPresenter
            => this.GetLocated().Locator as ObjectPresenter;

        public override bool IsTargetable => true;

        #region IActionProvider Members
        public Guid PresenterID => ID;

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // tactical
            foreach (var _act in GetTacticalActions(budget))
            {
                yield return _act;
            }

            // figure out where the actor is
            var _actorRgn = Locator.FindFirstLocator(budget.Actor)?.GeometricRegion;
            var _portalRgn = Locator.FindFirstLocator(this)?.GeometricRegion;

            // see if the actor can reach side A
            if (IsSideAccessible(false, _actorRgn, _portalRgn))
            {
                foreach (var _bAct in PortalledObjectA.GetActions(budget))
                {
                    yield return _bAct;
                }

            }

            // see if the actor can reach side B
            if (IsSideAccessible(true, _actorRgn, _portalRgn))
            {
                foreach (var _bAct in PortalledObjectB.GetActions(budget))
                {
                    yield return _bAct;
                }

            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        #region IOpenable Members

        #region IControlChange<OpenStatus> Members
        public void AddChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<OpenStatus> monitor)
        {
            _OCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        public bool CanChangeOpenState(OpenStatus testValue)
            => !_OCtrl.WillAbortChange(testValue);

        public OpenStatus OpenState
        {
            get => _OpenState;
            set
            {
                var _oldVal = _OpenState;
                if (!_OCtrl.WillAbortChange(value))
                {
                    _OCtrl.DoPreValueChanged(value);
                    _OpenState = value;
                    _OCtrl.DoValueChanged(value);
                    DoPropertyChanged(nameof(OpenState));

                    // notify locator zones
                    if (!NotifyLocatorZones())
                    {
                    }

                    // notify lighting...
                    if (!NotifyLighting(_oldVal))
                    {
                    }

                    // notify sound channels...
                    if (!NotifySoundChannels(_oldVal))
                    {
                    }

                    // TODO: start locators falling: land movement objects, unanchored furnishings and conveyances
                }
            }
        }

        public double OpenWeight
        {
            get => PortalledObjectA.Weight + PortalledObjectB.Weight;
            set => DoPropertyChanged(nameof(OpenWeight));
        }

        #endregion

        #region IAudibleOpenable Members
        private string GetMaterialString()
            => $@"{PortalledObjectA.ObjectMaterial.SoundQuality}";

        public SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"opening",
                (0, @"scraping"),
                (5, $@"{GetMaterialString()} scraping"),
                (10, $@"{GetMaterialString()} {PortalledObjectA.SoundDescription} moving")),
                5, 90, serialState);

        public SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closing",
                (0, @"scraping"),
                (5, $@"{GetMaterialString()} scraping"),
                (10, $@"{GetMaterialString()} {PortalledObjectA.SoundDescription} moving")),
                5, 90, serialState);

        public SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closed",
                (0, @"thud"),
                (5, $@"{GetMaterialString()} thud"),
                (10, $@"{GetMaterialString()} {PortalledObjectA.SoundDescription} closed")),
                0, 120, serialState);

        public SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"blocked",
                (0, @"rattling"),
                (8, $@"{GetMaterialString()} rattling"),
                (12, $@"{GetMaterialString()} {PortalledObjectA.SoundDescription} blocked")),
                5, 90, serialState);
        #endregion

        #region IMonitorChange<Activation> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<Activation> args)
        {
            // never let the sub parts adjuncts be deactivated
            args.DoAbort();
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Activation> args)
        {
        }
        #endregion

        #region public override IEnumerable<ICoreObject> Connected { get; }
        /// <summary>return portalled objects (one for each side of the portal</summary>
        public override IEnumerable<ICoreObject> Connected
        {
            get
            {
                // portalled objects
                yield return PortalledObjectA;
                yield return PortalledObjectB;

                // base
                foreach (ICoreObject _iCore in base.Connected)
                {
                    yield return _iCore;
                }

                yield break;
            }
        }
        #endregion

        /// <summary>Test if the inside or outside of the portal is accessible to the region</summary>
        public bool IsSideAccessible(bool inside, IGeometricRegion region)
        {
            var _portal = this.GetLocated()?.Locator.GeometricRegion;
            if (_portal != null)
            {
                return IsSideAccessible(inside, region, _portal);
            }

            return false;
        }

        #region public override IEnumerable<ICoreObject> Accessible(CoreActor actor)
        protected abstract bool IsSideAccessible(bool inside, IGeometricRegion principal, IGeometricRegion portal);

        public override IEnumerable<ICoreObject> Accessible(ICoreObject principal)
        {
            // relative regions
            var _actorRgn = Locator.FindFirstLocator(principal)?.GeometricRegion;
            var _portalRgn = Locator.FindFirstLocator(this)?.GeometricRegion;

            // test each side
            if (IsSideAccessible(false, _actorRgn, _portalRgn))
            {
                // only return stuff on the side
                foreach (var _o in _PObjA.Accessible(principal))
                {
                    yield return _o;
                }
            }

            if (IsSideAccessible(true, _actorRgn, _portalRgn))
            {
                // only return stuff on the side
                foreach (var _o in _PObjB.Accessible(principal))
                {
                    yield return _o;
                }
            }

            // everything except the sides
            foreach (var _obj in Connected.Where(_o => _o != _PObjA && _o != _PObjB))
            {
                yield return _obj;
            }

            yield break;
        }
        #endregion

        #region IAnchorage Members

        public bool CanAcceptAnchor(IAdjunctable newAnchor)
            => ((newAnchor == _PObjA) || (newAnchor == _PObjB));

        public bool CanEjectAnchor(IAdjunctable existingAnchor)
        {
            // don't want to lose these doors
            return false;
        }

        public void AcceptAnchor(IAdjunctable newAnchor)
        {
        }

        public void EjectAnchor(IAdjunctable existingAnchor)
        {
        }

        public IEnumerable<ICoreObject> Anchored { get { yield break; } }

        #endregion

        #region IControlChange<ICoreObject> Members

        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            // not checking at the moment
        }

        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            // not checking at the moment
        }

        #endregion

        #region ILoadedObjects Members

        public IEnumerable<ICoreObject> AllLoadedObjects()
        {
            yield return PortalledObjectA;
            yield return PortalledObjectB;
            foreach (var _core in PortalledObjectA.AllLoadedObjects().Union(PortalledObjectB.AllLoadedObjects()))
            {
                yield return _core;
            }

            yield break;
        }

        public bool ContentsAddToLoad
            => true;

        private void RecalcWeight() { Weight = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0); }

        public double TareWeight
        {
            get => _TareWeight;
            set
            {
                _TareWeight = value;
                RecalcWeight();
            }
        }

        public double LoadWeight
            => _PObjA.Weight + _PObjB.Weight;

        #endregion

        #region IMonitorChange<Weight> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            if (args.NewValue.PropertyType == Physical.PhysicalType.Weight)
            {
                RecalcWeight();
            }
        }

        #endregion

        protected CellLocation OffsetCell(Locator locator, AnchorFace face)
        {
            #region get location for reference shading
            var _rgn = locator.GeometricRegion;
            return face switch
            {
                AnchorFace.XLow => new CellLocation(_rgn.LowerZ, _rgn.LowerY, _rgn.LowerX - 1),
                AnchorFace.XHigh => new CellLocation(_rgn.LowerZ, _rgn.LowerY, _rgn.UpperX + 1),
                AnchorFace.YLow => new CellLocation(_rgn.LowerZ, _rgn.LowerY - 1, _rgn.LowerX),
                AnchorFace.YHigh => new CellLocation(_rgn.LowerZ, _rgn.UpperY + 1, _rgn.LowerX),
                AnchorFace.ZLow => new CellLocation(_rgn.LowerZ - 1, _rgn.LowerY, _rgn.LowerX),
                _ => new CellLocation(_rgn.UpperZ + 1, _rgn.LowerY, _rgn.LowerX),
            };
            #endregion
        }

        #region protected static VisualEffect OffsetVisualEffect(IGeometricRegion location, IList<SensoryBase> filteredSenses, Locator locator, CellLocation targetLocation)
        protected static VisualEffect OffsetVisualEffect(IGeometricRegion observer,
            IList<SensoryBase> filteredSenses, Locator locator, CellLocation targetLocation)
        {
            var _distance = IGeometricHelper.Distance(observer.GetPoint3D(), targetLocation);
            var _effect = VisualEffect.Unseen;

            #region magic dark
            // any magic darks on objects in this locator
            if (locator.Map.IsInMagicDarkness(targetLocation))
            {
                // dim under magic dark
                _effect = VisualEffect.DimTo50;

                foreach (var _magicDarkPiercing in filteredSenses.Where(_s => _s.IgnoresVisualEffects
                    && (_distance <= _s.Range)))
                {
                    if (!_magicDarkPiercing.UsesSenseTransit
                        || _magicDarkPiercing.CarrySenseInteraction(locator.Map, observer, targetLocation,
                        ITacticalInquiryHelper.EmptyArray))
                    {
                        if (_magicDarkPiercing.UsesSight)
                        {
                            // obtained highest detail
                            _effect = VisualEffect.Normal;
                            break;
                        }
                        else
                        {
                            _effect = VisualEffect.FormOnly;
                        }
                    }
                }
            }
            #endregion

            // still need to determine visual effect
            if (_effect == VisualEffect.Unseen)
            {
                // in range visual senses
                var _level = locator.Map.GetLightLevel(targetLocation);
                // TODO: max over all locations
                var _sight = filteredSenses.Where(_s => _s.UsesSight && _s.Range >= _distance).ToList();
                _effect = VisualEffectHandler.GetSightedVisualEffect(_sight, _level, _distance);

                // if not normal or monochrome, allow non-sighted sense a chance to affect visualization
                if ((_effect != VisualEffect.Brighter)
                    && (_effect != VisualEffect.Normal)
                    && (_effect != VisualEffect.Monochrome))
                {
                    foreach (var _formSense in filteredSenses.Where(_s => !_s.UsesSight
                        && (_distance <= _s.Range)))
                    {
                        if (!_formSense.UsesSenseTransit
                            || _formSense.CarrySenseInteraction(locator.Map, observer, targetLocation,
                            ITacticalInquiryHelper.EmptyArray))
                        {
                            _effect = VisualEffect.FormOnly;
                        }
                    }
                }
            }
            return _effect;
        }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => PortalledObjectA.GetInfo(actor, baseValues);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => PortalledObjectA.MergeConnectedInfos(fetchedInfo, actor);

        // IActionSource Members
        public IVolatileValue ActionClassLevel
            => new Deltable(1);

        #region ITacticalActionProvider Members

        public IEnumerable<OptionAimOption> PortalForceOptions
        {
            get
            {
                yield return new OptionAimOption() { Key = @"Push", Description = @"Push away", Name = @"Push" };
                yield return new OptionAimOption() { Key = @"Pull", Description = @"Pull towards", Name = @"Pull" };
                yield return new OptionAimOption() { Key = @"Left", Description = @"Slide left", Name = @"Left" };
                yield return new OptionAimOption() { Key = @"Right", Description = @"Slide right", Name = @"Right" };
                yield return new OptionAimOption() { Key = @"Up", Description = @"Slide up", Name = @"Up" };
                yield return new OptionAimOption() { Key = @"Down", Description = @"Slide down", Name = @"Down" };
                yield break;
            }
        }

        public IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            // tactical actions only are those directly provided by the object
            var _budget = budget as LocalActionBudget;
            if (_budget.CanPerformBrief)
            {
                // if there is an explicit opener/closer, no default OpenCloseAction
                if (!this.HasActiveAdjunct<OpenerCloserTarget>())
                {
                    yield return new OpenCloseAction(this, this, @"101");
                }

                if (_budget.CanPerformTotal)
                {
                    // force open
                    yield return new ForceOpen(this, this,
                        new OptionAim(@"Force.Type", @"Direction to force", true, FixedRange.One, FixedRange.One, PortalForceOptions), @"211");
                }
            }
            yield break;
        }

        public bool IsContextMenuOnly => true;

        #endregion

        public virtual int ArmorRating
            => this.GetArmorRating(PortalledObjectA.Sizer);

        public virtual void AttendeeAdjustments(IAttackSource source, AttackData attack) { this.DoAttendeeAdjustments(source, attack); }

        protected override string ClassIconKey
            => string.Empty;

        public override IEnumerable<string> IconKeys
            => PortalledObjectA?.IconKeys ?? new string[] { };

        #region IProvideSaves

        // portals are never "attended", so always fail saves

        public BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
            => null;

        public virtual bool AlwaysFailsSave
            => true;

        #endregion

        #region IStructureDamage

        public int GetHardness()
            => PortalledObjectA.GetHardness();

        public int StructurePoints
        {
            get => PortalledObjectA.StructurePoints;
            set
            {
                PortalledObjectA.StructurePoints = value;
                if (PortalledObjectA.StructurePoints <= 0)
                {
                    // A was destroyed, so also destroy B
                    PortalledObjectB.DoDestruction();

                    // then get rid of anything else
                    this.UnPath();
                    this.UnGroup();
                }
            }
        }

        // very unlikely to ever need these two
        public double FallReduce => PortalledObjectA.FallReduce;
        public int MaxFallSpeed => PortalledObjectA.MaxFallSpeed;

        #endregion
    }
}