using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    /// <summary>Basic definition of ammunition for inventory management</summary>
    [Serializable]
    public abstract class AmmunitionBase : WeaponHead, ICloneable, IAmmunitionBase
    {
        #region construction
        protected AmmunitionBase(string dmgRoll, DamageType dmgType,
            Material headMaterial, Lethality lethality = Lethality.NormallyLethal)
            : base(null, dmgRoll, dmgType, headMaterial, lethality)
        {
            _ProxyItem = SetupItem();
            _ProxyItem.ItemSizer.AddChangeMonitor(this);
        }

        protected AmmunitionBase(string dmgRoll, DamageType dmgType, int criticalLow, DeltableQualifiedDelta criticalMultiplier,
            Material headMaterial, Lethality lethality = Lethality.NormallyLethal)
            : base(null, dmgRoll, dmgType, criticalLow, criticalMultiplier, headMaterial, lethality)
        {
            _ProxyItem = SetupItem();
            _ProxyItem.ItemSizer.AddChangeMonitor(this);
        }

        protected AmmunitionBase(string dmgRoll, DamageType[] dmgTypes, int criticalLow, DeltableQualifiedDelta criticalMultiplier,
            Material headMaterial, Lethality lethality = Lethality.NormallyLethal)
            : base(null, dmgRoll, dmgTypes, criticalLow, criticalMultiplier, headMaterial, lethality)
        {
            _ProxyItem = SetupItem();
            _ProxyItem.ItemSizer.AddChangeMonitor(this);
        }
        #endregion

        /// <summary>Setup the item characteristics</summary>
        protected abstract ItemBase SetupItem();

        protected ItemBase _ProxyItem;

        protected override Creature MyPossessor => CreaturePossessor;
        protected override ItemSizer MySizer => ItemSizer;

        public bool IsLocal(Locator locator)
            => (this.GetLocated().Locator == locator) || this.HasActiveAdjunct<LocalUnstored>();

        /// <summary>Basics: every hardness of 5 gives a fall reduce of 1 (10 feet)</summary>
        public virtual double FallReduce => _ProxyItem.FallReduce;

        public virtual int MaxFallSpeed => _ProxyItem.MaxFallSpeed;

        protected override string MyName
            => GetKnownName(MyPossessor);

        public override decimal SpecialCost
        {
            get
            {
                // TODO: determine cost for special materials
                // NOTE: ammo is 1/50 of a weapon (2000/50=40)
                return TotalEnhancement.EffectiveValue * TotalEnhancement.EffectiveValue * 40m + (_FixedSpecialCost / 40m);
            }
        }

        #region public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        /// <summary>Used to report damage string in inventory lists, not for in-flight damage</summary>
        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            var _atk = (workSet.InteractData as AttackData);
            var _nonLethal = (_atk == null ? false : _atk.IsNonLethal);

            // accurate for sling ammo and shuriken
            var _roll = new DamageRollPrerequisite(this, workSet, @"Weapon", Name,
                WeaponDamageRollers.DiceLookup[MediumDamageRollString][ContainingWeapon.ItemSizer.EffectiveCreatureSize.Order],
                false, _nonLethal, @"Ammo", 0);
            yield return _roll;
            yield break;
        }
        #endregion

        #region IItemBase Members

        public int ArmorRating => _ProxyItem.ArmorRating;
        public virtual void AttendeeAdjustments(IAttackSource source, AttackData attack) { _ProxyItem.DoAttendeeAdjustments(source, attack); }
        public Price Price => _ProxyItem.Price;
        public Creature CreaturePossessor => _ProxyItem.CreaturePossessor;
        public Deltable Hardness => _ProxyItem.Hardness;
        public int StructurePoints { get => _ProxyItem.StructurePoints; set { _ProxyItem.StructurePoints = value; } }
        public Material ItemMaterial { get => _ProxyItem.ItemMaterial; set { _ProxyItem.ItemMaterial = value; } }
        public ItemSizer ItemSizer => _ProxyItem.ItemSizer;
        public Sizer Sizer => _ProxyItem.Sizer;
        public IGeometricSize GeometricSize => _ProxyItem.GeometricSize;
        public Deltable MaxStructurePoints => _ProxyItem.MaxStructurePoints;
        public double BaseWeight { get => _ProxyItem.BaseWeight; set { _ProxyItem.BaseWeight = value; } }
        public int GetHardness() => _ProxyItem.GetHardness();

        #endregion

        // ICoreItem Members
        public string OriginalName => _ProxyItem.OriginalName;
        public CoreActor Possessor
        {
            get => _ProxyItem.Possessor;
            set => _ProxyItem.Possessor = value;
        }

        #region ICloneable Members

        public abstract object Clone();

        /// <summary>copies all independent adjuncts (not sourced by another adjunct) from the source</summary>
        protected void CopyAdjuncts(IAmmunitionBase sourceAmmo)
        {
            // copy mergeable adjuncts...
            foreach (var _merge in (from _adj in sourceAmmo.Adjuncts
                                    where _adj.MergeID != null && _adj.IsCloneRoot
                                    select _adj))
            {
                var _clone = _merge.Clone() as Adjunct;
                AddAdjunct(_clone);
            }
        }

        #endregion

        #region public override void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        public override void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (sender == _CriticalRangeFactor)
            {
                DoPropertyChanged(@"CriticalLow");
            }
            else if (sender == _DamageBonus)
            {
                DoPropertyChanged(@"CurrentDamageRollString");
            }
            else if (sender == _TotalEnhancement)
            {
                // cost of enhancements changed
                var _square = TotalEnhancement.EffectiveValue;
                _square *= _square;
                var _newPrice = (2000m * _square) / 50;

                // adjust base price cost
                Price.BaseItemExtraPrice += (_newPrice - _LastPrice);
                _LastPrice = _newPrice;
            }
        }
        #endregion

        public abstract Type GetProjectileWeaponType();

        public override CoreSetting Setting
            => _ProxyItem?.CreaturePossessor?.GetTokened()?.Token.Context.ContextSet.Setting;

        public override WeaponHeadInfo ToWeaponHeadInfo(CoreActor actor, bool baseValues)
        {
            var _info = new AmmoInfo
            {
                ID = ID,
                Message = Name,
                DamageRollString = WeaponDamageRollers.DiceLookup[MediumDamageRollString][ItemSizer.EffectiveCreatureSize.Order].ToString(),
                Count = 1,
                BundleID = ID,
                InfoIDs = new HashSet<Guid>(GetInfoIDs(actor))
            };
            _info.Key = AmmoInfo.GetKey(ID, _info.InfoIDs);
            DoCommonInfo(_info, baseValues);
            return _info;
        }

        public abstract IAmmunitionBundle ToAmmunitionBundle(string name);

        public override double Weight => _ProxyItem.Weight;

        public IEnumerable<Guid> GetInfoIDs(CoreActor actor)
            => GetIdentityData.GetIdentities(this, actor).Select(_i => _i.MergeID ?? Guid.Empty);

        public bool IsInfoMatch(CoreActor actor, HashSet<Guid> infoIDs)
        {
            var _ids = new HashSet<Guid>(GetInfoIDs(actor));
            return ((_ids.Count == 0) && (infoIDs.Count == 0))
                || (infoIDs.All(_i => _ids.Contains(_i)) && _ids.All(_i => infoIDs.Contains(_i)));
        }

        #region IProvideSaves

        // saves are for proxy item...

        #region public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        public virtual BestSoftQualifiedDelta GetBestSoftSave(SavingThrowData saveData)
        {
            if (AlwaysFailsSave)
            {
                return null;
            }

            // potential save values
            var _deltables = new List<ConstDeltable>();

            // strongest magic-source aura
            var _casterLevel = (from _msaa in _ProxyItem.Adjuncts.OfType<MagicSourceAuraAdjunct>()
                                where _msaa.IsActive
                                orderby _msaa.CasterLevel descending
                                select new ConstDeltable(Math.Max(_msaa.CasterLevel / 2, 1))).FirstOrDefault();
            if (_casterLevel != null)
            {
                _deltables.Add(_casterLevel);
            }

            if (_deltables.Any())
            {
                return new BestSoftQualifiedDelta(_deltables.ToArray());
            }

            return null;
        }
        #endregion

        // this should never be called directly, since ammunition will be in a bundle...
        public virtual bool AlwaysFailsSave
            => true;

        #endregion
    }
}
