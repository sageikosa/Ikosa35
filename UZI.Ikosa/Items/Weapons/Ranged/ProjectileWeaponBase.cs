using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [Serializable]
    public abstract class ProjectileWeaponBase : WeaponBase, IProjectileWeapon, IActionProvider
    {
        #region construction
        protected ProjectileWeaponBase(string name, int rangeIncrement, Size itemSize)
            : base(name, itemSize)
        {
            _ProxyHead = GetProxyHead();
            _RangeIncrement = rangeIncrement;
        }
        #endregion

        private int _RangeIncrement = 0;

        /// <summary>Used to hold weapon head information for the projectile launcher</summary>
        private IWeaponHead _ProxyHead;

        protected abstract IWeaponHead GetProxyHead();

        public abstract IEnumerable<CoreAction> GetActions(CoreActionBudget budget);

        /// <summary>When in use, uses two hands, even if only 1 is slotted</summary>
        public abstract bool UsesTwoHands { get; }

        public override bool ProvokesMelee => true;

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);

        protected override void OnSetItemSlot()
        {
            if (!CreaturePossessor.Actions.Providers.ContainsKey(this))
            {
                CreaturePossessor.Actions.Providers.Add(this, (IActionProvider)this);
                _ProxyHead.CriticalRangeFactor.Deltas.Add(CreaturePossessor.CriticalRangeFactor);
                _ProxyHead.CriticalDamageFactor.Deltas.Add(CreaturePossessor.CriticalDamageFactor);
            }
            base.OnSetItemSlot();
        }

        protected override void OnClearSlots(ItemSlot slotA, ItemSlot slotB)
        {
            if (CreaturePossessor.Actions.Providers.ContainsKey(this))
            {
                CreaturePossessor.Actions.Providers.Remove(this);
                _ProxyHead.CriticalRangeFactor.Deltas.Remove(CreaturePossessor.CriticalRangeFactor);
                _ProxyHead.CriticalDamageFactor.Deltas.Remove(CreaturePossessor.CriticalDamageFactor);
            }
            base.OnClearSlots(slotA, slotB);
        }

        #region IProjectileWeapon Members
        /// <summary>Default 0</summary>
        public virtual int MaxStrengthBonus => 0;
        public IWeaponHead VirtualHead => _ProxyHead;
        /// <summary>Slings and Shuriken</summary>
        public abstract bool UsesStrengthDamage { get; }
        /// <summary>Longbow and Shortbow</summary>
        public abstract bool TakesStrengthDamagePenalty { get; }
        #endregion

        #region IRangedSource Members
        /// <summary>Range increment, or 3/2*RangeIncrement with far shot</summary>
        public virtual int RangeIncrement
        {
            get
            {
                if (Possessor != null)
                {
                    if (CreaturePossessor.Feats.Contains(typeof(Feats.FarShotFeat)))
                    {
                        return _RangeIncrement * 3 / 2;
                    }
                }

                return _RangeIncrement;
            }
        }

        /// <summary>Default RangeIncrement * 10</summary>
        public virtual int MaxRange
            => RangeIncrement * 10;

        #endregion

        #region IAttackSource Members

        public DeltableQualifiedDelta AttackBonus => _ProxyHead.AttackBonus;
        public int CriticalRange => _ProxyHead.CriticalRange;
        public DeltableQualifiedDelta CriticalRangeFactor => _ProxyHead.CriticalRangeFactor;
        public DeltableQualifiedDelta CriticalDamageFactor => _ProxyHead.CriticalDamageFactor;

        #region public void AttackResult(AttackResultStep result, Interaction workSet)
        public void AttackResult(AttackResultStep result, Interaction workSet)
        {
            // NOTE: don't think this gets used
            // RangedAmmunication handles the AttackResult and gathers adjuncts from the Projector already
        }
        #endregion

        #region public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction workSet)
        public IEnumerable<StepPrerequisite> AttackResultPrerequisites(Interaction attack)
        {
            var _unq = new List<string>();

            #region non-damage prerequisites
            // extra prerequisites due to the weapon...
            var _sources = new List<object>(); ;
            foreach (var _processor in from _proc in Adjuncts.OfType<ISecondaryAttackResult>()
                                       where _proc.PoweredUp
                                       select _proc)
            {
                if (!_sources.Contains(_processor.AttackResultSource))
                {
                    _sources.Add(_processor.AttackResultSource);
                    foreach (var _pre in _processor.AttackResultPrerequisites(attack))
                    {
                        // if the prerequisite assumes a unique key, check for existing
                        if (_pre.UniqueKey)
                        {
                            // if there are no matching bindKeys, add the prerequisite
                            if (!_unq.Contains(_pre.BindKey))
                            {
                                yield return _pre;
                            }
                        }
                        else
                        {
                            // add it!
                            yield return _pre;
                        }

                        // keys
                        _unq.Add(_pre.BindKey);
                    }
                }
            }
            #endregion

            yield break;
        }
        #endregion

        public bool IsSourceChannel(IAttackSource source)
        {
            // same weapon
            if (source == this)
            {
                return true;
            }

            // ranged ammunition from this weapon
            if ((source is RangedAmmunition _ammo) && (_ammo.Projector == this))
            {
                return true;
            }

            return false;
        }

        #endregion

        public int CriticalMultiplier
            => CriticalDamageFactor.QualifiedValue(new Interaction(CreaturePossessor, _ProxyHead, null, null));

        /// <summary>Normal effective critical low score</summary>
        public int CriticalLow
            => 21 - CriticalRange * CriticalRangeFactor.QualifiedValue(
                new Interaction(CreaturePossessor, _ProxyHead, null, null));

        #region IDamageSource Members

        public Alignment Alignment
            => this.GetAlignment();

        public virtual bool IsMagicalDamage
            => this.IsEnhancedActive() || this.IsMagicWeaponActive();

        public DeltableQualifiedDelta DamageBonus
            => _ProxyHead.DamageBonus;

        public abstract IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup);

        public IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            yield break;
        }

        /// <summary>Only for properties of the weapon (not the ammo)</summary>
        public string EffectiveDamageRollString(Interaction workSet)
        {
            var _build = new StringBuilder();
            foreach (var _roll in BaseDamageRollers(workSet, string.Empty, 0).Union(ExtraDamageRollers(workSet)))
            {
                _build.AppendFormat(@"{0}{1}", (_build.Length > 0 ? @" + " : string.Empty), _roll.ToString());
            }
            return _build.ToString();
        }

        public string MediumDamageRollString { get { return _ProxyHead.MediumDamageRollString; } set { _ProxyHead.MediumDamageRollString = value; } }

        public IEnumerable<DamageRollPrerequisite> ExtraDamageRollers(Interaction workSet)
        {
            // extra damage effects due to the weapon...
            foreach (var _dmgRoll in from _wed in this.ExtraDamages()
                                     where _wed.PoweredUp
                                     from _edr in _wed.DamageRollers(workSet)
                                     select _edr)
            {
                yield return _dmgRoll;
            }
            yield break;
        }

        public virtual DamageType[] DamageTypes
            => _ProxyHead.DamageTypes;
        #endregion

        public string CurrentDamageRollString
            => EffectiveDamageRollString(new Interaction(null, null, null, null));

        public ConstDeltable TotalEnhancement
            => _ProxyHead.TotalEnhancement;

        /// <summary>Lists the enhancement bonus of the weapon head (excludes Masterwork enhancement to attack)</summary>
        public int ListedEnhancement
            => TotalEnhancement.Deltas[typeof(Enhancement)]?.Value ?? 0;

        #region public override Info GetInfo(CoreActor actor, bool baseValues)

        protected ProjWeaponInfo ToInfo<ProjWeaponInfo>(CoreActor actor, bool baseValues)
            where ProjWeaponInfo : ProjectileWeaponInfo, new()
        {
            var _info = ObjectInfoFactory.CreateInfo<ProjWeaponInfo>(actor, this, baseValues);
            _info.MaxStrengthBonus = MaxStrengthBonus;
            _info.VirtualHead = VirtualHead.ToWeaponHeadInfo(actor, baseValues);
            _info.RangeIncrement = RangeIncrement;
            _info.MaxRange = MaxRange;
            _info.ProficiencyType = ProficiencyType;
            _info.WieldTemplate = GetWieldTemplate();
            return _info;
        }

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ToInfo<ProjectileWeaponInfo>(actor, baseValues);

        #endregion

        /// <summary>Wield template must be light or unarmed</summary>
        public override bool IsLightWeapon
        {
            get
            {
                switch (GetWieldTemplate())
                {
                    case WieldTemplate.Unarmed:
                    case WieldTemplate.Light:
                        return true;
                }
                return false;
            }
        }

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
