using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    /// <summary>
    /// Ammunition bundled together for interaction as a unit
    /// </summary>
    /// <typeparam name="AmmoType"></typeparam>
    /// <typeparam name="ProjectileWeapon"></typeparam>
    [Serializable]
    public abstract class AmmunitionBundle<AmmoType, ProjectileWeapon> : ItemBase,
        IActionProvider, IAmmunitionTypedBundle<AmmoType, ProjectileWeapon>
        where AmmoType : AmmunitionBoundBase<ProjectileWeapon>
        where ProjectileWeapon : WeaponBase, IProjectileWeapon
    {
        #region ctor()
        protected AmmunitionBundle(string name, Size naturalSize)
            : base(name, naturalSize)
        {
            _CCtrl = new ChangeController<int>(this, 0);
            _AmmoSets = [];
        }
        #endregion

        #region data
        private Dictionary<Guid, AmmoSet<AmmoType, ProjectileWeapon>> _AmmoSets;
        private ChangeController<int> _CCtrl;
        #endregion

        public Type AmmunitionType
            => typeof(AmmoType);

        public int Count
            => _AmmoSets.Sum(_as => _as.Value.Count);

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new AmmoBundleDropHandler());
            base.InitInteractionHandlers();
        }

        private void SyncWeightPrice()
        {
            DoPropertyChanged(nameof(Count));
            Weight = 0d;
            Price.BaseItemExtraPrice = _AmmoSets.Sum(_as => _as.Value.Price);
            if (_AmmoSets.Count == 0)
            {
                this.UnPath();
                this.UnGroup();
            }
        }

        public override double Weight
        {
            get => base.Weight;
            set { CoreSetWeight(_AmmoSets.Sum(_as => _as.Value.Weight)); }
        }

        public void SyncSets()
        {
            SyncWeightPrice();
        }

        public int? Capacity => null;

        public abstract IAmmunitionBase CreateAmmo();

        #region IControlChange<int> Members

        public void AddChangeMonitor(IMonitorChange<int> monitor)
        {
            _CCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<int> monitor)
        {
            _CCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region IActionProvider Members
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            yield return new ConsolidateAmmo<AmmoType, ProjectileWeapon>(this, new ActionTime(TimeType.Total), @"201");
            yield return new ExtractAmmo<AmmoType, ProjectileWeapon>(this, new ActionTime(TimeType.Total), @"202");
            yield break;
        }

        public virtual Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        #region public AmmoType GetAmmunition(CoreActor actor, HashSet<Guid> infoIDs)
        /// <summary>Finds ammo (doesn't use)</summary>
        public AmmoType GetAmmunition(CoreActor actor, HashSet<Guid> infoIDs)
        {
            var _sets = (from _as in AmmunitionSets
                         where _as.Ammunition.IsInfoMatch(actor, infoIDs)
                         select _as).ToList();
            if (_sets.Count == 1)
            {
                // only one
                return _sets[0].Ammunition;
            }
            else if (_sets.Count > 1)
            {
                // randomly pick one from the total matching
                var _total = (byte)Math.Min(_sets.Sum(_as => _as.Count), byte.MaxValue);
                var _pick = DieRoller.RollDie(actor.ID, _total, @"Ammo", @"Random get");
                foreach (var _ammo in _sets)
                {
                    // see if the random value falls in a particular set
                    if (_pick <= _ammo.Count)
                    {
                        return _ammo.Ammunition;
                    }

                    // or draw down the set excluded
                    _pick -= _ammo.Count;
                }

                // just in case
                return _sets.Last().Ammunition;
            }

            // none!
            return null;
        }
        #endregion

        #region public void Use(AmmoType ammunitionBase)
        public void Use(AmmoType ammunitionBase)
        {
            if (_AmmoSets.ContainsKey(ammunitionBase?.ID ?? Guid.Empty))
            {
                // get and update
                var _match = _AmmoSets[ammunitionBase.ID];
                _match.Count--;

                // remove?
                if (_match.Count <= 0)
                {
                    _AmmoSets.Remove(ammunitionBase.ID);
                }
                SyncWeightPrice();
            }
        }
        #endregion

        #region public IEnumerable<AmmoInfo> AmmunitionInfos(CoreActor actor)
        public IEnumerable<AmmoInfo> AmmunitionInfos(CoreActor actor)
        {
            var _gather = new List<AmmoInfo>();
            foreach (var _as in AmmunitionSets)
            {
                var _ammoInfo = _as.ToAmmoInfo(actor);
                if (_ammoInfo != null)
                {
                    _ammoInfo.BundleID = ID;
                    _ammoInfo.Key = AmmoInfo.GetKey(ID, _ammoInfo.InfoIDs);
                    var _match = _gather.FirstOrDefault(_g => _g.Key == _ammoInfo.Key);
                    if (_match != null)
                    {
                        _match.Count += _as.Count;
                    }
                    else
                    {
                        _gather.Add(_ammoInfo);
                    }
                }
            }
            return _gather.Select(_g => _g);
        }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ObjectInfoFactory.CreateInfo<AmmoBundleInfo>(actor, this, baseValues);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
        {
            if (fetchedInfo is AmmoBundleInfo _bundle)
            {
                _bundle.Ammunitions = AmmunitionInfos(actor).ToList();
            }
            return fetchedInfo;
        }

        public IEnumerable<AmmoSet<AmmoType, ProjectileWeapon>> AmmunitionSets
            => _AmmoSets.Select(_as => _as.Value).ToList();

        // master editing
        public IEnumerable<AmmoEditSet> AmmoSets
            => _AmmoSets.Select(_as => new AmmoEditSet(this, _as.Value.Ammunition, _as.Value.Count)).ToList();

        #region public public void SetCount(IAmmunitionBase ammunition, int count)
        public void SetCount(IAmmunitionBase ammunition, int count)
        {
            if (_AmmoSets.ContainsKey(ammunition.ID))
            {
                var _match = _AmmoSets[ammunition.ID];
                _match.Count = count;
                SyncWeightPrice();
            }
        }
        #endregion

        public int GetCount(IAmmunitionBase ammunition)
            => _AmmoSets.ContainsKey(ammunition.ID)
            ? _AmmoSets[ammunition.ID].Count
            : 0;

        #region public (IAmmunitionBase ammo, int count) ExtractAmmo(IAmmunitionBase ammo)
        public (IAmmunitionBase ammo, int count) ExtractAmmo(IAmmunitionBase ammo)
        {
            if (ammo is AmmoType _ammo)
            {
                var _extract = Extract(_ammo);
                if (_extract != null)
                {
                    return (_extract.Ammunition, _extract.Count);
                }
            }
            return (null, 0);
        }
        #endregion

        #region public (AmmoType ammunition, int count) Extract(AmmoType ammunition)
        public AmmoSet<AmmoType, ProjectileWeapon> Extract(AmmoType ammunition)
        {
            if (_AmmoSets.ContainsKey(ammunition.ID))
            {
                var _match = _AmmoSets[ammunition.ID];
                _AmmoSets.Remove(ammunition.ID);
                SyncWeightPrice();
                DoPropertyChanged(nameof(AmmoSets));
                return _match;
            }
            return null;
        }
        #endregion

        #region public AmmunitionBundle<AmmoType, ProjectileWeapon> Extract(CoreActor actor, params (HashSet<Guid> infoIDs, int count)[] exemplars)
        public AmmunitionBundle<AmmoType, ProjectileWeapon> Extract
            (CoreActor actor, params (HashSet<Guid> infoIDs, int count)[] exemplars)
        {
            AmmunitionBundle<AmmoType, ProjectileWeapon> _bundle = null;
            foreach (var (_infoIDs, _count) in exemplars)
            {
                // candidate sets to extract for this exemplar
                var _sets = (from _as in AmmunitionSets
                             where _as.Ammunition.IsInfoMatch(actor, _infoIDs)
                             select _as).ToList();

                // number to get for this examplar
                for (var _cx = 0; _cx < _count; _cx++)
                {
                    // pick one
                    var _total = (byte)Math.Min(_sets.Sum(_as => _as.Count), byte.MaxValue);
                    var _pick = DieRoller.RollDie(actor.ID, _total, @"Ammo", @"Random extract");
                    foreach (var _ammo in _sets.ToList())
                    {
                        // in this set
                        if (_pick <= _ammo.Count)
                        {
                            if (_bundle == null)
                            {
                                // needed to make a bundle
                                _bundle = _ammo.Ammunition.ToAmmunitionBundle(@"Extract") as AmmunitionBundle<AmmoType, ProjectileWeapon>;
                            }
                            else
                            {
                                // or just merge in
                                _bundle.Merge((_ammo.Ammunition, 1));
                            }

                            // draw down set
                            _ammo.Count--;

                            // remove set if needed
                            if (_ammo.Count <= 0)
                            {
                                _AmmoSets.Remove(_ammo.Ammunition.ID);
                                _sets.Remove(_ammo);
                            }
                            break;
                        }
                        else
                        {
                            // skip this set
                            _pick -= _ammo.Count;
                        }
                    }
                }
            }
            SyncWeightPrice();
            DoPropertyChanged(nameof(AmmoSets));
            return _bundle;
        }
        #endregion 

        #region public AmmunitionBundle<AmmoType, ProjectileWeapon> Merge(AmmunitionBundle<AmmoType, ProjectileWeapon> ammoGroup)
        public AmmunitionBundle<AmmoType, ProjectileWeapon> Merge(AmmunitionBundle<AmmoType, ProjectileWeapon> ammoGroup)
        {
            // bundle must be all same size
            if (ammoGroup.ItemSizer.ExpectedCreatureSize.Order == ItemSizer.ExpectedCreatureSize.Order)
            {
                // add to sets matching on adjuncts...
                foreach (var _ammoSet in ammoGroup.AmmunitionSets)
                {
                    // truly extract from source
                    var _extract = ammoGroup.Extract(_ammoSet.Ammunition);
                    var _match = _AmmoSets.Where(_as => _as.Value.Ammunition.IsSameAmmunition(_extract.Ammunition))
                        .FirstOrDefault();
                    if ((_match.Value?.Count ?? 0) > 0)
                    {
                        // add to ammoSet
                        _match.Value.Count += _extract.Count;
                        _match.Value.Ammunition.MergeIdentityCreatures(_extract.Ammunition);
                    }
                    else
                    {
                        // need a new ammoSet
                        _AmmoSets.Add(_extract.Ammunition.ID, _extract);
                    }
                }
                SyncWeightPrice();
                DoPropertyChanged(nameof(AmmoSets));

                // return remaining (which should be empty)
                return ammoGroup;
            }
            return ammoGroup;
        }
        #endregion

        #region public (AmmoType ammunition, int count) Merge((AmmoType ammunition, int count) ammoSet)
        public (AmmoType ammunition, int count) Merge((AmmoType ammunition, int count) ammoSet)
        {
            // bundle must be all same size
            if (ammoSet.ammunition.ItemSizer.ExpectedCreatureSize.Order == ItemSizer.ExpectedCreatureSize.Order)
            {
                var _match = _AmmoSets.Where(_as => _as.Value.Ammunition.IsSameAmmunition(ammoSet.ammunition))
                   .FirstOrDefault();
                if ((_match.Value?.Count ?? 0) > 0)
                {
                    // get and update
                    _match.Value.Count += ammoSet.count;
                    _match.Value.Ammunition.MergeIdentityCreatures(ammoSet.ammunition);
                }
                else
                {
                    // add new
                    _AmmoSets.Add(ammoSet.ammunition.ID, new AmmoSet<AmmoType, ProjectileWeapon>
                    {
                        Ammunition = (AmmoType)ammoSet.ammunition,
                        Count = ammoSet.count
                    });
                }
                SyncWeightPrice();
                DoPropertyChanged(nameof(AmmoSets));

                // return emptied set
                ammoSet.count = 0;
            }
            return ammoSet;
        }
        #endregion

        #region public (IAmmunitionBase ammo, int count) MergeAmmo((IAmmunitionBase ammo, int count) ammoSet)
        public (IAmmunitionBase ammo, int count) MergeAmmo((IAmmunitionBase ammo, int count) ammoSet)
        {
            if (ammoSet.ammo is AmmoType)
            {
                return Merge((ammoSet.ammo as AmmoType, ammoSet.count));
            }
            return ammoSet;
        }
        #endregion

        public override IEnumerable<string> IconKeys
            => _AmmoSets.Count > 0
            ? _AmmoSets[_AmmoSets.Keys.First()].Ammunition.IconKeys
            : base.IconKeys;

        /// <summary>Unattended bundle with no magic ammunition</summary>
        public override bool AlwaysFailsSave
            => !this.HasActiveAdjunct<Attended>()
            && !AmmoSets.Any(_as => _as.Ammunition.HasActiveAdjunct<MagicSourceAuraAdjunct>());
    }
}
