using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Secure a padlock to an openable.  Automatically binds to an IAnchorage, and controls an IOpenable.</summary>
    [Serializable]
    public class Hasp : ThrowBolt, IAnchorage
    {
        #region Construction
        /// <summary>Secure a padlock to an openable.  Automatically binds to an IAnchorage, and controls an IOpenable.</summary>
        public Hasp(string name, Material material, int disableDifficulty, IOpenable target, IAnchorage anchorage)
            : base(name, material, disableDifficulty, target, anchorage, false)
        {
            // setup capacity to hold fasteners
            _Connected = new Collection<ICoreObject>();
            _COCtrl = new ChangeController<ICoreObject>(this, null);
        }
        #endregion

        #region Private Data
        private ChangeController<ICoreObject> _COCtrl;
        private Collection<ICoreObject> _Connected;
        private double _TareWeight;
        #endregion

        public override IEnumerable<ICoreObject> Connected
            => _Connected?.Select(_c => _c) ?? new ICoreObject[] { };

        protected override bool OnWillAbort(OpenStatus openState)
            // if the fasten target is not closed, the hasp cannot change state
            => !FastenTarget.OpenState.IsClosed || base.OnWillAbort(openState);

        #region IAnchorage Members
        public bool CanAcceptAnchor(IAdjunctable newAnchor)
        {
            var _core = newAnchor as CoreObject;
            var _fast = _core.Adjuncts.OfType<Fastener>().FirstOrDefault();
            if (_fast != null)
            {
                return (_core != null) && !_Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Add");
            }
            return false;
        }

        public bool CanEjectAnchor(IAdjunctable existingAnchor)
            => (existingAnchor is ICoreObject _core)
            && _Connected.Contains(_core) && !_COCtrl.WillAbortChange(_core, @"Remove");

        public void AcceptAnchor(IAdjunctable newAnchor)
        {
            if (newAnchor is ICoreObject _core)
            {
                if (CanAcceptAnchor(newAnchor))
                {
                    _COCtrl.DoPreValueChanged(_core, @"Add");
                    _Connected.Add(_core);

                    // track weight
                    _core.AddChangeMonitor(this);

                    _COCtrl.DoValueChanged(_core, @"Add");
                    DoPropertyChanged(nameof(Connected));
                    DoPropertyChanged(nameof(Anchored));
                }
            }
        }

        public void EjectAnchor(IAdjunctable existingAnchor)
        {
            if (existingAnchor is ICoreObject _core)
            {
                if (CanEjectAnchor(existingAnchor))
                {
                    _COCtrl.DoPreValueChanged(_core, @"Remove");
                    _Connected.Remove(_core);

                    // untrack weight
                    _core.RemoveChangeMonitor(this);

                    _COCtrl.DoValueChanged(_core, @"Remove");
                    DoPropertyChanged(nameof(Connected));
                    DoPropertyChanged(nameof(Anchored));
                }
            }
        }

        public IEnumerable<ICoreObject> Anchored
            => _Connected.Select(_c => _c);
        #endregion

        #region IControlChange<ICoreObject> Members
        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            _COCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor)
        {
            _COCtrl.RemoveChangeMonitor(monitor);
        }
        #endregion

        #region ILoadedObjects Members

        public IEnumerable<ICoreObject> AllLoadedObjects()
            => AllConnected(null);

        public bool ContentsAddToLoad
            => true;

        private void RecalcWeight() { Weight = _TareWeight + (ContentsAddToLoad ? LoadWeight : 0); }

        public double TareWeight
        {
            get { return _TareWeight; }
            set
            {
                _TareWeight = value;
                RecalcWeight();
            }
        }

        public double LoadWeight
            => _Connected.Sum(bo => bo.Weight);

        #endregion

        #region IMonitorChange<Physical> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<Physical> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<Physical> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<Physical> args)
        {
            if (args.NewValue.PropertyType == Physical.PhysicalType.Weight)
                RecalcWeight();
        }

        #endregion

        #region IAudibleOpenable Members
        public override SoundRef GetOpeningSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"opening",
                (0, @"squeaking"),
                (5, $@"{GetMaterialString()}squeaking"),
                (10, $@"{GetMaterialString()} hasp swinging")),
                15, 90, serialState);

        public override SoundRef GetOpenedSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;

        public override SoundRef GetClosingSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closing",
                (0, @"squeaking"),
                (5, $@"{GetMaterialString()}squeaking"),
                (10, $@"{GetMaterialString()} hasp swinging")),
                15, 90, serialState);

        public override SoundRef GetClosedSound(Func<Guid> idFactory, object source, ulong serialState)
            => new SoundRef(new Audible(idFactory(), ID, @"closed",
                (0, @"bang"),
                (5, $@"{GetMaterialString()}bang"),
                (10, $@"{GetMaterialString()} hasp stopping")),
                12, 90, serialState);

        public override SoundRef GetBlockedSound(Func<Guid> idFactory, object source, ulong serialState)
            => null;
        #endregion
    }

    /// <summary>Binds a fastener to a hasp [ActionBase (Regular)]</summary>
    [Serializable]
    public class FastenToHasp : ActionBase
    {
        /// <summary>Binds a fastener to a hasp [ActionBase (Regular)]</summary>
        public FastenToHasp(FastenerObject source, string orderKey)
            : base(source, new ActionTime(TimeType.Regular), true, false, orderKey)
        {
        }

        public FastenerObject FastenerObject => Source as FastenerObject;
        public override string Key => @"Fastener.BindToHasp";
        public override string DisplayName(CoreActor actor) => $@"Fasten {FastenerObject.GetKnownName(actor)} to Hasp";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Fasten", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(FastenerObject, observer);
            return _obs;
        }

        protected override ActivityResponse OnCanPerformActivity(CoreActivity activity)
            // creature must be holding the fastener and the target must be a hasp
            => new ActivityResponse(((activity?.Targets[0]?.Target as Hasp) != null)
                && ((activity?.Actor as Creature)?.Body.ItemSlots.HeldObjects.Contains(FastenerObject) ?? false));

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _fast = FastenerObject;
            if (activity.Targets[0].Target is Hasp _hasp)
            {
                var _binding = new ObjectBound(_hasp);
                _fast.AddAdjunct(_binding);
                if (_binding.Anchor == FastenerObject)
                {
                    if (activity.Actor is Creature _critter)
                    {
                        // unslot from holding slot...
                        ItemSlot _slot = _critter.Body.ItemSlots.SlotForItem(_fast);
                        if (_slot != null)
                        {
                            // even a holding wrapper can handle this...
                            _slot.SlottedItem.ClearSlots();
                        }
                    }
                    return null;
                }
            }
            return activity.GetActivityResultNotifyStep(@"Couldn't attach");
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AwarenessAim(@"Hasp", @"Hasp", FixedRange.One, FixedRange.One, new MeleeRange(), new ObjectTargetType());
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }

    /// <summary>Unbinds a fastener from a hasp. [ActionBase (Regular)]</summary>
    [Serializable]
    public class UnfastenFromHasp : ActionBase
    {
        /// <summary>Unbinds a fastener from a hasp. [ActionBase (Regular)]</summary>
        public UnfastenFromHasp(FastenerObject source, string orderKey)
            : base(source, new ActionTime(TimeType.Regular), true, false, orderKey)
        {
        }

        public FastenerObject FastenerObject => Source as FastenerObject;
        public override string Key => @"Fastener.UnBindFromHasp";
        public override string DisplayName(CoreActor actor) => $@"Unfasten {FastenerObject.GetKnownName(actor)} from Hasp";

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Unfasten", activity.Actor, observer, FastenerObject as CoreObject);

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _binding = FastenerObject.Adjuncts.OfType<ObjectBound>().FirstOrDefault();
            if (_binding != null)
            {
                FastenerObject.RemoveAdjunct(_binding);
                if (_binding.Anchor == null)
                {
                    if (activity.Actor is Creature _critter)
                    {
                        var _slot = _critter.Body.ItemSlots[ItemSlot.HoldingSlot, true];
                        if (_slot == null)
                        {
                            // Drop (no holding slots)
                            Drop.DoDrop(_critter, FastenerObject, this, false);
                        }
                        else
                        {
                            // wrap in a holding wrapper
                            var _wrap = new HoldingWrapper(_critter, FastenerObject);

                            // put in slot
                            _wrap.SetItemSlot(_slot);
                        }
                    }
                }
            }
            return null;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}
