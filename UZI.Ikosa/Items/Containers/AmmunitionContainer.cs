using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Packaging;

namespace Uzi.Ikosa.Items
{
    /// <summary>Base interface for quivers, sling bullet pouches, shuriken pouches and crossbow bolt cases</summary>
    [Serializable]
    public abstract class AmmunitionContainer<AmmoType, ProjectileWeapon> : SlottedItemBase,
        IAmmunitionContainer, IAmmunitionTypedBundle<AmmoType, ProjectileWeapon>
        where AmmoType : AmmunitionBoundBase<ProjectileWeapon>
        where ProjectileWeapon : WeaponBase, IProjectileWeapon
    {
        #region ctor()
        public AmmunitionContainer(int capacity, string name, string slotType)
            : base(name, slotType)
        {
            _Capacity = capacity;
            _Bundle = CreateBundle();
        }
        #endregion

        #region data
        private int _Capacity;
        private AmmunitionBundle<AmmoType, ProjectileWeapon> _Bundle;
        #endregion

        protected abstract AmmunitionBundle<AmmoType, ProjectileWeapon> CreateBundle();

        public Type AmmunitionType
            => typeof(AmmoType);

        public override bool IsTransferrable
            => true;

        #region public int Capacity { get; set; }
        public int? Capacity
        {
            get { return _Capacity; }
            set
            {
                if (value >= Count)
                {
                    _Capacity = value ?? 0;
                    DoPropertyChanged(nameof(Capacity));
                }
            }
        }
        #endregion

        private void SyncWeight() { Weight = 0; }

        public IAmmunitionBase CreateAmmo()
            => _Bundle?.CreateAmmo();

        #region public AmmunitionBundle<AmmoType, ProjectileWeapon> ExtractAll()
        /// <summary>Gets the bundle in the container, and creates a new bundle for the container</summary>
        public AmmunitionBundle<AmmoType, ProjectileWeapon> ExtractAll()
        {
            var _bundle = _Bundle;
            _Bundle = CreateBundle();
            SyncSets();
            DoPropertyChanged(nameof(AmmoSets));
            return _bundle;
        }
        #endregion

        #region public AmmunitionBundle<AmmoType, ProjectileWeapon> Extract(CoreActor actor, params (List<Guid> infoIDs, int count)[] exemplars)
        /// <summary>Creates a bundle from expected ammo examples</summary>
        public AmmunitionBundle<AmmoType, ProjectileWeapon> Extract(CoreActor actor, params (HashSet<Guid> infoIDs, int count)[] exemplars)
        {
            var _return = _Bundle.Extract(actor, exemplars);
            SyncSets();
            DoPropertyChanged(nameof(AmmoSets));
            return _return;
        }
        #endregion

        #region public (IAmmunitionBase ammo, int count) ExtractAmmo(IAmmunitionBase ammo)
        public (IAmmunitionBase ammo, int count) ExtractAmmo(IAmmunitionBase ammo)
        {
            var _extract = _Bundle.ExtractAmmo(ammo);
            SyncSets();
            DoPropertyChanged(nameof(AmmoSets));
            return _extract;
        }
        #endregion

        #region public AmmunitionBundle<AmmoType, ProjectileWeapon> Merge(AmmunitionBundle<AmmoType, ProjectileWeapon> ammoGroup)
        /// <summary>Tries to merge the bundle into the container, returning any leftover</summary>
        public AmmunitionBundle<AmmoType, ProjectileWeapon> Merge(AmmunitionBundle<AmmoType, ProjectileWeapon> ammoGroup)
        {
            if ((ammoGroup.Count + Count) <= Capacity)
            {
                var _return = _Bundle.Merge(ammoGroup);
                SyncSets();
                return _return;
            }
            return ammoGroup;
        }
        #endregion

        public IEnumerable<AmmoInfo> AmmunitionInfos(CoreActor actor)
        {
            foreach (var _info in _Bundle.AmmunitionInfos(actor))
            {
                _info.BundleID = ID;
                yield return _info;
            }
            yield break;
        }

        /// <summary>Finds ammo (doesn't use)</summary>
        public AmmoType GetAmmunition(CoreActor actor, HashSet<Guid> infoIDs)
            => _Bundle.GetAmmunition(actor, infoIDs);

        public void Use(AmmoType ammunition)
        {
            _Bundle.Use(ammunition);
            SyncSets();
        }

        // master editing
        public IEnumerable<AmmoSet<AmmoType, ProjectileWeapon>> AmmunitionSets
            => _Bundle.AmmunitionSets;

        #region public (AmmoType ammunition, int count) Merge((AmmoType ammunition, int count) ammoSet)
        public (AmmoType ammunition, int count) Merge((AmmoType ammunition, int count) ammoSet)
        {
            var _return = _Bundle.Merge(ammoSet);
            SyncSets();
            DoPropertyChanged(nameof(AmmoSets));
            return _return;
        }
        #endregion

        #region bundle IMonitorChange quantity
        public void AddChangeMonitor(IMonitorChange<int> monitor)
        {
            _Bundle.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<int> monitor)
        {
            _Bundle.RemoveChangeMonitor(monitor);
        }
        #endregion

        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield return new ConsolidateAmmo<AmmoType, ProjectileWeapon>(this, new ActionTime(TimeType.Total), @"201");
            yield return new ExtractAmmo<AmmoType, ProjectileWeapon>(this, new ActionTime(TimeType.Total), @"202");
            // TODO: other container actions ???
            yield break;
        }

        public virtual Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        #region public override Info GetInfo(CoreActor actor, bool baseValues)
        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            var _info = ObjectInfoFactory.CreateInfo<AmmoBundleInfo>(actor, this, baseValues);
            _info.Capacity = Capacity;
            return _info;
        }
        #endregion

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
        {
            if (fetchedInfo is AmmoBundleInfo _bundle)
            {
                _bundle.Ammunitions = AmmunitionInfos(actor).ToList();
            }
            return fetchedInfo;
        }

        #region public (IAmmunitionBase ammo, int count) MergeAmmo((IAmmunitionBase ammo, int count) ammoSet)
        /// <summary>merges if the container can handle the entire capacity</summary>
        public (IAmmunitionBase ammo, int count) MergeAmmo((IAmmunitionBase ammo, int count) ammoSet)
        {
            if ((ammoSet.count + Count) <= Capacity)
            {
                var _return = _Bundle.MergeAmmo(ammoSet);
                SyncSets();
                DoPropertyChanged(nameof(AmmoSets));
                return _return;
            }
            return ammoSet;
        }
        #endregion

        #region public void SetCount(IAmmunitionBase ammo, int count)
        public void SetCount(IAmmunitionBase ammo, int count)
        {
            var _curr = _Bundle.GetCount(ammo);
            if (((count - _curr) + (Count)) <= Capacity)
            {
                _Bundle.SetCount(ammo, count);
                SyncSets();
            }
        }
        #endregion

        #region public void SyncSets()
        public void SyncSets()
        {
            _Bundle.SyncSets();
            Price.BaseItemExtraPrice = _Bundle.Price.BasePrice;
            Weight = 0d;
            DoPropertyChanged(nameof(Count));
        }
        #endregion

        public bool ContentsAddToLoad => true;

        #region public override double Weight { get; set; }
        /// <summary>Weight calculation + LoadWeight if necessary.  Setting to anything will recalculate</summary>
        public override double Weight
        {
            get { return base.Weight; }
            set
            {
                base.Weight = 0;
                CoreSetWeight(base.Weight + LoadWeight);
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

        public double LoadWeight => _Bundle.Weight;

        public int Count => _Bundle.Count;

        public IEnumerable<AmmoEditSet> AmmoSets
            => _Bundle.AmmoSets.Select(_as => new AmmoEditSet(this, _as.Ammunition, _as.Count)).ToList();
    }

    public interface IAmmunitionContainer : ISlottedItem, IAmmunitionBundle,
        IMonitorChange<DeltaValue>, ICorePart, IActionSource, IVisible
    {
    }
}
