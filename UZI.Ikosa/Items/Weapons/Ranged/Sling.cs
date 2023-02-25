using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Contracts;
using System.Linq;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    [
    Serializable,
    WeaponHead(@"1d4", DamageType.Bludgeoning, 20, 2, typeof(Materials.SteelMaterial), Contracts.Lethality.NormallyLethal),
    ItemInfo(@"Sling", @"Simple Ranged 1d4 (x2) Bludgeoning", @"sling")
    ]
    public class Sling : ProjectileWeaponBase, IActionProvider, IWieldMountable
    {
        // NOTE: using stones instead of bullets does lower damage (smaller size) and applies a -1 on attack!

        public Sling()
            : base(@"Sling", 50, Size.Tiny)
        {
            Setup();
        }

        protected override IWeaponHead GetProxyHead()
            => GetWeaponHead<Sling>(true);

        private void Setup()
        {
            ItemMaterial = Materials.LeatherMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Simple;
            _WieldTemplate = WieldTemplate.OneHanded;
            MaxStructurePoints.BaseValue = 2;
            Price.CorePrice = 0.25m;
            BaseWeight = 0.25d;
        }

        protected SlingAmmo[] _Ammo;

        /// <summary>Indicates the sling is ready to strike</summary>
        public bool IsLoaded => (_Ammo?.Length ?? 0) > 0;

        /// <summary>Indicates the sling has more than one bolt loaded (which will prevent regular attacks)</summary>
        public bool IsOverloaded => (_Ammo?.Length ?? 0) > 1;

        public SlingAmmo[] Ammunition { get => _Ammo; set => _Ammo = value; }

        /// <summary>Sling can add strength to damage (in any amount)</summary>
        public override int MaxStrengthBonus => 1000;

        public int MinimumReloadHands => 2;
        public TimeType ReloadAction => TimeType.Brief;
        public override bool IsActive => MainSlot != null;

        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            if (IsLoaded && !IsOverloaded)
            {
                var _rAmmo = new RangedAmmunition(this, Ammunition[0]);
                yield return new SlingStrike(_rAmmo, @"101");
            }
            yield break;
        }

        /// <summary>When in use, uses two hands, even if only 1 is slotted (FALSE)</summary>
        public override bool UsesTwoHands => false;

        #region IActionProvider Members

        public override IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // get budget info
            var _budget = budget as LocalActionBudget;

            // reload?
            var _rlAct = ReloadAction;
            if ((_rlAct == TimeType.Free)
                || ((_rlAct == TimeType.Brief) && _budget.CanPerformBrief)
                || ((_rlAct == TimeType.Total) && _budget.CanPerformTotal))
            {
                yield return new ReloadSling(this, @"201");
            }

            // regular attacks only if the xbow is loaded, but not overloaded
            if (IsActive)
            {
                // TODO: special weapon abilities

                if (_budget.CanPerformRegular)
                {
                    foreach (var _strike in WeaponStrikes())
                        yield return new RegularAttack(_strike);
                }
            }
            yield break;
        }

        #endregion

        #region public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        {
            // creature-based damage bonuses
            if (CreaturePossessor != null)
                yield return new DamageRollPrerequisite(typeof(Creature), workSet, $@"{keyFix}Creature", @"Creature",
                    new ConstantRoller(CreaturePossessor.ExtraWeaponDamage.QualifiedValue(workSet)), false,
                    (workSet.InteractData as AttackData)?.IsNonLethal ?? false, @"Creature", minGroup);
            yield break;
        }
        #endregion

        protected override string ClassIconKey => @"sling";

        /// <summary>Slings get damage bonuses for high strength</summary>
        public override bool UsesStrengthDamage => true;

        /// <summary>Slings get damage penalties for low strength</summary>
        public override bool TakesStrengthDamagePenalty => true;

        public override bool IsTransferrable => true;

        public IEnumerable<string> SlotTypes
        {
            get
            {
                yield return MountSlot.WieldMount;
                yield break;
            }
        }


        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => ToInfo<LoadableProjectileWeaponInfo>(actor, baseValues);

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
        {
            if (fetchedInfo is LoadableProjectileWeaponInfo _loadable)
            {
                _loadable.LoadedAmmunition = _Ammo?.Select(_a => GetInfoData.GetInfoFeedback(_a, actor)).ToList() ?? new List<Info>();
            }
            return fetchedInfo;
        }
    }
}
