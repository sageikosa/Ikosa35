using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class KeyRing : ItemBase, ITacticalActionProvider, IObjectContainer, IKeyRingMountable
    {
        public KeyRing(string name)
            : base(name, Size.Miniature)
        {
            _Keys = [];
            _KeyCtrl = new ChangeController<ICoreObject>(this, null);
            ItemMaterial = MetalMaterial.CommonStatic;
            BaseWeight = 1d / 16d;
        }

        #region data
        private ChangeController<ICoreObject> _KeyCtrl;
        private List<IKeyRingMountable> _Keys;
        #endregion

        protected override string ClassIconKey => nameof(KeyRing);

        public int Count => _Keys.Count;
        public IEnumerable<ICoreObject> Objects => _Keys.Select(_k => _k);
        public bool ContentsAddToLoad => true;

        #region public double MaximumLoadWeight { get; }
        public double MaximumLoadWeight
        {
            get
            {
                return ItemSizer.NaturalSize.Order switch
                {
                    -4 => 0.5d,
                    -3 => 2d,
                    -2 => 4d,
                    -1 => 8d,
                    0 => 16d,
                    1 => 32d,
                    2 => 64d,
                    3 => 128d,
                    4 => 256d,
                    _ => 2d,
                };
            }
            set { }
        }
        #endregion

        public bool Contains(ICoreObject item)
            => _Keys.Contains(item as IKeyRingMountable);

        public bool CanHold(ICoreObject obj)
            => (obj is IKeyRingMountable _krm)
            && !_Keys.Contains(_krm)
            && (StructurePoints > 0)
            && !_KeyCtrl.WillAbortChange(obj, @"Add")
            // no sizing offsets on neither key nor ring
            && (_krm.ItemSizer.SizeOffset.EffectiveValue == 0)
            && (ItemSizer.SizeOffset.EffectiveValue == 0)
            // key is smaller or equal to ring size
            && (_krm.ItemSizer.NaturalSize.Order <= ItemSizer.NaturalSize.Order)
            && ((_krm.Weight + LoadWeight) <= MaximumLoadWeight);

        public double LoadWeight
            => _Keys.Sum(bo => bo.Weight);

        #region public override double Weight { get; set; }
        /// <summary>Weight calculation + LoadWeight.  Setting to anything will recalculate</summary>
        public override double Weight
        {
            get { return base.Weight; }
            set
            {
                base.Weight = 0;
                CoreSetWeight(base.Weight + LoadWeight);
                DoPropertyChanged(nameof(Weight));
            }
        }
        #endregion

        #region public double TareWeight { get; set; }
        /// <summary>Implemented as alias for BaseWeight</summary>
        public double TareWeight
        {
            get { return BaseWeight; }
            set
            {
                BaseWeight = value;
                Weight = 0;
                DoPropertyChanged(nameof(TareWeight));
            }
        }
        #endregion

        #region public void Add(ICoreObject item)
        public void Add(ICoreObject item)
        {
            if (item is IKeyRingMountable _krm
                && !_Keys.Contains(_krm)
                && !_KeyCtrl.WillAbortChange(item, @"Add")
                && ((item.Weight + LoadWeight) <= MaximumLoadWeight))
            {
                _KeyCtrl.DoPreValueChanged(item, @"Add");
                item.AddAdjunct(new Contained(this));
                _Keys.Add(_krm);

                // track weight
                item.AddChangeMonitor(this);
                Weight = 0d;

                _KeyCtrl.DoValueChanged(item, @"Add");
                DoPropertyChanged(nameof(Connected));
                DoPropertyChanged(nameof(Objects));
                this.IncreaseSerialState();
            }
        }
        #endregion

        #region public bool Remove(ICoreObject item)
        public bool Remove(ICoreObject item)
        {
            if ((item is IKeyRingMountable _krm)
                && _Keys.Contains(item)
                && !_KeyCtrl.WillAbortChange(item, @"Remove"))
            {
                _KeyCtrl.DoPreValueChanged(item, @"Remove");
                _Keys.Remove(_krm);
                _krm.GetContained()?.Eject();

                // untrack weight
                if (ContentsAddToLoad)
                {
                    item.RemoveChangeMonitor(this);
                }

                Weight = 0d;

                _KeyCtrl.DoValueChanged(item, @"Remove");
                DoPropertyChanged(nameof(Connected));
                DoPropertyChanged(nameof(Objects));
                this.IncreaseSerialState();
                return true;
            }
            return false;
        }
        #endregion

        #region public IEnumerable<ICoreObject> AllLoadedObjects()
        public IEnumerable<ICoreObject> AllLoadedObjects()
        {
            foreach (var _key in _Keys)
            {
                yield return _key;
            }

            foreach (var _loaded in from _contain in _Keys.OfType<ILoadedObjects>()
                                    from _iC in _contain.AllLoadedObjects()
                                    select _iC)
            {
                yield return _loaded;
            }

            yield break;
        }
        #endregion

        #region IControlChange<ICoreObject> Members
        public void AddChangeMonitor(IMonitorChange<ICoreObject> monitor) { _KeyCtrl.AddChangeMonitor(monitor); }
        public void RemoveChangeMonitor(IMonitorChange<ICoreObject> monitor) { _KeyCtrl.RemoveChangeMonitor(monitor); }
        #endregion

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if (budget.Actor is Creature _critter)
            {
                if ((budget as LocalActionBudget)?.CanPerformTotal ?? false)
                {
                    if (StoreObject.CanStore(this, _critter))
                    {
                        yield return new StoreObject(this, @"201") { TimeCost = new ActionTime(Contracts.TimeType.Total) };
                    }

                    if (RetrieveObject.CanRetrieve(this, _critter))
                    {
                        yield return new LoadObject(this, @"211") { TimeCost = new ActionTime(Contracts.TimeType.Total) };
                    }

                    if ((Count > 0) && RetrieveObject.CanRetrieve(this, _critter))
                    {
                        yield return new RetrieveObject(this, @"202") { TimeCost = new ActionTime(Contracts.TimeType.Total) };
                        yield return new UnloadObject(this, @"212") { TimeCost = new ActionTime(Contracts.TimeType.Total) };
                    }
                }
            }

            foreach (var _key in _Keys.OfType<KeyItem>())
            {
                foreach (var _act in _key.GetActions(budget))
                {
                    yield return _act;
                }
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        public bool IsContextMenuOnly => throw new NotImplementedException();

        public IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
            => GetActions(budget);

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
                var _load = TareWeight + LoadWeight;

                // if the weight is now above the maximum load weight, the item that changed weight must be ejected
                if (_load > MaximumLoadWeight)
                {
                    if (sender is IObjectBase _object)
                    {
                        Remove(_object);

                        // if ejected from a container that is contained, try to place it in an outer container
                        var _newHolder = _object.FindAcceptableContainer(this);
                        if (_newHolder != null)
                        {
                            // move object to new container
                            _newHolder.Add(_object);
                            return;
                        }

                        // add object to locator (or create locator)
                        var _top = this.FindTopContainer();
                        if (_top != null)
                        {   // if ejected from an uncontained container
                            var _locator = Locator.FindFirstLocator(_top);
                            // ... or create a new locator (object presenter?)
                            var _location = new CellLocation(_locator.Location);
                            var _drop = new Drop(null, _locator?.Map, _location, true);
                            var _iAct = new Interaction(null, this, _object, _drop);
                            _object.HandleInteraction(_iAct);
                        }
                    }
                }
                else
                {
                    Weight = 0d;
                }
            }
        }
        #endregion
    }
}
