using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Items.Armor;
using Uzi.Ikosa.Items.Shields;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Manages encumberance (check) penalties
    /// </summary>
    [Serializable]
    public class Encumberance : IModifier, ICreatureBound, IMonitorChange<Physical>,
        IMonitorChange<DeltaValue>, IMonitorChange<Body>, IMonitorChange<ISlottedItem>,
        IControlChange<EncumberanceVal>
    {
        #region Construction
        public Encumberance(Creature loadCreature)
        {
            _LoadedCreature = loadCreature;
            _Terminator = new TerminateController(this);
            _ValueCtrlr = new ChangeController<DeltaValue>(this, new DeltaValue(0)); // delta (encumberance check penalty) control
            _EVCtrl = new ChangeController<EncumberanceVal>(this, EncumberanceVal.Unencumbered);
            _LoadedCreature.Abilities.Strength.AddChangeMonitor(this); // delta (strength) monitor
            _LoadedCreature.BodyDock.AddChangeMonitor(this);           // body monitor
            _LoadedCreature.ObjectLoad.AddChangeMonitor(this);         // weight monitor
            _Run = new Delta(0, typeof(Encumberance), @"Encumberance");
            _LoadedCreature.RunFactor.Deltas.Add(_Run);
            AttachBody(_LoadedCreature.Body);
            DoValueChanged();
        }
        #endregion

        #region Body swapping
        /// <summary>need to detach body item slots when no longer in use</summary>
        private void DetachBody(Body body)
        {
            _ArmorWorn = null;
            _ShieldCarried = null;
            foreach (var _slot in body.ItemSlots.AllSlots)
            {
                if (_slot.SlotType.Equals(ItemSlot.ArmorRobeSlot) || _slot.SlotType.Equals(ItemSlot.HoldingSlot))
                {
                    // unlink callback
                    // JDO: 20170512 (not sure why this is here -->): _slot.AddChangeMonitor(this);
                    _slot.RemoveChangeMonitor(this);
                }
            }
        }

        /// <summary>need to attach a body to get armor/shields and monitor item slots for changes</summary>
        private void AttachBody(Body body)
        {
            foreach (var _slot in body.ItemSlots.AllSlots)
            {
                if (_slot.SlotType.Equals(ItemSlot.ArmorRobeSlot))
                {
                    // found an armor slot (assume only one armor robe slot!)
                    _slot.AddChangeMonitor(this);
                }
                else if (_slot.SlotType.Equals(ItemSlot.HoldingSlot))
                {
                    // found a holding slot
                    _slot.AddChangeMonitor(this);
                }
            }

            FindActiveItems(body);
        }
        #endregion

        #region private void FindActiveItems(Body body)
        private void FindActiveItems(Body body)
        {
            foreach (var _slot in body.ItemSlots.AllSlots)
            {
                if (_slot.SlottedItem != null)
                {
                    if (_slot.SlotType == ItemSlot.ArmorRobeSlot)
                    {
                        // found an armor slot (assume only one armor robe slot!)
                        if (_slot.SlottedItem is ArmorBase _armor
                            && _armor.ProficiencyType != ArmorProficiencyType.None)
                        {
                            // got some armor!
                            _ArmorWorn = _armor;
                        }
                    }
                    else if (_slot.SlotType == ItemSlot.HoldingSlot)
                    {
                        // found a holding slot
                        if (_slot.SlottedItem is ShieldBase _shield)
                        {
                            if (_ShieldCarried != null)
                            {
                                // check if its a bigger penalty
                                if (_ShieldCarried.CheckPenalty.EffectiveValue < _shield.CheckPenalty.EffectiveValue)
                                {
                                    _ShieldCarried = _shield;
                                }
                            }
                            else
                            {
                                _ShieldCarried = _shield;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region state
        private Delta _Run;
        private ArmorBase _ArmorWorn = null;
        private ShieldBase _ShieldCarried = null;
        protected Creature _LoadedCreature;
        private TerminateController _Terminator;
        private ChangeController<DeltaValue> _ValueCtrlr;
        private ChangeController<EncumberanceVal> _EVCtrl = null;
        private int _LastValue = 1;
        #endregion

        // TODO: when overloaded, only move 5'/round as total action or drop held object

        public ArmorBase ArmorWorn => _ArmorWorn;

        public EncumberanceVal EncumberanceValue
            => Unencumbered ? EncumberanceVal.Unencumbered
            : Encumbered ? EncumberanceVal.Encumbered
            : AtlasMustShrug ? EncumberanceVal.Overloaded
            : EncumberanceVal.GreatlyEncumbered;

        #region Wimpy!
        /// <summary>Creature is below the light load limit and wearing nothing heavier than light armor.</summary>
        public bool Unencumbered
            => ((_ArmorWorn?.ProficiencyType ?? ArmorProficiencyType.None) <= ArmorProficiencyType.Light)
            && NotWeighedDown;

        /// <summary>Creature has an easy time carrying this stuff.</summary>
        public bool NotWeighedDown
            => (_LoadedCreature.ObjectLoad.Weight <= _LoadedCreature.CarryingCapacity.LightLoadLimit);
        #endregion

        #region Feel the weight!
        /// <summary>Creature is in the medium load range or wearing medium armor.</summary>
        public bool Encumbered
            => ((_ArmorWorn?.ProficiencyType ?? ArmorProficiencyType.None) <= ArmorProficiencyType.Medium)
            || WeighedDown;

        /// <summary>Creature will be encumbered</summary>
        public bool WeighedDown
            => (_LoadedCreature.ObjectLoad.Weight > _LoadedCreature.CarryingCapacity.LightLoadLimit)
            && (_LoadedCreature.ObjectLoad.Weight <= _LoadedCreature.CarryingCapacity.MediumLoadLimit);
        #endregion

        #region Heavy Load and Greatly Encumbered Flags
        /// <summary>Creature is heavily loaded or wearing heavy armor.</summary>
        public bool GreatlyEncumbered
            => ((_ArmorWorn?.ProficiencyType ?? ArmorProficiencyType.None) <= ArmorProficiencyType.Heavy)
            || Straining;

        /// <summary>Creature will be greatly encumbered</summary>
        public bool Straining
            => (_LoadedCreature.ObjectLoad.Weight > _LoadedCreature.CarryingCapacity.MediumLoadLimit);

        /// <summary>You're carrying too much!</summary>
        public bool AtlasMustShrug
            => (_LoadedCreature.ObjectLoad.Weight > _LoadedCreature.CarryingCapacity.HeavyLoadLimit);
        #endregion

        #region IDelta Members
        // Modifier represents the check penalty
        public int Value
        {
            get
            {
                if (_LastValue <= 0)
                {
                    return _LastValue;
                }

                // sum up penalty for armor
                var _penalty = 0;
                if (_ArmorWorn != null)
                {
                    _penalty += _ArmorWorn.CheckPenalty.EffectiveValue;
                }
                if (_ShieldCarried != null)
                {
                    _penalty += _ShieldCarried.CheckPenalty.EffectiveValue;
                }

                // get worst penalty we can find
                if (NotWeighedDown)
                {
                    _LastValue = _penalty;
                }
                else if (WeighedDown)
                {
                    _LastValue = (_penalty < -3 ? _penalty : -3);
                }
                else
                {
                    _LastValue = (_penalty < -6 ? _penalty : -6);
                }

                return _LastValue;
            }
        }

        public object Source
            => this;

        public string Name
            => @"Encumberance";

        public bool Enabled
        {
            get
            {
                return true;
            }
            set
            {
                // ignore ??? consider gaseous form...
            }
        }
        #endregion

        #region ValueChanged Event
        protected void DoValueChanged()
        {
            // delta value changed
            _ValueCtrlr.DoValueChanged(new DeltaValue(Value));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(@"Value"));

            // need to ensure condition and filter are applied (and provider)
            var _unprep = Creature.Adjuncts.OfType<UnpreparedToDodge>().FirstOrDefault(_d => _d.Source == this);
            if (AtlasMustShrug)
            {
                if (_unprep == null)
                {
                    _unprep = new UnpreparedToDodge(this);
                    Creature.AddAdjunct(_unprep);
                }
            }
            else if (_unprep != null)
            {
                _unprep.Eject();
            }

            // encumberance rating changed
            EncumberanceCtrl.DoValueChanged(EncumberanceValue);

            // run penalty if more than encumbered
            _Run.Value = (EncumberanceValue >= EncumberanceVal.Encumbered) ? -1 : 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EncumberanceValue)));
        }

        #region IControlChange<DeltaValue> Members
        public void AddChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _ValueCtrlr.AddChangeMonitor(subscriber);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> subscriber)
        {
            _ValueCtrlr.RemoveChangeMonitor(subscriber);
        }
        #endregion
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion

        // ICreatureBound Members
        public Creature Creature => _LoadedCreature;

        #region IMonitorChange<DeltaValue> Members
        void IMonitorChange<DeltaValue>.PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
        }

        // strength changed
        void IMonitorChange<DeltaValue>.ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            DoValueChanged();
        }

        void IMonitorChange<DeltaValue>.PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
        }
        #endregion

        #region IMonitorChange<Body> Members
        void IMonitorChange<Body>.PreValueChanged(object sender, ChangeValueEventArgs<Body> args)
        {
        }

        /// <summary>
        /// when a creature's body changes, re-do the event hooks and look for items in slots
        /// </summary>
        void IMonitorChange<Body>.ValueChanged(object sender, ChangeValueEventArgs<Body> args)
        {
            // body change (with size change and extra-carry change) effects load limits
            DetachBody(args.OldValue);
            AttachBody(args.NewValue);
            DoValueChanged();
        }

        void IMonitorChange<Body>.PreTestChange(object sender, AbortableChangeEventArgs<Body> args)
        {
        }
        #endregion

        #region IMonitorChange<ISlottedItem> Members
        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            // if anything (armor or shields) changes
            if (args.OldValue != null)
            {
                // at the very least, the old item is being removed, so clear our knowledge of armor and shields
                if (args.OldValue is ArmorBase)
                {
                    _ArmorWorn = null;
                }
                else if (args.OldValue is ShieldBase)
                {
                    _ShieldCarried = null;
                }
            }

            // find active shields and armor
            FindActiveItems(_LoadedCreature.Body);
            _LastValue = 1;
            DoValueChanged();
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args)
        {
        }
        #endregion

        #region IMonitorChange<Physical> Members
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
                DoValueChanged();
            }
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        // IControlChange<EncumberanceVal>
        private ChangeController<EncumberanceVal> EncumberanceCtrl
            => _EVCtrl ??= new ChangeController<EncumberanceVal>(this, EncumberanceValue);

        public void AddChangeMonitor(IMonitorChange<EncumberanceVal> monitor)
        {
            EncumberanceCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<EncumberanceVal> monitor)
        {
            EncumberanceCtrl.RemoveChangeMonitor(monitor);
        }
    }
}