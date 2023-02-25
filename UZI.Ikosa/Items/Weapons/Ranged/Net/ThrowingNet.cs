using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    // TODO: unfolded penalties
    // TODO: damaged to destruction

    [Serializable]
    [WeaponHead(@"-", DamageType.None, 20, 2, typeof(Materials.RopeMaterial), Lethality.AlwaysNonLethal)]
    [ItemInfo(@"Net", @"Exotic Ranged Entangling", @"net")]
    public class ThrowingNet : WeaponBase, IExoticWeapon, IActionProvider, IThrowableWeapon, ICanCover, ICanEscape, ICanBurstFree
    {
        #region construction
        public ThrowingNet()
            : base(@"Net", Size.Medium)
        {
            Setup();
        }

        private void Setup()
        {
            _MainHead = GetWeaponHead<ThrowingNet>();
            _MainHead.AddAdjunct(new ThrowingNetAttackResult(this));
            _WieldTemplate = WieldTemplate.Double;
            _Folded = true;
            ItemMaterial = Materials.RopeMaterial.Static;
            ProficiencyType = WeaponProficiencyType.Exotic;
            Price.CorePrice = 20m;
            BaseWeight = 6d;
            MaxStructurePoints.BaseValue = 5;
            _Burst = new Deltable(25);
            _Escape = new Deltable(20);
        }
        #endregion

        #region data
        private bool _Folded;
        private IWeaponHead _MainHead;
        private Deltable _Escape;
        private Deltable _Burst;
        #endregion

        public IWeaponHead MainHead => _MainHead;
        public override bool IsLightWeapon => false;
        public override bool IsTransferrable => true;
        public bool IsFolded
        {
            get => _Folded;
            set { _Folded = value; }
        }

        /// <summary>Active if slotted on both main and secondary slots</summary>
        public override bool IsActive => (MainSlot != null) && (SecondarySlot != null);

        #region public override IEnumerable<AttackActionBase> WeaponStrikes()
        public override IEnumerable<AttackActionBase> WeaponStrikes()
        {
            // NOTE: net has one strike mode
            yield return new ThrowStrike(_MainHead, this, AttackImpact.Touch, @"101");
            yield break;
        }
        #endregion

        #region IActionProvider Members
        public virtual IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            if (IsActive && (MainSlot is HoldingSlot))
            {

                // find strikes that can be performed based on available ammunition
                if (_budget.CanPerformRegular)
                {
                    foreach (var _strk in WeaponStrikes())
                    {
                        yield return new RegularAttack(_strk);
                    }
                }

                if (_budget.CanPerformTotal && !IsFolded)
                {
                    yield return new FoldNet(this, @"201");
                }
            }
            else if (Adjuncts.OfType<Covering>()
                .Any(_c => _c.IsCovering && _c.CoveringWrapper.CreaturePossessor == _budget.Actor))
            {
                if (_budget.CanPerformTotal)
                {
                    yield return new EscapeArtistry(this, false, @"101");
                    yield return new BurstFree(this, false, @"102");
                }
            }

            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            return GetInfoData.GetInfoFeedback(this, budget.Actor);
        }

        #endregion

        #region IRangedSource Members

        public virtual int RangeIncrement
            => CreaturePossessor?.Feats.Contains(typeof(Feats.FarShotFeat)) ?? false
            ? 20
            : 10;

        public virtual int MaxRange => RangeIncrement;

        #endregion

        public override ActionTime SlottingTime => new ActionTime(TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(TimeType.Brief);
        public override bool UnslottingProvokes => false;

        public override bool IsProficiencySuitable(Interaction interact)
            => (interact.InteractData is RangedAttackData);

        // ICanEscape
        public virtual ActionTime EscapeTime => new ActionTime(TimeType.Total);
        public string EscapeName(CoreActor actor) => GetKnownName(actor);
        public IInteract EscapeFrom => this;
        public Deltable EscapeDifficulty => _Escape;

        public virtual void DoEscape()
        {
            Adjuncts.OfType<Covering>().FirstOrDefault()?.Eject();
        }

        // ICanBurstFree
        public Deltable BurstFreeDifficulty => _Burst;
        public virtual ActionTime BurstFreeTime => new ActionTime(TimeType.Total);
        public string BurstFromName(CoreActor actor) => GetKnownName(actor);
        public IInteract BurstFrom => this;

        public virtual void DoBurstFree()
        {
            // destroy
            StructurePoints = 0;
        }

        // ICanCover
        public void ActivateCover(ICoreObject coreObj)
        {
            if ((coreObj is Creature _critter)
                && !_critter.Adjuncts.OfType<Entangled>()
                .Any(_e => _e.Source == this))
            {
                _critter.AddAdjunct(new Entangled(this));
            }
        }

        public void DeactivateCover(ICoreObject coreObj)
        {
            (coreObj as Creature)
                ?.Adjuncts.OfType<Entangled>()
                .FirstOrDefault(_e => _e.Source == this)
                ?.Eject();
        }
    }
}
