using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Items.Weapons;
using System.ComponentModel;
using Uzi.Ikosa.Movement;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons.Natural;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Creatures.BodyType
{
    [Serializable]
    public abstract class Body : ICreatureBind, IActionProvider, IMonitorChange<Size>,
        ILinkOwner<LinkableDock<Body>>, ICorePhysical, IMonitorChange<ISlottedItem>, IWeaponProficiency
    {
        #region Construction
        protected Body(Size size, Material bodyMaterial, bool useExtraCarry, int reach, bool groundStability)
        {
            // size
            _Sizer = new BodySizer(size);
            _Sizer.AddChangeMonitor(this);
            NaturalArmor = new NaturalArmorDelta(this);
            _UseExtraCarryingFactor = useExtraCarry;
            NaturalWeapons = [];
            _Dock = new BiDiPtr<BodyDock, Body>(this);
            _BodyMaterial = bodyMaterial;
            _Reach = new Deltable(reach);
            _Weight = 0;
            _Height = 0;
            _Length = 0;
            _Width = 0;
            _BaseHeight = 0;
            _BaseWeight = 0;
            _BaseLength = 0;
            _BaseWidth = 0;
            _ID = Guid.NewGuid();
            _PCtrl = new ChangeController<Physical>(this, new Physical(Physical.PhysicalType.Weight, 0));
            _Features = [];
            _Movements = [];
            _Stability = groundStability ? new GroundStability(this) : null;
        }
        #endregion

        #region Private and Protected Data
        protected ItemSlotSet _ItemSlots = new ItemSlotSet();
        protected Material _BodyMaterial;
        protected BiDiPtr<BodyDock, Body> _Dock;
        protected bool _UseExtraCarryingFactor;
        protected Deltable _Reach;
        protected BodySizer _Sizer;
        private double _BaseWeight;
        private double _Weight;
        private ChangeController<Physical> _PCtrl;
        private double _BaseHeight;
        private double _Height; // in decimal feet
        private double _BaseWidth;
        private double _Width; // in decimal feet
        private double _BaseLength;
        private double _Length; // in decimal feet
        private Collection<BodyFeature> _Features;
        private Collection<MovementBase> _Movements;
        private Guid _ID;
        private GroundStability _Stability;
        #endregion

        /// <summary>Manages natural weapons: reslots automatic weapons (such as claws) when not holding other items.  Provides proficiencies.</summary>
        public readonly NaturalWeaponSet NaturalWeapons;
        public ItemSlotSet ItemSlots => _ItemSlots;
        public NaturalArmorDelta NaturalArmor { get; protected set; }
        public Material BodyMaterial => _BodyMaterial;

        /// <summary>Can be turned into a skeleton</summary>
        public abstract bool HasBones { get; }

        /// <summary>Can be turned into a zombie</summary>
        public abstract bool HasAnatomy { get; }

        /// <summary>Beast of burden?</summary>
        public bool UseExtraCarryingFactor => _UseExtraCarryingFactor;

        /// <summary>True if this body is extra stable against trip or bull rush</summary>
        public bool GroundStability
            => _Stability != null;

        public Deltable ReachSquares => _Reach;
        public BodySizer Sizer => _Sizer;
        public Collection<BodyFeature> Features => _Features;

        /// <summary>Body specific movement modes</summary>
        public Collection<MovementBase> Movements => _Movements;

        #region public double BaseHeight { get; set; }
        /// <summary>Height when creature is at base size</summary>
        public double BaseHeight
        {
            get { return _BaseHeight; }
            set
            {
                _BaseHeight = value;
                Height = double.MinValue;
                DoPropertyChanged(@"BaseHeight");
            }
        }
        #endregion

        #region public double Height { get; set; }
        /// <summary>Set less than 0 to recalc from BaseHeight, otherwise BaseHeight will be calculated from value</summary>
        public double Height
        {
            get { return _Height; }
            set
            {
                var _factor = Sizer.SpatialFactor;
                if (value >= 0)
                {
                    // recalc base height
                    _BaseHeight = value / _factor;
                }

                // pre-height change
                var _newHeight = new Physical(Physical.PhysicalType.Height, BaseHeight * _factor);
                _PCtrl.DoPreValueChanged(_newHeight);

                // change
                _Height = BaseHeight * _factor;

                // notify
                DoPropertyChanged(@"Height");
                _PCtrl.DoValueChanged(_newHeight);
            }
        }
        #endregion

        #region public double BaseWidth { get; set; }
        /// <summary>Width when creature is at base size</summary>
        public double BaseWidth
        {
            get { return _BaseWidth; }
            set
            {
                _BaseWidth = value;
                Width = double.MinValue;
                DoPropertyChanged(@"BaseWidth");
            }
        }
        #endregion

        #region public double Width { get; set; }
        /// <summary>Set less than 0 to recalc from BaseWidth, otherwise BaseWidth will be calculated from value</summary>
        public double Width
        {
            get { return _Width; }
            set
            {
                var _factor = Sizer.SpatialFactor;
                if (value >= 0)
                {
                    // recalc base Width
                    _BaseWidth = value / _factor;
                }

                // pre-Width change
                var _newWidth = new Physical(Physical.PhysicalType.Width, BaseWidth * _factor);
                _PCtrl.DoPreValueChanged(_newWidth);

                // change
                _Width = BaseWidth * _factor;

                // notify
                DoPropertyChanged(@"Width");
                _PCtrl.DoValueChanged(_newWidth);
            }
        }
        #endregion

        #region public double BaseLength { get; set; }
        /// <summary>Length when creature is at base size</summary>
        public double BaseLength
        {
            get { return _BaseLength; }
            set
            {
                _BaseLength = value;
                Length = double.MinValue;
                DoPropertyChanged(@"BaseLength");
            }
        }
        #endregion

        #region public double Length { get; set; }
        /// <summary>Set less than 0 to recalc from BaseLength, otherwise BaseLength will be calculated from value</summary>
        public double Length
        {
            get { return _Length; }
            set
            {
                var _factor = Sizer.SpatialFactor;
                if (value >= 0)
                {
                    // recalc base Length
                    _BaseLength = value / _factor;
                }

                // pre-Length change
                var _newLength = new Physical(Physical.PhysicalType.Length, BaseLength * _factor);
                _PCtrl.DoPreValueChanged(_newLength);

                // change
                _Length = BaseLength * _factor;

                // notify
                DoPropertyChanged(@"Length");
                _PCtrl.DoValueChanged(_newLength);
            }
        }
        #endregion

        #region public double BaseWeight { get; set; }
        /// <summary>Weight when creature is at base size</summary>
        public double BaseWeight
        {
            get { return _BaseWeight; }
            set
            {
                _BaseWeight = value;
                Weight = double.MinValue;
                DoPropertyChanged(@"BaseWeight");
            }
        }
        #endregion

        #region public double Weight { get; set; }
        /// <summary>Set less than 0 to recalc from BaseWeight, otherwise BaseWeight will be calculated from value</summary>
        public double Weight
        {
            get { return _Weight; }
            set
            {
                var _factor = Sizer.WeightFactor;
                if (value >= 0)
                {
                    // set new base weight
                    _BaseWeight = value / _factor;
                }

                // pre-weight change
                var _newWeight = new Physical(Physical.PhysicalType.Weight, BaseWeight * _factor);
                _PCtrl.DoPreValueChanged(_newWeight);

                // change
                _Weight = _newWeight.Amount;

                // notify
                DoPropertyChanged(@"Weight");
                _PCtrl.DoValueChanged(_newWeight);
            }
        }
        #endregion

        public Guid ID => _ID;
        public Guid PresenterID => _ID;

        public Creature Creature => _Dock.LinkDock.Creature;

        #region protected virtual void OnConnectBody()
        protected virtual void OnConnectBody()
        {
            // movements
            foreach (var _mv in Movements)
            {
                Creature.Movements.Add(_mv);
            }

            // add actions and weight monitoring
            Creature.Actions.Providers.Add(this, this);
            AddChangeMonitor(Creature);
            Weight = double.MinValue;

            // natural weapons
            NaturalWeapons.BindTo(Creature);
            Creature.Proficiencies.Add(this);

            // stability
            if (_Stability != null)
            {
                Creature.Abilities.Dexterity.CheckQualifiers.Deltas.Add(_Stability);
                Creature.Abilities.Strength.CheckQualifiers.Deltas.Add(_Stability);
            }
        }
        #endregion

        #region protected virtual void OnDisconnectBody()
        protected virtual void OnDisconnectBody()
        {
            // stability
            if (_Stability != null)
            {
                _Stability.DoTerminate();
            }

            // natural weapons
            Creature.Proficiencies.Remove(this);
            NaturalWeapons.UnbindFromCreature();

            // movements
            foreach (var _mv in Movements)
            {
                Creature.Movements.Remove(_mv);
            }

            // remove actions and weight monitoring
            RemoveChangeMonitor(Creature);
            Creature.Actions.Providers.Remove(this);
            Creature.Weight = 0;
        }
        #endregion

        #region ICreatureBind Members
        public bool BindTo(Creature creature)
        {
            if (!_Dock.WillAbortChange(creature.BodyDock))
            {
                // dock
                _Dock.LinkDock = creature.BodyDock;
                return true;
            }
            return false;
        }

        public void UnbindFromCreature()
        {
            if (!_Dock.WillAbortChange(null))
            {
                // undock
                _Dock.LinkDock = null;
            }
        }
        #endregion

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        /// <summary>When overriding, yield the output to get drop items...</summary>
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // fight defensively (from the body? sure, why not, has to come from somewhere)
            if ((budget is LocalActionBudget _budget) && _budget.CanPerformRegular)
            {
                yield return new DefensiveCombatChoice(Creature);

                // multi-weapon fighting: more than one active weapon ...
                // ... or at least one double weapon capable of multi-head attacks
                if (((from _wpn in ItemSlots.HeldObjects.OfType<IWeapon>()
                      where _wpn.IsActive
                      select _wpn).Count() > 1) ||
                    ItemSlots.HeldObjects.OfType<DoubleMeleeWeaponBase>().Any(_dbl
                        => _dbl.IsActive && !_dbl.UseAsTwoHanded && (_dbl.SecondarySlot != null)))
                {
                    yield return new MultiWeaponCombatChoice(Creature);
                }
            }

            yield return new IgnoreFriendlyOpportunities(Creature);

            // pretty much just holding slots (¿for now?)
            foreach (var _action in from _ap in _ItemSlots.AllSlots.OfType<IActionProvider>()
                                    from _act in _ap.GetActions(budget)
                                    select _act)
            {
                yield return _action;
            }

            foreach (var _unslot in from _slot in ItemSlots.AllSlots
                                    where _slot.AllowUnSlotAction
                                    && (_slot.SlottedItem is SlottedItemBase)
                                    && !(_slot.SlottedItem is NaturalWeapon)
                                    && (_slot.SlottedItem.UnslottingTime != null)
                                    select new RemoveSlottedItem(_slot.SlottedItem as SlottedItemBase, $@"{_slot.SlotType}.{_slot.SubType}"))
            {
                yield return _unslot;
            }

            yield break;
        }
        #endregion

        #region IMonitorChange<Size> Members

        protected virtual void OnSizeChanged(Size oldValue, Size newValue)
        {
            foreach (var _wpn in NaturalWeapons)
            {
                // natural weapons are always sized for the creature
                _wpn.ItemSizer.ExpectedCreatureSize = newValue;
            }

            // set to less than 0 for recalculation
            Height = double.MinValue;
            Weight = double.MinValue;
            Width = double.MinValue;
            Length = double.MinValue;
        }

        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            OnSizeChanged(args.OldValue, args.NewValue);
        }

        #endregion

        #region ILinkOwner<LinkableDock<Body>> Members

        public void LinkDropped(LinkableDock<Body> changer)
        {
            if (changer is BodyDock _bodyDock)
            {
                if ((_Dock.LinkDock == _bodyDock) && (_bodyDock.Body != this))
                {
                    // undock body
                    OnDisconnectBody();
                    _Dock.LinkDock = null;
                }
            }
        }

        public void LinkAdded(LinkableDock<Body> changer)
        {
            if (changer is BodyDock _bodyDock)
            {
                OnConnectBody();
            }
        }

        #endregion

        #region IControlChange<Physical> Members

        public void AddChangeMonitor(IMonitorChange<Physical> monitor)
        {
            _PCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<Physical> monitor)
        {
            _PCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region public Info GetProviderInfo(CoreActionBudget budget)

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            var _info = new BodyInfo
            {
                Message = string.Empty,
                ID = ID,
                NaturalArmor = NaturalArmor.ToDeltableInfo(),
                Material = BodyMaterial.ToMaterialInfo(),
                UseExtraCarryingFactor = UseExtraCarryingFactor,
                ReachSquares = ReachSquares.ToDeltableInfo(),
                Size = Sizer.Size.ToSizeInfo(),
                BaseHeight = BaseHeight,
                BaseWeight = BaseWeight,
                BaseLength = BaseLength,
                BaseWidth = BaseWidth,
                Height = Height,
                Weight = Weight,
                Length = Length,
                Width = Width,
                Features = Features
                    .OrderByDescending(_f => _f.IsMajor)
                    .Select(_f => _f.Description).ToArray()
            };
            return _info;
        }

        #endregion

        protected abstract Body InternalClone(Material material);

        /// <summary>Clones body including features, itemslots and physical properties</summary>
        public Body CloneBody()
            => CloneBody(BodyMaterial);

        /// <summary>Clones body including features, itemslots and physical properties</summary>
        public Body CloneBody(Material material)
        {
            var _body = InternalClone(material);

            // natural armor
            _body.NaturalArmor.BaseValue = NaturalArmor.BaseValue;

            // features
            foreach (var _bFeat in Features)
            {
                _body.Features.Add(_bFeat);
            }

            // physical
            _body.BaseHeight = BaseHeight;
            _body.BaseLength = BaseLength;
            _body.BaseWeight = BaseWeight;
            _body.BaseWidth = BaseWidth;

            return _body;
        }

        #region IMonitorChange<ISlottedItem> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<ISlottedItem> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<ISlottedItem> args)
        {
            // when clearing the unarmed slot, we should fill it with something...
            if (args.NewValue == null)
            {
                if ((sender is ItemSlot _slot) && (_slot.SlotType == ItemSlot.UnarmedSlot))
                {
                    EnsureUnarmedWeapon(_slot);
                }
            }
        }

        protected void EnsureUnarmedWeapon(ItemSlot slot)
        {
            var _uData = new GetUnarmedWeapon(Creature);
            var _uSet = new Interaction(Creature, this, Creature, _uData);
            Creature.HandleInteraction(_uSet);
            var _feedback = _uSet.Feedback.OfType<GetUnarmedWeaponFeedback>().FirstOrDefault();
            if (_feedback != null)
            {
                _feedback.Weapon.SetItemSlot(slot);
            }
        }

        #endregion

        #region IWeaponProficiency Members

        public bool IsProficientWith(WeaponProficiencyType profType, int powerLevel)
            => false;

        public bool IsProficientWith<WpnType>(int powerLevel) where WpnType : IWeapon
            => false;

        public bool IsProficientWith(IWeapon weapon, int powerLevel)
            => (weapon != null) && (weapon.WieldTemplate == WieldTemplate.Unarmed);

        public bool IsProficientWithWeapon(Type type, int powerLevel)
            => false;

        public string Description
            => @"Proficient with unarmed strike";

        #endregion
    }
}
